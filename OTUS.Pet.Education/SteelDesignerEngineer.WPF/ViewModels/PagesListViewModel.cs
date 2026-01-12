using System.Collections.ObjectModel;
using SteelDesignerEngineer.WPF.Models;
using SteelDesignerEngineer.WPF.Services;
using SteelDesignerEngineer.WPF.Commands;
using System.Windows.Input;

namespace SteelDesignerEngineer.WPF.ViewModels;

/// <summary>
/// ViewModel для списка HTML страниц
/// </summary>
public class PagesListViewModel : ViewModelBase
{
    private readonly IApiService _apiService;

    private ObservableCollection<HtmlPageModel> _pages = new();
    public ObservableCollection<HtmlPageModel> Pages
    {
        get => _pages;
        set => SetProperty(ref _pages, value);
    }

    private HtmlPageModel? _selectedPage;
    public HtmlPageModel? SelectedPage
    {
        get => _selectedPage;
        set => SetProperty(ref _selectedPage, value);
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public ICommand LoadPagesCommand { get; }
    public ICommand RefreshCommand { get; }

    public PagesListViewModel(IApiService apiService)
    {
        _apiService = apiService;
        LoadPagesCommand = new RelayCommand(async () => await LoadPagesAsync());
        RefreshCommand = new RelayCommand(async () => await LoadPagesAsync());
    }

    public async Task LoadPagesAsync()
    {
        try
        {
            IsLoading = true;
            var pages = await _apiService.GetPagesAsync();
            Pages = new ObservableCollection<HtmlPageModel>(pages);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading pages: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
}
