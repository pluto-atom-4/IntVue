// Copyright (c) YourProjectName. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
public sealed partial class MainPage : Page
{
    private enum LogMessageType
    {
        Message,
        Success,
        Error
    }

    private MediaCapture? mediaCapture;
    private MediaPlayer? mediaPlayer;
    private List<DeviceInformation>? deviceList;
    private List<MediaFrameSource>? previewSourceList;
    private MediaFrameSource? currentPreviewSource;
    private StringBuilder logBuilder = new StringBuilder();

    private LowLagMediaRecording? mediaRecording;
    private bool isRecording = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainPage"/> class.
    /// </summary>
    public MainPage()
    {
        this.InitializeComponent();
        InitializeUI();
        Log("Application started", LogMessageType.Message);
    }

    private void InitializeUI()
    {
        PopulateCameraList();
    }

    private async void PopulateCameraList()
    {
        Log("Enumerating camera devices...", LogMessageType.Message);
        CbCameraList.Items.Clear();

        try
        {
            deviceList = (await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture)).ToList();

            if (deviceList.Count == 0)
            {
                Log("No camera devices found!", LogMessageType.Error);
                return;
            }

            foreach (var device in deviceList)
            {
                CbCameraList.Items.Add(device.Name);
                Log($"Found device: {device.Name} (ID: {device.Id})", LogMessageType.Message);
            }

            CbCameraList.SelectedIndex = 0;
            Log($"Found {deviceList.Count} camera(s)", LogMessageType.Success);
        }
        catch (Exception ex)
        {
            Log($"Error enumerating devices: {ex.Message}", LogMessageType.Error);
        }
    }

    private async void BtnInitializeDevice_Click(object sender, RoutedEventArgs e)
    {
        Log("Initializing device...", LogMessageType.Message);

        try
        {
            await InitializeMediaCapture();
            BtnPreview.IsEnabled = true;
            BtnRecord.IsEnabled = true;
            Log("Device initialized successfully", LogMessageType.Success);
        }
        catch (Exception ex)
        {
            Log($"Failed to initialize device: {ex.Message}", LogMessageType.Error);
        }
    }

    private async Task InitializeMediaCapture()
    {
        try
        {
            int deviceIdx = CbCameraList.SelectedIndex;
            if (deviceList == null || deviceIdx < 0)
            {
                Log("Select device before starting", LogMessageType.Error);
                return;
            }

            mediaCapture = new MediaCapture();

            var settings = new MediaCaptureInitializationSettings
            {
                VideoDeviceId = deviceList[deviceIdx].Id,
                StreamingCaptureMode = StreamingCaptureMode.AudioAndVideo
            };

            Log("Calling MediaCapture.InitializeAsync()...", LogMessageType.Message);
            await mediaCapture.InitializeAsync(settings);

            await PopulatePreviewSources();
        }
        catch (Exception ex)
        {
            Log($"MediaCapture initialization failed: {ex.Message}", LogMessageType.Error);

            if (mediaCapture != null)
            {
                mediaCapture.Dispose();
                mediaCapture = null;
            }
        }
    }

    private async Task PopulatePreviewSources()
    {
        Log("Populating preview sources...", LogMessageType.Message);

        try
        {
            if (mediaCapture == null)
                return;

            previewSourceList = new List<MediaFrameSource>();

            foreach (var source in mediaCapture.FrameSources.Values)
            {
                if (source.Info.MediaStreamType == MediaStreamType.VideoPreview ||
                    source.Info.MediaStreamType == MediaStreamType.VideoRecord)
                {
                    previewSourceList.Add(source);
                    Log($"Found preview source: {source.Info.SourceKind}", LogMessageType.Message);
                }
            }

            if (previewSourceList.Count > 0)
            {
                Log($"Found {previewSourceList.Count} preview source(s)", LogMessageType.Success);
            }
            else
            {
                Log("No preview sources found", LogMessageType.Error);
            }
        }
        catch (Exception ex)
        {
            Log($"Error populating preview sources: {ex.Message}", LogMessageType.Error);
        }
    }

    private void BtnPreview_Click(object sender, RoutedEventArgs e)
    {
        if (BtnPreview.Content.ToString() == "Start Preview")
        {
            StartPreview();
        }
        else
        {
            StopPreview();
        }
    }

    private bool StartPreview()
    {
        Log("Starting preview...", LogMessageType.Message);

        try
        {
            if (mediaCapture == null)
            {
                Log("MediaCapture not initialized", LogMessageType.Error);
                return false;
            }

            if (previewSourceList == null || previewSourceList.Count == 0)
            {
                Log("No preview sources available", LogMessageType.Error);
                return false;
            }

            currentPreviewSource = previewSourceList[0];

            mediaPlayer = new MediaPlayer
            {
                RealTimePlayback = true,
                AutoPlay = false,
                Source = MediaSource.CreateFromMediaFrameSource(currentPreviewSource)
            };

            PreviewControl.SetMediaPlayer(mediaPlayer);
            mediaPlayer.Play();

            BtnPreview.Content = "Stop Preview";

            Log("Preview started successfully", LogMessageType.Success);
            return true;
        }
        catch (Exception ex)
        {
            Log($"Error starting preview: {ex.Message}", LogMessageType.Error);
            return false;
        }
    }

    private void StopPreview()
    {
        Log("Stopping preview...", LogMessageType.Message);

        try
        {
            if (mediaPlayer != null)
            {
                mediaPlayer.Pause();
                mediaPlayer = null;
            }

            BtnPreview.Content = "Start Preview";

            Log("Preview stopped", LogMessageType.Success);
        }
        catch (Exception ex)
        {
            Log($"Error stopping preview: {ex.Message}", LogMessageType.Error);
        }
    }

    private async void BtnRecord_Click(object sender, RoutedEventArgs e)
    {
        if (mediaCapture == null)
        {
            Log("Initialize MediaCapture before recording.", LogMessageType.Error);
            return;
        }

        if (!isRecording)
        {
            await StartRecordingAsync();
        }
        else
        {
            await StopRecordingAsync();
        }
    }

    private async Task StartRecordingAsync()
    {
        try
        {
            Log("Preparing capture file storage...", LogMessageType.Message);
            StorageLibrary myVideos = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Videos);

            StorageFile file = await myVideos.SaveFolder.CreateFileAsync("video.mp4", CreationCollisionOption.GenerateUniqueName);
            MediaEncodingProfile encodingProfile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Auto);

            mediaCapture!.RecordLimitationExceeded += OnMediaCaptureRecordLimitationExceeded;

            Log("Initializing recording profile...", LogMessageType.Message);
            mediaRecording = await mediaCapture.PrepareLowLagRecordToStorageFileAsync(encodingProfile, file);

            await mediaRecording.StartAsync();

            isRecording = true;
            BtnRecord.Content = "Stop Recording";
            Log($"Successfully recording capture live to: {file.Path}", LogMessageType.Success);
        }
        catch (Exception ex)
        {
            Log($"Failed to start recording: {ex.Message}", LogMessageType.Error);
            isRecording = false;
            BtnRecord.Content = "Start Recording";
        }
    }

    private async Task StopRecordingAsync()
    {
        if (mediaRecording == null || !isRecording) return;

        try
        {
            Log("Sending stop session token command...", LogMessageType.Message);
            await mediaRecording.StopAsync();

            Log("Invoking finalization file IO flushes...", LogMessageType.Message);
            await mediaRecording.FinishAsync();
        }
        catch (Exception ex)
        {
            Log($"Error during stop pipeline: {ex.Message}", LogMessageType.Error);
        }
        finally
        {
            if (mediaCapture != null)
            {
                mediaCapture.RecordLimitationExceeded -= OnMediaCaptureRecordLimitationExceeded;
            }

            mediaRecording = null;
            isRecording = false;
            BtnRecord.Content = "Start Recording";
            Log("Recording pipeline dropped and saved clean.", LogMessageType.Success);
        }
    }

    private void OnMediaCaptureRecordLimitationExceeded(MediaCapture sender)
    {
        Log("System tracking limit threshold breached. Halting context.", LogMessageType.Error);

        _ = this.DispatcherQueue.EnqueueAsync(async () =>
        {
            await StopRecordingAsync();
        });
    }

    private void Log(string message, LogMessageType type = LogMessageType.Message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        var logEntry = $"[{timestamp}] {message}\n";

        logBuilder.Append(logEntry);

#if DEBUG
        var typeStr = type switch
        {
            LogMessageType.Success => "✓",
            LogMessageType.Error => "✗",
            _ => "•"
        };
        Trace.WriteLine($"[IntVue] {typeStr} {message}");
#endif
    }
}
