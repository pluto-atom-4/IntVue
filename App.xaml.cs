// Copyright (c) YourProjectName. All rights reserved.

using System;
using System.Diagnostics;
using System.Runtime.Versioning;

using IntVue.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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
    private Window? _window;

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
    protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        // Parse CLI arguments and configure feature flags
        var featureFlagService = Services.GetRequiredService<IFeatureFlagService>();
        bool productReviewEnabled = ParseCliArgs(args.Arguments ?? string.Empty);
        await featureFlagService.SetProductReviewEnabled(productReviewEnabled);

        this._window = new MainWindow();
        this._window.Activate();
    }

    /// <summary>
    /// Parses command-line arguments to extract feature flags.
    /// </summary>
    /// <param name="args">Command-line arguments string.</param>
    /// <returns>True if Product Review feature should be enabled; false otherwise.</returns>
    private static bool ParseCliArgs(string args)
    {
        if (string.IsNullOrWhiteSpace(args))
        {
            return false; // Default: feature disabled
        }

        if (args.Contains("--feature:product-review", StringComparison.OrdinalIgnoreCase))
        {
            return true; // Feature flag found: enabled
        }

        // Unrecognized flags detected - log warning
        if (!string.Equals(args, string.Empty, StringComparison.Ordinal))
        {
            Debug.WriteLine("Warning: Unrecognized CLI arguments detected. Supported flags: --feature:product-review");
        }

        return false;
    }

    /// <summary>
    /// Configure dependency injection services.
    /// </summary>
    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ICountdownService, CountdownService>();
        services.AddSingleton<IProductReviewService, ProductReviewService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IPlaylistService, PlaylistService>();
        services.AddSingleton<IFeatureFlagService, FeatureFlagService>();
        services.AddTransient<IntVue.ViewModels.ProductReviewViewModel>();
        return services.BuildServiceProvider();
    }
}
