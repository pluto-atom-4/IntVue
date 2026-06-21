// Copyright (c) YourProjectName. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    private MediaCapture? mediaCapture;
    private MediaPlayer? mediaPlayer;
    private List<DeviceInformation>? deviceList;
    private List<MediaFrameSource>? previewSourceList;
    private MediaFrameSource? currentPreviewSource;
    private StringBuilder logBuilder = new StringBuilder();
    private LowLagMediaRecording? mediaRecording;
    private StorageFile? recordedFile;
    private bool isRecording;
    private bool disposed;

    private enum LogMessageType
    {
        Message,
        Success,
        Error,
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MainPage"/> class.
    /// </summary>
    public MainPage()
    {
        this.InitializeComponent();
        this.InitializeUI();
        this.Log("Application started", LogMessageType.Message);
        this.Unloaded += (s, e) => this.Dispose();
    }

    /// <summary>
    /// Disposes resources owned by the page.
    /// </summary>
    public void Dispose()
    {
        if (this.disposed)
        {
            return;
        }

        this.mediaPlayer?.Dispose();
        this.mediaCapture?.Dispose();
        this.mediaRecording = null;
        this.recordedFile = null;

        this.disposed = true;
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
            this.deviceList = (await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture)).ToList();

            if (this.deviceList.Count == 0)
            {
                this.Log("No camera devices found!", LogMessageType.Error);
                return;
            }

            foreach (var device in this.deviceList)
            {
                this.CbCameraList.Items.Add(device.Name);
                this.Log($"Found device: {device.Name} (ID: {device.Id})", LogMessageType.Message);
            }

            this.CbCameraList.SelectedIndex = 0;
            this.Log($"Found {this.deviceList.Count} camera(s)", LogMessageType.Success);
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
            if (this.deviceList == null || deviceIdx < 0)
            {
                this.Log("Select device before starting", LogMessageType.Error);
                return;
            }

            this.mediaCapture = new MediaCapture();

            var settings = new MediaCaptureInitializationSettings
            {
                VideoDeviceId = this.deviceList[deviceIdx].Id,
                StreamingCaptureMode = StreamingCaptureMode.AudioAndVideo,
            };

            this.Log("Calling MediaCapture.InitializeAsync()...", LogMessageType.Message);
            await this.mediaCapture.InitializeAsync(settings);

            await this.PopulatePreviewSources();
        }
        catch (Exception ex)
        {
            this.Log($"MediaCapture initialization failed: {ex.Message}", LogMessageType.Error);

            if (this.mediaCapture != null)
            {
                this.mediaCapture.Dispose();
                this.mediaCapture = null;
            }
        }
    }

    private async Task PopulatePreviewSources()
    {
        this.Log("Populating preview sources...", LogMessageType.Message);

        try
        {
            if (this.mediaCapture == null)
            {
                return;
            }

            this.previewSourceList = new List<MediaFrameSource>();

            foreach (var source in this.mediaCapture.FrameSources.Values)
            {
                if (source.Info.MediaStreamType == MediaStreamType.VideoPreview ||
                    source.Info.MediaStreamType == MediaStreamType.VideoRecord)
                {
                    this.previewSourceList.Add(source);
                    this.Log($"Found preview source: {source.Info.SourceKind}", LogMessageType.Message);
                }
            }

            if (this.previewSourceList.Count > 0)
            {
                this.Log($"Found {this.previewSourceList.Count} preview source(s)", LogMessageType.Success);
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
            if (this.mediaCapture == null)
            {
                this.Log("MediaCapture not initialized", LogMessageType.Error);
                return false;
            }

            if (this.previewSourceList == null || this.previewSourceList.Count == 0)
            {
                this.Log("No preview sources available", LogMessageType.Error);
                return false;
            }

            this.currentPreviewSource = this.previewSourceList[0];

            this.mediaPlayer = new MediaPlayer
            {
                RealTimePlayback = true,
                AutoPlay = false,
                Source = MediaSource.CreateFromMediaFrameSource(this.currentPreviewSource),
            };

            this.PreviewControl.SetMediaPlayer(this.mediaPlayer);
            this.mediaPlayer.Play();

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

            if (this.mediaPlayer != null)
            {
                this.mediaPlayer.Pause();
                this.mediaPlayer = null;
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
        if (this.mediaCapture == null)
        {
            this.Log("Initialize MediaCapture before recording.", LogMessageType.Error);
            return;
        }

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
            this.recordedFile = file;
            MediaEncodingProfile encodingProfile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Auto);

            this.mediaCapture!.RecordLimitationExceeded += this.OnMediaCaptureRecordLimitationExceeded;

            this.Log("Initializing recording profile...", LogMessageType.Message);
            this.mediaRecording = await this.mediaCapture.PrepareLowLagRecordToStorageFileAsync(encodingProfile, file);

            await this.mediaRecording.StartAsync();

            this.isRecording = true;
            this.BtnRecord.Content = "Stop Recording";
            this.BtnPlay.IsEnabled = false;
            this.BtnPlay.Content = "Play Recording";
            this.Log($"Successfully recording capture live to: {file.Path}", LogMessageType.Success);
        }
        catch (Exception ex)
        {
            this.Log($"Failed to start recording: {ex.Message}", LogMessageType.Error);
            this.isRecording = false;
            this.BtnRecord.Content = "Start Recording";
        }
    }

    private async Task StopRecordingAsync()
    {
        if (this.mediaRecording == null || !this.isRecording)
        {
            return;
        }

        try
        {
            this.Log("Sending stop session token command...", LogMessageType.Message);
            await this.mediaRecording.StopAsync();

            this.Log("Invoking finalization file IO flushes...", LogMessageType.Message);
            await this.mediaRecording.FinishAsync();

            if (this.recordedFile != null)
            {
                this.BtnPlay.IsEnabled = true;
                this.Log("Recording saved. Click 'Play Recording' to review.", LogMessageType.Success);
            }
        }
        catch (Exception ex)
        {
            this.Log($"Error during stop pipeline: {ex.Message}", LogMessageType.Error);
        }
        finally
        {
            if (this.mediaCapture != null)
            {
                this.mediaCapture.RecordLimitationExceeded -= this.OnMediaCaptureRecordLimitationExceeded;
            }

            this.mediaRecording = null;
            this.isRecording = false;
            this.BtnRecord.Content = "Start Recording";
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
        if (this.recordedFile == null)
        {
            return;
        }

        try
        {
            // Stop live preview before switching to playback
            if (this.mediaPlayer != null)
            {
                this.mediaPlayer.Pause();
                this.PreviewControl.SetMediaPlayer(null);
                this.mediaPlayer = null;
            }

            this.mediaPlayer = new MediaPlayer();
            this.mediaPlayer.Source = MediaSource.CreateFromStorageFile(this.recordedFile);
            this.mediaPlayer.MediaEnded += this.OnPlaybackEnded;

            this.PreviewControl.SetMediaPlayer(this.mediaPlayer);
            this.PreviewControl.AreTransportControlsEnabled = true;
            this.mediaPlayer.Play();

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
        if (this.mediaPlayer != null)
        {
            this.mediaPlayer.MediaEnded -= this.OnPlaybackEnded;
            this.mediaPlayer.Pause();
            this.PreviewControl.SetMediaPlayer(null);
            this.mediaPlayer.Dispose();
            this.mediaPlayer = null;
        }

        this.PreviewControl.AreTransportControlsEnabled = false;
        this.BtnPlay.Content = "Play Recording";
        this.BtnPlay.IsEnabled = this.recordedFile != null;
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

    private void Log(string message, LogMessageType type = LogMessageType.Message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture);
        var logEntry = $"[{timestamp}] {message}\n";

        this.logBuilder.Append(logEntry);

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
}
