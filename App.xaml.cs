// Copyright (c) YourProjectName. All rights reserved.

using System;
using System.Diagnostics;
using System.Runtime.Versioning;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace IntVue;

/// <summary>
/// Application entry point. Uses direct code-behind approach for MVP stability.
/// </summary>
[SupportedOSPlatform("windows10.0.17763.0")]
public partial class App : Application
{
    /// <summary>
    /// Backing field for the main application window.
    /// </summary>
    private Window? window;

    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// </summary>
    public App()
    {
        this.InitializeComponent();

#if DEBUG
        // Add ConsoleTraceListener to route trace output to console
        Trace.Listeners.Add(new ConsoleTraceListener());
#endif

        Services = ConfigureServices();
    }

    /// <summary>
    /// Gets the application's <see cref="IServiceProvider"/>.
    /// </summary>
    public static IServiceProvider Services { get; private set; } = null!;

    /// <inheritdoc/>
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        this.window = new MainWindow();
        this.window.Activate();
    }

    /// <summary>
    /// Configure dependency injection services (currently minimal for MVP).
    /// </summary>
    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        return services.BuildServiceProvider();
    }
}
