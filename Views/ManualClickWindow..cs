using AutoClicker.ViewModels;
using Avalonia.Controls;
using SukiUI.Controls;

namespace AutoClicker.Views;

public partial class ManualClickWindow : SukiWindow
{
    ManualClickViewModel viewModel;
    public ManualClickWindow()
    {
        InitializeComponent();
        viewModel = new ManualClickViewModel();
        DataContext = viewModel;
    }
}