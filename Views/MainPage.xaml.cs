// Copyright (c) YourProjectName. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IntVue.Services;
using IntVue.ViewModels;
using IntVue.Views;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using Windows.Devices.Enumeration;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Media.Playback;
using Windows.Storage;

namespace IntVue.Views;

/// <summary>
/// Main application page with direct code-behind camera capture, preview, and recording logic.
/// Implements a simplified state machine without MVVM abstractions for MVP stability.
/// </summary>
public sealed partial class MainPage : Page, IDisposable
{
    private enum LogMessageType
    {
        Message,
        Success,
        Error,
    }

#pragma warning disable SA1201 // Elements should appear in the correct order
    private MediaCapture? _mediaCapture;
    private MediaPlayer? _mediaPlayer;
    private List<DeviceInformation>? _deviceList;
    private List<MediaFrameSource>? _previewSourceList;
    private MediaFrameSource? _currentPreviewSource;
    private StringBuilder _logBuilder = new StringBuilder();
    private LowLagMediaRecording? _mediaRecording;
    private StorageFile? _recordedFile;
    private bool _isRecording;
    private bool _disposed;
#pragma warning restore SA1201 // Elements should appear in the correct order

    /// <summary>
    /// Initializes a new instance of the <see cref="MainPage"/> class.
    /// </summary>
    public MainPage()
    {
        this.InitializeComponent();
        var countdownService = (ICountdownService?)App.Services.GetService(typeof(ICountdownService)) ?? new CountdownService();
        var featureFlagService = (IFeatureFlagService?)App.Services.GetService(typeof(IFeatureFlagService)) ?? new FeatureFlagService();
        this.ViewModel = new MainViewModel(countdownService, featureFlagService);
        this.DataContext = this.ViewModel;
        this.ViewModel.CountdownCompleted += this.OnCountdownCompleted;
        this.ViewModel.PropertyChanged += this.OnViewModelPropertyChanged;
        this.InitializeUI();
        this.Log("Application started", LogMessageType.Message);
        this.Unloaded += (s, e) => this.Dispose();
    }

    /// <summary>
    /// Gets the main view model for data binding.
    /// </summary>
    public MainViewModel ViewModel { get; private set; }

    /// <summary>
    /// Disposes resources owned by the page.
    /// </summary>
    public void Dispose()
    {
        if (this._disposed)
        {
            return;
        }

        this._mediaPlayer?.Dispose();
        this._mediaCapture?.Dispose();
        this._mediaRecording = null;
        this._recordedFile = null;

        this._disposed = true;
        GC.SuppressFinalize(this);
    }

    private void InitializeUI()
    {
        this.PopulateCameraList();
    }

    private async void PopulateCameraList()
    {
        this.Log("Enumerating camera devices...", LogMessageType.Message);
        this.CbCameraList.Items.Clear();

        try
        {
            this._deviceList = (await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture)).ToList();

            if (this._deviceList.Count == 0)
            {
                this.Log("No camera devices found!", LogMessageType.Error);
                return;
            }

            foreach (var device in this._deviceList)
            {
                this.CbCameraList.Items.Add(device.Name);
                this.Log($"Found device: {device.Name} (ID: {device.Id})", LogMessageType.Message);
            }

            this.CbCameraList.SelectedIndex = 0;
            this.Log($"Found {this._deviceList.Count} camera(s)", LogMessageType.Success);
        }
        catch (Exception ex)
        {
            this.Log($"Error enumerating devices: {ex.Message}", LogMessageType.Error);
        }
    }

    private async void BtnInitializeDevice_Click(object sender, RoutedEventArgs e)
    {
        this.Log("Initializing device...", LogMessageType.Message);

        try
        {
            await this.InitializeMediaCapture();
            this.BtnPreview.IsEnabled = true;
            this.BtnRecord.IsEnabled = true;
            this.Log("Device initialized successfully", LogMessageType.Success);
        }
        catch (Exception ex)
        {
            this.Log($"Failed to initialize device: {ex.Message}", LogMessageType.Error);
        }
    }

    private async Task InitializeMediaCapture()
    {
        try
        {
            int deviceIdx = this.CbCameraList.SelectedIndex;
            if (this._deviceList == null || deviceIdx < 0)
            {
                this.Log("Select device before starting", LogMessageType.Error);
                return;
            }

            this._mediaCapture = new MediaCapture();

            var settings = new MediaCaptureInitializationSettings
            {
                VideoDeviceId = this._deviceList[deviceIdx].Id,
                StreamingCaptureMode = StreamingCaptureMode.AudioAndVideo,
            };

            this.Log("Calling MediaCapture.InitializeAsync()...", LogMessageType.Message);
            await this._mediaCapture.InitializeAsync(settings);

            await this.PopulatePreviewSources();
        }
        catch (Exception ex)
        {
            this.Log($"MediaCapture initialization failed: {ex.Message}", LogMessageType.Error);

            if (this._mediaCapture != null)
            {
                this._mediaCapture.Dispose();
                this._mediaCapture = null;
            }
        }
    }

    private async Task PopulatePreviewSources()
    {
        this.Log("Populating preview sources...", LogMessageType.Message);

        try
        {
            if (this._mediaCapture == null)
            {
                return;
            }

            this._previewSourceList = new List<MediaFrameSource>();

            foreach (var source in this._mediaCapture.FrameSources.Values)
            {
                if (source.Info.MediaStreamType == MediaStreamType.VideoPreview ||
                    source.Info.MediaStreamType == MediaStreamType.VideoRecord)
                {
                    this._previewSourceList.Add(source);
                    this.Log($"Found preview source: {source.Info.SourceKind}", LogMessageType.Message);
                }
            }

            if (this._previewSourceList.Count > 0)
            {
                this.Log($"Found {this._previewSourceList.Count} preview source(s)", LogMessageType.Success);
            }
            else
            {
                this.Log("No preview sources found", LogMessageType.Error);
            }
        }
        catch (Exception ex)
        {
            this.Log($"Error populating preview sources: {ex.Message}", LogMessageType.Error);
        }
    }

    private void BtnPreview_Click(object sender, RoutedEventArgs e)
    {
        if (this.BtnPreview.Content.ToString() == "Start Preview")
        {
            this.StartPreview();
        }
        else
        {
            this.StopPreview();
        }
    }

    private bool StartPreview()
    {
        this.Log("Starting preview...", LogMessageType.Message);

        try
        {
            if (this._mediaCapture == null)
            {
                this.Log("MediaCapture not initialized", LogMessageType.Error);
                return false;
            }

            if (this._previewSourceList == null || this._previewSourceList.Count == 0)
            {
                this.Log("No preview sources available", LogMessageType.Error);
                return false;
            }

            this._currentPreviewSource = this._previewSourceList[0];

            this._mediaPlayer = new MediaPlayer
            {
                RealTimePlayback = true,
                AutoPlay = false,
                Source = MediaSource.CreateFromMediaFrameSource(this._currentPreviewSource),
            };

            this.PreviewControl.SetMediaPlayer(this._mediaPlayer);
            this._mediaPlayer.Play();

            this.BtnPreview.Content = "Stop Preview";

            this.Log("Preview started successfully", LogMessageType.Success);
            return true;
        }
        catch (Exception ex)
        {
            this.Log($"Error starting preview: {ex.Message}", LogMessageType.Error);
            return false;
        }
    }

    private void StopPreview()
    {
        this.Log("Stopping preview...", LogMessageType.Message);

        try
        {
            // If playback is active, stop it instead
            if (this.BtnPlay.Content.ToString() == "Stop Playback")
            {
                this.StopPlayback();
                return;
            }

            if (this._mediaPlayer != null)
            {
                this._mediaPlayer.Pause();
                this._mediaPlayer = null;
            }

            this.BtnPreview.Content = "Start Preview";

            this.Log("Preview stopped", LogMessageType.Success);
        }
        catch (Exception ex)
        {
            this.Log($"Error stopping preview: {ex.Message}", LogMessageType.Error);
        }
    }

    private async void BtnRecord_Click(object sender, RoutedEventArgs e)
    {
        if (this._mediaCapture == null)
        {
            this.Log("Initialize MediaCapture before recording.", LogMessageType.Error);
            return;
        }

        if (!this._isRecording)
        {
            if (!this.ViewModel.IsCountingDown)
            {
                await this.ViewModel.StartCountdownAsync();
            }
        }
        else
        {
            await this.StopRecordingAsync();
        }
    }

    private async Task StartRecordingAsync()
    {
        // Stop playback if active before starting new recording
        if (this.BtnPlay.Content.ToString() == "Stop Playback")
        {
            this.StopPlayback();
        }

        try
        {
            this.Log("Preparing capture file storage...", LogMessageType.Message);
            StorageLibrary myVideos = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Videos);

            StorageFile file = await myVideos.SaveFolder.CreateFileAsync("video.mp4", CreationCollisionOption.GenerateUniqueName);
            this._recordedFile = file;
            MediaEncodingProfile encodingProfile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Auto);

            this._mediaCapture!.RecordLimitationExceeded += this.OnMediaCaptureRecordLimitationExceeded;

            this.Log("Initializing recording profile...", LogMessageType.Message);
            this._mediaRecording = await this._mediaCapture.PrepareLowLagRecordToStorageFileAsync(encodingProfile, file);

            await this._mediaRecording.StartAsync();

            this._isRecording = true;
            this.BtnRecord.Content = "Stop Recording";
            this.BtnPlay.IsEnabled = false;
            this.BtnPlay.Content = "Play Recording";
            this.BtnDelete.IsEnabled = false;
            this.Log($"Successfully recording capture live to: {file.Path}", LogMessageType.Success);
        }
        catch (Exception ex)
        {
            this.Log($"Failed to start recording: {ex.Message}", LogMessageType.Error);
            this._isRecording = false;
            this.BtnRecord.Content = "Start Recording";
        }
    }

    private async Task StopRecordingAsync()
    {
        if (this._mediaRecording == null || !this._isRecording)
        {
            return;
        }

        try
        {
            this.Log("Sending stop session token command...", LogMessageType.Message);
            await this._mediaRecording.StopAsync();

            this.Log("Invoking finalization file IO flushes...", LogMessageType.Message);
            await this._mediaRecording.FinishAsync();

            if (this._recordedFile != null)
            {
                this.BtnPlay.IsEnabled = true;
                this.BtnDelete.IsEnabled = true;
                this.Log("Recording saved. Click 'Play Recording' to review.", LogMessageType.Success);
            }
        }
        catch (Exception ex)
        {
            this.Log($"Error during stop pipeline: {ex.Message}", LogMessageType.Error);
        }
        finally
        {
            if (this._mediaCapture != null)
            {
                this._mediaCapture.RecordLimitationExceeded -= this.OnMediaCaptureRecordLimitationExceeded;
            }

            this._mediaRecording = null;
            this._isRecording = false;
            this.BtnRecord.Content = "Start Recording";
            this.BtnRecord.IsEnabled = false;
            this.Log("Recording pipeline dropped and saved clean.", LogMessageType.Success);
        }
    }

    private void OnMediaCaptureRecordLimitationExceeded(MediaCapture sender)
    {
        this.Log("System tracking limit threshold breached. Halting context.", LogMessageType.Error);

        _ = this.DispatcherQueue.TryEnqueue(async () =>
        {
            await this.StopRecordingAsync();
        });
    }

    private async Task PlayRecordingAsync()
    {
        if (this._recordedFile == null)
        {
            return;
        }

        try
        {
            // Stop live preview before switching to playback
            if (this._mediaPlayer != null)
            {
                this._mediaPlayer.Pause();
                this.PreviewControl.SetMediaPlayer(null);
                this._mediaPlayer = null;
            }

            this._mediaPlayer = new MediaPlayer();
            this._mediaPlayer.Source = MediaSource.CreateFromStorageFile(this._recordedFile);
            this._mediaPlayer.MediaEnded += this.OnPlaybackEnded;

            this.PreviewControl.SetMediaPlayer(this._mediaPlayer);
            this.PreviewControl.AreTransportControlsEnabled = true;
            this._mediaPlayer.Play();

            // Update UI
            this.BtnPlay.Content = "Stop Playback";
            this.BtnPreview.IsEnabled = false;
            this.BtnRecord.IsEnabled = false;

            this.Log("Playback started.", LogMessageType.Message);
        }
        catch (Exception ex)
        {
            this.Log($"Playback failed: {ex.Message}", LogMessageType.Error);
            this.BtnPlay.IsEnabled = false;
        }
    }

    private void StopPlayback()
    {
        if (this._mediaPlayer != null)
        {
            this._mediaPlayer.MediaEnded -= this.OnPlaybackEnded;
            this._mediaPlayer.Pause();
            this.PreviewControl.SetMediaPlayer(null);
            this._mediaPlayer.Dispose();
            this._mediaPlayer = null;
        }

        this.PreviewControl.AreTransportControlsEnabled = false;
        this.BtnPlay.Content = "Play Recording";
        this.BtnPlay.IsEnabled = this._recordedFile != null;
        this.BtnDelete.IsEnabled = this._recordedFile != null;
        this.BtnPreview.Content = "Start Preview";
        this.BtnPreview.IsEnabled = true;
        this.BtnRecord.Content = "Start Recording";
        this.BtnRecord.IsEnabled = false;

        this.Log("Playback stopped.", LogMessageType.Message);
    }

    private void OnPlaybackEnded(MediaPlayer sender, object args)
    {
        this.DispatcherQueue.TryEnqueue(() => this.StopPlayback());
    }

    private async void BtnPlay_Click(object sender, RoutedEventArgs e)
    {
        if (this.BtnPlay.Content.ToString() == "Play Recording")
        {
            await this.PlayRecordingAsync();
        }
        else
        {
            this.StopPlayback();
        }
    }

    private async void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        await this.DeleteRecordingAsync();
    }

    private void BtnProductReview_Click(object sender, RoutedEventArgs e)
    {
        var mainWindow = Window.Current as MainWindow;
        var frame = mainWindow?.Content as Frame;

        if (frame != null)
        {
            frame.Navigate(typeof(ProductReviewPage));
            this.Log("Navigating to Product Review page", LogMessageType.Message);
        }
        else
        {
            this.Log("Error: Cannot navigate to ProductReviewPage - Frame not found", LogMessageType.Error);
        }
    }

    private async Task DeleteRecordingAsync()
    {
        // Validate recorded file exists
        if (this._recordedFile == null)
        {
            this.Log("No recording available to delete", LogMessageType.Error);
            return;
        }

        try
        {
            // Stop playback if active before deleting file
            if (this.BtnPlay.Content.ToString() == "Stop Playback")
            {
                this.Log("Stopping playback before deletion...", LogMessageType.Message);
                this.StopPlayback();
            }

            // Delete file
            this.Log($"Deleting recorded file: {this._recordedFile.Name}", LogMessageType.Message);
            await this._recordedFile.DeleteAsync();

            // Clear reference
            this._recordedFile = null;

            // Update UI state
            this.BtnDelete.IsEnabled = false;
            this.BtnPlay.IsEnabled = false;
            this.BtnPlay.Content = "Play Recording";
            this.BtnRecord.IsEnabled = true;
            this.BtnRecord.Content = "Start Recording";

            // Log success
            this.Log("Recording deleted successfully", LogMessageType.Success);
        }
        catch (FileNotFoundException ex)
        {
            this.Log($"File already deleted externally: {ex.Message}", LogMessageType.Error);
            this._recordedFile = null;
            this.BtnDelete.IsEnabled = false;
            this.BtnPlay.IsEnabled = false;
            this.BtnRecord.IsEnabled = true;
        }
        catch (UnauthorizedAccessException ex)
        {
            this.Log($"Permission denied when deleting file: {ex.Message}", LogMessageType.Error);
        }
        catch (Exception ex)
        {
            this.Log($"Error deleting recording: {ex.Message}", LogMessageType.Error);
        }
    }

    private void Log(string message, LogMessageType type = LogMessageType.Message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture);
        var logEntry = $"[{timestamp}] {message}\n";

        this._logBuilder.Append(logEntry);

        // Display in UI notification bar
        this.NotificationBar.Message = message;
        this.NotificationBar.Severity = type switch
        {
            LogMessageType.Success => InfoBarSeverity.Success,
            LogMessageType.Error => InfoBarSeverity.Error,
            _ => InfoBarSeverity.Informational,
        };
        this.NotificationBar.Title = type switch
        {
            LogMessageType.Success => "Success",
            LogMessageType.Error => "Error",
            _ => "Status",
        };
        this.NotificationBar.IsOpen = true;

#if DEBUG
        var typeStr = type switch
        {
            LogMessageType.Success => "✓",
            LogMessageType.Error => "✗",
            _ => "•",
        };
        Trace.WriteLine($"[IntVue] {typeStr} {message}");
#endif
    }

    private async void OnCountdownCompleted(object? sender, EventArgs e)
    {
        this.Log("Countdown completed, starting recording...", LogMessageType.Message);
        await this.StartRecordingAsync();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.IsCountingDown))
        {
            bool counting = this.ViewModel.IsCountingDown;
            this.BtnRecord.IsEnabled = !counting && this._mediaCapture != null && !this._isRecording;
            this.BtnPreview.IsEnabled = !counting;
            this.BtnPlay.IsEnabled = !counting && this._recordedFile != null && !this._isRecording;
            this.BtnDelete.IsEnabled = !counting && this._recordedFile != null && !this._isRecording;
        }
    }
}
