using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AutoClicker.Native;

public static class MouseClickDetection
{
    private const int WH_MOUSE_LL = 14;
    private const int WM_LBUTTONDOWN = 0x0201;
    private static LowLevelMouseProc _proc = HookCallback;
    private static IntPtr _hookID = IntPtr.Zero;

    public delegate void OnMouseClickCapturedHandler(bool isLeftClick,Point clickPosition);
    public static event OnMouseClickCapturedHandler OnMouseClickCaptured;

    public static void SetHook()
    {
        _hookID = SetHook(_proc);
        startHookTime = DateTime.Now;
    }
    public static void UnsetHook()
    {
        UnhookWindowsHookEx(_hookID);
    }

    private static IntPtr SetHook(LowLevelMouseProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
    private static DateTime startHookTime;
    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {

        if (nCode >= 0 && wParam == (IntPtr)WM_LBUTTONDOWN)
        {
            MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
            if (!IsMouseClickInApp(hookStruct.pt))
            {
                OnMouseClickCaptured?.Invoke(true,new Point(hookStruct.pt.x,hookStruct.pt.y));
            }
            else
            {
                OnMouseClickCaptured?.Invoke(false,Point.Empty);
            }
        }
        else
        {
            OnMouseClickCaptured?.Invoke(false,Point.Empty);
        }

        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    private static bool IsMouseClickInApp(POINT pt)
    {
        IntPtr hWnd = Process.GetCurrentProcess().MainWindowHandle;
        RECT rect;
        GetWindowRect(hWnd, out rect);
        return pt.x >= rect.Left && pt.x <= rect.Right && pt.y >= rect.Top && pt.y <= rect.Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        public uint mouseData;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
}
