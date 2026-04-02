using System.Linq;
using mbottrilby.ViewModels;
using Xunit;

namespace mbottrilby.Tests.ViewModels
{
    public sealed class TagWidgetViewModelTests
    {
        [Fact]
        public void ClearSelection_Restores_Empty_State()
        {
            mbottrilby.ViewModels.TagWidgetViewModel viewModel = new TagWidgetViewModel();
            viewModel.SetLoaded("test", new[] { new TagClipEntryViewModel("alpha", "test") });
            viewModel.IsDragAvailableTarget = true;
            viewModel.IsDragHoverTarget = true;
            viewModel.IsRemoveDragOperation = true;

            viewModel.ClearSelection();

            Assert.False(viewModel.HasSelectedTag);
            Assert.Null(viewModel.SelectedTagName);
            Assert.Equal("Tag: no &tag selected", viewModel.TitleText);
            Assert.Equal("Search for an existing &tag", viewModel.StatusText);
            Assert.Empty(viewModel.Clips);
            Assert.False(viewModel.IsDragAvailableTarget);
            Assert.False(viewModel.IsDragHoverTarget);
            Assert.False(viewModel.IsRemoveDragOperation);
            Assert.Equal("Search for an existing &tag", viewModel.DropHintText);
        }

        [Fact]
        public void SetLoading_Sets_Selected_Tag_And_Loading_Message()
        {
            mbottrilby.ViewModels.TagWidgetViewModel viewModel = new TagWidgetViewModel();

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
            mbottrilby.ViewModels.TagWidgetViewModel viewModel = new TagWidgetViewModel();

            viewModel.SetLoaded(
                "test",
                new[]
                {
                    new TagClipEntryViewModel("alpha", "test"),
                    new TagClipEntryViewModel("beta", "test")
                });

            Assert.True(viewModel.HasSelectedTag);
            Assert.Equal("test", viewModel.SelectedTagName);
            Assert.Equal("Tag: &test", viewModel.TitleText);
            Assert.Equal(string.Empty, viewModel.StatusText);
            Assert.Equal(new[] { "alpha", "beta" }, viewModel.Clips.Select(clip => clip.Trigger).ToArray());
        }

        [Fact]
        public void DropHintText_Uses_Remove_Copy_When_Remove_Drag_Is_Active()
        {
            mbottrilby.ViewModels.TagWidgetViewModel viewModel = new TagWidgetViewModel();
            viewModel.SetLoaded("test", new[] { new TagClipEntryViewModel("alpha", "test") });

            viewModel.IsRemoveDragOperation = true;
            viewModel.IsDragAvailableTarget = true;

            Assert.Equal("Drop here to remove from &test", viewModel.DropHintText);
        }

        [Fact]
        public void SetFailed_Keeps_Selected_Tag_And_Error_Message()
        {
            mbottrilby.ViewModels.TagWidgetViewModel viewModel = new TagWidgetViewModel();

            viewModel.SetFailed("missing", "Failed to load &missing");

            Assert.True(viewModel.HasSelectedTag);
            Assert.Equal("missing", viewModel.SelectedTagName);
            Assert.Equal("Tag: &missing", viewModel.TitleText);
            Assert.Equal("Failed to load &missing", viewModel.StatusText);
            Assert.Empty(viewModel.Clips);
        }
    }
}
