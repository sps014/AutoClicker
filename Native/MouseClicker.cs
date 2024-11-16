using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpHook;
using SharpHook.Native;

namespace AutoClicker.Native;

public static class MouseClicker
{
    //Get screen size of
    static EventSimulator simulator = new EventSimulator();

    public static void LeftClick(short x, short y)
    {
        simulator.SimulateMouseMovement(x, y);
        simulator.SimulateMousePress(MouseButton.Button1);
        simulator.SimulateMouseRelease(MouseButton.Button1);
    }

    public static void RightClick(short x, short y)
    {
        simulator.SimulateMouseMovement(x, y);
        simulator.SimulateMousePress(MouseButton.Button2);
        simulator.SimulateMouseRelease(MouseButton.Button2);
    }

    public static void PressKey(KeyCode keyCode)
    {
        simulator.SimulateKeyPress(keyCode);
        simulator.SimulateKeyRelease(keyCode);
    }

}

