using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using AutoClicker.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoClicker.ViewModels;

public partial class MainWindowViewModel:ObservableObject
{
    private MainWindow MainWindow;

    [ObservableProperty]
    private bool isWinAppDriverOpenedOnNonWindow = false;

    [ObservableProperty]
    private bool showLink = false;

    [ObservableProperty]
    private string message = "WinAppDriver is supported on windows Only";
    public MainWindowViewModel(MainWindow mainWindow)
    {
        MainWindow = mainWindow;
    }

    [RelayCommand]
    public void GoToWinAppDriver()
    {

        if(!OperatingSystem.IsWindows())
        {
            IsWinAppDriverOpenedOnNonWindow = true;
            return;
        }
        else if(!IsWinAppDriverInstalled())
        {
            IsWinAppDriverOpenedOnNonWindow = true;
            Message = "WinAppDriver is not installed, download from link below";
            ShowLink = true;
            return;
        }

        WinAppDriverWindow mcw = new();
        mcw.Show();
        MainWindow.Close();
    }

    bool IsWinAppDriverInstalled()
    {
        return File.Exists(AppConstants.WinAppDriverPath);
    }

    [RelayCommand]
    public void GoToSemiManual()
    {
        ManualClickWindow mcw = new ManualClickWindow();
        mcw.Show();
        MainWindow.Close();
    }

    [RelayCommand]
    public void OpenUrl()
    {
        var url = "https://github.com/microsoft/WinAppDriver/releases/download/v1.2.99/WindowsApplicationDriver-1.2.99-win-x64.exe";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            //https://stackoverflow.com/a/2796367/241446
            using var proc = new Process { StartInfo = { UseShellExecute = true, FileName = url } };
            proc.Start();

            return;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Process.Start("x-www-browser", url);
            return;
        }

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) throw new ArgumentException("invalid url: " + url);
        Process.Start("open", url);
        return;
    }
}
