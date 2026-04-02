using System.Linq;
using mbottrilby.Search;
using Xunit;

namespace mbottrilby.Tests.Search
{
    public sealed class ClipSearchStateTests
    {
        [Fact]
        public void Filters_By_CaseInsensitive_Prefix()
        {
            mbottrilby.Search.ClipSearchState state = new ClipSearchState(maxVisibleResults: 15);
            state.SetSource(new[] { "TestOne", "testTwo", "alpha" }, new[] { "topic" });

            state.AppendCharacter('t');
            state.AppendCharacter('e');

            Assert.Equal(2, state.FilteredResults.Count);
            Assert.Contains(state.FilteredResults, result => result.Value == "TestOne");
            Assert.Contains(state.FilteredResults, result => result.Value == "testTwo");
        }

        [Fact]
        public void Caps_Visible_Results_To_Max()
        {
            string[] source = Enumerable.Range(1, 30).Select(i => $"t{i:D2}").ToArray();
            mbottrilby.Search.ClipSearchState state = new ClipSearchState(maxVisibleResults: 15);
            state.SetSource(source, new[] { "tag" });

            state.AppendCharacter('t');

            Assert.Equal(15, state.FilteredResults.Count);
        }

        [Fact]
        public void ClearQuery_Resets_Query_And_Results()
        {
            mbottrilby.Search.ClipSearchState state = new ClipSearchState(maxVisibleResults: 15);
            state.SetSource(new[] { "abc", "abd" }, new[] { "atest" });
            state.AppendCharacter('a');

            state.ClearQuery();

            Assert.Equal(string.Empty, state.Query);
            Assert.Empty(state.FilteredResults);
        }

        [Fact]
        public void Backspace_Removes_Last_Character_And_Rebuilds()
        {
            mbottrilby.Search.ClipSearchState state = new ClipSearchState(maxVisibleResults: 15);
            state.SetSource(new[] { "abc", "abd", "axz" }, new[] { "atest" });
            state.AppendCharacter('a');
            state.AppendCharacter('b');

            bool removed = state.Backspace();

            Assert.True(removed);
            Assert.Equal("a", state.Query);
            Assert.Equal(4, state.FilteredResults.Count);
        }

        [Fact]
        public void FirstResultOrDefault_Returns_First_Filtered_Result()
        {
            mbottrilby.Search.ClipSearchState state = new ClipSearchState(maxVisibleResults: 15);
            state.SetSource(new[] { "test-a", "test-b" }, new[] { "test" });
            state.AppendCharacter('t');

            mbottrilby.Search.ClipSearchResult result = Assert.IsType<ClipSearchResult>(state.FirstResultOrDefault());
            Assert.Equal(SearchResultKind.Clip, result.Kind);
            Assert.Equal("test-a", result.Value);
        }

        [Fact]
        public void Ranks_StartsWith_Matches_Before_Substring_Matches()
        {
            mbottrilby.Search.ClipSearchState state = new ClipSearchState(maxVisibleResults: 15);
            state.SetSource(new[] { "zest", "test", "best", "esther" }, new[] { "atest" });

            state.AppendCharacter('e');
            state.AppendCharacter('s');
            state.AppendCharacter('t');

            string[] triggers = state.FilteredResults.Select(result => result.DisplayText).ToArray();

            Assert.Equal(new[] { "esther", "best", "test", "zest", "&atest" }, triggers);
        }

        [Fact]
        public void Highlights_Only_First_Matching_Substring()
        {
            mbottrilby.Search.ClipSearchState state = new ClipSearchState(maxVisibleResults: 15);
            state.SetSource(new[] { "testtest" }, new[] { "topic" });

            state.AppendCharacter('t');
            state.AppendCharacter('e');
            state.AppendCharacter('s');
            state.AppendCharacter('t');

            mbottrilby.Search.ClipSearchResult result = Assert.Single(state.FilteredResults);

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

        [Fact]
        public void Ranks_Clip_Results_Before_Tag_Results_By_Todo_Order()
        {
            mbottrilby.Search.ClipSearchState state = new ClipSearchState(maxVisibleResults: 15);
            state.SetSource(
                new[] { "test", "test1", "test2", "atest", "btest" },
                new[] { "test", "test1", "atest", "btest" }
            );

            state.AppendCharacter('t');
            state.AppendCharacter('e');
            state.AppendCharacter('s');
            state.AppendCharacter('t');

            string[] results = state.FilteredResults.Select(result => result.DisplayText).ToArray();

            Assert.Equal(
                new[] { "test", "&test", "test1", "test2", "&test1", "atest", "btest", "&atest", "&btest" },
                results
            );
        }

        [Fact]
        public void Supports_Searching_For_Tags_With_Ampersand()
        {
            mbottrilby.Search.ClipSearchState state = new ClipSearchState(maxVisibleResults: 15);
            state.SetSource(new[] { "test" }, new[] { "test", "team" });

            state.AppendCharacter('&');
            state.AppendCharacter('t');

            string[] results = state.FilteredResults.Select(result => result.DisplayText).ToArray();

            Assert.Equal(new[] { "&team", "&test" }, results);
        }
    }
}
