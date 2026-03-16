using System;
using System.Collections.Generic;

namespace ownbotsidekick.Search
{
    public enum SearchResultKind
    {
        Clip,
        Tag
    }

    public sealed class ClipSearchResult
    {
        public ClipSearchResult(
            SearchResultKind kind,
            string value,
            string displayText,
            IReadOnlyList<ClipSearchMatchSegment> segments)
        {
            Kind = kind;
            Value = value ?? throw new ArgumentNullException(nameof(value));
            DisplayText = displayText ?? throw new ArgumentNullException(nameof(displayText));
            Segments = segments ?? throw new ArgumentNullException(nameof(segments));
        }

        public SearchResultKind Kind { get; }

        public string Value { get; }

        public string DisplayText { get; }

        public IReadOnlyList<ClipSearchMatchSegment> Segments { get; }
    }
}
