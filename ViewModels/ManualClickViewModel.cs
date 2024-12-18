﻿using AutoClicker.Models;
using AutoClicker.Native;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharpHook;
using SharpHook.Native;
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
using static AutoClicker.Models.ManualClickItem;
using Point = System.Drawing.Point;

namespace AutoClicker.ViewModels;

public partial class ManualClickViewModel : ObservableObject
{
    private CancellationTokenSource? cancellationTokenSource;

    [NotifyPropertyChangedFor(nameof(StepsDoneText))]
    [ObservableProperty]
    private int repeatCount = 1;

    [ObservableProperty]
    ObservableCollection<ManualClickItem> manualClickItems = new();

    [ObservableProperty]
    private int totalSteps;

    [JsonIgnore]
    [ObservableProperty]
    private Point clickPosition = Point.Empty;

    public Brush StartStopButtonColor => new SolidColorBrush(IsTaskRunning ? Colors.Red : Colors.Green);

    [JsonIgnore]
    public string StartCaptureButtonText => CaptureMode == CaptureMode.Off ? "Pick Point" : "Stop Picking Point";

    [JsonIgnore]
    [NotifyPropertyChangedFor(nameof(StartCaptureButtonText))]
    [ObservableProperty]
    private CaptureMode captureMode = CaptureMode.Off;

    [ObservableProperty]
    private int delay = 100;

    [NotifyPropertyChangedFor(nameof(IsKeyComboVisible))]
    [ObservableProperty]
    private ManualOperationMode manualOperationMode = ManualOperationMode.Left;

    public string StartStopRecordingText => IsRecordingMouseClicks ? "Stop Recording (F6)" : "Start Recording (F6)";
    public Brush StartStopRecordingColor => new SolidColorBrush(IsRecordingMouseClicks ? Colors.DeepPink : Colors.DodgerBlue);

    [NotifyPropertyChangedFor(nameof(StartStopRecordingColor))]
    [NotifyPropertyChangedFor(nameof(StartStopRecordingText))]
    [ObservableProperty]
    private bool isRecordingMouseClicks = false;

    [JsonIgnore]
    public string StartBtnLabel => IsTaskRunning ? "Stop (F4)" : "Run (F4)";


    [JsonIgnore]
    [NotifyPropertyChangedFor(nameof(StartBtnLabel))]
    [NotifyPropertyChangedFor(nameof(StartStopButtonColor))]
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

    public bool IsKeyComboVisible=> ManualOperationMode == ManualOperationMode.KeyCode;

    [ObservableProperty]
    private string selectedKey=KeyCode.VcUndefined.ToString().TrimStart('V').TrimStart('c');

    public string[] KeyNames => ManualClickItem.GetAllKeyNames;
    public Window Window { get; }

    private Stopwatch stopwatch = new();

    public ManualClickViewModel(Window window)
    {
        GlobalKeyManager.Init();
        MouseClickDetection.Init(window);
        MouseClickDetection.OnMouseClickCaptured += MouseClickDetectionOnMouseClickCaptured;
        GlobalKeyManager.KeyPressed += KeyPressed;
        ManualClickItems.CollectionChanged += ManualClickItemsCollectionChanged;
        Window = window;
    }

    private void ManualClickItemsCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        TotalSteps = ManualClickItems.Count;
    }

    ~ManualClickViewModel()
    {
        MouseClickDetection.OnMouseClickCaptured -= MouseClickDetectionOnMouseClickCaptured;
        GlobalKeyManager.KeyPressed -= KeyPressed;
    }

    private void KeyPressed(object? sender, KeyboardHookEventArgs e)
    {
        if (e.Data.KeyCode == SharpHook.Native.KeyCode.VcF4)
        {
            StartOperation();
        }
        if (e.Data.KeyCode == SharpHook.Native.KeyCode.VcF6)
        {
            RecordingMousePositions();
        }

        if(IsRecordingMouseClicks)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                var time = stopwatch.ElapsedMilliseconds;

                ManualClickItems.Add(new ManualClickItem()
                {
                    Comment = $"Step {ManualClickItems.Count + 1}",
                    Delay = (int)time,
                    OperationMode =ManualOperationMode.KeyCode,
                    X = ClickPosition.X,
                    Y = ClickPosition.Y,
                    KeyCodes = GetCurrentEnumNames(e.Data.KeyCode).Select(x=>x.TrimStart('V').TrimStart('c')).ToArray()
                });
                stopwatch.Restart();
            });
        }
    }

    public static List<string> GetCurrentEnumNames(KeyCode currentEnum)
    {
        return Enum.GetValues(currentEnum.GetType())
            .Cast<Enum>().Where(currentEnum.HasFlag)
            .Select(e => e.ToString()).ToList();
    }

    private void MouseClickDetectionOnMouseClickCaptured(MouseHookEventArgs args, Point clickPosition)
    {
        var isLeftClick = args.Data.Button == SharpHook.Native.MouseButton.Button1;

        if (IsRecordingMouseClicks)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                var time = stopwatch.ElapsedMilliseconds;
                ManualClickItems.Add(new ManualClickItem()
                {
                    Comment = $"Step {ManualClickItems.Count+1}",
                    Delay = (int)time,
                    OperationMode = isLeftClick?ManualOperationMode.Left:ManualOperationMode.Right,
                    X = clickPosition.X,
                    Y = clickPosition.Y,
                });
                stopwatch.Restart();
            });
            return;
        }


        if (!isLeftClick)
        {
            return;
        }


        //in capture single point mode
        MouseClickDetection.UnsetHook();
        this.ClickPosition = clickPosition;
        CaptureMode = CaptureMode.Off;
        Window.Activate();
    }

    public void MoveUp(ManualClickItem item)
    {
        var index = ManualClickItems.IndexOf(item);

        if (index <= 0)
            return;

        ManualClickItems.RemoveAt(index);
        ManualClickItems.Insert(index - 1, item);

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
        //stop recording mouse clicks now 
        if(IsRecordingMouseClicks)
        {
            RecordingMousePositions();
        }

        if (IsTaskRunning)
        {
            cancellationTokenSource?.Cancel();
            IsTaskRunning = false;
            StepsDone = 0;
            return;
        }

        cancellationTokenSource = new CancellationTokenSource();

        Task.Run(async () =>
        {
            IsTaskRunning = true;
            int ct = 1;
            foreach (var time in Enumerable.Range(0, RepeatCount))
            {
                foreach (var step in ManualClickItems)
                {
                    if (cancellationTokenSource.IsCancellationRequested)
                        return;

                    await Task.Delay(step.Delay, cancellationTokenSource.Token);

                    if (cancellationTokenSource.IsCancellationRequested)
                        return;

                    if (step.OperationMode == ManualOperationMode.Left)
                        MouseClicker.LeftClick((short)step.X, (short)step.Y);
                    else if (step.OperationMode == ManualOperationMode.Right)
                        MouseClicker.RightClick((short)step.X, (short)step.Y);
                    else if (step.OperationMode == ManualOperationMode.KeyCode)
                        MouseClicker.PressKey(step.KeyCode);
                }
                StepsDone = ct++;
            }

            IsTaskRunning = false;
            StepsDone = 0;

        }, cancellationTokenSource.Token);

    }

    [RelayCommand]
    public void RecordingMousePositions()
    {
        if (IsRecordingMouseClicks)
        {
            stopwatch.Stop();
            MouseClickDetection.UnsetHook();
            IsRecordingMouseClicks = false;
            return;
        }

        MouseClickDetection.SetHook();
        IsRecordingMouseClicks = true;
        stopwatch.Start();

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
            ManualDTO.Load(files[0].Path.LocalPath, this);
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
        if (CaptureMode == CaptureMode.On)
        {
            await StartCapturingMouse();
        }


        ManualClickItems.Add(new ManualClickItem()
        {
            Delay = Delay,
            OperationMode = ManualOperationMode,
            X = ClickPosition.X,
            Y = ClickPosition.Y,
            KeyCodes = [SelectedKey]
        });

    }
}


public enum CaptureMode
{
    On,
    Off
}