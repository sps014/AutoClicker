using AutoClicker.ViewModels;
using Avalonia.Controls;
using SukiUI.Controls;

namespace AutoClicker;

public partial class MainWindow : SukiWindow
{
    MainWindowViewModel viewModel;
    public MainWindow()
    {

        InitializeComponent();
        viewModel = new MainWindowViewModel(this);
        DataContext = viewModel;
    }
}