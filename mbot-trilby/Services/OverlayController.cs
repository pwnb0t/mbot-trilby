using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

namespace mbottrilby.Services
{
    internal enum OverlayShowSource
    {
        Standard,
        Tray
    }

    internal sealed class OverlayController
    {
        private const int GwlExStyle = -20;
        private const int WsExNoActivate = 0x08000000;
        private const int WsExTransparent = 0x00000020;
        private static readonly IntPtr HwndTopMost = new(-1);
        private const int OverlayBottomReserveMinPixels = 56;
        private const int OverlayBottomReservePaddingPixels = 8;
        private const uint SwpNoActivate = 0x0010;
        private const uint SwpNoMove = 0x0002;
        private const uint SwpNoSize = 0x0001;
        private const uint SwpNoOwnerZOrder = 0x0200;
        private const uint SwpShowWindow = 0x0040;
        private const int SwHide = 0;
        private const int SwShowNoActivate = 4;

        private readonly Border _overlayPanelBorder;
        private readonly OverlayDiagnostics _diagnostics;
        private readonly Action<bool> _setOverlayVisible;
        private readonly Action<bool> _setTopmost;
        private readonly Action _prepareWindowForShow;
        private IntPtr _windowHandle = IntPtr.Zero;

        public OverlayController(
            Border overlayPanelBorder,
            OverlayDiagnostics diagnostics,
            Action<bool> setOverlayVisible,
            Action<bool> setTopmost,
            Action prepareWindowForShow
        )
        {
            _overlayPanelBorder = overlayPanelBorder;
            _diagnostics = diagnostics;
            _setOverlayVisible = setOverlayVisible;
            _setTopmost = setTopmost;
            _prepareWindowForShow = prepareWindowForShow;
        }

        public bool IsVisible { get; private set; }

        public void InitializeWindowHandle(IntPtr windowHandle)
        {
            _windowHandle = windowHandle;
            EnableNoActivateMode(windowHandle);
            SetOverlayInteractionEnabled(false);
            HideWindow();
            _diagnostics.Info("app", "Overlay no-activate mode enabled.");
        }

        public void ApplyOverlayPanelLayout()
        {
            _overlayPanelBorder.Margin = new Thickness(0);
        }

        public static double GetReservedBottomHeight()
        {
            var taskbarHeightEstimate = Math.Max(0, SystemParameters.PrimaryScreenHeight - SystemParameters.WorkArea.Height);
            return Math.Max(
                OverlayBottomReserveMinPixels,
                taskbarHeightEstimate + OverlayBottomReservePaddingPixels
            );
        }

        public bool IsPointInsideOverlayPanel(System.Windows.Point screenPoint)
        {
            if (_overlayPanelBorder.ActualWidth <= 0 || _overlayPanelBorder.ActualHeight <= 0)
            {
                return false;
            }

            var topLeft = _overlayPanelBorder.PointToScreen(new System.Windows.Point(0, 0));
            var bottomRight = _overlayPanelBorder.PointToScreen(
                new System.Windows.Point(_overlayPanelBorder.ActualWidth, _overlayPanelBorder.ActualHeight)
            );
            var panelRect = new Rect(topLeft, bottomRight);
            return panelRect.Contains(screenPoint);
        }

        public void Show(OverlayShowSource source, bool topmost)
        {
            _prepareWindowForShow();
            ShowWindowNoActivate();
            _setOverlayVisible(true);
            IsVisible = true;
            SetOverlayInteractionEnabled(true);
            _setTopmost(topmost);
            if (topmost)
            {
                ReassertTopmost();
            }
            if (source == OverlayShowSource.Tray)
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

            _setOverlayVisible(false);
            IsVisible = false;
            SetOverlayInteractionEnabled(false);
            _setTopmost(false);
            HideWindow();
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

        private void ReassertTopmost()
        {
            if (_windowHandle == IntPtr.Zero)
            {
                return;
            }

            SetWindowPos(
                _windowHandle,
                HwndTopMost,
                0,
                0,
                0,
                0,
                SwpNoActivate | SwpNoMove | SwpNoSize | SwpNoOwnerZOrder | SwpShowWindow
            );
        }

        private void ShowWindowNoActivate()
        {
            if (_windowHandle == IntPtr.Zero)
            {
                return;
            }

            ShowWindow(_windowHandle, SwShowNoActivate);
        }

        private void HideWindow()
        {
            if (_windowHandle == IntPtr.Zero)
            {
                return;
            }

            ShowWindow(_windowHandle, SwHide);
        }

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
        private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int x,
            int y,
            int cx,
            int cy,
            uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}
