// <copyright file="App.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace IntVue
{
    using IntVue.Services;
    using IntVue.ViewModels;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;

    /// <summary>
    /// Application entry point and DI configuration.
    /// </summary>
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
        /// Configure dependency injection services.
        /// </summary>
        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Services
            services.AddSingleton<IMediaCaptureService, MediaCaptureService>();

            // ViewModels
            services.AddTransient<MainViewModel>();

            return services.BuildServiceProvider();
        }
    }
}
