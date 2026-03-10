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
using System.Windows.Threading;
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
        private const int TopClipStatsLimit = 10;
        private const int RecentClipStatsLimit = 10;
        private static readonly TimeSpan RecentClipTimeRefreshInterval = TimeSpan.FromSeconds(15);

        private readonly string _logFilePath;
        private readonly AppSettings _settings;
        private readonly int _hideOverlayVirtualKey;
        private readonly int _clearSearchVirtualKey;
        private readonly int _playFirstPrimaryVirtualKey;
        private readonly int _playFirstSecondaryVirtualKey;
        private readonly OverlayViewModel _viewModel = new();
        private readonly List<string> _allClipTriggers = new();
        private readonly ClipSearchState _clipSearchState = new(MaxVisibleSearchResults);
        private string _topClipStatsDays = "7";
        private bool _topClipStatsGuildWide;
        private bool _recentStatsGuildWide;
        private bool _recentStatsIncludeRandom = true;
        private bool _hotkeyRegistered;
        private bool _exitRequested;
        private SidekickApiClientService? _sidekickApiClient;
        private ClipPlaybackCoordinator? _clipPlaybackCoordinator;
        private OverlayDiagnostics? _diagnostics;
        private readonly OverlayController _overlayController;
        private readonly DispatcherTimer _recentClipTimeRefreshTimer;
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
                    _settings.SidekickApi.ApiToken,
                    _settings.SidekickApi.GuildId,
                    _settings.SidekickApi.RequestingUserId
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
                setOverlayVisible: value => _viewModel.IsOverlayVisible = value,
                setTopmost: value => Topmost = value
            );
            _recentClipTimeRefreshTimer = new DispatcherTimer
            {
                Interval = RecentClipTimeRefreshInterval
            };
            _recentClipTimeRefreshTimer.Tick += RecentClipTimeRefreshTimer_Tick;
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

            Log("m'bot Trilby loaded.");
            Log($"Hotkey: {DescribeHotkey(_settings.Hotkey)}");
            Log(_settings.SidekickApi.Enabled
                ? "Sidekick API client enabled."
                : "Sidekick API client disabled in appsettings.");
            if (_settings.SidekickApi.Enabled && string.IsNullOrWhiteSpace(_settings.SidekickApi.ApiToken))
            {
                Log("Warning: SidekickApi.ApiToken is empty. API calls will fail with 401.");
            }
            Log($"Quick play triggers: 1={_settings.SidekickApi.QuickPlay1Trigger}, 2={_settings.SidekickApi.QuickPlay2Trigger}, 3={_settings.SidekickApi.QuickPlay3Trigger}");
            Log($"Sidekick requester user id: {_settings.SidekickApi.RequestingUserId}");
            if (_sidekickApiClient is not null)
            {
                _ = InitializeHealthCheckAsync();
                _ = LoadClipCatalogAsync("startup");
                _ = LoadTopClipStatsAsync("startup");
                _ = LoadRecentClipStatsAsync("startup");
            }
            else
            {
                UpdateClipCountText(0, "API disabled");
                _viewModel.TopStatsStatusText = "API disabled";
                _viewModel.RecentStatsStatusText = "API disabled";
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
            UpdateTopStatsFilterButtonVisuals();
            UpdateRecentStatsFilterButtonVisuals();
            UpdateRecentClipTimeTexts();
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
            await LoadTopClipStatsAsync("manual refresh");
            await LoadRecentClipStatsAsync("manual refresh");
        }

        private async void PlayRandomButton_Click(object sender, RoutedEventArgs e)
        {
            if (_clipPlaybackCoordinator is null)
            {
                return;
            }

            var result = await _clipPlaybackCoordinator.PlayRandomAsync();
            foreach (var logLine in result.LogLines)
            {
                Log(logLine);
            }

            if (result.ShouldHideOverlay)
            {
                ResetSearchState();
                _overlayController.Hide("Overlay hidden after random clip play.");
            }

            if (result.Success)
            {
                _ = LoadTopClipStatsAsync("after random play");
                _ = LoadRecentClipStatsAsync("after random play");
            }
        }

        private async void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (_clipPlaybackCoordinator is null)
            {
                return;
            }

            var result = await _clipPlaybackCoordinator.StopClipAsync();
            foreach (var logLine in result.LogLines)
            {
                Log(logLine);
            }
        }

        private async void SearchPanel_ClipSelected(object? sender, string trigger)
        {
            await PlayClipAsync(trigger, trigger);
        }

        private async void TopClipStatsItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.Button button ||
                button.Tag is not string trigger ||
                string.IsNullOrWhiteSpace(trigger))
            {
                return;
            }

            await PlayClipAsync(trigger, trigger);
        }

        private async void RecentClipStatsItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.Button button ||
                button.Tag is not string trigger ||
                string.IsNullOrWhiteSpace(trigger))
            {
                return;
            }

            await PlayClipAsync(trigger, trigger);
        }

        private async void TopStatsScopeMeButton_Click(object sender, RoutedEventArgs e)
        {
            _topClipStatsGuildWide = false;
            await LoadTopClipStatsAsync("scope me");
        }

        private async void TopStatsScopeGuildButton_Click(object sender, RoutedEventArgs e)
        {
            _topClipStatsGuildWide = true;
            await LoadTopClipStatsAsync("scope guild");
        }

        private async void TopStatsDays1Button_Click(object sender, RoutedEventArgs e)
        {
            _topClipStatsDays = "1";
            await LoadTopClipStatsAsync("window 1d");
        }

        private async void TopStatsDays7Button_Click(object sender, RoutedEventArgs e)
        {
            _topClipStatsDays = "7";
            await LoadTopClipStatsAsync("window 7d");
        }

        private async void TopStatsDays30Button_Click(object sender, RoutedEventArgs e)
        {
            _topClipStatsDays = "30";
            await LoadTopClipStatsAsync("window 30d");
        }

        private async void TopStatsDaysAllButton_Click(object sender, RoutedEventArgs e)
        {
            _topClipStatsDays = "all";
            await LoadTopClipStatsAsync("window all");
        }

        private async void RecentStatsScopeMeButton_Click(object sender, RoutedEventArgs e)
        {
            _recentStatsGuildWide = false;
            await LoadRecentClipStatsAsync("recent scope me");
        }

        private async void RecentStatsScopeEveryoneButton_Click(object sender, RoutedEventArgs e)
        {
            _recentStatsGuildWide = true;
            await LoadRecentClipStatsAsync("recent scope everyone");
        }

        private async void RecentStatsIncludeRandomButton_Click(object sender, RoutedEventArgs e)
        {
            _recentStatsIncludeRandom = !_recentStatsIncludeRandom;
            await LoadRecentClipStatsAsync("recent include random toggled");
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
            _recentClipTimeRefreshTimer.Stop();
            _recentClipTimeRefreshTimer.Tick -= RecentClipTimeRefreshTimer_Tick;

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
                ResetSearchState();
                _overlayController.Hide("Overlay hidden after clip play.");
            }

            if (result.Success)
            {
                _ = LoadTopClipStatsAsync("after clip play");
                _ = LoadRecentClipStatsAsync("after clip play");
            }

            return result.Success;
        }

        private async System.Threading.Tasks.Task LoadTopClipStatsAsync(string reason)
        {
            UpdateTopStatsFilterButtonVisuals();

            if (_sidekickApiClient is null)
            {
                _viewModel.TopClipStats = Array.Empty<TopClipStatEntryViewModel>();
                _viewModel.TopStatsStatusText = "API disabled";
                return;
            }

            try
            {
                Log($"Loading top clip stats ({reason})...");
                var catalog = await _sidekickApiClient.GetTopClipStatsAsync(
                    days: _topClipStatsDays,
                    limit: TopClipStatsLimit,
                    includeRandom: false,
                    guildWide: _topClipStatsGuildWide
                );

                var rows = catalog.Rows
                    .Select(row => new TopClipStatEntryViewModel(
                        row.Trigger,
                        row.PlayCount == 1 ? "1 play" : $"{row.PlayCount} plays"
                    ))
                    .ToArray();

                _viewModel.TopClipStats = rows;
                _viewModel.TopStatsStatusText = rows.Length == 0
                    ? "No plays in this window yet."
                    : string.Empty;
            }
            catch (Exception ex)
            {
                _viewModel.TopClipStats = Array.Empty<TopClipStatEntryViewModel>();
                _viewModel.TopStatsStatusText = "Failed to load top clips";
                Log($"Load top clip stats failed: {ex.Message}");
            }
        }

        private void UpdateTopStatsFilterButtonVisuals()
        {
            var defaultStyle = (Style)FindResource("StatsFilterButtonStyle");
            var selectedStyle = (Style)FindResource("StatsFilterButtonSelectedStyle");

            TopStatsScopeMeButton.Style = _topClipStatsGuildWide ? defaultStyle : selectedStyle;
            TopStatsScopeServerButton.Style = _topClipStatsGuildWide ? selectedStyle : defaultStyle;

            TopStatsDays1Button.Style = _topClipStatsDays == "1" ? selectedStyle : defaultStyle;
            TopStatsDays7Button.Style = _topClipStatsDays == "7" ? selectedStyle : defaultStyle;
            TopStatsDays30Button.Style = _topClipStatsDays == "30" ? selectedStyle : defaultStyle;
            TopStatsDaysAllButton.Style = _topClipStatsDays == "all" ? selectedStyle : defaultStyle;
        }

        private async System.Threading.Tasks.Task LoadRecentClipStatsAsync(string reason)
        {
            UpdateRecentStatsFilterButtonVisuals();

            if (_sidekickApiClient is null)
            {
                _viewModel.RecentClipStats = Array.Empty<RecentClipEntryViewModel>();
                _viewModel.RecentStatsStatusText = "API disabled";
                return;
            }

            try
            {
                Log($"Loading recent clip stats ({reason})...");
                var catalog = await _sidekickApiClient.GetRecentClipStatsAsync(
                    limit: RecentClipStatsLimit,
                    includeRandom: _recentStatsIncludeRandom,
                    guildWide: _recentStatsGuildWide
                );

                var rows = catalog.Rows
                    .Select(row => new RecentClipEntryViewModel(
                        row.Trigger,
                        row.Trigger,
                        row.PlayedAtUtc,
                        FormatTimeAgo(row.PlayedAtUtc),
                        row.Mode == "random"
                    ))
                    .ToArray();

                _viewModel.RecentClipStats = rows;
                _viewModel.RecentStatsStatusText = rows.Length == 0 ? "No recent plays yet." : string.Empty;
                UpdateRecentClipTimeTexts();
            }
            catch (Exception ex)
            {
                _viewModel.RecentClipStats = Array.Empty<RecentClipEntryViewModel>();
                _viewModel.RecentStatsStatusText = "Failed to load recents";
                Log($"Load recent clip stats failed: {ex.Message}");
            }
        }

        private void UpdateRecentStatsFilterButtonVisuals()
        {
            var defaultStyle = (Style)FindResource("StatsFilterButtonStyle");
            var selectedStyle = (Style)FindResource("StatsFilterButtonSelectedStyle");

            RecentStatsScopeMeButton.Style = _recentStatsGuildWide ? defaultStyle : selectedStyle;
            RecentStatsScopeEveryoneButton.Style = _recentStatsGuildWide ? selectedStyle : defaultStyle;
            RecentStatsIncludeRandomButton.Style = _recentStatsIncludeRandom ? selectedStyle : defaultStyle;
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
                HideOverlayWithConditionalSearchReset("Overlay hidden.");
                return;
            }

            ShowOverlay(OverlayShowSource.Standard);
        }

        private void ShowOverlayFromTray()
        {
            if (_overlayController.IsVisible)
            {
                return;
            }

            ShowOverlay(OverlayShowSource.Tray);
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
            var filteredResults = _clipSearchState.FilteredResults;
            _viewModel.SearchQueryDisplay = string.IsNullOrEmpty(query)
                ? "Start typing to search..."
                : query;
            _viewModel.VisibleClips = filteredResults.ToArray();
            _viewModel.NoResultsVisible = !string.IsNullOrEmpty(query) && filteredResults.Count == 0;
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
                HideOverlayWithConditionalSearchReset("Overlay hidden.");
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

        private static string FormatTimeAgo(string playedAtUtc)
        {
            if (!DateTimeOffset.TryParse(playedAtUtc, out var playedAt))
            {
                return string.Empty;
            }

            var delta = DateTimeOffset.UtcNow - playedAt.ToUniversalTime();
            if (delta < TimeSpan.Zero)
            {
                delta = TimeSpan.Zero;
            }

            if (delta.TotalSeconds < 15)
            {
                return "Just now";
            }

            if (delta.TotalMinutes < 1)
            {
                return "<1m ago";
            }

            if (delta.TotalHours < 1)
            {
                return $"{Math.Max(1, (int)delta.TotalMinutes)}m ago";
            }

            if (delta.TotalDays < 1)
            {
                return $"{Math.Max(1, (int)delta.TotalHours)}h ago";
            }

            if (delta.TotalDays < 30)
            {
                return $"{Math.Max(1, (int)delta.TotalDays)}d ago";
            }

            return $"{Math.Max(1, (int)(delta.TotalDays / 30))}mo ago";
        }

        private void HideOverlayWithConditionalSearchReset(string logMessage)
        {
            var hasQuery = !string.IsNullOrEmpty(_clipSearchState.Query);
            var hasResults = _clipSearchState.FilteredResults.Count > 0;
            if (!hasQuery || !hasResults)
            {
                ResetSearchState();
            }

            _recentClipTimeRefreshTimer.Stop();
            _overlayController.Hide(logMessage);
        }

        private void ShowOverlay(OverlayShowSource source)
        {
            UpdateRecentClipTimeTexts();
            _overlayController.Show(source, _settings.Overlay.Topmost);
            _recentClipTimeRefreshTimer.Start();
        }

        private void RecentClipTimeRefreshTimer_Tick(object? sender, EventArgs e)
        {
            if (!_overlayController.IsVisible)
            {
                _recentClipTimeRefreshTimer.Stop();
                return;
            }

            UpdateRecentClipTimeTexts();
        }

        private void UpdateRecentClipTimeTexts()
        {
            foreach (var row in _viewModel.RecentClipStats)
            {
                row.PlayedAgoText = FormatTimeAgo(row.PlayedAtUtc);
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
