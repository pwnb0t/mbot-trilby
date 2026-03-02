using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using ownbotsidekick.Configuration;
using ownbotsidekick.Controls;
using ownbotsidekick.Input;
using ownbotsidekick.Search;
using ownbotsidekick.Services;
using ownbotsidekick.ViewModels;

namespace ownbotsidekick
{
    public partial class MainWindow : Window
    {
        private const int WmHotKey = 0x0312;
        private const int HotkeyId = 1;
        private const int VkBack = 0x08;
        private const int Vk0 = 0x30;
        private const int Vk9 = 0x39;
        private const int VkA = 0x41;
        private const int VkZ = 0x5A;
        private const int VkNumpad0 = 0x60;
        private const int VkNumpad9 = 0x69;
        private const int MaxVisibleSearchResults = 15;

        private readonly string _logFilePath;
        private readonly AppSettings _settings;
        private readonly int _hideOverlayVirtualKey;
        private readonly int _clearSearchVirtualKey;
        private readonly int _playFirstPrimaryVirtualKey;
        private readonly int _playFirstSecondaryVirtualKey;
        private readonly OverlayViewModel _viewModel = new();
        private readonly List<string> _allClipTriggers = new();
        private readonly ClipSearchState _clipSearchState = new(MaxVisibleSearchResults);
        private bool _hotkeyRegistered;
        private bool _exitRequested;
        private SidekickApiClientService? _sidekickApiClient;
        private ClipPlaybackCoordinator? _clipPlaybackCoordinator;
        private OverlayDiagnostics? _diagnostics;
        private readonly OverlayController _overlayController;
        private TrayController? _trayController;
        private OverlayInputRouter? _overlayInputRouter;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _viewModel;
            SearchPanel.ClipSelected += SearchPanel_ClipSelected;

            _settings = AppSettingsLoader.LoadFromBaseDirectory(AppContext.BaseDirectory);
            Topmost = _settings.Overlay.Topmost;
            if (_settings.SidekickApi.Enabled)
            {
                _sidekickApiClient = new SidekickApiClientService(
                    _settings.SidekickApi.BaseUrl,
                    _settings.SidekickApi.GuildId
                );
            }

            _hideOverlayVirtualKey = ParseBindingVirtualKey(_settings.InputBindings.HideOverlayKey, Key.Escape);
            _clearSearchVirtualKey = ParseBindingVirtualKey(_settings.InputBindings.ClearSearchKey, Key.Tab);
            _playFirstPrimaryVirtualKey = ParseBindingVirtualKey(_settings.InputBindings.PlayFirstPrimaryKey, Key.Enter);
            _playFirstSecondaryVirtualKey = ParseBindingVirtualKey(_settings.InputBindings.PlayFirstSecondaryKey, Key.Space);

            _clipPlaybackCoordinator = new ClipPlaybackCoordinator(_sidekickApiClient);

            var logDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ownbotsidekick",
                "logs"
            );
            Directory.CreateDirectory(logDirectory);
            _logFilePath = Path.Combine(logDirectory, "overlay.log");
            _diagnostics = new OverlayDiagnostics(_logFilePath);
            _overlayController = new OverlayController(
                overlayPanelBorder: OverlayPanelBorder,
                diagnostics: _diagnostics,
                resetSearchState: ResetSearchState,
                setOverlayVisible: value => _viewModel.IsOverlayVisible = value,
                setTopmost: value => Topmost = value
            );
            _trayController = new TrayController(
                diagnostics: _diagnostics,
                showOverlay: ShowOverlayFromTray,
                hideOverlay: HideOverlayFromTray,
                toggleOverlay: ToggleOverlayVisibility,
                exitApp: ExitFromTray,
                appBaseDirectory: AppContext.BaseDirectory
            );
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RenderSearchState();

            Log("Overlay loaded.");
            Log($"Hotkey: {DescribeHotkey(_settings.Hotkey)}");
            Log(_settings.SidekickApi.Enabled
                ? "Sidekick API client enabled."
                : "Sidekick API client disabled in appsettings.");
            Log($"Quick play triggers: 1={_settings.SidekickApi.QuickPlay1Trigger}, 2={_settings.SidekickApi.QuickPlay2Trigger}, 3={_settings.SidekickApi.QuickPlay3Trigger}");
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
                _overlayController.Show(OverlayShowSource.Standard, _settings.Overlay.Topmost);
            }

            _overlayController.ApplyOverlayPanelLayout();
        }

        private async void QuickPlay1Button_Click(object sender, RoutedEventArgs e)
        {
            await PlayClipAsync("Quick Play 1", _settings.SidekickApi.QuickPlay1Trigger);
        }

        private async void QuickPlay2Button_Click(object sender, RoutedEventArgs e)
        {
            await PlayClipAsync("Quick Play 2", _settings.SidekickApi.QuickPlay2Trigger);
        }

        private async void QuickPlay3Button_Click(object sender, RoutedEventArgs e)
        {
            await PlayClipAsync("Quick Play 3", _settings.SidekickApi.QuickPlay3Trigger);
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
            _overlayController.Hide("Overlay hidden from close button.");
        }

        private void Log(string message)
        {
            _diagnostics?.Info("app", message);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var helper = new WindowInteropHelper(this);
            var source = HwndSource.FromHwnd(helper.Handle);
            source?.AddHook(WndProc);
            _overlayController.InitializeWindowHandle(helper.Handle);

            _trayController?.Initialize();
            RegisterOverlayHotkey(helper.Handle);
            _overlayInputRouter = new OverlayInputRouter(
                isOverlayVisible: () => _overlayController.IsVisible,
                handleOverlayVirtualKey: HandleOverlayKeyDown,
                isPointInsideOverlayPanel: _overlayController.IsPointInsideOverlayPanel,
                onOutsideClick: () => _overlayController.Hide("Overlay hidden (outside click)."),
                diagnostics: _diagnostics ?? new OverlayDiagnostics(_logFilePath),
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
            _overlayController.Hide("Overlay hidden to tray. Use tray icon -> Exit to close app.");
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

            _trayController?.Dispose();
            _trayController = null;

            _sidekickApiClient?.Dispose();
            _sidekickApiClient = null;
            _clipPlaybackCoordinator = null;
            _diagnostics = null;

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
                _clipSearchState.SetSource(_allClipTriggers);
                RenderSearchState();
                UpdateClipCountText(_allClipTriggers.Count, "loaded");
            }
            else
            {
                _allClipTriggers.Clear();
                _clipSearchState.SetSource(Array.Empty<string>());
                RenderSearchState();
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
                _overlayController.Hide("Overlay hidden after clip play.");
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
            if (_overlayController.IsVisible)
            {
                _overlayController.Hide("Overlay hidden.");
                return;
            }

            _overlayController.Show(OverlayShowSource.Standard, _settings.Overlay.Topmost);
        }

        private void ShowOverlayFromTray()
        {
            if (_overlayController.IsVisible)
            {
                return;
            }

            _overlayController.Show(OverlayShowSource.Tray, _settings.Overlay.Topmost);
        }

        private void HideOverlayFromTray()
        {
            if (!_overlayController.IsVisible)
            {
                return;
            }

            _overlayController.Hide("Overlay hidden from tray.");
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
            _viewModel.ClipCountText = $"Clips: {count} ({status})";
        }

        private void ResetSearchState()
        {
            _clipSearchState.ClearQuery();
            RenderSearchState();
        }

        private void RenderSearchState()
        {
            var query = _clipSearchState.Query;
            var filteredTriggers = _clipSearchState.FilteredTriggers;
            _viewModel.SearchQueryDisplay = string.IsNullOrEmpty(query)
                ? "Start typing to search..."
                : query;
            _viewModel.VisibleClips = filteredTriggers.ToArray();
            _viewModel.NoResultsVisible = !string.IsNullOrEmpty(query) && filteredTriggers.Count == 0;
        }

        private async System.Threading.Tasks.Task PlayFirstFilteredResultAsync()
        {
            if (string.IsNullOrEmpty(_clipSearchState.Query))
            {
                return;
            }

            var first = _clipSearchState.FirstResultOrDefault();
            if (string.IsNullOrEmpty(first))
            {
                return;
            }

            await PlayClipAsync(first, first);
        }

        private bool HandleOverlayKeyDown(int virtualKey)
        {
            if (virtualKey == _hideOverlayVirtualKey)
            {
                _overlayController.Hide("Overlay hidden.");
                return true;
            }

            if (virtualKey == _clearSearchVirtualKey)
            {
                ResetSearchState();
                return true;
            }

            if (virtualKey == VkBack)
            {
                if (!_clipSearchState.Backspace())
                {
                    return false;
                }

                RenderSearchState();
                return true;
            }

            if (virtualKey == _playFirstPrimaryVirtualKey || virtualKey == _playFirstSecondaryVirtualKey)
            {
                if (string.IsNullOrEmpty(_clipSearchState.Query))
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

            _clipSearchState.AppendCharacter(character.Value);
            RenderSearchState();
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

        private static int ParseBindingVirtualKey(string configuredKey, Key defaultKey)
        {
            if (Enum.TryParse<Key>(configuredKey, true, out var key) && key != Key.None)
            {
                return KeyInterop.VirtualKeyFromKey(key);
            }

            return KeyInterop.VirtualKeyFromKey(defaultKey);
        }

        private static string DescribeHotkey(HotkeySettings hotkey)
        {
            return $"{hotkey.Modifiers}+{hotkey.Key}";
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

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

    }
}
