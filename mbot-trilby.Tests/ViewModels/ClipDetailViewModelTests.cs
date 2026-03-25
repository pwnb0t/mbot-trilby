using mbottrilby.ViewModels;
using Xunit;

namespace mbottrilby.Tests.ViewModels
{
    public sealed class ClipDetailViewModelTests
    {
        [Fact]
        public void ShowPlaceholder_Resets_To_Default_Text()
        {
            var viewModel = new ClipDetailViewModel();

            viewModel.ShowClip("hello", "https://example.com/test", "1.25s", "2.5s", "pwnb0t");
            viewModel.ShowPlaceholder();

            Assert.False(viewModel.HasClip);
            Assert.Equal("Hover a clip to see details", viewModel.StatusText);
            Assert.Equal(string.Empty, viewModel.TriggerText);
        }

        [Fact]
        public void ShowClip_Formats_Fields_For_Display()
        {
            var viewModel = new ClipDetailViewModel();

            viewModel.ShowClip("hello", "https://example.com/test", "1.25s", "2.5s", "pwnb0t");

            Assert.True(viewModel.HasClip);
            Assert.Equal("hello", viewModel.TriggerText);
            Assert.Equal("https://example.com/test", viewModel.SourceUrlText);
            Assert.Equal("1.25s", viewModel.StartOffsetText);
            Assert.Equal("2.5s", viewModel.ClipLengthText);
            Assert.Equal("pwnb0t", viewModel.AddedByText);
        }

        [Fact]
        public void ShowClip_Trims_Long_Source_Urls()
        {
            var viewModel = new ClipDetailViewModel();
            var longUrl = "https://example.com/" + new string('a', 150);

            viewModel.ShowClip("hello", longUrl, string.Empty, string.Empty, "pwnb0t");

            Assert.EndsWith("…", viewModel.SourceUrlText);
            Assert.True(viewModel.SourceUrlText.Length <= 100);
        }
    }
}
