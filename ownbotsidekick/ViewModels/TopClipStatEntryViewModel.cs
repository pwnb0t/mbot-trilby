namespace ownbotsidekick.ViewModels
{
    internal sealed class TopClipStatEntryViewModel
    {
        public TopClipStatEntryViewModel(string trigger, string displayText)
        {
            Trigger = trigger;
            DisplayText = displayText;
        }

        public string Trigger { get; }
        public string DisplayText { get; }
    }
}
