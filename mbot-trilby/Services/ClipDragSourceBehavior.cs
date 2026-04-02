using System;
namespace mbottrilby.Services
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

            mbottrilby.Services.ClipAssignmentDragData dragData = getDragData(button);
            if (dragData is null || string.IsNullOrWhiteSpace(dragData.Value))
            {
                return;
            }

            System.Windows.Point currentPosition = e.GetPosition(relativeTo);
            double horizontalDistance = Math.Abs(currentPosition.X - _dragStartPoint.Value.X);
            double verticalDistance = Math.Abs(currentPosition.Y - _dragStartPoint.Value.Y);
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
                System.Windows.DataObject dataObject = ClipAssignmentDragDrop.CreateDataObject(dragData.Kind, dragData.Value, dragData.SourceTagName);
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
