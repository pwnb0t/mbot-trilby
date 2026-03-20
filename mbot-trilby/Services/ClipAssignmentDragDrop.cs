namespace mbottrilby.Services
{
    internal static class ClipAssignmentDragDrop
    {
        public const string ClipTriggerFormat = "mbot-trilby/clip-trigger";
        public const string SourceTagNameFormat = "mbot-trilby/source-tag-name";

        public static System.Windows.DataObject CreateDataObject(string clipTrigger, string? sourceTagName = null)
        {
            var dataObject = new System.Windows.DataObject(ClipTriggerFormat, clipTrigger);
            if (!string.IsNullOrWhiteSpace(sourceTagName))
            {
                dataObject.SetData(SourceTagNameFormat, sourceTagName);
            }

            return dataObject;
        }

        public static ClipAssignmentDragData? TryRead(System.Windows.IDataObject dataObject)
        {
            if (!dataObject.GetDataPresent(ClipTriggerFormat))
            {
                return null;
            }

            var trigger = dataObject.GetData(ClipTriggerFormat) as string;
            if (string.IsNullOrWhiteSpace(trigger))
            {
                return null;
            }

            var sourceTagName = dataObject.GetDataPresent(SourceTagNameFormat)
                ? dataObject.GetData(SourceTagNameFormat) as string
                : null;

            return new ClipAssignmentDragData(trigger.Trim(), string.IsNullOrWhiteSpace(sourceTagName) ? null : sourceTagName.Trim());
        }
    }

    internal sealed class ClipAssignmentDragData
    {
        public ClipAssignmentDragData(string trigger, string? sourceTagName)
        {
            Trigger = trigger;
            SourceTagName = sourceTagName;
        }

        public string Trigger { get; }
        public string? SourceTagName { get; }
    }

    internal sealed class ClipAssignmentDragChangedEventArgs : EventArgs
    {
        public ClipAssignmentDragChangedEventArgs(ClipAssignmentDragData? dragData)
        {
            DragData = dragData;
        }

        public ClipAssignmentDragData? DragData { get; }
    }
}
