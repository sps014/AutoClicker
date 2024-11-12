using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AutoClicker.Native;

public static class MouseClicker
{
    //Get screen size of x or y of primary display
    [DllImport("user32.dll")]
    public static extern int GetSystemMetrics(int nIndex);

    // Import the mouse_event function from user32.dll
    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

    // Constants for mouse events
    private const uint MOUSEEVENTF_MOVE = 0x0001;
    private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    private const uint MOUSEEVENTF_LEFTUP = 0x0004;
    private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
    private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
    static uint MOUSEEVENTF_ABSOLUTE = 0x8000;


    public static void LeftClick(double x, double y)
    {
        var screenWidth = GetSystemMetrics(0);
        // SM_CXSCREEN
        var screenHeight = GetSystemMetrics(1); // SM_CYSCREEN 
        
        // Normalize coordinates to the range of 0 to 65535
        uint normalizedX = (uint)((x / screenWidth) * 65535);
        uint normalizedY = (uint)((y / screenHeight) * 65535);

        // Move the mouse to the specified coordinates
        mouse_event(MOUSEEVENTF_MOVE| MOUSEEVENTF_ABSOLUTE, normalizedX, normalizedY, 0, 0);
        // Simulate left button down and up
        mouse_event(MOUSEEVENTF_LEFTDOWN| MOUSEEVENTF_ABSOLUTE, normalizedX, normalizedY, 0, 0);
        mouse_event(MOUSEEVENTF_LEFTUP | MOUSEEVENTF_ABSOLUTE, normalizedX, normalizedY, 0, 0);
    }

    public static void RightClick(double x, double y)
    {
        var screenWidth = GetSystemMetrics(0);
        // SM_CXSCREEN
        var screenHeight = GetSystemMetrics(1); // SM_CYSCREEN 

        // Normalize coordinates to the range of 0 to 65535
        uint normalizedX = (uint)((x / screenWidth) * 65535);
        uint normalizedY = (uint)((y / screenHeight) * 65535);

        // Move the mouse to the specified coordinates
        mouse_event(MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE, normalizedX, normalizedY, 0, 0);
        // Simulate right button down and up
        mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_ABSOLUTE, normalizedX, normalizedY, 0, 0);
        mouse_event(MOUSEEVENTF_RIGHTUP | MOUSEEVENTF_ABSOLUTE, normalizedX, normalizedY, 0, 0);
    }

}

