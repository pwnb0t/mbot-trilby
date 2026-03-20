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
using mbottrilby.Configuration;
using mbottrilby.Controls;
using mbottrilby.Input;
using mbottrilby.Search;
using mbottrilby.Services;
using mbottrilby.ViewModels;

namespace mbottrilby
{
    public partial class MainWindow : Window
    {
        private const int WmHotKey = 0x0312;
        private const int HotkeyId = 1;
        private const int VkBack = 0x08;
        private const int VkMinus = 0xBD;
        private const int VkOemMinus = 0xBD;
        private const int VkEquals = 0xBB;
        private const int VkOemPlus = 0xBB;
        private const int Vk0 = 0x30;
        private const int Vk7 = 0x37;
        private const int Vk9 = 0x39;
        private const int VkA = 0x41;
        private const int VkShift = 0x10;
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
        private readonly TrilbyAuthenticationService _trilbyAuthenticationService = new();
        private readonly List<string> _allClipTriggers = new();
        private readonly List<string> _allTagNames = new();
        private readonly ClipSearchState _clipSearchState = new(MaxVisibleSearchResults);
        private readonly UserSettingsStateStore _userSettingsStore;
        private readonly UserSettingsState _userSettings;
        private readonly IReadOnlyList<QuickPlaySlotViewModel> _quickPlaySlots;
        private readonly ClipDragSourceBehavior _clipDragSourceBehavior = new();
        private readonly CurrentIntroSlotViewModel _currentIntroSlot = new();
        private readonly TagWidgetViewModel _tagWidget = new();
        private ClipAssignmentDragData? _activeClipAssignmentDragData;
        private string _topClipStatsDays = "7";
        private bool _topClipStatsGuildWide;
        private bool _clipAssignmentDragActive;
        private int _quickPlayDragHoverSlot;
        private bool _currentIntroDragHover;
        private bool _tagDropZoneHover;
        private bool _recentStatsGuildWide = true;
        private bool _recentStatsIncludeRandom = true;
        private bool _hotkeyRegistered;
        private bool _exitRequested;
        private TrilbyApiClientService? _trilbyApiClient;
        private ClipPlaybackCoordinator? _clipPlaybackCoordinator;
        private OverlayDiagnostics? _diagnostics;
        private readonly OverlayController _overlayController;
        private readonly DispatcherTimer _recentClipTimeRefreshTimer;
        private TrayController? _trayController;
        private OverlayInputRouter? _overlayInputRouter;
        private SettingsWindow? _settingsWindow;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _viewModel;
            SearchPanel.SearchResultSelected += SearchPanel_SearchResultSelected;
            SearchPanel.ClipAssignmentDragStateChanged += SearchPanel_ClipAssignmentDragStateChanged;

            _settings = AppSettingsLoader.LoadFromBaseDirectory(AppContext.BaseDirectory);
            Topmost = _settings.Overlay.Topmost;

            _hideOverlayVirtualKey = ParseBindingVirtualKey(_settings.InputBindings.HideOverlayKey, Key.Escape);
            _clearSearchVirtualKey = ParseBindingVirtualKey(_settings.InputBindings.ClearSearchKey, Key.Tab);
            _playFirstPrimaryVirtualKey = ParseBindingVirtualKey(_settings.InputBindings.PlayFirstPrimaryKey, Key.Enter);
            _playFirstSecondaryVirtualKey = ParseBindingVirtualKey(_settings.InputBindings.PlayFirstSecondaryKey, Key.Space);

            _clipPlaybackCoordinator = new ClipPlaybackCoordinator(_trilbyApiClient);

            var logDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "mbot-trilby",
                "logs"
            );
            Directory.CreateDirectory(logDirectory);
            _logFilePath = Path.Combine(logDirectory, "overlay.log");
            var userStateDirectory = Path.GetDirectoryName(logDirectory) ?? logDirectory;
            _userSettingsStore = new UserSettingsStateStore(userStateDirectory);
            _userSettings = _userSettingsStore.Load();
            _quickPlaySlots = Enumerable.Range(1, 8)
                .Select(slotIndex => new QuickPlaySlotViewModel(slotIndex))
                .ToArray();
            _viewModel.QuickPlaySlots = _quickPlaySlots;
            _viewModel.CurrentIntroSlot = _currentIntroSlot;
            _viewModel.TagWidget = _tagWidget;
            if (!string.IsNullOrWhiteSpace(_userSettings.SelectedTagName))
            {
                _tagWidget.SetLoading(_userSettings.SelectedTagName);
            }
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
                openSettings: OpenSettingsWindow,
                exitApp: ExitFromTray,
                appBaseDirectory: AppContext.BaseDirectory
            );
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RenderSearchState();
            UpdateQuickPlaySlots();

            Log("m'bot Trilby loaded.");
            Log($"Hotkey: {DescribeHotkey(_settings.Hotkey)}");
            Log($"Selected environment: {GetSelectedEnvironmentName()}");
            Log(
                string.Join(
                    ", ",
                    _quickPlaySlots.Select(slot => $"{slot.SlotIndex}={DescribeQuickPlaySlot(slot.Trigger)}")
                )
            );
            _ = InitializeAuthenticatedStateAsync("startup");
            Log("Overlay starts hidden until authenticated hotkey use.");

            _overlayController.ApplyOverlayPanelLayout();
            UpdateTopStatsFilterButtonVisuals();
            UpdateRecentStatsFilterButtonVisuals();
            UpdateRecentClipTimeTexts();
            OpenSettingsWindow();
        }

        private async void QuickPlayButton_Click(object sender, RoutedEventArgs e)
        {
            var slot = TryGetQuickPlaySlot(sender);
            if (slot is null)
            {
                return;
            }

            await PlayQuickPlaySlotAsync($"Quick Play {slot.SlotIndex}", slot.Trigger);
        }

        private async void RefreshClipsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!await EnsureAuthenticatedApiClientAsync("manual refresh"))
            {
                return;
            }

            await LoadClipCatalogAsync("manual refresh");
            await LoadTagCatalogAsync("manual refresh");
            await LoadTopClipStatsAsync("manual refresh");
            await LoadRecentClipStatsAsync("manual refresh");
            await LoadCurrentIntroAsync("manual refresh");
        }

        private async void PlayRandomButton_Click(object sender, RoutedEventArgs e)
        {
            if (_clipPlaybackCoordinator is null)
            {
                return;
            }

            ResetSearchState();
            _overlayController.Hide("Overlay hidden after random clip play request.");

            var result = await _clipPlaybackCoordinator.PlayRandomAsync();
            foreach (var logLine in result.LogLines)
            {
                Log(logLine);
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

        private async void CurrentIntroButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_currentIntroSlot.IsAssigned || string.IsNullOrWhiteSpace(_currentIntroSlot.Trigger))
            {
                return;
            }

            await PlayClipAsync("Current Intro", _currentIntroSlot.Trigger);
        }

        private async void SearchPanel_SearchResultSelected(object? sender, ClipSearchResult searchResult)
        {
            if (searchResult.Kind == SearchResultKind.Clip)
            {
                await PlayClipAsync(searchResult.Value, searchResult.Value);
                return;
            }

            await SelectTagAsync(searchResult.Value, "search result");
        }

        private void SearchPanel_ClipAssignmentDragStateChanged(object? sender, ClipAssignmentDragChangedEventArgs e)
        {
            SetActiveClipAssignmentDragData(e.DragData);
        }

        private void ClipDragSourceButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _clipDragSourceBehavior.HandlePreviewMouseLeftButtonDown(sender, e, this);
        }

        private void ClipDragSourceButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _clipDragSourceBehavior.HandlePreviewMouseLeftButtonUp();
        }

        private void ClipDragSourceButton_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _clipDragSourceBehavior.HandlePreviewMouseMove(
                sender,
                e,
                this,
                button =>
                {
                    var trigger = button.Tag as string;
                    if (string.IsNullOrWhiteSpace(trigger))
                    {
                        return null;
                    }

                    var sourceTagName = button.DataContext is TagClipEntryViewModel tagClip
                        ? tagClip.TagName
                        : null;
                    return new ClipAssignmentDragData(trigger, sourceTagName);
                },
                SetActiveClipAssignmentDragData);
        }

        private void SetActiveClipAssignmentDragData(ClipAssignmentDragData? dragData)
        {
            _activeClipAssignmentDragData = dragData;
            _clipAssignmentDragActive = dragData is not null;
            if (!_clipAssignmentDragActive)
            {
                _quickPlayDragHoverSlot = 0;
                _currentIntroDragHover = false;
                _tagDropZoneHover = false;
            }

            UpdateQuickPlaySlots();
            UpdateCurrentIntroSlot();
            UpdateTagDropZone();
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

        private async void TagClipItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.Button button ||
                button.Tag is not string trigger ||
                string.IsNullOrWhiteSpace(trigger))
            {
                return;
            }

            await PlayClipAsync(trigger, trigger);
        }

        private void ClearSelectedTagButton_Click(object sender, RoutedEventArgs e)
        {
            _tagWidget.ClearSelection();
            _userSettings.SelectedTagName = null;
            SaveUserSettings();
        }

        private void TagDropZone_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            if (!_tagWidget.HasSelectedTag || !HasClipTriggerDragData(e))
            {
                e.Effects = System.Windows.DragDropEffects.None;
                return;
            }

            _tagDropZoneHover = true;
            e.Effects = System.Windows.DragDropEffects.Copy;
            UpdateTagDropZone();
        }

        private void TagDropZone_DragLeave(object sender, System.Windows.DragEventArgs e)
        {
            _tagDropZoneHover = false;
            UpdateTagDropZone();
        }

        private void TagDropZone_DragOver(object sender, System.Windows.DragEventArgs e)
        {
            e.Effects = _tagWidget.HasSelectedTag && HasClipTriggerDragData(e)
                ? System.Windows.DragDropEffects.Copy
                : System.Windows.DragDropEffects.None;
            e.Handled = true;
        }

        private async void TagDropZone_Drop(object sender, System.Windows.DragEventArgs e)
        {
            _tagDropZoneHover = false;
            var dragData = TryGetDroppedClipAssignment(e);
            var selectedTagName = _tagWidget.SelectedTagName;
            if (dragData is null || string.IsNullOrWhiteSpace(selectedTagName))
            {
                UpdateTagDropZone();
                return;
            }

            if (_trilbyApiClient is null)
            {
                UpdateTagDropZone();
                return;
            }

            try
            {
                if (IsSelectedTagRemovalDrag(dragData, selectedTagName))
                {
                    await _trilbyApiClient.RemoveClipFromTagAsync(selectedTagName, dragData.Trigger);
                    Log($"Removed clip '{dragData.Trigger}' from &{selectedTagName}");
                }
                else
                {
                    await _trilbyApiClient.AddClipToTagAsync(selectedTagName, dragData.Trigger);
                    Log($"Added clip '{dragData.Trigger}' to &{selectedTagName}");
                }

                await SelectTagAsync(selectedTagName, "tag drop refresh");
            }
            catch (Exception ex)
            {
                var action = IsSelectedTagRemovalDrag(dragData, selectedTagName) ? "remove" : "add";
                Log($"Failed to {action} clip '{dragData.Trigger}' {(action == "remove" ? "from" : "to")} &{selectedTagName}: {ex.Message}");
                UpdateTagDropZone();
            }
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

        private void QuickPlayButton_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            if (!HasClipTriggerDragData(e))
            {
                e.Effects = System.Windows.DragDropEffects.None;
                return;
            }

            _quickPlayDragHoverSlot = GetQuickPlaySlotIndex(sender);
            e.Effects = System.Windows.DragDropEffects.Copy;
            UpdateQuickPlaySlots();
        }

        private void QuickPlayButton_DragLeave(object sender, System.Windows.DragEventArgs e)
        {
            _quickPlayDragHoverSlot = 0;
            UpdateQuickPlaySlots();
        }

        private void QuickPlayButton_DragOver(object sender, System.Windows.DragEventArgs e)
        {
            e.Effects = HasClipTriggerDragData(e)
                ? System.Windows.DragDropEffects.Copy
                : System.Windows.DragDropEffects.None;
            e.Handled = true;
        }

        private void CurrentIntroButton_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            if (!HasClipTriggerDragData(e))
            {
                e.Effects = System.Windows.DragDropEffects.None;
                return;
            }

            _currentIntroDragHover = true;
            e.Effects = System.Windows.DragDropEffects.Copy;
            UpdateCurrentIntroSlot();
        }

        private void CurrentIntroButton_DragLeave(object sender, System.Windows.DragEventArgs e)
        {
            _currentIntroDragHover = false;
            UpdateCurrentIntroSlot();
        }

        private void CurrentIntroButton_DragOver(object sender, System.Windows.DragEventArgs e)
        {
            e.Effects = HasClipTriggerDragData(e)
                ? System.Windows.DragDropEffects.Copy
                : System.Windows.DragDropEffects.None;
            e.Handled = true;
        }

        private async void CurrentIntroButton_Drop(object sender, System.Windows.DragEventArgs e)
        {
            _currentIntroDragHover = false;
            var dragData = TryGetDroppedClipAssignment(e);
            if (dragData is null || string.IsNullOrWhiteSpace(dragData.Trigger))
            {
                UpdateCurrentIntroSlot();
                return;
            }

            if (_trilbyApiClient is null)
            {
                UpdateCurrentIntroSlot();
                return;
            }

            try
            {
                var currentIntro = await _trilbyApiClient.SetCurrentIntroAsync(dragData.Trigger);
                _currentIntroSlot.Trigger = currentIntro.Trigger;
                Log($"Assigned current intro -> {dragData.Trigger}");
            }
            catch (Exception ex)
            {
                Log($"Failed to assign current intro: {ex.Message}");
            }
            finally
            {
                UpdateCurrentIntroSlot();
            }
        }

        private void QuickPlayButton_Drop(object sender, System.Windows.DragEventArgs e)
        {
            HandleQuickPlayDrop(GetQuickPlaySlotIndex(sender), e);
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

            _trilbyApiClient?.Dispose();
            _trilbyApiClient = null;
            _clipPlaybackCoordinator = null;
            _diagnostics = null;

            base.OnClosed(e);
        }

        private async System.Threading.Tasks.Task InitializeHealthCheckAsync()
        {
            try
            {
                if (_trilbyApiClient is null)
                {
                    return;
                }

                var message = await _trilbyApiClient.GetHealthSummaryAsync();
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
                _clipSearchState.SetSource(_allClipTriggers, _allTagNames);
                RenderSearchState();
                UpdateClipCountText(_allClipTriggers.Count, "loaded");
            }
            else
            {
                _allClipTriggers.Clear();
                _clipSearchState.SetSource(Array.Empty<string>(), _allTagNames);
                RenderSearchState();
                UpdateClipCountText(0, _trilbyApiClient is null ? "API disabled" : "load failed");
            }
        }

        private async System.Threading.Tasks.Task LoadTagCatalogAsync(string reason)
        {
            if (_clipPlaybackCoordinator is null)
            {
                return;
            }

            var result = await _clipPlaybackCoordinator.LoadTagsAsync(reason);
            foreach (var logLine in result.LogLines)
            {
                Log(logLine);
            }

            if (result.Success)
            {
                _allTagNames.Clear();
                _allTagNames.AddRange(result.TagNames);
            }
            else
            {
                _allTagNames.Clear();
            }

            _clipSearchState.SetSource(_allClipTriggers, _allTagNames);
            RenderSearchState();

            if (_tagWidget.HasSelectedTag && !string.IsNullOrWhiteSpace(_tagWidget.SelectedTagName))
            {
                await SelectTagAsync(_tagWidget.SelectedTagName, $"{reason} tag refresh");
            }
        }

        private async System.Threading.Tasks.Task SelectTagAsync(string tagName, string reason)
        {
            if (_trilbyApiClient is null)
            {
                _tagWidget.SetFailed(tagName, "API disabled");
                return;
            }

            ResetSearchState();
            _tagWidget.SetLoading(tagName);

            try
            {
                Log($"Loading tag clips ({reason}) for &{tagName}...");
                var catalog = await _trilbyApiClient.ListTagClipsAsync(tagName);
                var clips = catalog.Triggers
                    .Select(trigger => new TagClipEntryViewModel(trigger, catalog.TagName))
                    .ToArray();
                _tagWidget.SetLoaded(catalog.TagName, clips);
                _userSettings.SelectedTagName = catalog.TagName;
                SaveUserSettings();
                Log($"Loaded {clips.Length} clips for &{catalog.TagName}.");
            }
            catch (Exception ex)
            {
                _tagWidget.SetFailed(tagName, $"Failed to load &{tagName}");
                Log($"Load tag clips failed for &{tagName}: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task<bool> PlayClipAsync(string clipName, string trigger)
        {
            if (_clipPlaybackCoordinator is null)
            {
                return false;
            }

            ResetSearchState();
            _overlayController.Hide("Overlay hidden after clip play request.");

            var result = await _clipPlaybackCoordinator.PlayClipAsync(clipName, trigger);
            foreach (var logLine in result.LogLines)
            {
                Log(logLine);
            }

            if (result.Success)
            {
                _ = LoadTopClipStatsAsync("after clip play");
                _ = LoadRecentClipStatsAsync("after clip play");
            }

            return result.Success;
        }

        private async System.Threading.Tasks.Task PlayQuickPlaySlotAsync(string slotName, string? trigger)
        {
            if (string.IsNullOrWhiteSpace(trigger))
            {
                Log($"{slotName} is unassigned.");
                return;
            }

            await PlayClipAsync(slotName, trigger);
        }

        private async System.Threading.Tasks.Task PlayQuickPlaySlotByIndexAsync(int slotIndex)
        {
            var slot = _quickPlaySlots.FirstOrDefault(item => item.SlotIndex == slotIndex);
            if (slot is null)
            {
                return;
            }

            await PlayQuickPlaySlotAsync($"Quick Play {slot.SlotIndex}", slot.Trigger);
        }

        private async System.Threading.Tasks.Task LoadTopClipStatsAsync(string reason)
        {
            UpdateTopStatsFilterButtonVisuals();

            if (_trilbyApiClient is null)
            {
                _viewModel.TopClipStats = Array.Empty<TopClipStatEntryViewModel>();
                _viewModel.TopStatsStatusText = "API disabled";
                return;
            }

            try
            {
                Log($"Loading top clip stats ({reason})...");
                var catalog = await _trilbyApiClient.GetTopClipStatsAsync(
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

            if (_trilbyApiClient is null)
            {
                _viewModel.RecentClipStats = Array.Empty<RecentClipEntryViewModel>();
                _viewModel.RecentStatsStatusText = "API disabled";
                return;
            }

            try
            {
                Log($"Loading recent clip stats ({reason})...");
                var catalog = await _trilbyApiClient.GetRecentClipStatsAsync(
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

        private async System.Threading.Tasks.Task LoadCurrentIntroAsync(string reason)
        {
            if (_trilbyApiClient is null)
            {
                _currentIntroSlot.Trigger = null;
                UpdateCurrentIntroSlot();
                return;
            }

            try
            {
                Log($"Loading current intro ({reason})...");
                var currentIntro = await _trilbyApiClient.GetCurrentIntroAsync();
                _currentIntroSlot.Trigger = currentIntro.Trigger;
            }
            catch (Exception ex)
            {
                Log($"Load current intro failed: {ex.Message}");
            }

            UpdateCurrentIntroSlot();
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

            _ = ShowOverlayIfAuthenticatedAsync(OverlayShowSource.Standard);
        }

        private void ShowOverlayFromTray()
        {
            OpenSettingsWindow();
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
            _viewModel.VisibleSearchResults = filteredResults.ToArray();
            _viewModel.NoResultsVisible = !string.IsNullOrEmpty(query) && filteredResults.Count == 0;
        }

        private async System.Threading.Tasks.Task PlayFirstFilteredResultAsync()
        {
            if (string.IsNullOrEmpty(_clipSearchState.Query))
            {
                return;
            }

            var first = _clipSearchState.FirstResultOrDefault();
            if (first is null)
            {
                return;
            }

            if (first.Kind == SearchResultKind.Clip)
            {
                await PlayClipAsync(first.Value, first.Value);
                return;
            }

            await SelectTagAsync(first.Value, "primary search action");
        }

        private bool HandleOverlayKeyDown(int virtualKey, bool isAltDown)
        {
            if (isAltDown)
            {
                var quickPlaySlotIndex = TryGetQuickPlaySlotIndexFromAltHotkey(virtualKey);
                if (quickPlaySlotIndex > 0)
                {
                    _ = PlayQuickPlaySlotByIndexAsync(quickPlaySlotIndex);
                    return true;
                }

                if (IsPlayRandomAltHotkey(virtualKey))
                {
                    PlayRandomButton_Click(this, new RoutedEventArgs());
                    return true;
                }

                if (IsStopAltHotkey(virtualKey))
                {
                    StopButton_Click(this, new RoutedEventArgs());
                    return true;
                }
            }

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

            var character = TryGetSearchCharacter(virtualKey);
            if (character is null)
            {
                return false;
            }

            _clipSearchState.AppendCharacter(character.Value);
            RenderSearchState();
            return true;
        }

        private static char? TryGetSearchCharacter(int virtualKey)
        {
            if (virtualKey == Vk7 && IsShiftPressed())
            {
                return '&';
            }

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

        private static bool IsShiftPressed()
        {
            return (GetKeyState(VkShift) & 0x8000) != 0;
        }

        private static int TryGetQuickPlaySlotIndexFromAltHotkey(int virtualKey)
        {
            if (virtualKey >= 0x31 && virtualKey <= 0x38)
            {
                return virtualKey - 0x30;
            }

            return 0;
        }

        private static bool IsPlayRandomAltHotkey(int virtualKey)
        {
            return virtualKey == VkMinus || virtualKey == VkOemMinus;
        }

        private static bool IsStopAltHotkey(int virtualKey)
        {
            return virtualKey == VkEquals || virtualKey == VkOemPlus;
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

        private void HandleQuickPlayDrop(int slotIndex, System.Windows.DragEventArgs e)
        {
            _quickPlayDragHoverSlot = 0;
            var dragData = TryGetDroppedClipAssignment(e);
            if (dragData is null || string.IsNullOrWhiteSpace(dragData.Trigger))
            {
                UpdateQuickPlaySlots();
                return;
            }

            SetQuickPlayTrigger(slotIndex, dragData.Trigger);
            SaveUserSettings();
            UpdateQuickPlaySlots();
            Log($"Assigned quick play slot {slotIndex} -> {dragData.Trigger}");
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

        private void UpdateQuickPlaySlots()
        {
            foreach (var slot in _quickPlaySlots)
            {
                slot.Trigger = _userSettings.GetTrigger(slot.SlotIndex);
                slot.IsDragHoverTarget = _clipAssignmentDragActive && _quickPlayDragHoverSlot == slot.SlotIndex;
                slot.IsDragAvailableTarget = _clipAssignmentDragActive && _quickPlayDragHoverSlot != slot.SlotIndex;
            }
        }

        private void UpdateCurrentIntroSlot()
        {
            _currentIntroSlot.IsDragHoverTarget = _clipAssignmentDragActive && _currentIntroDragHover;
            _currentIntroSlot.IsDragAvailableTarget = _clipAssignmentDragActive && !_currentIntroDragHover;
        }

        private void UpdateTagDropZone()
        {
            _tagWidget.IsRemoveDragOperation = _clipAssignmentDragActive &&
                _tagWidget.HasSelectedTag &&
                _activeClipAssignmentDragData is not null &&
                !string.IsNullOrWhiteSpace(_tagWidget.SelectedTagName) &&
                IsSelectedTagRemovalDrag(_activeClipAssignmentDragData, _tagWidget.SelectedTagName);
            _tagWidget.IsDragHoverTarget = _clipAssignmentDragActive && _tagDropZoneHover && _tagWidget.HasSelectedTag;
            _tagWidget.IsDragAvailableTarget = _clipAssignmentDragActive && !_tagDropZoneHover && _tagWidget.HasSelectedTag;
        }

        private void SaveUserSettings()
        {
            _userSettingsStore.Save(_userSettings);
        }

        private async System.Threading.Tasks.Task InitializeAuthenticatedStateAsync(string reason)
        {
            if (!await EnsureAuthenticatedApiClientAsync(reason))
            {
                UpdateClipCountText(0, "Not signed in");
                _viewModel.TopStatsStatusText = "Not signed in";
                _viewModel.RecentStatsStatusText = "Not signed in";
                return;
            }

            _ = InitializeHealthCheckAsync();
            _ = LoadClipCatalogAsync(reason);
            _ = LoadTagCatalogAsync(reason);
            _ = LoadTopClipStatsAsync(reason);
            _ = LoadRecentClipStatsAsync(reason);
            _ = LoadCurrentIntroAsync(reason);
        }

        private async System.Threading.Tasks.Task<bool> EnsureAuthenticatedApiClientAsync(string reason)
        {
            var environmentName = GetSelectedEnvironmentName();
            var environment = _settings.TrilbyEnvironments.GetByName(environmentName);
            var session = _userSettings.GetSession(environmentName);
            if (session is null || !session.IsAuthenticated)
            {
                _trilbyApiClient = null;
                _clipPlaybackCoordinator = new ClipPlaybackCoordinator(_trilbyApiClient);
                return false;
            }

            if (TrilbyAuthenticationService.IsExpired(session))
            {
                try
                {
                    Log($"Refreshing expired session for {environmentName} ({reason})...");
                    var refreshedSession = await _trilbyAuthenticationService.RefreshSessionAsync(
                        environment.BaseUrl,
                        session.RefreshToken ?? string.Empty);
                    _userSettings.SetSession(environmentName, refreshedSession);
                    session = refreshedSession;
                    SaveUserSettings();
                }
                catch (Exception ex)
                {
                    Log($"Session refresh failed for {environmentName}: {ex.Message}");
                    _userSettings.SetSession(environmentName, null);
                    SaveUserSettings();
                    _trilbyApiClient = null;
                    _clipPlaybackCoordinator = new ClipPlaybackCoordinator(_trilbyApiClient);
                    OpenSettingsWindow();
                    return false;
                }
            }

            _trilbyApiClient = new TrilbyApiClientService(
                environment.BaseUrl,
                session.AccessToken ?? string.Empty,
                session.GuildId);
            _clipPlaybackCoordinator = new ClipPlaybackCoordinator(_trilbyApiClient);
            return true;
        }

        private async System.Threading.Tasks.Task ShowOverlayIfAuthenticatedAsync(OverlayShowSource source)
        {
            if (!await EnsureAuthenticatedApiClientAsync("show overlay"))
            {
                OpenSettingsWindow();
                return;
            }

            ShowOverlay(source);
        }

        private void OpenSettingsWindow()
        {
            if (_settingsWindow is not null)
            {
                _settingsWindow.Activate();
                return;
            }

            _settingsWindow = new SettingsWindow(
                _settings.TrilbyEnvironments,
                getSelectedEnvironmentName: GetSelectedEnvironmentName,
                setSelectedEnvironmentName: SetSelectedEnvironmentName,
                getSession: environmentName => _userSettings.GetSession(environmentName),
                signInAsync: SignInToEnvironmentAsync,
                signOut: SignOutEnvironment,
                log: Log);
            _settingsWindow.Closed += (_, _) => _settingsWindow = null;
            _settingsWindow.Show();
        }

        private async System.Threading.Tasks.Task SignInToEnvironmentAsync(string environmentName)
        {
            var environment = _settings.TrilbyEnvironments.GetByName(environmentName);
            var session = await _trilbyAuthenticationService.SignInAsync(environment.BaseUrl);
            _userSettings.SetSession(environmentName, session);
            SaveUserSettings();
            Log($"Signed in to {environmentName} as {session.Username} for guild {session.GuildName} ({session.GuildId}).");
            await InitializeAuthenticatedStateAsync($"{environmentName} sign in");
            _settingsWindow?.RefreshView();
        }

        private void SignOutEnvironment(string environmentName)
        {
            _userSettings.SetSession(environmentName, null);
            SaveUserSettings();
            if (string.Equals(GetSelectedEnvironmentName(), environmentName, StringComparison.OrdinalIgnoreCase))
            {
                _trilbyApiClient = null;
                _clipPlaybackCoordinator = new ClipPlaybackCoordinator(_trilbyApiClient);
                _overlayController.Hide("Overlay hidden after sign out.");
                UpdateClipCountText(0, "Signed out");
                _viewModel.TopStatsStatusText = "Signed out";
                _viewModel.RecentStatsStatusText = "Signed out";
            }

            Log($"Signed out of {environmentName}.");
            _settingsWindow?.RefreshView();
        }

        private string GetSelectedEnvironmentName()
        {
            return _userSettings.SelectedEnvironmentName;
        }

        private void SetSelectedEnvironmentName(string environmentName)
        {
            _userSettings.SelectedEnvironmentName = environmentName;
            SaveUserSettings();
            Log($"Selected environment changed to {environmentName}.");
            _ = InitializeAuthenticatedStateAsync($"{environmentName} selected");
        }

        private void SetQuickPlayTrigger(int slotIndex, string trigger)
        {
            _userSettings.SetTrigger(slotIndex, trigger);
        }

        private static int GetQuickPlaySlotIndex(object sender)
        {
            if (sender is not System.Windows.Controls.Button button)
            {
                return 0;
            }

            return button.Name switch
            {
                _ when button.Tag is int slotIndex => slotIndex,
                _ when button.Tag is string slotIndexText && int.TryParse(slotIndexText, out var parsedSlotIndex) => parsedSlotIndex,
                _ => 0
            };
        }

        private QuickPlaySlotViewModel? TryGetQuickPlaySlot(object sender)
        {
            var slotIndex = GetQuickPlaySlotIndex(sender);
            if (slotIndex <= 0)
            {
                return null;
            }

            return _quickPlaySlots.FirstOrDefault(slot => slot.SlotIndex == slotIndex);
        }

        private static string DescribeQuickPlaySlot(string? trigger)
        {
            return string.IsNullOrWhiteSpace(trigger) ? "<empty>" : trigger;
        }

        private static bool HasClipTriggerDragData(System.Windows.DragEventArgs e)
        {
            return TryGetDroppedClipAssignment(e) is not null;
        }

        private static ClipAssignmentDragData? TryGetDroppedClipAssignment(System.Windows.DragEventArgs e)
        {
            return ClipAssignmentDragDrop.TryRead(e.Data);
        }

        private static bool IsSelectedTagRemovalDrag(ClipAssignmentDragData dragData, string selectedTagName)
        {
            return !string.IsNullOrWhiteSpace(dragData.SourceTagName) &&
                string.Equals(dragData.SourceTagName, selectedTagName, StringComparison.OrdinalIgnoreCase);
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

        [DllImport("user32.dll")]
        private static extern short GetKeyState(int virtualKey);

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
