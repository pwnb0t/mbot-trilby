using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
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

        private readonly string _logFilePath;
        private readonly AppSettings _settings;
        private bool _hotkeyRegistered;
        private bool _exitRequested;
        private Forms.NotifyIcon? _trayIcon;
        private Icon? _customTrayIcon;
        private SidekickApiClientService? _sidekickApiClient;

        public MainWindow()
        {
            InitializeComponent();

            _settings = LoadSettings();
            Topmost = _settings.Overlay.Topmost;
            if (_settings.SidekickApi.Enabled)
            {
                _sidekickApiClient = new SidekickApiClientService(
                    _settings.SidekickApi.BaseUrl,
                    _settings.SidekickApi.GuildId
                );
            }

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
            Log("Overlay loaded.");
            Log($"Hotkey: {DescribeHotkey(_settings.Hotkey)}");
            Log(_settings.SidekickApi.Enabled
                ? "Sidekick API client enabled."
                : "Sidekick API client disabled in appsettings.");
            Log($"Clip triggers: A={_settings.SidekickApi.ClipATrigger}, B={_settings.SidekickApi.ClipBTrigger}, C={_settings.SidekickApi.ClipCTrigger}");
            if (_sidekickApiClient is not null)
            {
                _ = InitializeHealthCheckAsync();
            }

            if (_settings.Overlay.StartHidden)
            {
                Hide();
                Log("Overlay starts hidden per appsettings.");
            }
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

            LogTextBox.AppendText(timestamped + Environment.NewLine);
            LogTextBox.ScrollToEnd();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var helper = new WindowInteropHelper(this);
            var source = HwndSource.FromHwnd(helper.Handle);
            source?.AddHook(WndProc);
            EnableNoActivateMode(helper.Handle);

            InitializeTrayIcon();
            RegisterOverlayHotkey(helper.Handle);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (_exitRequested)
            {
                base.OnClosing(e);
                return;
            }

            e.Cancel = true;
            Hide();
            Log("Overlay hidden to tray. Use tray icon -> Exit to close app.");
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_hotkeyRegistered)
            {
                var helper = new WindowInteropHelper(this);
                UnregisterHotKey(helper.Handle, HotkeyId);
                _hotkeyRegistered = false;
            }

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

        private async System.Threading.Tasks.Task PlayClipAsync(string clipName, string trigger)
        {
            if (_sidekickApiClient is null)
            {
                Log($"{clipName} clicked, but Sidekick API is disabled.");
                return;
            }

            if (string.IsNullOrWhiteSpace(trigger))
            {
                Log($"{clipName} clicked, but trigger is empty.");
                return;
            }

            Log($"{clipName} clicked -> trigger '{trigger}'");

            try
            {
                var message = await _sidekickApiClient.PlayTriggerAsync(trigger);
                Log(message);
            }
            catch (Exception ex)
            {
                Log($"Play trigger failed: {ex.Message}");
            }
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
            if (Visibility == Visibility.Visible)
            {
                Hide();
                Log("Overlay hidden.");
                return;
            }

            Show();
            Topmost = _settings.Overlay.Topmost;
            Log("Overlay shown.");
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
            if (Visibility == Visibility.Visible)
            {
                return;
            }

            Show();
            Topmost = _settings.Overlay.Topmost;
            Log("Overlay shown from tray.");
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
            if (Visibility != Visibility.Visible)
            {
                return;
            }

            Hide();
            Log("Overlay hidden from tray.");
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
