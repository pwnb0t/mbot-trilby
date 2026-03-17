using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ownbotsidekick.ViewModels
{
    internal sealed class TagWidgetViewModel : INotifyPropertyChanged
    {
        private string _titleText = "Tag: no &tag selected";
        private string _statusText = "Search for an existing &tag";
        private IReadOnlyList<TagClipEntryViewModel> _clips = new List<TagClipEntryViewModel>();
        private string? _selectedTagName;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string TitleText
        {
            get => _titleText;
            private set => SetField(ref _titleText, value);
        }

        public string StatusText
        {
            get => _statusText;
            private set => SetField(ref _statusText, value);
        }

        public IReadOnlyList<TagClipEntryViewModel> Clips
        {
            get => _clips;
            private set => SetField(ref _clips, value);
        }

        public string? SelectedTagName
        {
            get => _selectedTagName;
            private set
            {
                if (!SetField(ref _selectedTagName, value))
                {
                    return;
                }

                OnPropertyChanged(nameof(HasSelectedTag));
            }
        }

        public bool HasSelectedTag => !string.IsNullOrWhiteSpace(SelectedTagName);

        public void ClearSelection()
        {
            SelectedTagName = null;
            TitleText = "Tag: no &tag selected";
            StatusText = "Search for an existing &tag";
            Clips = new List<TagClipEntryViewModel>();
        }

        public void SetLoading(string tagName)
        {
            SelectedTagName = tagName;
            TitleText = $"Tag: &{tagName}";
            StatusText = $"Loading clips for &{tagName}...";
            Clips = new List<TagClipEntryViewModel>();
        }

        public void SetLoaded(string tagName, IReadOnlyList<TagClipEntryViewModel> clips)
        {
            SelectedTagName = tagName;
            TitleText = $"Tag: &{tagName}";
            StatusText = clips.Count == 0 ? $"No clips in &{tagName} yet." : string.Empty;
            Clips = clips;
        }

        public void SetFailed(string tagName, string message)
        {
            SelectedTagName = tagName;
            TitleText = $"Tag: &{tagName}";
            StatusText = message;
            Clips = new List<TagClipEntryViewModel>();
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

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            if (propertyName is null)
            {
                return;
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
