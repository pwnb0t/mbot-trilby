using System;
using System.Collections.Generic;
using System.Linq;

namespace ownbotsidekick.Search
{
    public sealed class ClipSearchState
    {
        private readonly int _maxVisibleResults;
        private readonly List<string> _allClipTriggers = new();
        private readonly List<string> _allTagNames = new();
        private readonly List<ClipSearchResult> _filteredResults = new();
        private string _query = string.Empty;

        public ClipSearchState(int maxVisibleResults)
        {
            _maxVisibleResults = maxVisibleResults;
        }

        public string Query => _query;
        public IReadOnlyList<ClipSearchResult> FilteredResults => _filteredResults;

        public void SetSource(IEnumerable<string> clipTriggers, IEnumerable<string> tagNames)
        {
            _allClipTriggers.Clear();
            _allClipTriggers.AddRange(clipTriggers);
            _allTagNames.Clear();
            _allTagNames.AddRange(tagNames);
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

        public ClipSearchResult? FirstResultOrDefault()
        {
            return _filteredResults.Count > 0 ? _filteredResults[0] : null;
        }

        private void Rebuild()
        {
            _filteredResults.Clear();
            if (string.IsNullOrEmpty(_query))
            {
                return;
            }

            _filteredResults.AddRange(
                _allClipTriggers
                    .Select(trigger => CreateResultOrDefault(SearchResultKind.Clip, trigger))
                    .Concat(_allTagNames.Select(tagName => CreateResultOrDefault(SearchResultKind.Tag, tagName)))
                    .Where(result => result is not null)
                    .Select(result => result!)
                    .OrderBy(GetSearchBucket, Comparer<int>.Default)
                    .ThenBy(result => result.DisplayText, StringComparer.OrdinalIgnoreCase)
                    .Take(_maxVisibleResults)
            );
        }

        private int GetSearchBucket(ClipSearchResult result)
        {
            var comparableText = GetComparableSearchText(result.Kind, result.Value, result.DisplayText);
            var isExactMatch = string.Equals(comparableText, _query, StringComparison.OrdinalIgnoreCase);
            var isStartsWithMatch = comparableText.StartsWith(_query, StringComparison.OrdinalIgnoreCase);

            if (isExactMatch && result.Kind == SearchResultKind.Clip)
            {
                return 0;
            }

            if (isExactMatch && result.Kind == SearchResultKind.Tag)
            {
                return 1;
            }

            if (isStartsWithMatch && result.Kind == SearchResultKind.Clip)
            {
                return 2;
            }

            if (isStartsWithMatch && result.Kind == SearchResultKind.Tag)
            {
                return 3;
            }

            if (result.Kind == SearchResultKind.Clip)
            {
                return 4;
            }

            return 5;
        }

        private ClipSearchResult? CreateResultOrDefault(SearchResultKind kind, string value)
        {
            var displayText = kind == SearchResultKind.Tag ? $"&{value}" : value;
            var comparableText = GetComparableSearchText(kind, value, displayText);
            var matchIndex = comparableText.IndexOf(_query, StringComparison.OrdinalIgnoreCase);
            if (matchIndex < 0)
            {
                return null;
            }

            var displayMatchIndex = matchIndex + (displayText.Length - comparableText.Length);

            var segments = new List<ClipSearchMatchSegment>();
            if (displayMatchIndex > 0)
            {
                segments.Add(new ClipSearchMatchSegment(
                    displayText[..displayMatchIndex],
                    isMatch: false
                ));
            }

            segments.Add(new ClipSearchMatchSegment(
                displayText.Substring(displayMatchIndex, _query.Length),
                isMatch: true
            ));

            var remainingStartIndex = displayMatchIndex + _query.Length;
            if (remainingStartIndex < displayText.Length)
            {
                segments.Add(new ClipSearchMatchSegment(
                    displayText[remainingStartIndex..],
                    isMatch: false
                ));
            }

            return new ClipSearchResult(kind, value, displayText, segments);
        }

        private string GetComparableSearchText(SearchResultKind kind, string value, string displayText)
        {
            return kind == SearchResultKind.Tag && !_query.StartsWith('&')
                ? value
                : displayText;
        }
    }
}
