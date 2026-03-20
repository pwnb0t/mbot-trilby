using System;

namespace mbottrilby.Search
{
    public sealed class ClipSearchMatchSegment
    {
        public ClipSearchMatchSegment(string text, bool isMatch)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
            IsMatch = isMatch;
        }

        public string Text { get; }

        public bool IsMatch { get; }
    }
}
