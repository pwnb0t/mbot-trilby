using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using ownbotsidekick.Controls;
using ownbotsidekick.Input;
using ownbotsidekick.Services;
using Forms = System.Windows.Forms;

namespace ownbotsidekick
{
    public partial class MainWindow : Window
    {
        private const int WmHotKey = 0x0312;
        private const int HotkeyId = 1;
        private const int GwlExStyle = -20;
        private const int WsExNoActivate = 0x08000000;
        private const int WsExTransparent = 0x00000020;
        private const int VkBack = 0x08;
        private const int VkTab = 0x09;
        private const int VkReturn = 0x0D;
        private const int VkEscape = 0x1B;
        private const int VkSpace = 0x20;
        private const int Vk0 = 0x30;
        private const int Vk9 = 0x39;
        private const int VkA = 0x41;
        private const int VkZ = 0x5A;
        private const int VkNumpad0 = 0x60;
        private const int VkNumpad9 = 0x69;
        private const int MaxVisibleSearchResults = 15;
        private const int OverlayBottomReserveMinPixels = 56;
        private const int OverlayBottomReservePaddingPixels = 8;

        private readonly string _logFilePath;
        private readonly AppSettings _settings;
        private readonly List<string> _allClipTriggers = new();
        private readonly List<string> _filteredClipTriggers = new();
        private bool _hotkeyRegistered;
        private bool _exitRequested;
        private string _searchQuery = string.Empty;
        private Forms.NotifyIcon? _trayIcon;
        private Icon? _customTrayIcon;
        private SidekickApiClientService? _sidekickApiClient;
        private ClipPlaybackCoordinator? _clipPlaybackCoordinator;
        private OverlayInputRouter? _overlayInputRouter;
        private IntPtr _windowHandle = IntPtr.Zero;
        private bool _overlayVisible;

        public MainWindow()
        {
            InitializeComponent();
            SearchPanel.ClipSelected += SearchPanel_ClipSelected;

            _settings = LoadSettings();
            Topmost = _settings.Overlay.Topmost;
            if (_settings.SidekickApi.Enabled)
            {
                _sidekickApiClient = new SidekickApiClientService(
                    _settings.SidekickApi.BaseUrl,
                    _settings.SidekickApi.GuildId
                );
            }
            _clipPlaybackCoordinator = new ClipPlaybackCoordinator(_sidekickApiClient);

            var logDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ownbotsidekick",
                "logs"
            );
            Directory.CreateDirectory(logDirectory);
            _logFilePath = Path.Combine(logDirectory, "overlay.log");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RenderSearchState();

            Log("Overlay loaded.");
            Log($"Hotkey: {DescribeHotkey(_settings.Hotkey)}");
            Log(_settings.SidekickApi.Enabled
                ? "Sidekick API client enabled."
                : "Sidekick API client disabled in appsettings.");
            Log($"Clip triggers: A={_settings.SidekickApi.ClipATrigger}, B={_settings.SidekickApi.ClipBTrigger}, C={_settings.SidekickApi.ClipCTrigger}");
            if (_sidekickApiClient is not null)
            {
                _ = InitializeHealthCheckAsync();
                _ = LoadClipCatalogAsync("startup");
            }
            else
            {
                UpdateClipCountText(0, "API disabled");
            }

            if (_settings.Overlay.StartHidden)
            {
                Log("Overlay starts hidden per appsettings.");
            }
            else
            {
                ShowOverlay("Overlay shown.");
            }

            ApplyOverlayPanelLayout();
        }

        private async void ClipAButton_Click(object sender, RoutedEventArgs e)
        {
            await PlayClipAsync("Clip A", _settings.SidekickApi.ClipATrigger);
        }

        private async void ClipBButton_Click(object sender, RoutedEventArgs e)
        {
            await PlayClipAsync("Clip B", _settings.SidekickApi.ClipBTrigger);
        }

        private async void ClipCButton_Click(object sender, RoutedEventArgs e)
        {
            await PlayClipAsync("Clip C", _settings.SidekickApi.ClipCTrigger);
        }

        private async void RefreshClipsButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadClipCatalogAsync("manual refresh");
        }

        private async void SearchPanel_ClipSelected(object? sender, string trigger)
        {
            await PlayClipAsync(trigger, trigger);
        }

        private void CloseOverlayButton_Click(object sender, RoutedEventArgs e)
        {
            HideOverlay("Overlay hidden from close button.");
        }

        private void Log(string message)
        {
            var timestamped = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} {message}";
            Debug.WriteLine(timestamped);
            try
            {
                File.AppendAllText(_logFilePath, timestamped + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to write log file: {ex.Message}");
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var helper = new WindowInteropHelper(this);
            _windowHandle = helper.Handle;
            var source = HwndSource.FromHwnd(helper.Handle);
            source?.AddHook(WndProc);
            EnableNoActivateMode(helper.Handle);
            SetOverlayInteractionEnabled(false);

            InitializeTrayIcon();
            RegisterOverlayHotkey(helper.Handle);
            _overlayInputRouter = new OverlayInputRouter(
                isOverlayVisible: () => _overlayVisible,
                handleOverlayVirtualKey: HandleOverlayKeyDown,
                isPointInsideOverlayPanel: IsPointInsideOverlayPanel,
                onOutsideClick: () => HideOverlay("Overlay hidden (outside click)."),
                log: Log,
                dispatcher: Dispatcher
            );
            _overlayInputRouter.Start();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (_exitRequested)
            {
                base.OnClosing(e);
                return;
            }

            e.Cancel = true;
            HideOverlay("Overlay hidden to tray. Use tray icon -> Exit to close app.");
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_hotkeyRegistered)
            {
                var helper = new WindowInteropHelper(this);
                UnregisterHotKey(helper.Handle, HotkeyId);
                _hotkeyRegistered = false;
            }

            _overlayInputRouter?.Dispose();
            _overlayInputRouter = null;

            if (_trayIcon is not null)
            {
                _trayIcon.Visible = false;
                _trayIcon.Dispose();
                _trayIcon = null;
            }

            if (_customTrayIcon is not null)
            {
                _customTrayIcon.Dispose();
                _customTrayIcon = null;
            }

            _sidekickApiClient?.Dispose();
            _sidekickApiClient = null;
            _clipPlaybackCoordinator = null;

            base.OnClosed(e);
        }

        private async System.Threading.Tasks.Task InitializeHealthCheckAsync()
        {
            try
            {
                if (_sidekickApiClient is null)
                {
                    return;
                }

                var message = await _sidekickApiClient.GetHealthSummaryAsync();
                Log(message);
            }
            catch (Exception ex)
            {
                Log($"Health check failed: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task LoadClipCatalogAsync(string reason)
        {
            if (_clipPlaybackCoordinator is null)
            {
                return;
            }

            var result = await _clipPlaybackCoordinator.LoadClipsAsync(reason);
            foreach (var logLine in result.LogLines)
            {
                Log(logLine);
            }

            if (result.Success)
            {
                _allClipTriggers.Clear();
                _allClipTriggers.AddRange(result.Triggers);
                RebuildFilteredResults();
                UpdateClipCountText(_allClipTriggers.Count, "loaded");
            }
            else
            {
                _allClipTriggers.Clear();
                RebuildFilteredResults();
                UpdateClipCountText(0, _sidekickApiClient is null ? "API disabled" : "load failed");
            }
        }

        private async System.Threading.Tasks.Task<bool> PlayClipAsync(string clipName, string trigger)
        {
            if (_clipPlaybackCoordinator is null)
            {
                return false;
            }

            var result = await _clipPlaybackCoordinator.PlayClipAsync(clipName, trigger);
            foreach (var logLine in result.LogLines)
            {
                Log(logLine);
            }

            if (result.ShouldHideOverlay)
            {
                HideOverlay("Overlay hidden after clip play.");
            }

            return result.Success;
        }

        private void RegisterOverlayHotkey(IntPtr hwnd)
        {
            var modifiers = ParseModifiers(_settings.Hotkey.Modifiers) | HotkeyModifiers.NoRepeat;
            var key = ParseKey(_settings.Hotkey.Key);
            var virtualKey = KeyInterop.VirtualKeyFromKey(key);

            _hotkeyRegistered = RegisterHotKey(hwnd, HotkeyId, (uint)modifiers, (uint)virtualKey);
            if (_hotkeyRegistered)
            {
                Log($"Global hotkey registered: {DescribeHotkey(_settings.Hotkey)}");
                return;
            }

            Log("Failed to register configured hotkey. Falling back to Alt+Oem3.");
            _settings.Hotkey = new HotkeySettings();
            modifiers = ParseModifiers(_settings.Hotkey.Modifiers) | HotkeyModifiers.NoRepeat;
            key = ParseKey(_settings.Hotkey.Key);
            virtualKey = KeyInterop.VirtualKeyFromKey(key);
            _hotkeyRegistered = RegisterHotKey(hwnd, HotkeyId, (uint)modifiers, (uint)virtualKey);

            if (_hotkeyRegistered)
            {
                Log($"Fallback hotkey registered: {DescribeHotkey(_settings.Hotkey)}");
            }
            else
            {
                Log("Fallback hotkey registration also failed.");
            }
        }

        private void ToggleOverlayVisibility()
        {
            if (_overlayVisible)
            {
                HideOverlay("Overlay hidden.");
                return;
            }

            ShowOverlay("Overlay shown.");
        }

        private void InitializeTrayIcon()
        {
            var trayIconImage = LoadTrayIcon();
            _trayIcon = new Forms.NotifyIcon
            {
                Icon = trayIconImage,
                Text = "ownbotsidekick",
                Visible = true
            };

            var trayMenu = new Forms.ContextMenuStrip();
            trayMenu.Items.Add("Show Overlay", null, (_, _) => ShowOverlayFromTray());
            trayMenu.Items.Add("Hide Overlay", null, (_, _) => HideOverlayFromTray());
            trayMenu.Items.Add(new Forms.ToolStripSeparator());
            trayMenu.Items.Add("Exit", null, (_, _) => ExitFromTray());

            _trayIcon.ContextMenuStrip = trayMenu;
            _trayIcon.DoubleClick += (_, _) => ToggleOverlayVisibility();
            Log(trayIconImage == SystemIcons.Application
                ? "Tray icon initialized with fallback icon (mbot.ico not available)."
                : "Tray icon initialized from mbot.ico.");
        }

        private Icon LoadTrayIcon()
        {
            var iconPath = Path.Combine(AppContext.BaseDirectory, "mbot.ico");
            if (!File.Exists(iconPath))
            {
                return SystemIcons.Application;
            }

            try
            {
                _customTrayIcon = new Icon(iconPath);
                return _customTrayIcon;
            }
            catch
            {
                return SystemIcons.Application;
            }
        }

        private void ShowOverlayFromTray()
        {
            if (_overlayVisible)
            {
                return;
            }

            ShowOverlay("Overlay shown from tray.");
        }

        private void EnableNoActivateMode(IntPtr hwnd)
        {
            var currentExStyle = GetWindowLongPtr(hwnd, GwlExStyle).ToInt64();
            var updatedExStyle = new IntPtr(currentExStyle | WsExNoActivate);
            SetWindowLongPtr(hwnd, GwlExStyle, updatedExStyle);
            Log("Overlay no-activate mode enabled.");
        }

        private void HideOverlayFromTray()
        {
            if (!_overlayVisible)
            {
                return;
            }

            HideOverlay("Overlay hidden from tray.");
        }

        private void ExitFromTray()
        {
            _exitRequested = true;
            Log("Exit requested from tray.");
            Close();
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WmHotKey && wParam.ToInt32() == HotkeyId)
            {
                ToggleOverlayVisibility();
                handled = true;
            }

            return IntPtr.Zero;
        }

        private void UpdateClipCountText(int count, string status)
        {
            ClipCountTextBlock.Text = $"Clips: {count} ({status})";
        }

        private void ApplyOverlayPanelLayout()
        {
            var taskbarHeightEstimate = Math.Max(0, SystemParameters.PrimaryScreenHeight - SystemParameters.WorkArea.Height);
            var reservedBottom = Math.Max(
                OverlayBottomReserveMinPixels,
                taskbarHeightEstimate + OverlayBottomReservePaddingPixels
            );
            OverlayPanelBorder.Margin = new Thickness(0, 0, 0, reservedBottom);
        }

        private void ResetSearchState()
        {
            _searchQuery = string.Empty;
            RebuildFilteredResults();
        }

        private void RebuildFilteredResults()
        {
            _filteredClipTriggers.Clear();

            if (!string.IsNullOrEmpty(_searchQuery))
            {
                _filteredClipTriggers.AddRange(
                    _allClipTriggers
                        .Where(trigger => trigger.StartsWith(_searchQuery, StringComparison.OrdinalIgnoreCase))
                        .Take(MaxVisibleSearchResults)
                );
            }

            RenderSearchState();
        }

        private void RenderSearchState()
        {
            SearchPanel.UpdateSearchState(_searchQuery, _filteredClipTriggers);
        }

        private async System.Threading.Tasks.Task PlayFirstFilteredResultAsync()
        {
            if (string.IsNullOrEmpty(_searchQuery) || _filteredClipTriggers.Count == 0)
            {
                return;
            }

            var first = _filteredClipTriggers[0];
            await PlayClipAsync(first, first);
        }

        private bool IsPointInsideOverlayPanel(System.Windows.Point screenPoint)
        {
            if (OverlayPanelBorder.ActualWidth <= 0 || OverlayPanelBorder.ActualHeight <= 0)
            {
                return false;
            }

            var topLeft = OverlayPanelBorder.PointToScreen(new System.Windows.Point(0, 0));
            var panelRect = new Rect(topLeft.X, topLeft.Y, OverlayPanelBorder.ActualWidth, OverlayPanelBorder.ActualHeight);
            return panelRect.Contains(screenPoint);
        }

        private bool HandleOverlayKeyDown(int virtualKey)
        {
            if (virtualKey == VkEscape)
            {
                HideOverlay("Overlay hidden.");
                return true;
            }

            if (virtualKey == VkTab)
            {
                ResetSearchState();
                return true;
            }

            if (virtualKey == VkBack)
            {
                if (string.IsNullOrEmpty(_searchQuery))
                {
                    return false;
                }

                _searchQuery = _searchQuery[..^1];
                RebuildFilteredResults();
                return true;
            }

            if (virtualKey == VkReturn || virtualKey == VkSpace)
            {
                if (string.IsNullOrEmpty(_searchQuery))
                {
                    return false;
                }

                _ = PlayFirstFilteredResultAsync();
                return true;
            }

            var character = TryGetAlphanumericCharacter(virtualKey);
            if (character is null)
            {
                return false;
            }

            _searchQuery += character.Value;
            RebuildFilteredResults();
            return true;
        }

        private static char? TryGetAlphanumericCharacter(int virtualKey)
        {
            if (virtualKey >= VkA && virtualKey <= VkZ)
            {
                return char.ToLowerInvariant((char)virtualKey);
            }

            if (virtualKey >= Vk0 && virtualKey <= Vk9)
            {
                return (char)virtualKey;
            }

            if (virtualKey >= VkNumpad0 && virtualKey <= VkNumpad9)
            {
                return (char)('0' + (virtualKey - VkNumpad0));
            }

            return null;
        }

        private static AppSettings LoadSettings()
        {
            var settingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (!File.Exists(settingsPath))
            {
                return new AppSettings();
            }

            try
            {
                var json = File.ReadAllText(settingsPath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return settings ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }

        private static HotkeyModifiers ParseModifiers(string configuredModifiers)
        {
            var result = HotkeyModifiers.None;
            var pieces = configuredModifiers.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var piece in pieces)
            {
                if (piece.Equals("Alt", StringComparison.OrdinalIgnoreCase))
                {
                    result |= HotkeyModifiers.Alt;
                }
                else if (piece.Equals("Control", StringComparison.OrdinalIgnoreCase) || piece.Equals("Ctrl", StringComparison.OrdinalIgnoreCase))
                {
                    result |= HotkeyModifiers.Control;
                }
                else if (piece.Equals("Shift", StringComparison.OrdinalIgnoreCase))
                {
                    result |= HotkeyModifiers.Shift;
                }
                else if (piece.Equals("Win", StringComparison.OrdinalIgnoreCase) || piece.Equals("Windows", StringComparison.OrdinalIgnoreCase))
                {
                    result |= HotkeyModifiers.Win;
                }
            }

            return result == HotkeyModifiers.None ? HotkeyModifiers.Alt : result;
        }

        private static Key ParseKey(string configuredKey)
        {
            if (Enum.TryParse<Key>(configuredKey, true, out var key) && key != Key.None)
            {
                return key;
            }

            return Key.Oem3;
        }

        private static string DescribeHotkey(HotkeySettings hotkey)
        {
            return $"{hotkey.Modifiers}+{hotkey.Key}";
        }

        private void HideOverlay(string logMessage)
        {
            if (!_overlayVisible)
            {
                return;
            }

            ResetSearchState();
            RootOverlayGrid.Visibility = Visibility.Collapsed;
            _overlayVisible = false;
            SetOverlayInteractionEnabled(false);
            Log(logMessage);
        }

        private void ShowOverlay(string logMessage)
        {
            ResetSearchState();
            RootOverlayGrid.Visibility = Visibility.Visible;
            _overlayVisible = true;
            SetOverlayInteractionEnabled(true);
            Topmost = _settings.Overlay.Topmost;
            Log(logMessage);
        }

        private void SetOverlayInteractionEnabled(bool enabled)
        {
            if (_windowHandle == IntPtr.Zero)
            {
                return;
            }

            var currentExStyle = GetWindowLongPtr(_windowHandle, GwlExStyle).ToInt64();
            long updatedExStyle;
            if (enabled)
            {
                updatedExStyle = (currentExStyle | WsExNoActivate) & ~WsExTransparent;
            }
            else
            {
                updatedExStyle = (currentExStyle | WsExNoActivate | WsExTransparent);
            }

            SetWindowLongPtr(_windowHandle, GwlExStyle, new IntPtr(updatedExStyle));
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
        private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [Flags]
        private enum HotkeyModifiers : uint
        {
            None = 0x0000,
            Alt = 0x0001,
            Control = 0x0002,
            Shift = 0x0004,
            Win = 0x0008,
            NoRepeat = 0x4000
        }

        private sealed class AppSettings
        {
            public HotkeySettings Hotkey { get; set; } = new();
            public OverlaySettings Overlay { get; set; } = new();
            public SidekickApiSettings SidekickApi { get; set; } = new();
        }

        private sealed class HotkeySettings
        {
            public string Modifiers { get; set; } = "Alt";
            public string Key { get; set; } = "Oem3";
        }

        private sealed class OverlaySettings
        {
            public bool StartHidden { get; set; }
            public bool Topmost { get; set; } = true;
        }

        private sealed class SidekickApiSettings
        {
            public bool Enabled { get; set; }
            public string BaseUrl { get; set; } = "http://127.0.0.1:8765";
            public long GuildId { get; set; }
            public string ClipATrigger { get; set; } = "clip-a";
            public string ClipBTrigger { get; set; } = "clip-b";
            public string ClipCTrigger { get; set; } = "clip-c";
        }
    }
}
