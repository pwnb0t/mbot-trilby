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
        private bool _isDragHoverTarget;
        private bool _isDragAvailableTarget;
        private bool _isRemoveDragOperation;

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
                OnPropertyChanged(nameof(DropHintText));
            }
        }

        public bool HasSelectedTag => !string.IsNullOrWhiteSpace(SelectedTagName);

        public bool IsDragHoverTarget
        {
            get => _isDragHoverTarget;
            set => SetField(ref _isDragHoverTarget, value);
        }

        public bool IsDragAvailableTarget
        {
            get => _isDragAvailableTarget;
            set => SetField(ref _isDragAvailableTarget, value);
        }

        public bool IsRemoveDragOperation
        {
            get => _isRemoveDragOperation;
            set
            {
                if (!SetField(ref _isRemoveDragOperation, value))
                {
                    return;
                }

                OnPropertyChanged(nameof(DropHintText));
            }
        }

        public string DropHintText => HasSelectedTag
            ? IsRemoveDragOperation
                ? $"Drag clips here to remove from &{SelectedTagName}"
                : $"Drag clips here to add to &{SelectedTagName}"
            : "Search for an existing &tag";

        public void ClearSelection()
        {
            SelectedTagName = null;
            TitleText = "Tag: no &tag selected";
            StatusText = "Search for an existing &tag";
            Clips = new List<TagClipEntryViewModel>();
            IsDragHoverTarget = false;
            IsDragAvailableTarget = false;
            IsRemoveDragOperation = false;
        }

        public void SetLoading(string tagName)
        {
            SelectedTagName = tagName;
            TitleText = $"Tag: &{tagName}";
            StatusText = $"Loading clips for &{tagName}...";
            Clips = new List<TagClipEntryViewModel>();
            IsRemoveDragOperation = false;
        }

        public void SetLoaded(string tagName, IReadOnlyList<TagClipEntryViewModel> clips)
        {
            SelectedTagName = tagName;
            TitleText = $"Tag: &{tagName}";
            StatusText = clips.Count == 0 ? $"No clips in &{tagName} yet." : string.Empty;
            Clips = clips;
            IsRemoveDragOperation = false;
        }

        public void SetFailed(string tagName, string message)
        {
            SelectedTagName = tagName;
            TitleText = $"Tag: &{tagName}";
            StatusText = message;
            Clips = new List<TagClipEntryViewModel>();
            IsRemoveDragOperation = false;
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
