namespace ownbotsidekick.ViewModels
{
    internal sealed class TopClipStatEntryViewModel
    {
        public TopClipStatEntryViewModel(string trigger, string playCountText)
        {
            Trigger = trigger;
            PlayCountText = playCountText;
        }

        public string Trigger { get; }
        public string PlayCountText { get; }
    }
}
