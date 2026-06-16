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
    private bool isRecording = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainPage"/> class.
    /// </summary>
    public MainPage()
    {
        this.InitializeComponent();

        var svc = (IMediaCaptureService?)App.Services.GetService(typeof(IMediaCaptureService));
        this.ViewModel = (InterviewViewModel?)App.Services.GetService(typeof(InterviewViewModel)) ?? new InterviewViewModel(svc!);
        this.DataContext = this.ViewModel;

        this.Unloaded += (s, e) => this.OnPageUnloaded();
        this.Loaded += (s, e) => this.OnPageLoaded();
    }

    /// <summary>
    /// Gets the ViewModel for this page.
    /// </summary>
    public InterviewViewModel ViewModel { get; private set; } = null!;

    private async void OnPageLoaded()
    {
        this.TxtConsent.Tapped += (_, _) => this.OnConsentTapped();
        this.TxtConsent.PointerReleased += (_, _) => this.OnConsentTapped();

        // Load saved consent state from LocalSettings
        var hasConsent = ApplicationData.Current.LocalSettings.Values.TryGetValue(ConsentKey, out var value) &&
                         value is bool consentValue && consentValue;

        if (!hasConsent)
        {
            // Show consent dialog on first load
            var consented = await this.ShowConsentDialogAsync();
            this.ViewModel.ConsentGiven = consented;
        }
        else
        {
            this.ViewModel.ConsentGiven = true;
        }

        this.UpdateConsentText();
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

    private async void BtnPreview_Click(object sender, RoutedEventArgs e)
    {
        if (this.BtnPreview.Content.ToString() == "Start Preview")
        {
            await this.StartPreviewAsync();
        }
        else
        {
            await this.StopPreviewAsync();
        }
    }

    private async Task StartPreviewAsync()
    {
        if (!this.ViewModel.ConsentGiven)
        {
            await this.ShowErrorDialog("Consent Required", "Please consent to camera and microphone recording before starting the preview.");
            return;
        }

        try
        {
            var success = await this.ViewModel.StartPreviewAsync(this.PreviewControl).ConfigureAwait(false);
            if (success)
            {
                this.BtnPreview.Content = "Stop Preview";
            }
            else
            {
                await this.ShowErrorDialog("Camera Access Denied", "Camera and microphone permissions are required. Please enable them in device settings.");
            }
        }
        catch (InvalidOperationException ex) when (ex.InnerException is COMException)
        {
            await this.ShowErrorDialog(
                "Camera Hardware Issue",
                "The camera preview cannot be displayed. This is usually caused by graphics driver issues.\n\n" +
                "Try: 1) Restart your device  2) Update graphics drivers  3) Check if another app is using the camera");
        }
        catch (InvalidOperationException ex)
        {
            await this.ShowErrorDialog("Camera Not Available", ex.Message ?? "No camera device found.");
        }
        catch (Exception ex)
        {
            await this.ShowErrorDialog("Preview Error", $"An unexpected error occurred: {ex.Message}");
        }
    }

    private async Task StopPreviewAsync()
    {
        try
        {
            await this.ViewModel.StopPreviewAsync().ConfigureAwait(false);
            this.BtnPreview.Content = "Start Preview";
        }
        catch (Exception ex)
        {
            await this.ShowErrorDialog("Stop Preview Error", $"Failed to stop preview: {ex.Message}");
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

    private async void BtnRecord_Click(object sender, RoutedEventArgs e)
    {
        if (!this.isRecording)
        {
            await this.StartRecordingAsync();
        }
        else
        {
            await this.StopRecordingAsync();
        }
    }

    private async Task StartRecordingAsync()
    {
        // Stop preview before recording (preview and recording are mutually exclusive)
        if (this.ViewModel.IsPreviewing)
        {
            try
            {
                await this.ViewModel.StopPreviewAsync().ConfigureAwait(false);
                this.BtnPreview.Content = "Start Preview";
            }
            catch
            {
                // Swallow error; preview may already be stopped
            }
        }

        try
        {
            await this.ViewModel.StartRecordingAsync("recording").ConfigureAwait(false);
            this.isRecording = true;
            this.BtnRecord.Content = "Stop Recording";
        }
        catch (InvalidOperationException ex) when (ex.InnerException is COMException)
        {
            await this.ShowErrorDialog("Camera Hardware Issue", "Unable to start recording. Another app may be using the camera or drivers need updating.");
            this.isRecording = false;
            this.BtnRecord.Content = "Start Recording";
        }
        catch (Exception ex)
        {
            await this.ShowErrorDialog("Recording Error", $"Failed to start recording: {ex.Message}");
            this.isRecording = false;
            this.BtnRecord.Content = "Start Recording";
        }
    }

    private async Task StopRecordingAsync()
    {
        try
        {
            await this.ViewModel.StopRecordingAsync().ConfigureAwait(false);
            this.isRecording = false;
            this.BtnRecord.Content = "Start Recording";
        }
        catch (Exception ex)
        {
            await this.ShowErrorDialog("Stop Recording Error", $"Failed to stop recording: {ex.Message}");
        }
    }

    // InitializeComponent is provided by generated XAML code (MainPage.g.i.cs)
}
