using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

namespace ownbotsidekick.Services
{
    internal sealed class OverlayController
    {
        private const int GwlExStyle = -20;
        private const int WsExNoActivate = 0x08000000;
        private const int WsExTransparent = 0x00000020;
        private const int OverlayBottomReserveMinPixels = 56;
        private const int OverlayBottomReservePaddingPixels = 8;

        private readonly Grid _rootOverlayGrid;
        private readonly Border _overlayPanelBorder;
        private readonly OverlayDiagnostics _diagnostics;
        private readonly Action _resetSearchState;
        private readonly Action<bool> _setTopmost;
        private IntPtr _windowHandle = IntPtr.Zero;

        public OverlayController(
            Grid rootOverlayGrid,
            Border overlayPanelBorder,
            OverlayDiagnostics diagnostics,
            Action resetSearchState,
            Action<bool> setTopmost
        )
        {
            _rootOverlayGrid = rootOverlayGrid;
            _overlayPanelBorder = overlayPanelBorder;
            _diagnostics = diagnostics;
            _resetSearchState = resetSearchState;
            _setTopmost = setTopmost;
        }

        public bool IsVisible { get; private set; }

        public void InitializeWindowHandle(IntPtr windowHandle)
        {
            _windowHandle = windowHandle;
            EnableNoActivateMode(windowHandle);
            SetOverlayInteractionEnabled(false);
            _diagnostics.Info("app", "Overlay no-activate mode enabled.");
        }

        public void ApplyOverlayPanelLayout()
        {
            var taskbarHeightEstimate = Math.Max(0, SystemParameters.PrimaryScreenHeight - SystemParameters.WorkArea.Height);
            var reservedBottom = Math.Max(
                OverlayBottomReserveMinPixels,
                taskbarHeightEstimate + OverlayBottomReservePaddingPixels
            );
            _overlayPanelBorder.Margin = new Thickness(0, 0, 0, reservedBottom);
        }

        public bool IsPointInsideOverlayPanel(System.Windows.Point screenPoint)
        {
            if (_overlayPanelBorder.ActualWidth <= 0 || _overlayPanelBorder.ActualHeight <= 0)
            {
                return false;
            }

            var topLeft = _overlayPanelBorder.PointToScreen(new System.Windows.Point(0, 0));
            var panelRect = new Rect(topLeft.X, topLeft.Y, _overlayPanelBorder.ActualWidth, _overlayPanelBorder.ActualHeight);
            return panelRect.Contains(screenPoint);
        }

        public void Show(string logMessage, bool topmost)
        {
            _resetSearchState();
            _rootOverlayGrid.Visibility = Visibility.Visible;
            IsVisible = true;
            SetOverlayInteractionEnabled(true);
            _setTopmost(topmost);
            if (logMessage.Contains("from tray", StringComparison.OrdinalIgnoreCase))
            {
                _diagnostics.OverlayShownFromTray();
            }
            else
            {
                _diagnostics.OverlayShown();
            }
        }

        public void Hide(string logMessage)
        {
            if (!IsVisible)
            {
                return;
            }

            _resetSearchState();
            _rootOverlayGrid.Visibility = Visibility.Collapsed;
            IsVisible = false;
            SetOverlayInteractionEnabled(false);
            _diagnostics.OverlayHidden(logMessage);
        }

        private void EnableNoActivateMode(IntPtr windowHandle)
        {
            var currentExStyle = GetWindowLongPtr(windowHandle, GwlExStyle).ToInt64();
            var updatedExStyle = new IntPtr(currentExStyle | WsExNoActivate);
            SetWindowLongPtr(windowHandle, GwlExStyle, updatedExStyle);
        }

        private void SetOverlayInteractionEnabled(bool enabled)
        {
            if (_windowHandle == IntPtr.Zero)
            {
                return;
            }

            var currentExStyle = GetWindowLongPtr(_windowHandle, GwlExStyle).ToInt64();
            long updatedExStyle;
            if (enabled)
            {
                updatedExStyle = (currentExStyle | WsExNoActivate) & ~WsExTransparent;
            }
            else
            {
                updatedExStyle = (currentExStyle | WsExNoActivate | WsExTransparent);
            }

            SetWindowLongPtr(_windowHandle, GwlExStyle, new IntPtr(updatedExStyle));
        }

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
        private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
    }
}
