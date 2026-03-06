using System;
using System.Collections.Generic;

namespace ownbotsidekick.Search
{
    public sealed class ClipSearchResult
    {
        public ClipSearchResult(string trigger, IReadOnlyList<ClipSearchMatchSegment> segments)
        {
            Trigger = trigger ?? throw new ArgumentNullException(nameof(trigger));
            Segments = segments ?? throw new ArgumentNullException(nameof(segments));
        }

        public string Trigger { get; }

        public IReadOnlyList<ClipSearchMatchSegment> Segments { get; }
    }
}
