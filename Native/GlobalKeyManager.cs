using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using SharpHook;
using SharpHook.Reactive;

namespace AutoClicker.Native;

public static class GlobalKeyManager
{
    static SimpleReactiveGlobalHook hook= new SimpleReactiveGlobalHook();

    public static void Init()
    {
        hook.KeyPressed.Subscribe(OnKeyPressed);
        Task.Run(() =>
        {
            hook.Run();
        });

    }

    public static EventHandler<KeyboardHookEventArgs>? KeyPressed;

    private static void OnKeyPressed(KeyboardHookEventArgs args)
    {
        KeyPressed?.Invoke(null,args);
    }
}
