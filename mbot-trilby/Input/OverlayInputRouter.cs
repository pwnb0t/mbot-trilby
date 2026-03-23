using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;
using mbottrilby.Services;

namespace mbottrilby.Input
{
    internal sealed class OverlayInputRouter : IDisposable
    {
        private const int WhKeyboardLl = 13;
        private const int WhMouseLl = 14;
        private const int WmKeyDown = 0x0100;
        private const int WmSysKeyDown = 0x0104;
        private const int LlkhfAltdown = 0x20;
        private const int WmLButtonDown = 0x0201;
        private const int WmRButtonDown = 0x0204;
        private const int WmMButtonDown = 0x0207;

        private readonly Func<bool> _isOverlayVisible;
        private readonly Func<int, bool, bool> _handleGlobalHotkey;
        private readonly Func<int, bool, bool> _handleOverlayVirtualKey;
        private readonly Func<System.Windows.Point, bool> _isPointInsideOverlayPanel;
        private readonly Action _onOutsideClick;
        private readonly OverlayDiagnostics _diagnostics;
        private readonly Dispatcher _dispatcher;
        private readonly LowLevelKeyboardProc _keyboardHookProc;
        private readonly LowLevelMouseProc _mouseHookProc;

        private IntPtr _keyboardHookHandle = IntPtr.Zero;
        private IntPtr _mouseHookHandle = IntPtr.Zero;

        public OverlayInputRouter(
            Func<bool> isOverlayVisible,
            Func<int, bool, bool> handleGlobalHotkey,
            Func<int, bool, bool> handleOverlayVirtualKey,
            Func<System.Windows.Point, bool> isPointInsideOverlayPanel,
            Action onOutsideClick,
            OverlayDiagnostics diagnostics,
            Dispatcher dispatcher
        )
        {
            _isOverlayVisible = isOverlayVisible;
            _handleGlobalHotkey = handleGlobalHotkey;
            _handleOverlayVirtualKey = handleOverlayVirtualKey;
            _isPointInsideOverlayPanel = isPointInsideOverlayPanel;
            _onOutsideClick = onOutsideClick;
            _diagnostics = diagnostics;
            _dispatcher = dispatcher;
            _keyboardHookProc = KeyboardHookCallback;
            _mouseHookProc = MouseHookCallback;
        }

        public void Start()
        {
            using var currentProcess = Process.GetCurrentProcess();
            using var currentModule = currentProcess.MainModule;
            var moduleName = currentModule?.ModuleName;
            var moduleHandle = moduleName is null ? IntPtr.Zero : GetModuleHandle(moduleName);

            _keyboardHookHandle = SetWindowsHookEx(WhKeyboardLl, _keyboardHookProc, moduleHandle, 0);
            if (_keyboardHookHandle == IntPtr.Zero)
            {
                _diagnostics.HookInitFailed("keyboard");
            }
            else
            {
                _diagnostics.HookInitialized("keyboard");
            }

            _mouseHookHandle = SetWindowsHookEx(WhMouseLl, _mouseHookProc, moduleHandle, 0);
            if (_mouseHookHandle == IntPtr.Zero)
            {
                _diagnostics.HookInitFailed("mouse");
            }
            else
            {
                _diagnostics.HookInitialized("mouse");
            }
        }

        public void Dispose()
        {
            if (_keyboardHookHandle != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_keyboardHookHandle);
                _keyboardHookHandle = IntPtr.Zero;
            }

            if (_mouseHookHandle != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_mouseHookHandle);
                _mouseHookHandle = IntPtr.Zero;
            }
        }

        private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0)
            {
                return CallNextHookEx(_keyboardHookHandle, nCode, wParam, lParam);
            }

            var message = wParam.ToInt32();
            if (message != WmKeyDown && message != WmSysKeyDown)
            {
                return CallNextHookEx(_keyboardHookHandle, nCode, wParam, lParam);
            }

            var keyboardData = Marshal.PtrToStructure<KbdLlHookStruct>(lParam);
            var isAltDown = message == WmSysKeyDown || (keyboardData.Flags & LlkhfAltdown) != 0;
            if (_handleGlobalHotkey(keyboardData.VkCode, isAltDown))
            {
                return (IntPtr)1;
            }

            if (!_isOverlayVisible())
            {
                return CallNextHookEx(_keyboardHookHandle, nCode, wParam, lParam);
            }

            var handled = _handleOverlayVirtualKey(keyboardData.VkCode, isAltDown);
            if (handled)
            {
                return (IntPtr)1;
            }

            return CallNextHookEx(_keyboardHookHandle, nCode, wParam, lParam);
        }

        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0)
            {
                return CallNextHookEx(_mouseHookHandle, nCode, wParam, lParam);
            }

            if (!_isOverlayVisible())
            {
                return CallNextHookEx(_mouseHookHandle, nCode, wParam, lParam);
            }

            var message = wParam.ToInt32();
            if (message != WmLButtonDown && message != WmRButtonDown && message != WmMButtonDown)
            {
                return CallNextHookEx(_mouseHookHandle, nCode, wParam, lParam);
            }

            var mouseData = Marshal.PtrToStructure<MsLlHookStruct>(lParam);
            var clickPoint = new System.Windows.Point(mouseData.Pt.X, mouseData.Pt.Y);
            _dispatcher.BeginInvoke(() =>
            {
                if (_isOverlayVisible() && !_isPointInsideOverlayPanel(clickPoint))
                {
                    _onOutsideClick();
                }
            });

            return CallNextHookEx(_mouseHookHandle, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct KbdLlHookStruct
        {
            public int VkCode;
            public int ScanCode;
            public int Flags;
            public int Time;
            public IntPtr DwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PointStruct
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MsLlHookStruct
        {
            public PointStruct Pt;
            public int MouseData;
            public int Flags;
            public int Time;
            public IntPtr DwExtraInfo;
        }
    }
}
