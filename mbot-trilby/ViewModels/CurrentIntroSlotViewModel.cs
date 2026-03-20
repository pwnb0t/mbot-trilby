using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace mbottrilby.ViewModels
{
    internal sealed class CurrentIntroSlotViewModel : INotifyPropertyChanged
    {
        private string? _trigger;
        private bool _isDragHoverTarget;
        private bool _isDragAvailableTarget;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string? Trigger
        {
            get => _trigger;
            set
            {
                if (!SetField(ref _trigger, NormalizeTrigger(value)))
                {
                    return;
                }

                OnPropertyChanged(nameof(DisplayText));
                OnPropertyChanged(nameof(IsAssigned));
            }
        }

        public string DisplayText => IsAssigned ? _trigger! : "Drop a clip here";

        public bool IsAssigned => !string.IsNullOrWhiteSpace(_trigger);

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

        private static string? NormalizeTrigger(string? trigger)
        {
            return string.IsNullOrWhiteSpace(trigger) ? null : trigger.Trim();
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
}
