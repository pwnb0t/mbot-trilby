namespace ownbotsidekick.ViewModels
{
    internal sealed class RecentClipEntryViewModel
    {
        public RecentClipEntryViewModel(string trigger, string triggerDisplay, string playedAgoText, bool isRandom)
        {
            Trigger = trigger;
            TriggerDisplay = triggerDisplay;
            PlayedAgoText = playedAgoText;
            IsRandom = isRandom;
        }

        public string Trigger { get; }
        public string TriggerDisplay { get; }
        public string PlayedAgoText { get; }
        public bool IsRandom { get; }
    }
}
