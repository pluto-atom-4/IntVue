// <copyright file="MediaCaptureService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace IntVue.Services
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Threading;
    using System.Threading.Tasks;

    using IntVue.Helpers;

    using Microsoft.UI.Xaml.Controls;

    using Windows.Devices.Enumeration;
    using Windows.Media.Capture;
    using Windows.Media.Capture.Frames;
    using Windows.Media.Core;
    using Windows.Media.MediaProperties;
    using Windows.Media.Playback;
    using Windows.Storage;

    /// <summary>
    /// MediaCaptureService implements IMediaCaptureService using Windows.Media.Capture.
    /// Renders camera preview via MediaPlayerElement (MediaSource + MediaPlayer) and records to ApplicationData.LocalFolder.
    /// Uses the Microsoft-recommended approach per WinUI 3 camera quickstart.
    /// </summary>
    [SupportedOSPlatform("windows10.0.17763.0")]
    public class MediaCaptureService : IMediaCaptureService, IAsyncDisposable, IDisposable
    {
        private MediaCapture? mediaCapture;
        private MediaPlayer? previewMediaPlayer;
        private MediaSource? previewMediaSource;
        private LowLagMediaRecording? lowLagRecording;
        private StorageFile? currentFile;
        private bool initialized;

        /// <summary>
        /// Gets a value indicating whether a recording is currently in progress.
        /// </summary>
        public bool IsRecording => this.lowLagRecording != null;

        /// <summary>
        /// Initialize the underlying MediaCapture resources and select a camera device.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to abort initialization.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous initialization operation.</returns>
        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            if (this.initialized)
            {
#if DEBUG
                Trace.WriteLine("[IntVue.Debug] MediaCaptureService.InitializeAsync: Already initialized, skipping.");
#endif
                return;
            }

#if DEBUG
            Trace.WriteLine("[IntVue.Debug] MediaCaptureService.InitializeAsync: Starting initialization...");
#endif

            this.mediaCapture = new MediaCapture();

#if DEBUG
            Trace.WriteLine("[IntVue.Debug] MediaCaptureService.InitializeAsync: MediaCapture instance created.");
            Trace.WriteLine("[IntVue.Debug] MediaCaptureService.InitializeAsync: Enumerating video capture devices...");
#endif

            var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

#if DEBUG
            Debug.WriteLine($"[IntVue.Debug] MediaCaptureService.InitializeAsync: Found {devices.Count} video device(s).");
#endif

            if (devices.Count == 0)
            {
#if DEBUG
                Trace.WriteLine("[IntVue.Debug] MediaCaptureService.InitializeAsync: WARNING - No camera device found. Preview mode disabled.");
#endif
                this.initialized = true;
                return;
            }

            DeviceInformation? front = null;
            for (var i = 0; i < devices.Count; i++)
            {
                var d = devices[i];
#if DEBUG
                Debug.WriteLine($"[IntVue.Debug] MediaCaptureService.InitializeAsync: Device[{i}]: Name='{d.Name}', ID='{d.Id}', EnclosureLocation={d.EnclosureLocation?.Panel}");
#endif
                if (d.EnclosureLocation != null && d.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Front)
                {
                    front = d;
#if DEBUG
                    Debug.WriteLine($"[IntVue.Debug] MediaCaptureService.InitializeAsync: Front camera selected - '{front.Name}'");
#endif
                    break;
                }
            }

            if (front == null && devices.Count > 0)
            {
                front = devices[0];
#if DEBUG
                Debug.WriteLine($"[IntVue.Debug] MediaCaptureService.InitializeAsync: No front camera found, using first device - '{front.Name}'");
#endif
            }

#if DEBUG
            Debug.WriteLine($"[IntVue.Debug] MediaCaptureService.InitializeAsync: Selected device ID: '{front?.Id}'");
            Trace.WriteLine("[IntVue.Debug] MediaCaptureService.InitializeAsync: Calling MediaCapture.InitializeAsync()...");
#endif

            var settings = new MediaCaptureInitializationSettings
            {
                VideoDeviceId = front?.Id,
                StreamingCaptureMode = StreamingCaptureMode.AudioAndVideo,
            };

            try
            {
                await this.mediaCapture.InitializeAsync(settings);
#if DEBUG
                Trace.WriteLine("[IntVue.Debug] MediaCaptureService.InitializeAsync: MediaCapture.InitializeAsync() completed successfully.");

                // Phase 1: Enumerate all frame sources and their formats (diagnostic)
                Trace.WriteLine($"[IntVue.Debug] MediaCaptureService.InitializeAsync: Available frame sources: {this.mediaCapture.FrameSources.Count}");
                foreach (var kvp in this.mediaCapture.FrameSources)
                {
                    var key = kvp.Key;
                    var frameSource = kvp.Value;
                    var sourceKind = frameSource.Info.SourceKind;
                    var format = frameSource.CurrentFormat;
                    var formatSubtype = format?.Subtype ?? "No format set";
                    var frameRate = format != null ? (double)format.FrameRate.Numerator / format.FrameRate.Denominator : 0;

                    Trace.WriteLine($"[IntVue.Debug] MediaCaptureService.InitializeAsync:   [{key}] Kind={sourceKind}, Format={formatSubtype}, FPS={frameRate:F2}");
                }

                // Detect Surface integrated camera
                var deviceIdPrefix = front?.Id.Substring(0, Math.Min(30, front?.Id.Length ?? 0)) ?? "Unknown";
                var isSurfaceIntegrated = front?.Id.Contains("DISPLAY", StringComparison.OrdinalIgnoreCase) ?? false;
                Trace.WriteLine($"[IntVue.Debug] MediaCaptureService.InitializeAsync: Device ID prefix: {deviceIdPrefix}...");
                if (isSurfaceIntegrated)
                {
                    Trace.WriteLine("[IntVue.Debug] MediaCaptureService.InitializeAsync: DETECTED: Surface integrated camera (DISPLAY in device ID)");
                }
#endif
                this.initialized = true;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine($"[IntVue.Debug] MediaCaptureService.InitializeAsync: ERROR - {ex.GetType().Name}: {ex.Message}");
#endif
                throw;
            }
        }

        /// <summary>
        /// Request camera and microphone permissions and return true if both are allowed.
        /// </summary>
        /// <returns>True when camera and microphone access are both allowed.</returns>
        public Task<bool> RequestPermissionsAsync()
        {
#if DEBUG
            Trace.WriteLine("[IntVue.Debug] MediaCaptureService.RequestPermissionsAsync: Checking camera and microphone permissions...");
#endif

            try
            {
                var camInfo = DeviceAccessInformation.CreateFromDeviceClass(DeviceClass.VideoCapture);
                var micInfo = DeviceAccessInformation.CreateFromDeviceClass(DeviceClass.AudioCapture);

                var camAllowed = camInfo?.CurrentStatus == DeviceAccessStatus.Allowed;
                var micAllowed = micInfo?.CurrentStatus == DeviceAccessStatus.Allowed;

#if DEBUG
                Debug.WriteLine($"[IntVue.Debug] MediaCaptureService.RequestPermissionsAsync: Camera permission={camInfo?.CurrentStatus}, Microphone permission={micInfo?.CurrentStatus}");
                Debug.WriteLine($"[IntVue.Debug] MediaCaptureService.RequestPermissionsAsync: Both allowed={camAllowed && micAllowed}");
#endif

                return Task.FromResult(camAllowed && micAllowed);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine($"[IntVue.Debug] MediaCaptureService.RequestPermissionsAsync: ERROR - {ex.GetType().Name}: {ex.Message}");
#endif
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Start camera preview using MediaSource and MediaPlayer.
        /// Uses the Microsoft-recommended approach: MediaCapture → MediaFrameSource → MediaSource → MediaPlayer → MediaPlayerElement.
        /// </summary>
        /// <param name="previewHost">MediaPlayerElement to render the preview stream.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task StartPreviewAsync(object previewHost)
        {
#if DEBUG
            Trace.WriteLine("[IntVue.Debug] MediaCaptureService.StartPreviewAsync: Starting preview...");
#endif

            if (this.mediaCapture == null)
            {
#if DEBUG
                Trace.WriteLine("[IntVue.Debug] MediaCaptureService.StartPreviewAsync: MediaCapture is null, initializing...");
#endif
                await this.InitializeAsync().ConfigureAwait(false);
            }

            if (this.mediaCapture == null)
            {
#if DEBUG
                Trace.WriteLine("[IntVue.Debug] MediaCaptureService.StartPreviewAsync: ERROR - MediaCapture failed to initialize.");
#endif
                throw new InvalidOperationException("MediaCapture not initialized");
            }

#if DEBUG
            Trace.WriteLine("[IntVue.Debug] MediaCaptureService.StartPreviewAsync: MediaCapture is initialized.");
#endif

            if (previewHost is not MediaPlayerElement mediaPlayerElement)
            {
#if DEBUG
                Debug.WriteLine($"[IntVue.Debug] MediaCaptureService.StartPreviewAsync: ERROR - previewHost is not MediaPlayerElement, it is {previewHost?.GetType().Name ?? "null"}");
#endif
                throw new ArgumentException("previewHost must be a MediaPlayerElement", nameof(previewHost));
            }

#if DEBUG
            Trace.WriteLine("[IntVue.Debug] MediaCaptureService.StartPreviewAsync: Preview host is MediaPlayerElement.");
#endif

            try
            {
#if DEBUG
                Debug.WriteLine($"[IntVue.Debug] MediaCaptureService.StartPreviewAsync: FrameSources count: {this.mediaCapture.FrameSources.Count}");

                // Phase 1: Enumerate all frame sources to understand what's available
                foreach (var kvp in this.mediaCapture.FrameSources)
                {
                    var key = kvp.Key;
                    var src = kvp.Value;
                    var kind = src.Info.SourceKind;
                    var currentFormat = src.CurrentFormat;
                    var subtype = currentFormat?.Subtype ?? "No format";
                    var frameRate = currentFormat != null ? (double)currentFormat.FrameRate.Numerator / currentFormat.FrameRate.Denominator : 0;
                    Trace.WriteLine($"[IntVue.Debug] MediaCaptureService.StartPreviewAsync: FrameSource[{key}] Kind={kind}, Format={subtype}, FPS={frameRate:F2}");
                }
#endif

                // Get the best video frame source: prefer Color over Image/Depth/Audio
                // For Surface cameras with multiple frame sources, explicitly select the best one
                var frameSource = this.mediaCapture.FrameSources.Values
                    .Where(fs => fs.Info.SourceKind == MediaFrameSourceKind.Color)
                    .OrderByDescending(fs => fs.CurrentFormat?.FrameRate.Numerator ?? 0)
                    .FirstOrDefault()
                    ?? this.mediaCapture.FrameSources.Values.FirstOrDefault();

#if DEBUG
                if (frameSource != null)
                {
                    var selectedKind = frameSource.Info.SourceKind;
                    var selectedFormat = frameSource.CurrentFormat?.Subtype ?? "Unknown";
                    var selectedFps = frameSource.CurrentFormat != null
                        ? (double)frameSource.CurrentFormat.FrameRate.Numerator / frameSource.CurrentFormat.FrameRate.Denominator
                        : 0;
                    Trace.WriteLine($"[IntVue.Debug] MediaCaptureService.StartPreviewAsync: Selected frame source - Kind={selectedKind}, Format={selectedFormat}, FPS={selectedFps:F2}");
                }
#endif

                if (frameSource == null)
                {
#if DEBUG
                    Trace.WriteLine("[IntVue.Debug] MediaCaptureService.StartPreviewAsync: ERROR - No video frame source available from MediaCapture.");
#endif
                    throw new InvalidOperationException("No video frame source available from MediaCapture");
                }

#if DEBUG
                Debug.WriteLine($"[IntVue.Debug] MediaCaptureService.StartPreviewAsync: Frame source obtained - {frameSource.GetType().Name}");
#endif

                // Create MediaSource from the frame source
#if DEBUG
                Trace.WriteLine("[IntVue.Debug] MediaCaptureService.StartPreviewAsync: Creating MediaSource from frame source...");
                Trace.WriteLine($"[IntVue.Debug] MediaCaptureService.StartPreviewAsync:   FrameSource type: {frameSource.GetType().Name}");
#endif
                this.previewMediaSource = MediaSource.CreateFromMediaFrameSource(frameSource);

#if DEBUG
                Trace.WriteLine("[IntVue.Debug] MediaCaptureService.StartPreviewAsync: MediaSource created successfully.");
                Trace.WriteLine($"[IntVue.Debug] MediaCaptureService.StartPreviewAsync:   MediaSource type: {this.previewMediaSource?.GetType().Name}");
                Trace.WriteLine($"[IntVue.Debug] MediaCaptureService.StartPreviewAsync:   MediaSource state: {(this.previewMediaSource != null ? "Valid" : "Null")}");
#endif

                // Clean up any previous MediaPlayer before creating new one (fixes resource leak on double-start)
#if DEBUG
                Trace.WriteLine("[IntVue.Debug] MediaCaptureService.StartPreviewAsync: Checking for previous MediaPlayer to clean up...");
#endif
                if (this.previewMediaPlayer != null)
                {
                    try
                    {
#if DEBUG
                        Trace.WriteLine("[IntVue.Debug] MediaCaptureService.StartPreviewAsync: Previous MediaPlayer exists, disposing...");
                        Trace.WriteLine($"[IntVue.Debug] MediaCaptureService.StartPreviewAsync:   State before cleanup - Source: {(this.previewMediaPlayer.Source != null ? "Set" : "Null")}, IsMuted: {this.previewMediaPlayer.IsMuted}");
#endif
                        this.previewMediaPlayer.Source = null;
                        this.previewMediaPlayer.Dispose();

#if DEBUG
                        Trace.WriteLine("[IntVue.Debug] MediaCaptureService.StartPreviewAsync: Previous MediaPlayer disposed successfully.");
#endif
                    }
                    catch (Exception cleanupEx)
                    {
#if DEBUG
                        Trace.WriteLine($"[IntVue.Debug] MediaCaptureService.StartPreviewAsync: WARNING - Error during MediaPlayer cleanup: {cleanupEx.GetType().Name}");
                        Trace.WriteLine($"[IntVue.Debug] MediaCaptureService.StartPreviewAsync:   Error message: {cleanupEx.Message}");
#endif
                    }
                    finally
                    {
                        this.previewMediaPlayer = null;
                    }
                }
#if DEBUG
                else
                {
                    Trace.WriteLine("[IntVue.Debug] MediaCaptureService.StartPreviewAsync: No previous MediaPlayer to clean up.");
                }
#endif

                // Create MediaPlayer and bind to MediaPlayerElement
#if DEBUG
                Trace.WriteLine("[IntVue.Debug] MediaCaptureService.StartPreviewAsync: Creating new MediaPlayer...");
#endif
                this.previewMediaPlayer = new MediaPlayer();
                this.previewMediaPlayer.AutoPlay = true;
                this.previewMediaPlayer.Source = this.previewMediaSource;

#if DEBUG
                Trace.WriteLine("[IntVue.Debug] MediaCaptureService.StartPreviewAsync: AutoPlay enabled on MediaPlayer.");
#endif

#if DEBUG
                Trace.WriteLine("[IntVue.Debug] MediaCaptureService.StartPreviewAsync: Checking UI thread affinity before SetMediaPlayer...");
#endif

                // Validate MediaPlayer state before binding (diagnostic)
#if DEBUG
                Trace.WriteLine($"[IntVue.Debug] MediaCaptureService.StartPreviewAsync: MediaPlayer validation - Source={this.previewMediaPlayer.Source != null}, AutoPlay={this.previewMediaPlayer.AutoPlay}");
#endif

                // Ensure SetMediaPlayer is called on the UI thread (fixes COMException from threading issues)
                try
                {
#if DEBUG
                    var threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
                    Trace.WriteLine($"[IntVue.Debug] MediaCaptureService.StartPreviewAsync: Current thread ID: {threadId}");
                    Trace.WriteLine("[IntVue.Debug] MediaCaptureService.StartPreviewAsync: Checking UI thread affinity...");
#endif

                    if (!mediaPlayerElement.DispatcherQueue.HasThreadAccess)
                    {
#if DEBUG
                        Trace.WriteLine($"[IntVue.Debug] MediaCaptureService.StartPreviewAsync: Not on UI thread, DispatcherQueue state: HasThreadAccess=false");
                        Trace.WriteLine("[IntVue.Debug] MediaCaptureService.StartPreviewAsync: Marshaling SetMediaPlayer call to UI thread using TryEnqueue...");
#endif

                        // Marshal to UI thread using DispatcherQueue.TryEnqueue
                        var startTime = System.Diagnostics.Stopwatch.StartNew();
                        bool enqueued = mediaPlayerElement.DispatcherQueue.TryEnqueue(
                            Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal,
                            () =>
                            {
#if DEBUG
                                Trace.WriteLine("[IntVue.Debug] MediaCaptureService.StartPreviewAsync: SetMediaPlayer executing on UI thread...");
#endif
                                mediaPlayerElement.SetMediaPlayer(this.previewMediaPlayer);
                            });
                        startTime.Stop();

#if DEBUG
                        Trace.WriteLine($"[IntVue.Debug] MediaCaptureService.StartPreviewAsync: TryEnqueue result: {(enqueued ? "Success" : "Failed")}, Time: {startTime.ElapsedMilliseconds}ms");
#endif

                        if (!enqueued)
                        {
#if DEBUG
                            Trace.WriteLine("[IntVue.Debug] MediaCaptureService.StartPreviewAsync: ERROR - Failed to enqueue SetMediaPlayer on UI thread. DispatcherQueue may be invalid.");
#endif
                            throw new InvalidOperationException("Failed to marshal SetMediaPlayer call to UI thread");
                        }
                    }
                    else
                    {
#if DEBUG
                        Trace.WriteLine("[IntVue.Debug] MediaCaptureService.StartPreviewAsync: Already on UI thread (HasThreadAccess=true), calling SetMediaPlayer directly...");
#endif
                        var startTime = System.Diagnostics.Stopwatch.StartNew();
                        mediaPlayerElement.SetMediaPlayer(this.previewMediaPlayer);
                        startTime.Stop();

#if DEBUG
                        Trace.WriteLine($"[IntVue.Debug] MediaCaptureService.StartPreviewAsync: SetMediaPlayer call completed. Time: {startTime.ElapsedMilliseconds}ms");
#endif
                    }

                    // Explicitly start playback for frame sources (AutoPlay doesn't work reliably with MediaFrameSource)
                    if (this.previewMediaPlayer.PlaybackSession.PlaybackState != Windows.Media.Playback.MediaPlaybackState.Playing)
                    {
#if DEBUG
                        Trace.WriteLine("[IntVue.Debug] MediaCaptureService.StartPreviewAsync: PlaybackState not Playing, calling Play()...");
#endif
                        this.previewMediaPlayer.Play();
                    }

#if DEBUG
                    Trace.WriteLine("[IntVue.Debug] MediaCaptureService.StartPreviewAsync: Preview binding successful. MediaPlayer is now rendering.");

                    // Phase 1: Validate MediaPlayer post-binding state
                    Trace.WriteLine("[IntVue.Debug] MediaCaptureService.StartPreviewAsync: Post-binding validation:");
                    Trace.WriteLine($"[IntVue.Debug] MediaCaptureService.StartPreviewAsync:   MediaPlayer.Source != null: {this.previewMediaPlayer.Source != null}");
                    Trace.WriteLine($"[IntVue.Debug] MediaCaptureService.StartPreviewAsync:   MediaPlayer.PlaybackSession.PlaybackState: {this.previewMediaPlayer.PlaybackSession.PlaybackState}");
                    Trace.WriteLine($"[IntVue.Debug] MediaCaptureService.StartPreviewAsync:   MediaPlayer.IsMuted: {this.previewMediaPlayer.IsMuted}");
                    Trace.WriteLine($"[IntVue.Debug] MediaCaptureService.StartPreviewAsync:   MediaPlayer.Volume: {this.previewMediaPlayer.Volume}");
                    Trace.WriteLine($"[IntVue.Debug] MediaCaptureService.StartPreviewAsync:   MediaPlayer.AutoPlay: {this.previewMediaPlayer.AutoPlay}");
#endif
                }
                catch (COMException comEx)
                {
#if DEBUG
                    Trace.WriteLine($"[IntVue.Debug] MediaCaptureService.StartPreviewAsync: COMException caught during SetMediaPlayer");
                    Trace.WriteLine($"[IntVue.Debug] MediaCaptureService.StartPreviewAsync:   HResult: 0x{comEx.HResult:X8}");
                    Trace.WriteLine($"[IntVue.Debug] MediaCaptureService.StartPreviewAsync:   Message: {comEx.Message}");
                    Trace.WriteLine($"[IntVue.Debug] MediaCaptureService.StartPreviewAsync:   Source: {comEx.Source}");
                    if (comEx.InnerException != null)
                    {
                        Trace.WriteLine($"[IntVue.Debug] MediaCaptureService.StartPreviewAsync:   InnerException: {comEx.InnerException.GetType().Name} - {comEx.InnerException.Message}");
                    }
#endif
                    throw new InvalidOperationException(
                        "Failed to bind MediaPlayer to preview control. This may indicate a graphics driver issue or incompatible display settings. " +
                        "Try updating your graphics drivers or restarting your device.",
                        comEx);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Trace.WriteLine("[IntVue.Debug] MediaCaptureService.StartPreviewAsync: Unhandled exception during preview setup");
                Trace.WriteLine($"[IntVue.Debug] MediaCaptureService.StartPreviewAsync:   Exception type: {ex.GetType().Name}");
                Trace.WriteLine($"[IntVue.Debug] MediaCaptureService.StartPreviewAsync:   Message: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Trace.WriteLine($"[IntVue.Debug] MediaCaptureService.StartPreviewAsync:   InnerException: {ex.InnerException.GetType().Name} - {ex.InnerException.Message}");
                }

                Trace.WriteLine($"[IntVue.Debug] MediaCaptureService.StartPreviewAsync: StackTrace: {ex.StackTrace}");
#endif
                throw;
            }
        }

        /// <summary>
        /// Stop the camera preview if active.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task StopPreviewAsync()
        {
#if DEBUG
            Trace.WriteLine("[IntVue.Debug] MediaCaptureService.StopPreviewAsync: Stopping preview...");
#endif

            try
            {
                if (this.previewMediaPlayer != null)
                {
#if DEBUG
                    Trace.WriteLine("[IntVue.Debug] MediaCaptureService.StopPreviewAsync: Disposing MediaPlayer...");
#endif
                    this.previewMediaPlayer.Source = null;
                    this.previewMediaPlayer.Dispose();
                    this.previewMediaPlayer = null;
                }

                if (this.previewMediaSource != null)
                {
#if DEBUG
                    Trace.WriteLine("[IntVue.Debug] MediaCaptureService.StopPreviewAsync: Disposing MediaSource...");
#endif
                    this.previewMediaSource.Dispose();
                    this.previewMediaSource = null;
                }

#if DEBUG
                Trace.WriteLine("[IntVue.Debug] MediaCaptureService.StopPreviewAsync: Preview stopped successfully.");
#endif

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine($"[IntVue.Debug] MediaCaptureService.StopPreviewAsync: ERROR - {ex.GetType().Name}: {ex.Message}");
#endif
            }
        }

        /// <summary>
        /// Start recording to a file in ApplicationData.LocalFolder and return the file path.
        /// </summary>
        /// <param name="baseFileName">Base file name suggested by the caller; will be sanitized.</param>
        /// <returns>Full path to the recording file.</returns>
        public async Task<string> StartRecordingAsync(string baseFileName)
        {
#if DEBUG
            Debug.WriteLine($"[IntVue.Debug] MediaCaptureService.StartRecordingAsync: Starting recording with base name '{baseFileName}'...");
#endif

            if (this.mediaCapture == null)
            {
#if DEBUG
                Trace.WriteLine("[IntVue.Debug] MediaCaptureService.StartRecordingAsync: MediaCapture is null, initializing...");
#endif
                await this.InitializeAsync().ConfigureAwait(false);
            }

            if (this.mediaCapture == null)
            {
#if DEBUG
                Trace.WriteLine("[IntVue.Debug] MediaCaptureService.StartRecordingAsync: ERROR - MediaCapture failed to initialize.");
#endif
                throw new InvalidOperationException("MediaCapture not initialized");
            }

            var safe = FileHelpers.SanitizeFileName(baseFileName);
            var fileName = safe.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) ? safe : safe + ".mp4";

#if DEBUG
            Debug.WriteLine($"[IntVue.Debug] MediaCaptureService.StartRecordingAsync: Creating recording file '{fileName}'...");
#endif

            this.currentFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);

#if DEBUG
            Debug.WriteLine($"[IntVue.Debug] MediaCaptureService.StartRecordingAsync: Recording file created at '{this.currentFile.Path}'");
            Trace.WriteLine("[IntVue.Debug] MediaCaptureService.StartRecordingAsync: Preparing low-lag recording...");
#endif

            var profile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Auto);
            this.lowLagRecording = await this.mediaCapture.PrepareLowLagRecordToStorageFileAsync(profile, this.currentFile);

#if DEBUG
            Trace.WriteLine("[IntVue.Debug] MediaCaptureService.StartRecordingAsync: Low-lag recording prepared. Starting recording...");
#endif

            await this.lowLagRecording.StartAsync();

#if DEBUG
            Debug.WriteLine($"[IntVue.Debug] MediaCaptureService.StartRecordingAsync: Recording started successfully. File: {this.currentFile.Path}");
#endif

            return this.currentFile.Path;
        }

        /// <summary>
        /// Stop the current recording if one is in progress.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task StopRecordingAsync()
        {
#if DEBUG
            Trace.WriteLine("[IntVue.Debug] MediaCaptureService.StopRecordingAsync: Stopping recording...");
#endif

            if (this.lowLagRecording != null)
            {
#if DEBUG
                Trace.WriteLine("[IntVue.Debug] MediaCaptureService.StopRecordingAsync: Calling StopAsync()...");
#endif
                await this.lowLagRecording.StopAsync();

#if DEBUG
                Trace.WriteLine("[IntVue.Debug] MediaCaptureService.StopRecordingAsync: Calling FinishAsync()...");
#endif
                await this.lowLagRecording.FinishAsync();
                this.lowLagRecording = null;

#if DEBUG
                Trace.WriteLine("[IntVue.Debug] MediaCaptureService.StopRecordingAsync: Recording stopped successfully.");
#endif
            }
            else
            {
#if DEBUG
                Trace.WriteLine("[IntVue.Debug] MediaCaptureService.StopRecordingAsync: No recording in progress.");
#endif
            }
        }

        /// <summary>
        /// Asynchronously dispose managed resources used by the service.
        /// Stops active preview/recording and releases MediaCapture.
        /// </summary>
        /// <returns>A <see cref="Task"/> that completes when disposal is finished.</returns>
        public async Task DisposeAsync()
        {
            try
            {
                if (this.previewMediaPlayer != null)
                {
                    this.previewMediaPlayer.Source = null;
                    this.previewMediaPlayer.Dispose();
                    this.previewMediaPlayer = null;
                }

                if (this.previewMediaSource != null)
                {
                    this.previewMediaSource.Dispose();
                    this.previewMediaSource = null;
                }

                if (this.lowLagRecording != null)
                {
                    _ = this.lowLagRecording.StopAsync();
                    _ = this.lowLagRecording.FinishAsync();
                    this.lowLagRecording = null;
                }

                if (this.mediaCapture != null)
                {
                    this.mediaCapture.Dispose();
                    this.mediaCapture = null;
                }

                this.initialized = false;
            }
            catch
            {
                // Swallow dispose exceptions; callers should handle runtime errors earlier.
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Disposes resources asynchronously.
        /// </summary>
        /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
        ValueTask IAsyncDisposable.DisposeAsync() => new ValueTask(this.DisposeAsync());

        /// <summary>
        /// Implements <see cref="IDisposable"/> to satisfy CA1001 (owns disposable fields).
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (this.previewMediaPlayer != null)
                {
                    this.previewMediaPlayer.Source = null;
                    this.previewMediaPlayer.Dispose();
                    this.previewMediaPlayer = null;
                }

                if (this.previewMediaSource != null)
                {
                    this.previewMediaSource.Dispose();
                    this.previewMediaSource = null;
                }

                if (this.lowLagRecording != null)
                {
                    this.lowLagRecording.StopAsync().AsTask().GetAwaiter().GetResult();
                    this.lowLagRecording.FinishAsync().AsTask().GetAwaiter().GetResult();
                    this.lowLagRecording = null;
                }

                if (this.mediaCapture != null)
                {
                    this.mediaCapture.Dispose();
                    this.mediaCapture = null;
                }

                this.initialized = false;
            }
            catch
            {
                // Swallow exceptions during Dispose to avoid throwing from finalizers.
            }
        }
    }
}
