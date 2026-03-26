using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace mbottrilby.ViewModels
{
    internal sealed class TagWidgetViewModel : INotifyPropertyChanged
    {
        private readonly string _widgetLabel;
        private readonly string _emptyStatusText;
        private string _titleText;
        private string _statusText;
        private IReadOnlyList<TagClipEntryViewModel> _clips = new List<TagClipEntryViewModel>();
        private string? _selectedTagName;
        private bool _isDragHoverTarget;
        private bool _isDragAvailableTarget;
        private bool _isRemoveDragOperation;
        private bool _isTagDragHoverTarget;
        private bool _isTagDragAvailableTarget;

        public TagWidgetViewModel(string widgetLabel = "Tag", string emptyStatusText = "Search for an existing &tag")
        {
            _widgetLabel = widgetLabel;
            _emptyStatusText = emptyStatusText;
            _titleText = $"{_widgetLabel}: no &tag selected";
            _statusText = _emptyStatusText;
        }

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
            set
            {
                if (!SetField(ref _isDragHoverTarget, value))
                {
                    return;
                }

                OnPropertyChanged(nameof(DropHintText));
            }
        }

        public bool IsDragAvailableTarget
        {
            get => _isDragAvailableTarget;
            set
            {
                if (!SetField(ref _isDragAvailableTarget, value))
                {
                    return;
                }

                OnPropertyChanged(nameof(DropHintText));
            }
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

        public bool IsTagDragHoverTarget
        {
            get => _isTagDragHoverTarget;
            set
            {
                if (!SetField(ref _isTagDragHoverTarget, value))
                {
                    return;
                }

                OnPropertyChanged(nameof(IsTagDropActive));
                OnPropertyChanged(nameof(TagDropHintText));
            }
        }

        public bool IsTagDragAvailableTarget
        {
            get => _isTagDragAvailableTarget;
            set
            {
                if (!SetField(ref _isTagDragAvailableTarget, value))
                {
                    return;
                }

                OnPropertyChanged(nameof(IsTagDropActive));
                OnPropertyChanged(nameof(TagDropHintText));
            }
        }

        public string DropHintText => HasSelectedTag
            ? IsDragHoverTarget || IsDragAvailableTarget
                ? IsRemoveDragOperation
                    ? $"Drop here to remove from &{SelectedTagName}"
                    : $"Drop here to add to &{SelectedTagName}"
                : $"Drag clips here to add to &{SelectedTagName}"
            : "Search for an existing &tag";

        public bool IsTagDropActive => IsTagDragHoverTarget || IsTagDragAvailableTarget;

        public string TagDropHintText => IsTagDropActive
            ? "Drop here to open tagged clips"
            : string.Empty;

        public void ClearSelection()
        {
            SelectedTagName = null;
            TitleText = $"{_widgetLabel}: no &tag selected";
            StatusText = _emptyStatusText;
            Clips = new List<TagClipEntryViewModel>();
            IsDragHoverTarget = false;
            IsDragAvailableTarget = false;
            IsRemoveDragOperation = false;
            IsTagDragHoverTarget = false;
            IsTagDragAvailableTarget = false;
        }

        public void SetLoading(string tagName)
        {
            SelectedTagName = tagName;
            TitleText = $"{_widgetLabel}: &{tagName}";
            StatusText = $"Loading clips for &{tagName}...";
            Clips = new List<TagClipEntryViewModel>();
            IsRemoveDragOperation = false;
            IsTagDragHoverTarget = false;
            IsTagDragAvailableTarget = false;
        }

        public void SetLoaded(string tagName, IReadOnlyList<TagClipEntryViewModel> clips)
        {
            SelectedTagName = tagName;
            TitleText = $"{_widgetLabel}: &{tagName}";
            StatusText = clips.Count == 0 ? $"No clips in &{tagName} yet." : string.Empty;
            Clips = clips;
            IsRemoveDragOperation = false;
            IsTagDragHoverTarget = false;
            IsTagDragAvailableTarget = false;
        }

        public void SetFailed(string tagName, string message)
        {
            SelectedTagName = tagName;
            TitleText = $"{_widgetLabel}: &{tagName}";
            StatusText = message;
            Clips = new List<TagClipEntryViewModel>();
            IsRemoveDragOperation = false;
            IsTagDragHoverTarget = false;
            IsTagDragAvailableTarget = false;
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
