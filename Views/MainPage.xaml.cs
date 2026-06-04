// <copyright file="MainPage.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace IntVue.Views
{
    using System;
    using System.Threading.Tasks;

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

            this.Unloaded += (s, e) => this.OnPageUnloaded();
            this.Loaded += (s, e) => this.OnPageLoaded();
        }

        private void OnPageLoaded()
        {
            this.TxtConsent.Tapped += (_, _) => this.OnConsentTapped();
            this.TxtConsent.PointerReleased += (_, _) => this.OnConsentTapped();
            this.UpdateConsentText();
        }

        private void OnConsentTapped()
        {
            this.viewModel.ConsentGiven = !this.viewModel.ConsentGiven;
            this.UpdateConsentText();
        }

        private void UpdateConsentText()
        {
            if (this.viewModel.ConsentGiven)
            {
                this.TxtConsent.Text = "✓ Consent given. You can now start the preview.";
            }
            else
            {
                this.TxtConsent.Text = "By enabling camera/microphone you consent to local recording. [Tap to consent]";
            }
        }

        private async void OnPageUnloaded()
        {
            if (this.viewModel.IsPreviewing)
            {
                await this.viewModel.StopPreviewAsync().ConfigureAwait(false);
            }
        }

        private async void BtnStartPreview_Click(object sender, RoutedEventArgs e)
        {
            if (!this.viewModel.ConsentGiven)
            {
                await this.ShowErrorDialog("Consent Required", "Please consent to camera and microphone recording before starting the preview.");
                return;
            }

            try
            {
                var success = await this.viewModel.StartPreviewAsync(this.PreviewControl).ConfigureAwait(false);
                if (!success)
                {
                    await this.ShowErrorDialog("Camera Access Denied", "Camera and microphone permissions are required to use the preview feature. Please enable them in your device settings.");
                }
            }
            catch (ArgumentException)
            {
                await this.ShowErrorDialog("Invalid Preview Control", "The preview control is not properly configured.");
            }
            catch (InvalidOperationException ex)
            {
                await this.ShowErrorDialog("Camera Not Available", ex.Message ?? "No camera device found or MediaCapture failed to initialize.");
            }
            catch (Exception ex)
            {
                await this.ShowErrorDialog("Preview Error", $"An unexpected error occurred while starting the preview: {ex.Message}");
            }
        }

        private async Task ShowErrorDialog(string title, string message)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot,
            };

            await dialog.ShowAsync();
        }

        private async void BtnStartRecording_Click(object sender, RoutedEventArgs e)
        {
            if (!this.viewModel.IsPreviewing)
            {
                await this.ShowErrorDialog("Preview Required", "Please start the camera preview before recording.");
                return;
            }

            try
            {
                var sanitized = "recording";
                await this.viewModel.StartRecordingAsync(sanitized).ConfigureAwait(false);
            }
            catch (InvalidOperationException ex)
            {
                await this.ShowErrorDialog("Recording Error", $"Failed to start recording: {ex.Message}");
            }
            catch (Exception ex)
            {
                await this.ShowErrorDialog("Recording Error", $"An unexpected error occurred: {ex.Message}");
            }
        }

        private async void BtnStopRecording_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await this.viewModel.StopRecordingAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await this.ShowErrorDialog("Stop Recording Error", $"Failed to stop recording: {ex.Message}");
            }
        }

        // InitializeComponent is provided by generated XAML code (MainPage.g.i.cs)
    }
}
