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

            Assert.Equal(2, state.FilteredResults.Count);
            Assert.Contains(state.FilteredResults, result => result.Trigger == "TestOne");
            Assert.Contains(state.FilteredResults, result => result.Trigger == "testTwo");
        }

        [Fact]
        public void Caps_Visible_Results_To_Max()
        {
            var source = Enumerable.Range(1, 30).Select(i => $"t{i:D2}").ToArray();
            var state = new ClipSearchState(maxVisibleResults: 15);
            state.SetSource(source);

            state.AppendCharacter('t');

            Assert.Equal(15, state.FilteredResults.Count);
        }

        [Fact]
        public void ClearQuery_Resets_Query_And_Results()
        {
            var state = new ClipSearchState(maxVisibleResults: 15);
            state.SetSource(new[] { "abc", "abd" });
            state.AppendCharacter('a');

            state.ClearQuery();

            Assert.Equal(string.Empty, state.Query);
            Assert.Empty(state.FilteredResults);
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
            Assert.Equal(3, state.FilteredResults.Count);
        }

        [Fact]
        public void FirstResultOrDefault_Returns_First_Filtered_Result()
        {
            var state = new ClipSearchState(maxVisibleResults: 15);
            state.SetSource(new[] { "test-a", "test-b" });
            state.AppendCharacter('t');

            Assert.Equal("test-a", state.FirstResultOrDefault());
        }

        [Fact]
        public void Ranks_StartsWith_Matches_Before_Substring_Matches()
        {
            var state = new ClipSearchState(maxVisibleResults: 15);
            state.SetSource(new[] { "zest", "test", "best", "esther" });

            state.AppendCharacter('e');
            state.AppendCharacter('s');
            state.AppendCharacter('t');

            var triggers = state.FilteredResults.Select(result => result.Trigger).ToArray();

            Assert.Equal(new[] { "esther", "best", "test", "zest" }, triggers);
        }

        [Fact]
        public void Highlights_Only_First_Matching_Substring()
        {
            var state = new ClipSearchState(maxVisibleResults: 15);
            state.SetSource(new[] { "testtest" });

            state.AppendCharacter('t');
            state.AppendCharacter('e');
            state.AppendCharacter('s');
            state.AppendCharacter('t');

            var result = Assert.Single(state.FilteredResults);

            Assert.Collection(
                result.Segments,
                segment =>
                {
                    Assert.Equal("test", segment.Text);
                    Assert.True(segment.IsMatch);
                },
                segment =>
                {
                    Assert.Equal("test", segment.Text);
                    Assert.False(segment.IsMatch);
                }
            );
        }
    }
}
