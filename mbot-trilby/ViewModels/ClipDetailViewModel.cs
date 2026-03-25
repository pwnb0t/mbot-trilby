using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace mbottrilby.ViewModels
{
    internal sealed class ClipDetailViewModel : INotifyPropertyChanged
    {
        private const int SourceUrlMaxLength = 100;

        private string _titleText = "Clip Details";
        private string _statusText = "Hover a clip to see details";
        private string _triggerText = string.Empty;
        private string _sourceUrlText = string.Empty;
        private string _startOffsetText = string.Empty;
        private string _clipLengthText = string.Empty;
        private string _addedByText = string.Empty;
        private string _tagsText = string.Empty;
        private string _tagClipListText = string.Empty;
        private DetailContentKind _contentKind = DetailContentKind.None;

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

        public string TriggerText
        {
            get => _triggerText;
            private set => SetField(ref _triggerText, value);
        }

        public string SourceUrlText
        {
            get => _sourceUrlText;
            private set => SetField(ref _sourceUrlText, value);
        }

        public string StartOffsetText
        {
            get => _startOffsetText;
            private set => SetField(ref _startOffsetText, value);
        }

        public string ClipLengthText
        {
            get => _clipLengthText;
            private set => SetField(ref _clipLengthText, value);
        }

        public string AddedByText
        {
            get => _addedByText;
            private set => SetField(ref _addedByText, value);
        }

        public string TagsText
        {
            get => _tagsText;
            private set => SetField(ref _tagsText, value);
        }

        public string TagClipListText
        {
            get => _tagClipListText;
            private set => SetField(ref _tagClipListText, value);
        }

        public bool HasClip => _contentKind == DetailContentKind.Clip;

        public bool HasTag => _contentKind == DetailContentKind.Tag;

        public void ShowPlaceholder()
        {
            TitleText = "Clip Details";
            StatusText = "Hover a clip to see details";
            TriggerText = string.Empty;
            SourceUrlText = string.Empty;
            StartOffsetText = string.Empty;
            ClipLengthText = string.Empty;
            AddedByText = string.Empty;
            TagsText = string.Empty;
            TagClipListText = string.Empty;
            SetContentKind(DetailContentKind.None);
        }

        public void ShowClip(
            string trigger,
            string? sourceUrl,
            string? startOffsetText,
            string? clipLengthText,
            string? addedBy,
            IReadOnlyList<string> tagNames)
        {
            TitleText = "Clip Details";
            StatusText = string.Empty;
            TriggerText = trigger;
            SourceUrlText = TrimSourceUrl(sourceUrl);
            StartOffsetText = startOffsetText ?? string.Empty;
            ClipLengthText = clipLengthText ?? string.Empty;
            AddedByText = string.IsNullOrWhiteSpace(addedBy) ? string.Empty : addedBy.Trim();
            TagsText = tagNames.Count == 0 ? "(none)" : string.Join(", ", tagNames.Select(tagName => $"&{tagName}"));
            TagClipListText = string.Empty;
            SetContentKind(DetailContentKind.Clip);
        }

        public void ShowTag(string tagName, IReadOnlyList<string> clipTriggers)
        {
            TitleText = "Tag Details";
            StatusText = string.Empty;
            TriggerText = $"&{tagName}";
            SourceUrlText = string.Empty;
            StartOffsetText = string.Empty;
            ClipLengthText = string.Empty;
            AddedByText = string.Empty;
            TagsText = string.Empty;
            TagClipListText = clipTriggers.Count == 0
                ? "(no clips)"
                : string.Join(", ", clipTriggers);
            SetContentKind(DetailContentKind.Tag);
        }

        private static string TrimSourceUrl(string? sourceUrl)
        {
            if (string.IsNullOrWhiteSpace(sourceUrl))
            {
                return string.Empty;
            }

            var trimmed = sourceUrl.Trim();
            if (trimmed.Length <= SourceUrlMaxLength)
            {
                return trimmed;
            }

            return trimmed[..(SourceUrlMaxLength - 3)] + "...";
        }

        private void SetContentKind(DetailContentKind contentKind)
        {
            if (_contentKind == contentKind)
            {
                return;
            }

            _contentKind = contentKind;
            OnPropertyChanged(nameof(HasClip));
            OnPropertyChanged(nameof(HasTag));
        }

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value))
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

    internal enum DetailContentKind
    {
        None,
        Clip,
        Tag,
    }
}
