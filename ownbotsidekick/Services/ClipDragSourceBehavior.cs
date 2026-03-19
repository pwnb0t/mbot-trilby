using System;
namespace ownbotsidekick.Services
{
    internal sealed class ClipDragSourceBehavior
    {
        private System.Windows.Point? _dragStartPoint;
        private System.Windows.Controls.Button? _dragSourceButton;

        public void HandlePreviewMouseLeftButtonDown(
            object sender,
            System.Windows.Input.MouseButtonEventArgs e,
            System.Windows.UIElement relativeTo)
        {
            if (sender is not System.Windows.Controls.Button button)
            {
                return;
            }

            _dragSourceButton = button;
            _dragStartPoint = e.GetPosition(relativeTo);
        }

        public void HandlePreviewMouseLeftButtonUp()
        {
            _dragSourceButton = null;
            _dragStartPoint = null;
        }

        public void HandlePreviewMouseMove(
            object sender,
            System.Windows.Input.MouseEventArgs e,
            System.Windows.UIElement relativeTo,
            Func<System.Windows.Controls.Button, ClipAssignmentDragData?> getDragData,
            Action<ClipAssignmentDragData?> setDragData)
        {
            if (e.LeftButton != System.Windows.Input.MouseButtonState.Pressed)
            {
                return;
            }

            if (
                sender is not System.Windows.Controls.Button button ||
                _dragSourceButton != button ||
                _dragStartPoint is null
            )
            {
                return;
            }

            var dragData = getDragData(button);
            if (dragData is null || string.IsNullOrWhiteSpace(dragData.Trigger))
            {
                return;
            }

            var currentPosition = e.GetPosition(relativeTo);
            var horizontalDistance = Math.Abs(currentPosition.X - _dragStartPoint.Value.X);
            var verticalDistance = Math.Abs(currentPosition.Y - _dragStartPoint.Value.Y);
            if (
                horizontalDistance < System.Windows.SystemParameters.MinimumHorizontalDragDistance &&
                verticalDistance < System.Windows.SystemParameters.MinimumVerticalDragDistance
            )
            {
                return;
            }

            setDragData(dragData);
            try
            {
                var dataObject = ClipAssignmentDragDrop.CreateDataObject(dragData.Trigger, dragData.SourceTagName);
                System.Windows.DragDrop.DoDragDrop(button, dataObject, System.Windows.DragDropEffects.Copy);
            }
            finally
            {
                _dragSourceButton = null;
                _dragStartPoint = null;
                setDragData(null);
            }
        }
    }
}
