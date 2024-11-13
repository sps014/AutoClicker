using AutoClicker.ViewModels;
using Avalonia.Controls;
using SukiUI.Controls;

namespace AutoClicker.Views;

public partial class ManualClickWindow : SukiWindow
{
    ManualClickViewModel viewModel;
    public ManualClickWindow()
    {
        viewModel = new ManualClickViewModel(this);
        DataContext = viewModel;
        InitializeComponent();
    }

    private void Delete_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var item = (sender as MenuItem)!.DataContext as ManualClickItem;
        viewModel.Delete(item!);
    }
    private void DeleteAll_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        viewModel.DeleteAll();
    }

    private void MoveUp_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var item = (sender as MenuItem)!.DataContext as ManualClickItem;
        viewModel.MoveUp(item!);
    }

    private void MoveDown_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var item = (sender as MenuItem)!.DataContext as ManualClickItem;
        viewModel.MoveDown(item!);
    }
}