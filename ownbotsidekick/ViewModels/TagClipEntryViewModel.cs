namespace ownbotsidekick.ViewModels
{
    internal sealed class TagClipEntryViewModel
    {
        public TagClipEntryViewModel(string trigger)
        {
            Trigger = trigger;
        }

        public string Trigger { get; }
    }
}
