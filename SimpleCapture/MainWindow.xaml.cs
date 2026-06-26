using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.WinUI;

using Microsoft.UI.Xaml;

using Windows.Devices.Enumeration;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Media.Playback;
using Windows.Storage;

namespace SimpleCapture;

public sealed partial class MainWindow : Window
{
    private enum LogMessageType
    {
        Message,
        Success,
        Error
    }

    private MediaCapture? _mediaCapture;
    private MediaPlayer? _mediaPlayer;
    private List<DeviceInformation>? _deviceList;
    private List<MediaFrameSource>? _previewSourceList;
    private MediaFrameSource? _currentPreviewSource;
    private StringBuilder _logBuilder = new StringBuilder();

    // 1. Declare class variable for media recording session
    private LowLagMediaRecording? _mediaRecording;
    private bool _isRecording = false;

    public MainWindow()
    {
        this.InitializeComponent();
        this.AppWindow.Title = "Simple Camera Capture - Microsoft Sample";

        InitializeUI();
        Log("Application started", LogMessageType.Message);
    }

    private void InitializeUI()
    {
        // Populate capture mode options
        cbCaptureMode.Items.Add("Video");
        cbCaptureMode.Items.Add("Video");
        cbCaptureMode.Items.Add("AudioAndVideo");
        cbCaptureMode.SelectedIndex = 0;

        // Populate memory preference options
        cbMemoryPreferenceList.Items.Add("Default");
        cbMemoryPreferenceList.Items.Add("Auto");
        cbMemoryPreferenceList.Items.Add("Cpu");
        cbMemoryPreferenceList.SelectedIndex = 0;

        PopulateCameraList();
    }

    private async void PopulateCameraList()
    {
        Log("Enumerating camera devices...", LogMessageType.Message);
        cbDeviceList.Items.Clear();

        try
        {
            // Enumerate video capture devices
            _deviceList = (await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture)).ToList();

            if (_deviceList.Count == 0)
            {
                Log("No camera devices found!", LogMessageType.Error);
                return;
            }

            foreach (var device in _deviceList)
            {
                cbDeviceList.Items.Add(device.Name);
                Log($"Found device: {device.Name} (ID: {device.Id})", LogMessageType.Message);
            }

            cbDeviceList.SelectedIndex = 0;
            Log($"Found {_deviceList.Count} camera(s)", LogMessageType.Success);
        }
        catch (Exception ex)
        {
            Log($"Error enumerating devices: {ex.Message}", LogMessageType.Error);
        }
    }

    private async void BtnStartDevice_Click(object sender, RoutedEventArgs e)
    {
        Log("Initializing device...", LogMessageType.Message);

        try
        {
            await InitializeMediaCapture();
            btnPreview.IsEnabled = true;
            btnReset.IsEnabled = true;
            btnRecord.IsEnabled = true;
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
            int deviceIdx = cbDeviceList.SelectedIndex;
            if (_deviceList == null || deviceIdx < 0)
            {
                Log("Select device before starting", LogMessageType.Error);
                return;
            }

            _mediaCapture = new MediaCapture();

            // Map UI Selection to the proper StreamingCaptureMode
            StreamingCaptureMode captureMode = StreamingCaptureMode.Video;
            if (cbCaptureMode.SelectedIndex == 1)
            {
                captureMode = StreamingCaptureMode.Audio;
            }
            else if (cbCaptureMode.SelectedIndex == 2)
            {
                captureMode = StreamingCaptureMode.AudioAndVideo;
            }

            var settings = new MediaCaptureInitializationSettings
            {
                VideoDeviceId = _deviceList[deviceIdx].Id,
                StreamingCaptureMode = captureMode
            };

            int memoryPrefIdx = cbMemoryPreferenceList.SelectedIndex;
            if (memoryPrefIdx == 1)
            {
                settings.MemoryPreference = MediaCaptureMemoryPreference.Auto;

            }
            else if (memoryPrefIdx == 2)
            {
                settings.MemoryPreference = MediaCaptureMemoryPreference.Cpu;
            }

            Log("Calling MediaCapture.InitializeAsync()...", LogMessageType.Message);
            await _mediaCapture.InitializeAsync(settings);

            // Populate preview sources
            await PopulatePreviewSources();

            txtSettings.Text = $"Device: {_deviceList[deviceIdx].Name}\nStreamingMode: Video";
        }
        catch (Exception ex)
        {
            Log($"MediaCapture initialization failed: {ex.Message}", LogMessageType.Error);

            if (_mediaCapture != null)
            {
                _mediaCapture.Dispose();
                _mediaCapture = null;
            }
        }
    }

    private async Task PopulatePreviewSources()
    {
        Log("Populating preview sources...", LogMessageType.Message);

        try
        {
            if (_mediaCapture == null)
                return;

            cbPreviewSourceList.Items.Clear();
            _previewSourceList = new List<MediaFrameSource>();

            foreach (var source in _mediaCapture.FrameSources.Values)
            {
                if (source.Info.MediaStreamType == MediaStreamType.VideoPreview ||
                    source.Info.MediaStreamType == MediaStreamType.VideoRecord)
                {
                    _previewSourceList.Add(source);
                    cbPreviewSourceList.Items.Add($"{source.Info.SourceKind}");
                    Log($"Found preview source: {source.Info.SourceKind}", LogMessageType.Message);
                }
            }

            if (_previewSourceList.Count > 0)
            {
                cbPreviewSourceList.SelectedIndex = 0;
                Log($"Found {_previewSourceList.Count} preview source(s)", LogMessageType.Success);
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
        if (btnPreview.Content.ToString() == "Start Preview")
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
            if (_mediaCapture == null)
            {
                Log("MediaCapture not initialized", LogMessageType.Error);
                return false;
            }

            int idx = cbPreviewSourceList.SelectedIndex;
            if (_previewSourceList == null || idx < 0)
            {
                Log("Select preview source", LogMessageType.Error);
                return false;
            }

            _currentPreviewSource = _previewSourceList[idx];

            _mediaPlayer = new MediaPlayer
            {
                RealTimePlayback = true,
                AutoPlay = false,
                Source = MediaSource.CreateFromMediaFrameSource(_currentPreviewSource)
            };

            myPreview.SetMediaPlayer(_mediaPlayer);
            _mediaPlayer.Play();

            btnPreview.Content = "Stop Preview";
            cbPreviewSourceList.IsEnabled = false;

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
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Pause();
                _mediaPlayer = null;
            }

            btnPreview.Content = "Start Preview";
            cbPreviewSourceList.IsEnabled = true;

            Log("Preview stopped", LogMessageType.Success);
        }
        catch (Exception ex)
        {
            Log($"Error stopping preview: {ex.Message}", LogMessageType.Error);
        }
    }

    private void BtnReset_Click(object sender, RoutedEventArgs e)
    {
        Log("Resetting device...", LogMessageType.Message);

        try
        {
            StopPreview();

            if (_mediaCapture != null)
            {
                _mediaCapture.Dispose();
                _mediaCapture = null;
            }

            btnPreview.IsEnabled = false;
            btnReset.IsEnabled = false;
            btnRecord.IsEnabled = false;
            btnPreview.Content = "Start Preview";
            cbPreviewSourceList.IsEnabled = true;
            txtSettings.Text = "";

            Log("Device reset", LogMessageType.Success);
        }
        catch (Exception ex)
        {
            Log($"Error resetting device: {ex.Message}", LogMessageType.Error);
        }
    }

    // 2. Action Handler linked to your "Record" button UI
    private async void BtnRecord_Click(object sender, RoutedEventArgs e)
    {
        if (_mediaCapture == null)
        {
            Log("Initialize MediaCapture before recording.", LogMessageType.Error);
            return;
        }

        if (!_isRecording)
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

            // Track selected mode: 0 = Video, 1 = Audio, 2 = AudioAndVideo
            int selectedMode = cbCaptureMode.SelectedIndex;
            MediaEncodingProfile encodingProfile;
            StorageFile file;

            if (selectedMode == 1) // Audio Only
            {
                file = await myVideos.SaveFolder.CreateFileAsync("audio.mp3", CreationCollisionOption.GenerateUniqueName);
                encodingProfile = MediaEncodingProfile.CreateMp3(AudioEncodingQuality.High);
            }
            else // Video Only or Audio and Video
            {
                file = await myVideos.SaveFolder.CreateFileAsync("video.mp4", CreationCollisionOption.GenerateUniqueName);
                encodingProfile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Auto);
            }

            // Register OS max limit rule event (3 hours limit)
            _mediaCapture!.RecordLimitationExceeded += M_mediaCapture_RecordLimitationExceeded;

            Log("Initializing recording profile...", LogMessageType.Message);
            _mediaRecording = await _mediaCapture.PrepareLowLagRecordToStorageFileAsync(encodingProfile, file);

            await _mediaRecording.StartAsync();

            _isRecording = true;
            btnRecord.Content = "Stop Recording"; // Update your button element name accordingly
            txtStatus.Text = $"í´´ RECORDING -> {file.Name}";
            Log($"Successfully recording capture live to: {file.Path}", LogMessageType.Success);
        }
        catch (Exception ex)
        {
            Log($"Failed to start recording: {ex.Message}", LogMessageType.Error);
            _isRecording = false;
            btnRecord.Content = "Start Recording";
        }
    }

    private async Task StopRecordingAsync()
    {
        if (_mediaRecording == null || !_isRecording) return;

        try
        {
            Log("Sending stop session token command...", LogMessageType.Message);
            await _mediaRecording.StopAsync();

            Log("Invoking finalization file IO flushes...", LogMessageType.Message);
            await _mediaRecording.FinishAsync();
        }
        catch (Exception ex)
        {
            Log($"Error during stop pipeline: {ex.Message}", LogMessageType.Error);
        }
        finally
        {
            if (_mediaCapture != null)
            {
                _mediaCapture.RecordLimitationExceeded -= M_mediaCapture_RecordLimitationExceeded;
            }

            _mediaRecording = null;
            _isRecording = false;
            btnRecord.Content = "Start Recording";
            Log("Recording pipeline dropped and saved clean.", LogMessageType.Success);
        }
    }

    // Handles background execution interruption loops gracefully if limits hit
    private async void M_mediaCapture_RecordLimitationExceeded(MediaCapture sender)
    {

        Log("System tracking limit threshold breached. Halting context.", LogMessageType.Error);

        // Marshall background hardware event handler thread cleanly onto WinUI UI Thread
        _ = this.DispatcherQueue.EnqueueAsync(async () =>
            {
                await StopRecordingAsync();
                txtStatus.Text = "Record limitation exceeded.";
            });
    }

    private void ResetMediaCapturePipeline()
    {
        if (_mediaCapture != null)
        {
            _mediaCapture.RecordLimitationExceeded -= M_mediaCapture_RecordLimitationExceeded;
            _mediaCapture.Dispose();
            _mediaCapture = null;
        }
    }

    private void Log(string message, LogMessageType type = LogMessageType.Message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        var logEntry = $"[{timestamp}] {message}\n";

        _logBuilder.Append(logEntry);
        txtLog.Text = _logBuilder.ToString();

#if DEBUG
        var typeStr = type switch
        {
            LogMessageType.Success => "✓",
            LogMessageType.Error => "✗",
            _ => "•"
        };
        Trace.WriteLine($"[SimpleCapture] {typeStr} {message}");
#endif
    }
}
