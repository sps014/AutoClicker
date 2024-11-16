using System;
using System.Linq;
using System.Text.Json.Serialization;
using AutoClicker.ViewModels;
using SharpHook.Native;

namespace AutoClicker.Models;

public record ManualClickItem
{
    public int X { get; set; }
    public int Y { get; set; }
    public ManualOperationMode OperationMode { get; set; }
    public int Delay { get; set; }
    public string Comment { get; set; } = string.Empty;

    public bool KeyCodeFieldEnabled => OperationMode == ManualOperationMode.KeyCode;
    public string[] KeyCodes { get; set; } = [];

    [JsonIgnore]
    public KeyCode KeyCode
    {
        get
        {
            KeyCode c=KeyCode.VcUndefined;

            foreach(var key in KeyCodes)
            {
                if (Enum.TryParse<KeyCode>("Vc" + key, out KeyCode code))
                    c |= code;
            }
            return c;
        }
    }

    [JsonIgnore]
    public string ReadableKeyName
    {
        get
        {
            if (KeyCode == KeyCode.VcUndefined)
                return "-";

            return string.Join("+", KeyCodes.Where(x=>x!= "Undefined"));
        }
    }


    [JsonIgnore]
    public string[] KeyNames => GetAllKeyNames;

    public static string[] GetAllKeyNames = Enum.GetNames(typeof(KeyCode)).Select(x => x.TrimStart('V').TrimStart('c')).ToArray();
}

public enum ManualOperationMode
{
    Left, Right, KeyCode
}