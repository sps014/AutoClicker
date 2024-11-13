using AutoClicker.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SukiUI.Controls;

namespace AutoClicker;

public partial class WinAppDriverWindow : SukiWindow
{
    WinAppDriverWindowViewModel viewModel;
    public WinAppDriverWindow()
    {
        InitializeComponent();
        viewModel = new WinAppDriverWindowViewModel();
        DataContext = viewModel;
    }
}