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
            Func<System.Windows.Controls.Button, string?> getClipTrigger,
            Action<bool> setDragActive)
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

            var trigger = getClipTrigger(button);
            if (string.IsNullOrWhiteSpace(trigger))
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

                setDragActive(true);
            try
            {
                var dragData = new System.Windows.DataObject(ClipAssignmentDragDrop.ClipTriggerFormat, trigger);
                System.Windows.DragDrop.DoDragDrop(button, dragData, System.Windows.DragDropEffects.Copy);
            }
            finally
            {
                _dragSourceButton = null;
                _dragStartPoint = null;
                setDragActive(false);
            }
        }
    }
}
