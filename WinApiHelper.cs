using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace AcrylicKeyboard
{
    class WinApiHelper
    {
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        [DllImport("user32.dll")]
        internal static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        internal static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true,
            CallingConvention = CallingConvention.Winapi)]
        internal static extern short GetKeyState(int keyCode);

        [DllImport("user32.dll")]
        static extern short VkKeyScan(char ch);

        [DllImport("user32.dll")]
        static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);

        [DllImport("gdi32.dll")]
        static extern IntPtr CreateRoundRectRgn(int x1, int y1, int x2, int y2, int cx, int cy);

        internal static bool IsCapsLock => ((ushort) GetKeyState(0x14) & 0xffff) != 0;

        internal static bool IsNumLock => ((ushort) GetKeyState(0x90) & 0xffff) != 0;

        internal static bool IsScrollLock => ((ushort) GetKeyState(0x91) & 0xffff) != 0;

        internal static short CharToVirtualKey(char c)
        {
            return VkKeyScan(c);
        }

        internal static void EnableBlur(Window window, AccentState state)
        {
            var windowHelper = new WindowInteropHelper(window);

            var accent = new AccentPolicy();
            accent.AccentState = state;
            accent.GradientColor = 0xFFFFFFF;
            var accentStructSize = Marshal.SizeOf(accent);

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData();
            data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.SizeOfData = accentStructSize;
            data.Data = accentPtr;

            SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }

        internal static void MakeUnfocusable(Window window)
        {
            var helper = new WindowInteropHelper(window);
            SetWindowLong(helper.Handle, GWL_EXSTYLE,
                GetWindowLong(helper.Handle, GWL_EXSTYLE) | WS_EX_NOACTIVATE);
        }

        internal static void ClipWindow(Window window, Rect bounds)
        {
            var helper = new WindowInteropHelper(window);
            var rectPtr = IntPtr.Zero;
            if (!bounds.IsEmpty)
            {
                rectPtr = CreateRoundRectRgn((int) bounds.X, (int) bounds.Y, (int) (bounds.X + bounds.Width),
                    (int) (bounds.Y + bounds.Height), 0, 0);
            }

            SetWindowRgn(helper.Handle, rectPtr, true);
        }

        internal enum AccentState
        {
            ACCENT_DISABLED = 0,
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_ENABLE_ACRYLIC_BLURBEHIND = 4,
            ACCENT_INVALID_STATE = 5
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct AccentPolicy
        {
            public AccentState AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        internal enum WindowCompositionAttribute
        {
            // ...
            WCA_ACCENT_POLICY = 19
            // ...
        }
    }
}