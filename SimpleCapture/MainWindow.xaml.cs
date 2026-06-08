namespace SimpleCapture
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Media;
    using Windows.Devices.Enumeration;
    using Windows.Media;
    using Windows.Media.Capture;
    using Windows.Media.Capture.Frames;
    using Windows.Media.Core;
    using Windows.Media.MediaProperties;
    using Windows.Media.Playback;

    public sealed partial class MainWindow : Window
    {
        private enum LogMessageType
        {
            Message,
            Success,
            Error
        }

        private MediaCapture? m_MediaCapture;
        private MediaPlayer? m_MediaPlayer;
        private List<DeviceInformation>? m_deviceList;
        private List<MediaFrameSource>? m_previewSourceList;
        private MediaFrameSource? m_currentPreviewSource;
        private StringBuilder m_logBuilder = new StringBuilder();

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
                m_deviceList = (await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture)).ToList();

                if (m_deviceList.Count == 0)
                {
                    Log("No camera devices found!", LogMessageType.Error);
                    return;
                }

                foreach (var device in m_deviceList)
                {
                    cbDeviceList.Items.Add(device.Name);
                    Log($"Found device: {device.Name} (ID: {device.Id})", LogMessageType.Message);
                }

                cbDeviceList.SelectedIndex = 0;
                Log($"Found {m_deviceList.Count} camera(s)", LogMessageType.Success);
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
                if (m_deviceList == null || deviceIdx < 0)
                {
                    Log("Select device before starting", LogMessageType.Error);
                    return;
                }

                m_MediaCapture = new MediaCapture();
                var settings = new MediaCaptureInitializationSettings
                {
                    VideoDeviceId = m_deviceList[deviceIdx].Id,
                    StreamingCaptureMode = StreamingCaptureMode.Video
                };

                int memoryPrefIdx = cbMemoryPreferenceList.SelectedIndex;
                if (memoryPrefIdx == 1)
                    settings.MemoryPreference = MediaCaptureMemoryPreference.Auto;
                else if (memoryPrefIdx == 2)
                    settings.MemoryPreference = MediaCaptureMemoryPreference.Cpu;

                Log("Calling MediaCapture.InitializeAsync()...", LogMessageType.Message);
                await m_MediaCapture.InitializeAsync(settings);

                // Populate preview sources
                await PopulatePreviewSources();

                txtSettings.Text = $"Device: {m_deviceList[deviceIdx].Name}\nStreamingMode: Video";
            }
            catch (Exception ex)
            {
                Log($"MediaCapture initialization failed: {ex.Message}", LogMessageType.Error);
                if (m_MediaCapture != null)
                {
                    m_MediaCapture.Dispose();
                    m_MediaCapture = null;
                }
            }
        }

        private async Task PopulatePreviewSources()
        {
            Log("Populating preview sources...", LogMessageType.Message);

            try
            {
                if (m_MediaCapture == null)
                    return;

                cbPreviewSourceList.Items.Clear();
                m_previewSourceList = new List<MediaFrameSource>();

                foreach (var source in m_MediaCapture.FrameSources.Values)
                {
                    if (source.Info.MediaStreamType == MediaStreamType.VideoPreview ||
                        source.Info.MediaStreamType == MediaStreamType.VideoRecord)
                    {
                        m_previewSourceList.Add(source);
                        cbPreviewSourceList.Items.Add($"{source.Info.SourceKind}");
                        Log($"Found preview source: {source.Info.SourceKind}", LogMessageType.Message);
                    }
                }

                if (m_previewSourceList.Count > 0)
                {
                    cbPreviewSourceList.SelectedIndex = 0;
                    Log($"Found {m_previewSourceList.Count} preview source(s)", LogMessageType.Success);
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
                if (m_MediaCapture == null)
                {
                    Log("MediaCapture not initialized", LogMessageType.Error);
                    return false;
                }

                int idx = cbPreviewSourceList.SelectedIndex;
                if (m_previewSourceList == null || idx < 0)
                {
                    Log("Select preview source", LogMessageType.Error);
                    return false;
                }

                m_currentPreviewSource = m_previewSourceList[idx];

                m_MediaPlayer = new MediaPlayer
                {
                    RealTimePlayback = true,
                    AutoPlay = false,
                    Source = MediaSource.CreateFromMediaFrameSource(m_currentPreviewSource)
                };

                myPreview.SetMediaPlayer(m_MediaPlayer);
                m_MediaPlayer.Play();

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
                if (m_MediaPlayer != null)
                {
                    m_MediaPlayer.Pause();
                    m_MediaPlayer = null;
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

                if (m_MediaCapture != null)
                {
                    m_MediaCapture.Dispose();
                    m_MediaCapture = null;
                }

                btnPreview.IsEnabled = false;
                btnReset.IsEnabled = false;
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

        private void Log(string message, LogMessageType type = LogMessageType.Message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            var logEntry = $"[{timestamp}] {message}\n";

            m_logBuilder.Append(logEntry);
            txtLog.Text = m_logBuilder.ToString();

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
}
