// Copyright (c) YourProjectName. All rights reserved.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using IntVue.Services;
using IntVue.ViewModels;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;

namespace IntVue.Views;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

/// <summary>
/// The main content page displayed inside the application window.
/// Add your UI logic, event handlers, and data binding here.
/// </summary>
public sealed partial class MainPage : Page
{
    private const string ConsentKey = "HasGivenConsent";

    /// <inheritdoc/>
    public InterviewViewModel ViewModel { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MainPage"/> class.
    /// </summary>
    public MainPage()
    {
        this.InitializeComponent();

        var svc = (IMediaCaptureService?)App.Services.GetService(typeof(IMediaCaptureService));
        this.ViewModel = (InterviewViewModel?)App.Services.GetService(typeof(InterviewViewModel)) ?? new InterviewViewModel(svc!);
        this.DataContext = this.ViewModel;

        // Subscribe to ViewModel property changes to ensure UI stays in sync (fallback for binding issues)
        this.ViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(InterviewViewModel.StopPreviewButtonVisibility))
            {
#if DEBUG
                Trace.WriteLine($"[IntVue.Debug] MainPage: Detected StopPreviewButtonVisibility change to {this.ViewModel.StopPreviewButtonVisibility}");
#endif

                // Ensure button visibility is updated
                this.BtnStopPreview.Visibility = this.ViewModel.StopPreviewButtonVisibility;
            }
        };

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

        // Initialize Stop Preview button visibility (should start collapsed)
        this.BtnStopPreview.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
#if DEBUG
        Trace.WriteLine("[IntVue.Debug] MainPage.OnPageLoaded: Stop Preview button initialized to Collapsed.");
#endif
#endif

        this.TxtConsent.Tapped += (_, _) => this.OnConsentTapped();
        this.TxtConsent.PointerReleased += (_, _) => this.OnConsentTapped();

#if DEBUG
        Trace.WriteLine("[IntVue.Debug] MainPage.OnPageLoaded: Checking for saved consent...");
#endif

        // Load saved consent state from LocalSettings
        var hasConsent = ApplicationData.Current.LocalSettings.Values.TryGetValue(ConsentKey, out var value) &&
                         value is bool consentValue && consentValue;

        if (hasConsent)
        {
#if DEBUG
            Trace.WriteLine("[IntVue.Debug] MainPage.OnPageLoaded: Found saved consent, loading...");
#endif
            this.ViewModel.ConsentGiven = true;
        }
        else
        {
#if DEBUG
            Trace.WriteLine("[IntVue.Debug] MainPage.OnPageLoaded: No saved consent, showing consent dialog...");
#endif

            // Show consent dialog on first load
            var consented = await this.ShowConsentDialogAsync();

#if DEBUG
            Debug.WriteLine($"[IntVue.Debug] MainPage.OnPageLoaded: Consent dialog result: {consented}");
#endif

            this.ViewModel.ConsentGiven = consented;
        }

        this.UpdateConsentText();

#if DEBUG
        Trace.WriteLine("[IntVue.Debug] MainPage.OnPageLoaded: Page initialization complete.");
#endif
    }

    private void OnConsentTapped()
    {
        this.ViewModel.ConsentGiven = !this.ViewModel.ConsentGiven;
        ApplicationData.Current.LocalSettings.Values[ConsentKey] = this.ViewModel.ConsentGiven;
        this.UpdateConsentText();
    }

    private async Task<bool> ShowConsentDialogAsync()
    {
        var dialog = new ContentDialog
        {
            Title = "Recording Consent",
            Content = "This application will record audio and video from your camera and microphone.\n\nDo you consent to this recording?",
            PrimaryButtonText = "I Consent",
            CloseButtonText = "Decline",
            XamlRoot = this.XamlRoot,
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            ApplicationData.Current.LocalSettings.Values[ConsentKey] = true;
            return true;
        }

        return false;
    }

    private async void BtnStopPreview_Click(object sender, RoutedEventArgs e)
    {
#if DEBUG
        Trace.WriteLine("[IntVue.Debug] MainPage.BtnStopPreview_Click: Stop Preview button clicked.");
#endif
        try
        {
            await this.ViewModel.StopPreviewAsync().ConfigureAwait(false);
#if DEBUG
            Trace.WriteLine("[IntVue.Debug] MainPage.BtnStopPreview_Click: Preview stopped successfully.");
#endif
        }
        catch (InvalidOperationException ex)
        {
#if DEBUG
            Debug.WriteLine($"[IntVue.Debug] MainPage.BtnStopPreview_Click: InvalidOperationException - {ex.Message}");
#endif

            // Expected if preview already stopped or not initialized
            try
            {
                await this.ShowErrorDialog("Stop Preview Error", $"Preview already stopped or error: {ex.Message}");
            }
            catch (Exception dialogEx)
            {
#if DEBUG
                Debug.WriteLine($"[IntVue.Debug] MainPage.BtnStopPreview_Click: Error showing dialog - {dialogEx.GetType().Name}");
#endif
            }
        }
        catch (Exception ex)
        {
#if DEBUG
            Debug.WriteLine($"[IntVue.Debug] MainPage.BtnStopPreview_Click: Exception - {ex.GetType().Name}: {ex.Message}");
            Debug.WriteLine($"[IntVue.Debug] MainPage.BtnStopPreview_Click: StackTrace: {ex.StackTrace}");
#endif
            try
            {
                await this.ShowErrorDialog("Stop Preview Error", $"Failed to stop preview: {ex.Message}");
            }
            catch (Exception dialogEx)
            {
#if DEBUG
                Debug.WriteLine($"[IntVue.Debug] MainPage.BtnStopPreview_Click: Error showing dialog - {dialogEx.GetType().Name}");
#endif
            }
        }
        finally
        {
            // Stop Preview button visibility managed by ViewModel binding
#if DEBUG
            Trace.WriteLine("[IntVue.Debug] MainPage.BtnStopPreview_Click: Stop Preview button visibility will be updated by ViewModel.");
#endif
        }
    }

    private void UpdateConsentText()
    {
        if (this.ViewModel.ConsentGiven)
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
        if (this.ViewModel.IsPreviewing)
        {
            await this.ViewModel.StopPreviewAsync().ConfigureAwait(false);
        }
    }

    private async void BtnStartPreview_Click(object sender, RoutedEventArgs e)
    {
#if DEBUG
        Trace.WriteLine("[IntVue.Debug] MainPage.BtnStartPreview_Click: Start Preview button clicked.");
        Debug.WriteLine($"[IntVue.Debug] MainPage.BtnStartPreview_Click: ConsentGiven={this.ViewModel.ConsentGiven}, PreviewControl type={this.PreviewControl?.GetType().Name}");
#endif

        if (!this.ViewModel.ConsentGiven)
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

            var success = await this.ViewModel.StartPreviewAsync(this.PreviewControl).ConfigureAwait(false);

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
                Trace.WriteLine("[IntVue.Debug] MainPage.BtnStartPreview_Click: Preview started successfully. Stop Preview button visibility managed by ViewModel binding.");
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
        Debug.WriteLine($"[IntVue.Debug] MainPage.BtnStartRecording_Click: IsPreviewing={this.ViewModel.IsPreviewing}");
#endif

        // Stop preview before recording (Microsoft docs: preview and recording are mutually exclusive)
        // Surface built-in camera uses exclusive hardware control
        if (this.ViewModel.IsPreviewing)
        {
#if DEBUG
            Trace.WriteLine("[IntVue.Debug] MainPage.BtnStartRecording_Click: Stopping preview before recording (hardware exclusivity on Surface camera)...");
#endif
            try
            {
                await this.ViewModel.StopPreviewAsync().ConfigureAwait(false);
            }
            catch (Exception stopEx)
            {
#if DEBUG
                Debug.WriteLine($"[IntVue.Debug] MainPage.BtnStartRecording_Click: Warning - error stopping preview: {stopEx.Message}");
#endif
            }
        }

        try
        {
#if DEBUG
            Trace.WriteLine("[IntVue.Debug] MainPage.BtnStartRecording_Click: Calling viewModel.StartRecordingAsync()...");
#endif
            var sanitized = "recording";
            await this.ViewModel.StartRecordingAsync(sanitized).ConfigureAwait(false);

#if DEBUG
            Trace.WriteLine("[IntVue.Debug] MainPage.BtnStartRecording_Click: Recording started successfully.");
#endif
        }
        catch (InvalidOperationException ex) when (ex.InnerException is COMException comEx)
        {
#if DEBUG
            Debug.WriteLine($"[IntVue.Debug] MainPage.BtnStartRecording_Click: COMException during recording - HResult=0x{comEx.HResult:X8}: {ex.Message}");
#endif
            await this.ShowErrorDialog(
                "Camera Hardware Issue",
                "Unable to start recording. This may occur if:\n" +
                "• Another app is using the camera\n" +
                "• Camera driver needs updating\n" +
                "• Try restarting the app");
        }
        catch (InvalidOperationException ex)
        {
#if DEBUG
            Debug.WriteLine($"[IntVue.Debug] MainPage.BtnStartRecording_Click: InvalidOperationException - {ex.Message}");
#endif
            await this.ShowErrorDialog("Recording Error", $"Failed to start recording: {ex.Message}");
        }
        catch (COMException comEx)
        {
#if DEBUG
            Debug.WriteLine($"[IntVue.Debug] MainPage.BtnStartRecording_Click: COMException - HResult=0x{comEx.HResult:X8}: {comEx.Message}");
#endif
            await this.ShowErrorDialog("Hardware Error", $"Camera hardware error (0x{comEx.HResult:X8}): {comEx.Message}");
        }
        catch (Exception ex)
        {
#if DEBUG
            Debug.WriteLine($"[IntVue.Debug] MainPage.BtnStartRecording_Click: Unhandled Exception - {ex.GetType().Name}: {ex.Message}");
            Debug.WriteLine($"[IntVue.Debug] MainPage.BtnStartRecording_Click: StackTrace: {ex.StackTrace}");
#endif
            await this.ShowErrorDialog("Recording Error", $"An unexpected error occurred: {ex.GetType().Name} - {ex.Message}");
        }
    }

    private async void BtnStopRecording_Click(object sender, RoutedEventArgs e)
    {
#if DEBUG
        Trace.WriteLine("[IntVue.Debug] MainPage.BtnStopRecording_Click: Stop Recording button clicked.");
        Debug.WriteLine($"[IntVue.Debug] MainPage.BtnStopRecording_Click: IsRecording={this.ViewModel.IsRecording}");
#endif

        try
        {
#if DEBUG
            Trace.WriteLine("[IntVue.Debug] MainPage.BtnStopRecording_Click: Calling viewModel.StopRecordingAsync()...");
#endif
            await this.ViewModel.StopRecordingAsync().ConfigureAwait(false);

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
