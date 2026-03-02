using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace ownbotsidekick.ViewModels
{
    internal sealed class OverlayViewModel : INotifyPropertyChanged
    {
        private bool _isOverlayVisible;
        private string _clipCountText = "Clips: 0";
        private string _searchQueryDisplay = "Start typing to search...";
        private bool _noResultsVisible;
        private IReadOnlyList<string> _visibleClips = new List<string>();

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

        public IReadOnlyList<string> VisibleClips
        {
            get => _visibleClips;
            set => SetField(ref _visibleClips, value);
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
