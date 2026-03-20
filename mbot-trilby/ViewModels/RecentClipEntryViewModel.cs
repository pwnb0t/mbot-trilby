using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace mbottrilby.ViewModels
{
    internal sealed class RecentClipEntryViewModel : INotifyPropertyChanged
    {
        private string _playedAgoText;

        public RecentClipEntryViewModel(
            string trigger,
            string triggerDisplay,
            string playedAtUtc,
            string playedAgoText,
            bool isRandom
        )
        {
            Trigger = trigger;
            TriggerDisplay = triggerDisplay;
            PlayedAtUtc = playedAtUtc;
            _playedAgoText = playedAgoText;
            IsRandom = isRandom;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Trigger { get; }
        public string TriggerDisplay { get; }
        public string PlayedAtUtc { get; }
        public string PlayedAgoText
        {
            get => _playedAgoText;
            set
            {
                if (_playedAgoText == value)
                {
                    return;
                }

                _playedAgoText = value;
                OnPropertyChanged();
            }
        }

        public bool IsRandom { get; }

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
