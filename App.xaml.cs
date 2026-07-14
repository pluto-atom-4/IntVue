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

    /// <summary>
    /// Gets the questions directory path from CLI arguments (--questions-dir).
    /// </summary>
    public static string? QuestionsDirectory { get; private set; }

    /// <inheritdoc/>
    protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        // Parse CLI arguments and configure feature flags
        var featureFlagService = Services.GetRequiredService<IFeatureFlagService>();
        string cliArgs = string.Join(" ", Environment.GetCommandLineArgs());
        Debug.WriteLine($"[App.OnLaunched] Full CLI args: {cliArgs}");

        (bool productReviewEnabled, string? questionsDir) = ParseCliArgs(cliArgs);
        Debug.WriteLine($"[App.OnLaunched] Parsed: productReviewEnabled={productReviewEnabled}, questionsDir='{questionsDir}'");

        await featureFlagService.SetProductReviewEnabled(productReviewEnabled);
        QuestionsDirectory = questionsDir;
        Debug.WriteLine($"[App.OnLaunched] App.QuestionsDirectory set to: '{QuestionsDirectory}'");

        this._window = new MainWindow();
        this._window.Activate();
    }

    /// <summary>
    /// Parses command-line arguments to extract feature flags and options.
    /// Uses Environment.GetCommandLineArgs() array directly for better parsing.
    /// </summary>
    /// <returns>Tuple: (productReviewEnabled, questionsDirectory).</returns>
    private static (bool productReviewEnabled, string? questionsDirectory) ParseCliArgs(string args)
    {
        // Use the raw argument array for more reliable parsing
        var cmdArgs = Environment.GetCommandLineArgs();

        bool productReviewEnabled = false;
        string? questionsDir = null;

        for (int i = 0; i < cmdArgs.Length; i++)
        {
            string arg = cmdArgs[i];

            // Check for feature flag
            if (arg.Equals("--feature:product-review", StringComparison.OrdinalIgnoreCase))
            {
                productReviewEnabled = true;
            }

            // Check for questions directory flag
            if (arg.StartsWith("--questions-dir=", StringComparison.OrdinalIgnoreCase))
            {
                // Extract the part after "--questions-dir="
                questionsDir = arg.Substring("--questions-dir=".Length);

                // Remove surrounding quotes if present
                if ((questionsDir.StartsWith('"') && questionsDir.EndsWith('"')) ||
                    (questionsDir.StartsWith('\'') && questionsDir.EndsWith('\'')))
                {
                    questionsDir = questionsDir.Substring(1, questionsDir.Length - 2);
                }

                Debug.WriteLine($"[ParseCliArgs] Extracted questions directory: '{questionsDir}'");
            }
        }

        Debug.WriteLine($"[ParseCliArgs] Result: productReviewEnabled={productReviewEnabled}, questionsDir='{questionsDir}'");
        return (productReviewEnabled, questionsDir);
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
