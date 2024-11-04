using AutoClicker.Native;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
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

    [ObservableProperty]
    private Point clickPosition = Point.Empty;

    [ObservableProperty]
    private string startCaptureButtonText = "Pick Point";

    [ObservableProperty]
    private CaptureMode captureMode = CaptureMode.Off;

    [ObservableProperty]
    private int delay = 100;

    [ObservableProperty]
    private ManualOperationMode manualOperationMode=ManualOperationMode.Left;

    public string StartBtnLabel => IsTaskRunning ? "Stop" : "Start";

    [NotifyPropertyChangedFor(nameof(StartBtnLabel))]
    [ObservableProperty]
    private bool isTaskRunning;


    [NotifyPropertyChangedFor(nameof(StepPercent))]
    [NotifyPropertyChangedFor(nameof(StepsDoneText))]
    [ObservableProperty]
    private int stepsDone;

    public float StepPercent => StepsDone * 100.0f / RepeatCount;

    public string StepsDoneText => $"{StepsDone}/{RepeatCount}";


    public ManualClickViewModel()
    {
        MouseClickDetection.OnMouseClickCaptured += MouseClickDetectionOnMouseClickCaptured;
    }

    ~ManualClickViewModel()
    {
        MouseClickDetection.OnMouseClickCaptured -= MouseClickDetectionOnMouseClickCaptured;
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

        var window = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        WindowHelper.BringToFront(window.TryGetPlatformHandle().Handle);
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
    public async Task StartOperation()
    {
        if(IsTaskRunning)
        {
            cancellationTokenSource?.Cancel();
            IsTaskRunning = false;
            StepsDone = 0;
            return;
        }

        cancellationTokenSource = new CancellationTokenSource();

        await Task.Run(async() =>
        {
            IsTaskRunning = true;
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
                StepsDone = time;
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