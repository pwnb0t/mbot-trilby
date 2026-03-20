namespace mbottrilby.ViewModels
{
    internal sealed class TagClipEntryViewModel
    {
        public TagClipEntryViewModel(string trigger, string tagName)
        {
            Trigger = trigger;
            TagName = tagName;
        }

        public string Trigger { get; }
        public string TagName { get; }
    }
}
