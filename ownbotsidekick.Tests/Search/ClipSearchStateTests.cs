using System.Linq;
using ownbotsidekick.Search;
using Xunit;

namespace ownbotsidekick.Tests.Search
{
    public sealed class ClipSearchStateTests
    {
        [Fact]
        public void Filters_By_CaseInsensitive_Prefix()
        {
            var state = new ClipSearchState(maxVisibleResults: 15);
            state.SetSource(new[] { "TestOne", "testTwo", "alpha" });

            state.AppendCharacter('t');
            state.AppendCharacter('e');

            Assert.Equal(2, state.FilteredTriggers.Count);
            Assert.Contains("TestOne", state.FilteredTriggers);
            Assert.Contains("testTwo", state.FilteredTriggers);
        }

        [Fact]
        public void Caps_Visible_Results_To_Max()
        {
            var source = Enumerable.Range(1, 30).Select(i => $"t{i:D2}").ToArray();
            var state = new ClipSearchState(maxVisibleResults: 15);
            state.SetSource(source);

            state.AppendCharacter('t');

            Assert.Equal(15, state.FilteredTriggers.Count);
        }

        [Fact]
        public void ClearQuery_Resets_Query_And_Results()
        {
            var state = new ClipSearchState(maxVisibleResults: 15);
            state.SetSource(new[] { "abc", "abd" });
            state.AppendCharacter('a');

            state.ClearQuery();

            Assert.Equal(string.Empty, state.Query);
            Assert.Empty(state.FilteredTriggers);
        }

        [Fact]
        public void Backspace_Removes_Last_Character_And_Rebuilds()
        {
            var state = new ClipSearchState(maxVisibleResults: 15);
            state.SetSource(new[] { "abc", "abd", "axz" });
            state.AppendCharacter('a');
            state.AppendCharacter('b');

            var removed = state.Backspace();

            Assert.True(removed);
            Assert.Equal("a", state.Query);
            Assert.Equal(3, state.FilteredTriggers.Count);
        }

        [Fact]
        public void FirstResultOrDefault_Returns_First_Filtered_Result()
        {
            var state = new ClipSearchState(maxVisibleResults: 15);
            state.SetSource(new[] { "test-a", "test-b" });
            state.AppendCharacter('t');

            Assert.Equal("test-a", state.FirstResultOrDefault());
        }
    }
}
