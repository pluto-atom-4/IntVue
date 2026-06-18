// Copyright (c) YourProjectName. All rights reserved.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using IntVue.Services;
using IntVue.ViewModels;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace IntVue.Views;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

/// <summary>
/// The main content page displayed inside the application window.
/// Add your UI logic, event handlers, and data binding here.
/// </summary>
public sealed partial class MainPage : Page
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainPage"/> class.
    /// </summary>
    public MainPage()
    {
        this.InitializeComponent();

        var svc = (IMediaCaptureService?)App.Services.GetService(typeof(IMediaCaptureService));
        this.ViewModel = (InterviewViewModel?)App.Services.GetService(typeof(InterviewViewModel)) ?? new InterviewViewModel(svc!);
        this.DataContext = this.ViewModel;

        // Show error dialog when recording error occurs
        this.ViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(InterviewViewModel.RecordingError)
                && !string.IsNullOrEmpty(this.ViewModel.RecordingError))
                _ = this.ShowErrorDialog("Recording Error", this.ViewModel.RecordingError);
        };

        this.Unloaded += (s, e) => this.OnPageUnloaded();
        this.Loaded += (s, e) => this.OnPageLoaded();
    }

    /// <summary>
    /// Gets the ViewModel for this page.
    /// </summary>
    public InterviewViewModel ViewModel { get; private set; } = null!;

    private async void OnPageLoaded()
    {
        await this.ViewModel.LoadCamerasAsync();
    }

    private async void BtnInitializeDevice_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await this.ViewModel.InitializeDeviceAsync();
        }
        catch (Exception ex)
        {
            await this.ShowErrorDialog("Device Initialization Error", $"Failed to initialize device: {ex.Message}");
        }
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
        try
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

            await this.ViewModel.ToggleRecordingAsync("recording").ConfigureAwait(false);
        }
        catch (InvalidOperationException ex) when (ex.InnerException is COMException)
        {
            await this.ShowErrorDialog("Camera Hardware Issue", "Unable to start/stop recording. Another app may be using the camera or drivers need updating.");
        }
        catch (Exception ex)
        {
            await this.ShowErrorDialog("Recording Error", $"Failed: {ex.Message}");
        }
    }

    // InitializeComponent is provided by generated XAML code (MainPage.g.i.cs)
}
