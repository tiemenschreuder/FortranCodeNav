using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace VSIntegration
{
    public static class Native
    {
        #region Delegates

        public delegate int HookProc(int code, IntPtr wParam, IntPtr lParam);

        #endregion

        public const int WS_EX_TOOLWINDOW = 0x00000080;
        public const int WM_MOUSEACTIVATE = 0x0021;
        public const uint MA_NOACTIVATE = 3;
        public const uint MA_NOACTIVATEANDEAT = 4;
        public const int WM_ACTIVATE = 6;
        public const int WA_INACTIVE = 0;
        public const int WM_FOCUS = 7;
        public const int WS_EX_NOACTIVATE = 0x08000000;
        public const int WS_EX_TOPMOST = 0x00000008;
        public const uint TRANSITION_KEY_DOWN = 0;
        public const uint TRANSITION_KEY_UP = 2147483648;
        public const int WH_KEYBOARD = 2;
        public const int WH_MOUSE = 7;
        public const int HC_ACTION = 0;
        public const int VK_UP = 0x26;
        public const int VK_LEFT = 0x25;
        public const int VK_RIGHT = 0x27;
        public const int VK_RETURN = 0x0D;
        public const int VK_ESCAPE = 0x1B;
        public const int VK_TAB = 0x09;
        public const int VK_DOWN = 0x28;
        public const int VK_SPACE = 0x20;
        public const int VK_BACK = 0x08;
        public const int KEY_DOWN = 0x40000000;
        public const int WM_LBUTTONDOWN = 0x0201;
        public const int WM_RBUTTONDOWN = 0x0204;
        public const int WM_MBUTTONDOWN = 0x0207;
        public const int WM_NCLBUTTONDOWN = 0x0A1;
        public const int WM_NCRBUTTONDOWN = 0x0A4;
        public const int WM_NCMBUTTONDOWN = 0x0A7;
        public const int WM_MOUSEWHEEL = 0x020A;

        [DllImport("user32.dll")]
        public static extern IntPtr SetActiveWindow(IntPtr handle);

        [DllImport("User32.Dll", EntryPoint = "PostMessageA", SetLastError = true)]
        public static extern bool PostMessage(IntPtr hWnd, uint msg, int wParam, int lParam);

        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowsHookEx(int code, HookProc func, IntPtr hInstance, uint threadID);

        [DllImport("user32.dll")]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        public static extern int CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "GetGUIThreadInfo")]
        public static extern bool GetGUIThreadInfo(uint tId, out GUITHREADINFO threadInfo);

        [DllImport("user32.dll", EntryPoint = "ClientToScreen")]
        public static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

        #region Nested type: GUITHREADINFO

        [StructLayout(LayoutKind.Sequential)]
        public struct GUITHREADINFO
        {
            public uint cbSize;
            public uint flags;
            public IntPtr hwndActive;
            public IntPtr hwndFocus;
            public IntPtr hwndCapture;
            public IntPtr hwndMenuOwner;
            public IntPtr hwndMoveSize;
            public IntPtr hwndCaret;
            public RECT rcCaret;
        } ;

        #endregion

        #region Nested type: MouseHookStruct

        [StructLayout(LayoutKind.Sequential)]
        public class MouseHookStruct
        {
            public POINT pt;
            public int hwnd;
            public int wHitTestCode;
            public int dwExtraInfo;
        }

        #endregion

        #region Nested type: MouseHookStructEx

        [StructLayout(LayoutKind.Sequential)]
        public class MouseHookStructEx
        {
            public MouseHookStruct MouseHookStruct;
            public int mouseWheelDelta;
        }

        #endregion

        #region Nested type: POINT

        [StructLayout(LayoutKind.Sequential)]
        public class POINT
        {
            public int x;
            public int y;
        }

        #endregion

        #region Nested type: RECT

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public uint Left;
            public uint Top;
            public uint Right;
            public uint Bottom;
        } ;

        #endregion
    }
}