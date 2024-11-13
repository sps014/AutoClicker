using System;
using System.Collections.Generic;
using System.Linq;
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

        ManualClickWindow mcw = new ManualClickWindow();
        mcw.Show();
        MainWindow.Close();
    }

    [RelayCommand]
    public void GoToSemiManual()
    {
        ManualClickWindow mcw = new ManualClickWindow();
        mcw.Show();
        MainWindow.Close();
    }
}
