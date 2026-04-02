namespace mbottrilby.Services
{
    internal enum OverlayDragDataKind
    {
        Clip,
        Tag
    }

    internal static class ClipAssignmentDragDrop
    {
        public const string DragKindFormat = "mbot-trilby/drag-kind";
        public const string ClipTriggerFormat = "mbot-trilby/clip-trigger";
        public const string TagNameFormat = "mbot-trilby/tag-name";
        public const string SourceTagNameFormat = "mbot-trilby/source-tag-name";

        public static System.Windows.DataObject CreateDataObject(
            OverlayDragDataKind kind,
            string value,
            string? sourceTagName = null)
        {
            System.Windows.DataObject dataObject = new System.Windows.DataObject(DragKindFormat, kind.ToString());
            if (kind == OverlayDragDataKind.Clip)
            {
                dataObject.SetData(ClipTriggerFormat, value);
            }
            else
            {
                dataObject.SetData(TagNameFormat, value);
            }

            if (!string.IsNullOrWhiteSpace(sourceTagName))
            {
                dataObject.SetData(SourceTagNameFormat, sourceTagName);
            }

            return dataObject;
        }

        public static ClipAssignmentDragData? TryRead(System.Windows.IDataObject dataObject)
        {
            mbottrilby.Services.OverlayDragDataKind kind = OverlayDragDataKind.Clip;
            if (dataObject.GetDataPresent(DragKindFormat))
            {
                string kindValue = dataObject.GetData(DragKindFormat) as string;
                if (!Enum.TryParse(kindValue, true, out kind))
                {
                    return null;
                }
            }

            string? value;
            if (kind == OverlayDragDataKind.Clip)
            {
                if (!dataObject.GetDataPresent(ClipTriggerFormat))
                {
                    return null;
                }

                value = dataObject.GetData(ClipTriggerFormat) as string;
            }
            else
            {
                if (!dataObject.GetDataPresent(TagNameFormat))
                {
                    return null;
                }

                value = dataObject.GetData(TagNameFormat) as string;
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            string sourceTagName = dataObject.GetDataPresent(SourceTagNameFormat)
                ? dataObject.GetData(SourceTagNameFormat) as string
                : null;

            return new ClipAssignmentDragData(
                kind,
                value.Trim(),
                string.IsNullOrWhiteSpace(sourceTagName) ? null : sourceTagName.Trim());
        }
    }

    internal sealed class ClipAssignmentDragData
    {
        public ClipAssignmentDragData(OverlayDragDataKind kind, string value, string? sourceTagName)
        {
            Kind = kind;
            Value = value;
            SourceTagName = sourceTagName;
        }

        public OverlayDragDataKind Kind { get; }
        public string Value { get; }
        public string Trigger => Value;
        public string TagName => Value;
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
