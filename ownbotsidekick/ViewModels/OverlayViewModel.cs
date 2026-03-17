using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using ownbotsidekick.Search;

namespace ownbotsidekick.ViewModels
{
    internal sealed class OverlayViewModel : INotifyPropertyChanged
    {
        private bool _isOverlayVisible;
        private string _clipCountText = "Clips: 0";
        private string _searchQueryDisplay = "Start typing to search...";
        private bool _noResultsVisible;
        private IReadOnlyList<ClipSearchResult> _visibleSearchResults = new List<ClipSearchResult>();
        private string _topStatsTitle = "Top Clips";
        private string _topStatsStatusText = "Loading...";
        private IReadOnlyList<TopClipStatEntryViewModel> _topClipStats = new List<TopClipStatEntryViewModel>();
        private string _recentStatsTitle = "Recently Played";
        private string _recentStatsStatusText = "Loading...";
        private IReadOnlyList<RecentClipEntryViewModel> _recentClipStats = new List<RecentClipEntryViewModel>();
        private IReadOnlyList<QuickPlaySlotViewModel> _quickPlaySlots = new List<QuickPlaySlotViewModel>();
        private CurrentIntroSlotViewModel _currentIntroSlot = new();
        private TagWidgetViewModel _tagWidget = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsOverlayVisible
        {
            get => _isOverlayVisible;
            set
            {
                if (!SetField(ref _isOverlayVisible, value))
                {
                    return;
                }

                OnPropertyChanged(nameof(OverlayVisibility));
            }
        }

        public Visibility OverlayVisibility => IsOverlayVisible ? Visibility.Visible : Visibility.Collapsed;

        public string ClipCountText
        {
            get => _clipCountText;
            set => SetField(ref _clipCountText, value);
        }

        public string SearchQueryDisplay
        {
            get => _searchQueryDisplay;
            set => SetField(ref _searchQueryDisplay, value);
        }

        public bool NoResultsVisible
        {
            get => _noResultsVisible;
            set => SetField(ref _noResultsVisible, value);
        }

        public IReadOnlyList<ClipSearchResult> VisibleSearchResults
        {
            get => _visibleSearchResults;
            set => SetField(ref _visibleSearchResults, value);
        }

        public string TopStatsTitle
        {
            get => _topStatsTitle;
            set => SetField(ref _topStatsTitle, value);
        }

        public string TopStatsStatusText
        {
            get => _topStatsStatusText;
            set => SetField(ref _topStatsStatusText, value);
        }

        public IReadOnlyList<TopClipStatEntryViewModel> TopClipStats
        {
            get => _topClipStats;
            set => SetField(ref _topClipStats, value);
        }

        public string RecentStatsTitle
        {
            get => _recentStatsTitle;
            set => SetField(ref _recentStatsTitle, value);
        }

        public string RecentStatsStatusText
        {
            get => _recentStatsStatusText;
            set => SetField(ref _recentStatsStatusText, value);
        }

        public IReadOnlyList<RecentClipEntryViewModel> RecentClipStats
        {
            get => _recentClipStats;
            set => SetField(ref _recentClipStats, value);
        }

        public IReadOnlyList<QuickPlaySlotViewModel> QuickPlaySlots
        {
            get => _quickPlaySlots;
            set => SetField(ref _quickPlaySlots, value);
        }

        public CurrentIntroSlotViewModel CurrentIntroSlot
        {
            get => _currentIntroSlot;
            set => SetField(ref _currentIntroSlot, value);
        }

        public TagWidgetViewModel TagWidget
        {
            get => _tagWidget;
            set => SetField(ref _tagWidget, value);
        }

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private void OnPropertyChanged(string? propertyName)
        {
            if (propertyName is null)
            {
                return;
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
