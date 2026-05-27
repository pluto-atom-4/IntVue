using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using IntVue.Services;
using IntVue.ViewModels;

namespace IntVue;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    // Backing field for the main application window
    private Window? _window;

    public App()
    {
        InitializeComponent();
        Services = ConfigureServices();
    }

    // InitializeComponent is provided by generated XAML code (App.g.i.cs)

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Services
        services.AddSingleton<IMediaCaptureService, MediaCaptureService>();
        // navigation, data services, etc:
        // services.AddSingleton<INavigationService, NavigationService>();

        // ViewModels
        services.AddTransient<MainViewModel>();

        return services.BuildServiceProvider();
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();
        _window.Activate();
    }
}
