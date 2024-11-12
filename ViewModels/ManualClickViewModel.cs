using AutoClicker.Models;
using AutoClicker.Native;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharpHook;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using static AutoClicker.ViewModels.ManualClickItem;
using Point = System.Drawing.Point;

namespace AutoClicker.ViewModels;

public partial class ManualClickViewModel:ObservableObject
{
    private CancellationTokenSource? cancellationTokenSource;

    [NotifyPropertyChangedFor(nameof(StepsDoneText))]
    [ObservableProperty]
    private int repeatCount = 1;
    
    [ObservableProperty]
    ObservableCollection<ManualClickItem> manualClickItems = new();

    [JsonIgnore]
    [ObservableProperty]
    private Point clickPosition = Point.Empty;

    [JsonIgnore]
    [ObservableProperty]
    private string startCaptureButtonText = "Pick Point";

    [JsonIgnore]
    [ObservableProperty]
    private CaptureMode captureMode = CaptureMode.Off;

    [ObservableProperty]
    private int delay = 100;

    [ObservableProperty]
    private ManualOperationMode manualOperationMode=ManualOperationMode.Left;

    [JsonIgnore]
    public string StartBtnLabel => IsTaskRunning ? "Stop (F4)" : "Start (F4)";

    [JsonIgnore]
    [NotifyPropertyChangedFor(nameof(StartBtnLabel))]
    [ObservableProperty]
    private bool isTaskRunning;


    [JsonIgnore]
    [NotifyPropertyChangedFor(nameof(StepPercent))]
    [NotifyPropertyChangedFor(nameof(StepsDoneText))]
    [ObservableProperty]
    private int stepsDone;

    [JsonIgnore]
    public float StepPercent => StepsDone * 100.0f / RepeatCount;

    [JsonIgnore]
    public string StepsDoneText => $"{StepsDone}/{RepeatCount}";

    public Window Window { get; }

    public ManualClickViewModel(Window window)
    {
        GlobalKeyManager.Init();
        MouseClickDetection.OnMouseClickCaptured += MouseClickDetectionOnMouseClickCaptured;
        GlobalKeyManager.KeyPressed += KeyPressed;
        Window = window;
    }


    ~ManualClickViewModel()
    {
        MouseClickDetection.OnMouseClickCaptured -= MouseClickDetectionOnMouseClickCaptured;
        GlobalKeyManager.KeyPressed -= KeyPressed;
    }

    private void KeyPressed(object? sender, KeyboardHookEventArgs e)
    {
        if(e.Data.KeyCode==SharpHook.Native.KeyCode.VcF4)
        {
            StartOperation();
        }
    }

    private void MouseClickDetectionOnMouseClickCaptured(bool isLeftClick,Point clickPosition)
    {

        if (!isLeftClick)
        {
            return;
        }

        MouseClickDetection.UnsetHook();

        StartCaptureButtonText = "Pick Point";
        this.ClickPosition = clickPosition;
        CaptureMode = CaptureMode.Off;
        WindowHelper.BringToFront(Window!.TryGetPlatformHandle()!.Handle);
    }

    public void MoveUp(ManualClickItem item)
    {
        var index = ManualClickItems.IndexOf(item);

        if(index <=0)
            return;

        ManualClickItems.RemoveAt(index);
        ManualClickItems.Insert(index-1,item);

    }
    public void MoveDown(ManualClickItem item)
    {
        var index = ManualClickItems.IndexOf(item);

        if (index < 0 || index >= ManualClickItems.Count - 1)
            return;

        ManualClickItems.RemoveAt(index);
        ManualClickItems.Insert(index + 1, item);
    }

    [RelayCommand]
    public void StartOperation()
    {
        if(IsTaskRunning)
        {
            cancellationTokenSource?.Cancel();
            IsTaskRunning = false;
            StepsDone = 0;
            return;
        }

        cancellationTokenSource = new CancellationTokenSource();

        Task.Run(async() =>
        {
            IsTaskRunning = true;
            int ct = 1;
            foreach(var time in Enumerable.Range(0,RepeatCount))
            {
                foreach(var step in ManualClickItems)
                {
                    if (cancellationTokenSource.IsCancellationRequested)
                        return;

                    await Task.Delay(step.Delay,cancellationTokenSource.Token);

                    if (cancellationTokenSource.IsCancellationRequested)
                        return;

                    if (step.OperationMode == ManualOperationMode.Left)
                        MouseClicker.LeftClick((uint)step.X, (uint)step.Y);
                    else if (step.OperationMode == ManualOperationMode.Right)
                        MouseClicker.RightClick((uint)step.X, (uint)step.Y);
                }
                StepsDone = ct++;
            }

            IsTaskRunning = false;
            StepsDone = 0;

        },cancellationTokenSource.Token);

    }

    [RelayCommand]
    public void Delete(ManualClickItem item)
    {
        ManualClickItems.Remove(item);
    }

    [RelayCommand]
    public void DeleteAll()
    {
        ManualClickItems.Clear();
    }

    [RelayCommand]
    public async Task Load()
    {
        var type = new FilePickerFileType("Auto Clicker");
        type.MimeTypes = ["application/json"];
        type.Patterns = ["*.acr"];

        // Start async operation to open the dialog.
        var files = await Window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Steps File",
            AllowMultiple = false,
            FileTypeFilter = [type]
        });

        if (files.Count >= 1)
        {
            ManualDTO.Load(files[0].Path.LocalPath,this);
        }
    }

    [RelayCommand]
    public async Task Save()
    {
        var type = new FilePickerFileType("Auto Clicker");
        type.MimeTypes = ["application/json"];
        type.Patterns = ["*.acr"];


        // Start async operation to open the dialog.
        var files = await Window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Steps File",
            FileTypeChoices = [type]
        });

        if (files == null)
            return;

        if (!string.IsNullOrEmpty(files?.Path.ToString()))
        {
            ManualDTO.Save(files!.Path.LocalPath, this);
        }
    }



    [RelayCommand]
    public async Task StartCapturingMouse()
    {
        StartCaptureButtonText = CaptureMode==CaptureMode.On?"Pick Point": "Stop Picking Point";
        await Task.Delay(100);

        if (CaptureMode == CaptureMode.Off)
        {
            MouseClickDetection.SetHook();
            CaptureMode = CaptureMode.On;
        }
        else
        {
            MouseClickDetection.UnsetHook();
            CaptureMode = CaptureMode.Off;
        }
    }
    [RelayCommand]
    public async Task AddCapturedPoint()
    {
        //stop the capturing if its in progress
        if(CaptureMode==CaptureMode.On)
        {
            await StartCapturingMouse();
        }

        ManualClickItems.Add(new ManualClickItem()
        {
            Delay=Delay,
            OperationMode=ManualOperationMode,
            X = ClickPosition.X,
            Y = ClickPosition.Y,
        });
    }
}
public record ManualClickItem
{
    public int X  { get; set; }
    public int Y { get; set; }
    public ManualOperationMode OperationMode { get; set; }
    public int Delay { get; set; }
    public string Comment { get; set; } = string.Empty;
}
public enum ManualOperationMode
{
    Left, Right, Other
}

public enum CaptureMode
{
    On,
    Off
}