// <copyright file="MainPage.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace IntVue.Views
{
    using IntVue.Services;
    using IntVue.ViewModels;

    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

    // To learn more about WinUI, the WinUI project structure,
    // and more about our project templates, see: http://aka.ms/winui-project-info.

    /// <summary>
    /// The main content page displayed inside the application window.
    /// Add your UI logic, event handlers, and data binding here.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly InterviewViewModel viewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainPage"/> class.
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();

            var svc = (IMediaCaptureService?)App.Services.GetService(typeof(IMediaCaptureService));
            this.viewModel = (InterviewViewModel?)App.Services.GetService(typeof(InterviewViewModel)) ?? new InterviewViewModel(svc!);
            this.DataContext = this.viewModel;
        }

        private async void BtnStartPreview_Click(object sender, RoutedEventArgs e)
        {
            await this.viewModel.StartPreviewAsync(this.PreviewControl).ConfigureAwait(false);
        }

        private async void BtnStartRecording_Click(object sender, RoutedEventArgs e)
        {
            var sanitized = "recording";
            await this.viewModel.StartRecordingAsync(sanitized).ConfigureAwait(false);
        }

        private async void BtnStopRecording_Click(object sender, RoutedEventArgs e)
        {
            await this.viewModel.StopRecordingAsync().ConfigureAwait(false);
        }

        // InitializeComponent is provided by generated XAML code (MainPage.g.i.cs)
    }
}
