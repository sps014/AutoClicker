using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using SharpHook;
using SharpHook.Native;

namespace AutoClicker.Native;

public static class MouseClickDetection
{
    private const int WH_MOUSE_LL = 14;
    private const int WM_LBUTTONDOWN = 0x0201;
    private static IntPtr _hookID = IntPtr.Zero;

    public delegate void OnMouseClickCapturedHandler(bool isLeftClick, Point clickPosition);
    public static event OnMouseClickCapturedHandler? OnMouseClickCaptured;
    public static bool Enabled { get; private set; } = false;
    public static Window? Window { get; private set; }

    public static void Init(Window window)
    {
        Window = window;
        GlobalKeyManager.hook.MouseClicked.Subscribe(OnMouseClicked);
    }

    private static void OnMouseClicked(MouseHookEventArgs args)
    {
        if (!Enabled)
            return;

        var point = new Point(args.RawEvent.Mouse.X, args.RawEvent.Mouse.Y);

        bool isLeftButtonOutsideApp = args.Data.Button == MouseButton.Button1 || !IsMouseClickInApp(point);

        OnMouseClickCaptured?.Invoke(isLeftButtonOutsideApp, isLeftButtonOutsideApp ? point : Point.Empty);
    }

    public static void SetHook()
    {
        Enabled = true;
    }
    public static void UnsetHook()
    {
        Enabled = false;
    }

    private static bool IsMouseClickInApp(Point pt)
    {
        if (Window == null)
            return false;

        var rect = Window.Bounds;
        return pt.X >= rect.Left && pt.X <= rect.Right && pt.Y >= rect.Top && pt.Y <= rect.Bottom;
    }
}
