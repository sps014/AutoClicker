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

    public delegate void OnMouseClickCapturedHandler(MouseHookEventArgs args, Point clickPosition);
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

        if (IsMouseClickInApp(point))
            return;

        OnMouseClickCaptured?.Invoke(args, point);
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

        var rect = Window.Bounds; // size of window
        rect = new Avalonia.Rect(Window.Position.X,Window.Position.Y, rect.Width, rect.Height);// add current position
        return pt.X >= rect.Left && pt.X <= rect.Right && pt.Y >= rect.Top && pt.Y <= rect.Bottom;
    }
}
