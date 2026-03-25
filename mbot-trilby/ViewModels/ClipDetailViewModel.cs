using System.ComponentModel;
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
        private bool _hasClip;

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

        public bool HasClip
        {
            get => _hasClip;
            private set => SetField(ref _hasClip, value);
        }

        public void ShowPlaceholder()
        {
            TitleText = "Clip Details";
            StatusText = "Hover a clip to see details";
            TriggerText = string.Empty;
            SourceUrlText = string.Empty;
            StartOffsetText = string.Empty;
            ClipLengthText = string.Empty;
            AddedByText = string.Empty;
            HasClip = false;
        }

        public void ShowClip(string trigger, string? sourceUrl, string? startOffsetText, string? clipLengthText, string? addedBy)
        {
            TitleText = "Clip Details";
            StatusText = string.Empty;
            TriggerText = trigger;
            SourceUrlText = TrimSourceUrl(sourceUrl);
            StartOffsetText = startOffsetText ?? string.Empty;
            ClipLengthText = clipLengthText ?? string.Empty;
            AddedByText = string.IsNullOrWhiteSpace(addedBy) ? string.Empty : addedBy.Trim();
            HasClip = true;
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

            return trimmed[..(SourceUrlMaxLength - 1)] + "…";
        }

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value))
            {
                return false;
            }

            field = value;
            if (propertyName is not null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            return true;
        }
    }
}
