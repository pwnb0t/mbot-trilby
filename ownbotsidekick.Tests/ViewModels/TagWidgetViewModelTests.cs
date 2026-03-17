using System.Linq;
using ownbotsidekick.ViewModels;
using Xunit;

namespace ownbotsidekick.Tests.ViewModels
{
    public sealed class TagWidgetViewModelTests
    {
        [Fact]
        public void ClearSelection_Restores_Empty_State()
        {
            var viewModel = new TagWidgetViewModel();
            viewModel.SetLoaded("test", new[] { new TagClipEntryViewModel("alpha") });
            viewModel.IsDragAvailableTarget = true;
            viewModel.IsDragHoverTarget = true;

            viewModel.ClearSelection();

            Assert.False(viewModel.HasSelectedTag);
            Assert.Null(viewModel.SelectedTagName);
            Assert.Equal("Tag: no &tag selected", viewModel.TitleText);
            Assert.Equal("Search for an existing &tag", viewModel.StatusText);
            Assert.Empty(viewModel.Clips);
            Assert.False(viewModel.IsDragAvailableTarget);
            Assert.False(viewModel.IsDragHoverTarget);
            Assert.Equal("Search for an existing &tag", viewModel.DropHintText);
        }

        [Fact]
        public void SetLoading_Sets_Selected_Tag_And_Loading_Message()
        {
            var viewModel = new TagWidgetViewModel();

            viewModel.SetLoading("test");

            Assert.True(viewModel.HasSelectedTag);
            Assert.Equal("test", viewModel.SelectedTagName);
            Assert.Equal("Tag: &test", viewModel.TitleText);
            Assert.Equal("Loading clips for &test...", viewModel.StatusText);
            Assert.Empty(viewModel.Clips);
            Assert.Equal("Drag clips here to add to &test", viewModel.DropHintText);
        }

        [Fact]
        public void SetLoaded_With_Clips_Clears_Status_And_Keeps_Entries()
        {
            var viewModel = new TagWidgetViewModel();

            viewModel.SetLoaded(
                "test",
                new[]
                {
                    new TagClipEntryViewModel("alpha"),
                    new TagClipEntryViewModel("beta")
                });

            Assert.True(viewModel.HasSelectedTag);
            Assert.Equal("test", viewModel.SelectedTagName);
            Assert.Equal("Tag: &test", viewModel.TitleText);
            Assert.Equal(string.Empty, viewModel.StatusText);
            Assert.Equal(new[] { "alpha", "beta" }, viewModel.Clips.Select(clip => clip.Trigger).ToArray());
        }

        [Fact]
        public void SetFailed_Keeps_Selected_Tag_And_Error_Message()
        {
            var viewModel = new TagWidgetViewModel();

            viewModel.SetFailed("missing", "Failed to load &missing");

            Assert.True(viewModel.HasSelectedTag);
            Assert.Equal("missing", viewModel.SelectedTagName);
            Assert.Equal("Tag: &missing", viewModel.TitleText);
            Assert.Equal("Failed to load &missing", viewModel.StatusText);
            Assert.Empty(viewModel.Clips);
        }
    }
}
