using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AutoClicker.Native;

internal class WindowHelper
{
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    public static void BringToFront(IntPtr handle)
    {
        SetForegroundWindow(handle);
    }
}
