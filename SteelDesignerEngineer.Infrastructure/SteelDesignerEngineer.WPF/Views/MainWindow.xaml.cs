using System.Windows;
using SteelDesignerEngineer.WPF.ViewModels;

namespace SteelDesignerEngineer.WPF.Views;

/// <summary>
/// Главное окно WPF приложения - браузер для портала
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        
        _viewModel = viewModel;
        DataContext = _viewModel;

        // Привязка WebView к ViewModel
        _viewModel.SetWebView(WebViewBrowser);

        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.InitializeAsync();
    }
}
