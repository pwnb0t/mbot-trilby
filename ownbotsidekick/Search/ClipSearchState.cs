using System;
using System.Collections.Generic;
using System.Linq;

namespace ownbotsidekick.Search
{
    public sealed class ClipSearchState
    {
        private readonly int _maxVisibleResults;
        private readonly List<string> _allTriggers = new();
        private readonly List<string> _filteredTriggers = new();
        private string _query = string.Empty;

        public ClipSearchState(int maxVisibleResults)
        {
            _maxVisibleResults = maxVisibleResults;
        }

        public string Query => _query;
        public IReadOnlyList<string> FilteredTriggers => _filteredTriggers;

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
            return _filteredTriggers.Count > 0 ? _filteredTriggers[0] : null;
        }

        private void Rebuild()
        {
            _filteredTriggers.Clear();
            if (string.IsNullOrEmpty(_query))
            {
                return;
            }

            _filteredTriggers.AddRange(
                _allTriggers
                    .Where(trigger => trigger.StartsWith(_query, StringComparison.OrdinalIgnoreCase))
                    .Take(_maxVisibleResults)
            );
        }
    }
}
