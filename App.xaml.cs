// <copyright file="App.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using IntVue.Services;
using IntVue.ViewModels;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace IntVue;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    // Backing field for the main application window
    private Window? window;

    public App()
    {
        this.InitializeComponent();
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

    /// <inheritdoc/>
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        this.window = new MainWindow();
        this.window.Activate();
    }
}
