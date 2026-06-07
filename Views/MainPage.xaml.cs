// <copyright file="MainPage.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace IntVue.Views
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
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
        private readonly IConsentService consentService;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainPage"/> class.
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();

            var svc = (IMediaCaptureService?)App.Services.GetService(typeof(IMediaCaptureService));
            this.consentService = (IConsentService?)App.Services.GetService(typeof(IConsentService)) ?? new ConsentService();
            this.viewModel = (InterviewViewModel?)App.Services.GetService(typeof(InterviewViewModel)) ?? new InterviewViewModel(svc!);
            this.DataContext = this.viewModel;

            this.Unloaded += (s, e) => this.OnPageUnloaded();
            this.Loaded += (s, e) => this.OnPageLoaded();
        }

        private async void OnPageLoaded()
        {
#if DEBUG
            Trace.WriteLine("[IntVue.Debug] MainPage.OnPageLoaded: Page loaded, initializing UI...");

            // Phase 1: Log MediaPlayerElement layout and visibility
            try
            {
                Trace.WriteLine($"[IntVue.Debug] MainPage.OnPageLoaded: PreviewControl layout info:");
                Trace.WriteLine($"[IntVue.Debug]   ActualWidth={this.PreviewControl.ActualWidth}, ActualHeight={this.PreviewControl.ActualHeight}");
                Trace.WriteLine($"[IntVue.Debug]   Visibility: {this.PreviewControl.Visibility}");
                Trace.WriteLine($"[IntVue.Debug]   Opacity: {this.PreviewControl.Opacity}");
                Trace.WriteLine($"[IntVue.Debug]   Parent: {this.PreviewControl.Parent?.GetType().Name ?? "null"}");

                var parentControl = this.PreviewControl.Parent as Microsoft.UI.Xaml.Controls.Panel;
                if (parentControl != null)
                {
                    Trace.WriteLine($"[IntVue.Debug]   Parent is {parentControl.GetType().Name} with {parentControl.Children.Count} children");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[IntVue.Debug] MainPage.OnPageLoaded: ERROR getting PreviewControl bounds - {ex.GetType().Name}: {ex.Message}");
            }
#endif

            this.TxtConsent.Tapped += (_, _) => this.OnConsentTapped();
            this.TxtConsent.PointerReleased += (_, _) => this.OnConsentTapped();

#if DEBUG
            Trace.WriteLine("[IntVue.Debug] MainPage.OnPageLoaded: Checking for saved consent...");
#endif

            // Load saved consent state
            if (this.consentService.HasGivenConsent)
            {
#if DEBUG
                Trace.WriteLine("[IntVue.Debug] MainPage.OnPageLoaded: Found saved consent, loading...");
#endif
                this.viewModel.ConsentGiven = true;
            }
            else
            {
#if DEBUG
                Trace.WriteLine("[IntVue.Debug] MainPage.OnPageLoaded: No saved consent, showing consent dialog...");
#endif

                // Show consent dialog on first load
                var consented = await this.consentService.RequestConsentAsync(this.XamlRoot);

#if DEBUG
                Debug.WriteLine($"[IntVue.Debug] MainPage.OnPageLoaded: Consent dialog result: {consented}");
#endif

                this.viewModel.ConsentGiven = consented;
            }

            this.UpdateConsentText();

#if DEBUG
            Trace.WriteLine("[IntVue.Debug] MainPage.OnPageLoaded: Page initialization complete.");
#endif
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
#if DEBUG
            Trace.WriteLine("[IntVue.Debug] MainPage.BtnStartPreview_Click: Start Preview button clicked.");
            Debug.WriteLine($"[IntVue.Debug] MainPage.BtnStartPreview_Click: ConsentGiven={this.viewModel.ConsentGiven}, PreviewControl type={this.PreviewControl?.GetType().Name}");
#endif

            if (!this.viewModel.ConsentGiven)
            {
#if DEBUG
                Trace.WriteLine("[IntVue.Debug] MainPage.BtnStartPreview_Click: User consent not given, showing error dialog.");
#endif
                await this.ShowErrorDialog("Consent Required", "Please consent to camera and microphone recording before starting the preview.");
                return;
            }

            try
            {
#if DEBUG
                Trace.WriteLine("[IntVue.Debug] MainPage.BtnStartPreview_Click: Calling viewModel.StartPreviewAsync()...");
#endif
                if (this.PreviewControl == null)
                {
                    await this.ShowErrorDialog("Preview Control Error", "Preview control not initialized.");
                    return;
                }

                var success = await this.viewModel.StartPreviewAsync(this.PreviewControl).ConfigureAwait(false);

#if DEBUG
                Debug.WriteLine($"[IntVue.Debug] MainPage.BtnStartPreview_Click: StartPreviewAsync returned {success}");
#endif

                if (!success)
                {
#if DEBUG
                    Trace.WriteLine("[IntVue.Debug] MainPage.BtnStartPreview_Click: Preview start failed, showing error dialog.");
#endif
                    await this.ShowErrorDialog("Camera Access Denied", "Camera and microphone permissions are required to use the preview feature. Please enable them in your device settings.");
                }
                else
                {
#if DEBUG
                    Trace.WriteLine("[IntVue.Debug] MainPage.BtnStartPreview_Click: Preview started successfully.");

                    // Phase 1: Log PostPreview element state (diagnostic)
                    try
                    {
                        Trace.WriteLine($"[IntVue.Debug] MainPage.BtnStartPreview_Click: Post-preview PreviewControl state:");
                        Trace.WriteLine($"[IntVue.Debug]   ActualWidth={this.PreviewControl.ActualWidth}, ActualHeight={this.PreviewControl.ActualHeight}");
                        Trace.WriteLine($"[IntVue.Debug]   Visibility: {this.PreviewControl.Visibility}");
                        Trace.WriteLine($"[IntVue.Debug]   Opacity: {this.PreviewControl.Opacity}");
                        Trace.WriteLine($"[IntVue.Debug]   Background: {(this.PreviewControl.Background != null ? "Set" : "Null")}");

                        // Check if MediaPlayer is set
                        if (this.PreviewControl.MediaPlayer != null)
                        {
                            Trace.WriteLine($"[IntVue.Debug]   MediaPlayer set: True");
                            Trace.WriteLine($"[IntVue.Debug]   MediaPlayer.Source: {(this.PreviewControl.MediaPlayer.Source != null ? "Set" : "Null")}");
                            Trace.WriteLine($"[IntVue.Debug]   MediaPlayer.PlaybackState: {this.PreviewControl.MediaPlayer.PlaybackSession?.PlaybackState}");
                        }
                        else
                        {
                            Trace.WriteLine($"[IntVue.Debug]   MediaPlayer set: False");
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"[IntVue.Debug] MainPage.BtnStartPreview_Click: ERROR logging post-preview state - {ex.GetType().Name}: {ex.Message}");
                    }
#endif
                }
            }
            catch (ArgumentException ex)
            {
#if DEBUG
                Debug.WriteLine($"[IntVue.Debug] MainPage.BtnStartPreview_Click: ArgumentException - {ex.Message}");
#endif
                await this.ShowErrorDialog("Invalid Preview Control", "The preview control is not properly configured.");
            }
            catch (InvalidOperationException ex) when (ex.InnerException is COMException comEx)
            {
                // Specific handling for COMException wrapped in InvalidOperationException (from SetMediaPlayer)
#if DEBUG
                Trace.WriteLine($"[IntVue.Debug] MainPage.BtnStartPreview_Click: COMException caught - HResult=0x{comEx.HResult:X8}, Message={comEx.Message}");
#endif
                var errorMessage = "The camera preview cannot be displayed. This is usually caused by graphics driver issues or compatibility problems.\n\n" +
                    "Try the following:\n" +
                    "1) Restart your device\n" +
                    "2) Update your graphics drivers\n" +
                    "3) Check if another app is using the camera\n" +
                    "4) Try a different camera if available";

                await this.ShowErrorDialog("Camera Hardware Issue", errorMessage);
            }
            catch (InvalidOperationException ex)
            {
#if DEBUG
                Debug.WriteLine($"[IntVue.Debug] MainPage.BtnStartPreview_Click: InvalidOperationException - {ex.Message}");
#endif
                await this.ShowErrorDialog("Camera Not Available", ex.Message ?? "No camera device found or MediaCapture failed to initialize.");
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine($"[IntVue.Debug] MainPage.BtnStartPreview_Click: Exception - {ex.GetType().Name}: {ex.Message}");
#endif
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
#if DEBUG
            Trace.WriteLine("[IntVue.Debug] MainPage.BtnStartRecording_Click: Start Recording button clicked.");
            Debug.WriteLine($"[IntVue.Debug] MainPage.BtnStartRecording_Click: IsPreviewing={this.viewModel.IsPreviewing}");
#endif

            if (!this.viewModel.IsPreviewing)
            {
#if DEBUG
                Trace.WriteLine("[IntVue.Debug] MainPage.BtnStartRecording_Click: Preview not active, showing error dialog.");
#endif
                await this.ShowErrorDialog("Preview Required", "Please start the camera preview before recording.");
                return;
            }

            try
            {
#if DEBUG
                Trace.WriteLine("[IntVue.Debug] MainPage.BtnStartRecording_Click: Calling viewModel.StartRecordingAsync()...");
#endif
                var sanitized = "recording";
                await this.viewModel.StartRecordingAsync(sanitized).ConfigureAwait(false);

#if DEBUG
                Trace.WriteLine("[IntVue.Debug] MainPage.BtnStartRecording_Click: Recording started successfully.");
#endif
            }
            catch (InvalidOperationException ex)
            {
#if DEBUG
                Debug.WriteLine($"[IntVue.Debug] MainPage.BtnStartRecording_Click: InvalidOperationException - {ex.Message}");
#endif
                await this.ShowErrorDialog("Recording Error", $"Failed to start recording: {ex.Message}");
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine($"[IntVue.Debug] MainPage.BtnStartRecording_Click: Exception - {ex.GetType().Name}: {ex.Message}");
#endif
                await this.ShowErrorDialog("Recording Error", $"An unexpected error occurred: {ex.Message}");
            }
        }

        private async void BtnStopRecording_Click(object sender, RoutedEventArgs e)
        {
#if DEBUG
            Trace.WriteLine("[IntVue.Debug] MainPage.BtnStopRecording_Click: Stop Recording button clicked.");
            Debug.WriteLine($"[IntVue.Debug] MainPage.BtnStopRecording_Click: IsRecording={this.viewModel.IsRecording}");
#endif

            try
            {
#if DEBUG
                Trace.WriteLine("[IntVue.Debug] MainPage.BtnStopRecording_Click: Calling viewModel.StopRecordingAsync()...");
#endif
                await this.viewModel.StopRecordingAsync().ConfigureAwait(false);

#if DEBUG
                Trace.WriteLine("[IntVue.Debug] MainPage.BtnStopRecording_Click: Recording stopped successfully.");
#endif
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine($"[IntVue.Debug] MainPage.BtnStopRecording_Click: Exception - {ex.GetType().Name}: {ex.Message}");
#endif
                await this.ShowErrorDialog("Stop Recording Error", $"Failed to stop recording: {ex.Message}");
            }
        }

        // InitializeComponent is provided by generated XAML code (MainPage.g.i.cs)
    }
}
