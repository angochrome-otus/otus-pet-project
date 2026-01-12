using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SteelDesignerEngineer.WPF.Services;
using SteelDesignerEngineer.WPF.ViewModels;
using SteelDesignerEngineer.WPF.Views;

namespace SteelDesignerEngineer.WPF;

/// <summary>
/// WPF приложение для визуального редактирования HTML страниц портала
/// </summary>
public partial class App : Application
{
    public IServiceProvider ServiceProvider { get; private set; } = null!;
    public IConfiguration Configuration { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Настройка конфигурации
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        Configuration = builder.Build();

        // Настройка DI контейнера
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        ServiceProvider = serviceCollection.BuildServiceProvider();

        // Запуск главного окна
        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton(Configuration);

        // HTTP Client with ApiService - автоматически регистрирует IApiService
        services.AddHttpClient<IApiService, ApiService>(client =>
        {
            var baseUrl = Configuration["ApiSettings:BaseUrl"] ?? "https://steel-designer-engineer.ru";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // Services
        services.AddSingleton<IFileService, FileService>();

        // ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<EditorViewModel>();
        services.AddTransient<PagesListViewModel>();

        // Views
        services.AddTransient<MainWindow>();
    }
}
