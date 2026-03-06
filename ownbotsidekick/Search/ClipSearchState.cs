using System;
using System.Collections.Generic;
using System.Linq;

namespace ownbotsidekick.Search
{
    public sealed class ClipSearchState
    {
        private readonly int _maxVisibleResults;
        private readonly List<string> _allTriggers = new();
        private readonly List<ClipSearchResult> _filteredResults = new();
        private string _query = string.Empty;

        public ClipSearchState(int maxVisibleResults)
        {
            _maxVisibleResults = maxVisibleResults;
        }

        public string Query => _query;
        public IReadOnlyList<ClipSearchResult> FilteredResults => _filteredResults;

        public void SetSource(IEnumerable<string> triggers)
        {
            _allTriggers.Clear();
            _allTriggers.AddRange(triggers);
            Rebuild();
        }

        public void ClearQuery()
        {
            _query = string.Empty;
            Rebuild();
        }

        public void AppendCharacter(char value)
        {
            _query += value;
            Rebuild();
        }

        public bool Backspace()
        {
            if (string.IsNullOrEmpty(_query))
            {
                return false;
            }

            _query = _query[..^1];
            Rebuild();
            return true;
        }

        public string? FirstResultOrDefault()
        {
            return _filteredResults.Count > 0 ? _filteredResults[0].Trigger : null;
        }

        private void Rebuild()
        {
            _filteredResults.Clear();
            if (string.IsNullOrEmpty(_query))
            {
                return;
            }

            _filteredResults.AddRange(
                _allTriggers
                    .Select(CreateResultOrDefault)
                    .Where(result => result is not null)
                    .Select(result => result!)
                    .OrderBy(result => GetSearchBucket(result.Trigger), Comparer<int>.Default)
                    .ThenBy(result => result.Trigger, StringComparer.OrdinalIgnoreCase)
                    .Take(_maxVisibleResults)
            );
        }

        private int GetSearchBucket(string trigger)
        {
            return trigger.StartsWith(_query, StringComparison.OrdinalIgnoreCase) ? 0 : 1;
        }

        private ClipSearchResult? CreateResultOrDefault(string trigger)
        {
            var matchIndex = trigger.IndexOf(_query, StringComparison.OrdinalIgnoreCase);
            if (matchIndex < 0)
            {
                return null;
            }

            var segments = new List<ClipSearchMatchSegment>();
            if (matchIndex > 0)
            {
                segments.Add(new ClipSearchMatchSegment(
                    trigger[..matchIndex],
                    isMatch: false
                ));
            }

            segments.Add(new ClipSearchMatchSegment(
                trigger.Substring(matchIndex, _query.Length),
                isMatch: true
            ));

            var remainingStartIndex = matchIndex + _query.Length;
            if (remainingStartIndex < trigger.Length)
            {
                segments.Add(new ClipSearchMatchSegment(
                    trigger[remainingStartIndex..],
                    isMatch: false
                ));
            }

            return new ClipSearchResult(trigger, segments);
        }
    }
}
