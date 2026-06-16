// Copyright (c) YourProjectName. All rights reserved.

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

namespace IntVue.Services;

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
    private MediaFrameSource? previewFrameSource;
    private LowLagMediaRecording? lowLagRecording;
    private StorageFile? currentFile;
    private bool initialized;

    /// <summary>
    /// Gets a value indicating whether a recording is currently in progress.
    /// </summary>
    public bool IsRecording => this.lowLagRecording != null;

    /// <summary>
    /// Initialize the underlying MediaCapture resources.
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

        try
        {
            this.mediaCapture = new MediaCapture();
            var settings = new MediaCaptureInitializationSettings
            {
                StreamingCaptureMode = StreamingCaptureMode.AudioAndVideo,
            };

            await this.mediaCapture.InitializeAsync(settings);

#if DEBUG
            Trace.WriteLine("[IntVue.Debug] MediaCaptureService.InitializeAsync: MediaCapture initialized successfully.");
#endif
            this.initialized = true;
        }
        catch (Exception ex)
        {
#if DEBUG
            Debug.WriteLine($"[IntVue.Debug] MediaCaptureService.InitializeAsync: ERROR - {ex.GetType().Name}: {ex.Message}");
#endif

            // Mark as initialized to prevent repeated attempts
            this.initialized = true;
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
            // Get the first video frame source (simple selection)
            var frameSource = this.mediaCapture.FrameSources.Values.FirstOrDefault();
            if (frameSource == null)
            {
#if DEBUG
                Trace.WriteLine("[IntVue.Debug] MediaCaptureService.StartPreviewAsync: ERROR - No video frame source available.");
#endif
                throw new InvalidOperationException("No video frame source available");
            }

            this.previewFrameSource = frameSource;
            this.previewMediaSource = MediaSource.CreateFromMediaFrameSource(frameSource);

#if DEBUG
            Trace.WriteLine("[IntVue.Debug] MediaCaptureService.StartPreviewAsync: MediaSource created from frame source.");
#endif

            // Clean up previous MediaPlayer if it exists
            if (this.previewMediaPlayer != null)
            {
                try
                {
                    this.previewMediaPlayer.Source = null;
                    this.previewMediaPlayer.Dispose();
                }
                catch (Exception ex)
                {
#if DEBUG
                    Debug.WriteLine($"[IntVue.Debug] MediaCaptureService.StartPreviewAsync: Error cleaning up previous MediaPlayer - {ex.Message}");
#endif
                }
                finally
                {
                    this.previewMediaPlayer = null;
                }
            }

            // Create and configure new MediaPlayer
            this.previewMediaPlayer = new MediaPlayer();
            this.previewMediaPlayer.AutoPlay = true;
            this.previewMediaPlayer.Source = this.previewMediaSource;

            // Ensure SetMediaPlayer is called on the UI thread
            if (!mediaPlayerElement.DispatcherQueue.HasThreadAccess)
            {
#if DEBUG
                Trace.WriteLine("[IntVue.Debug] MediaCaptureService.StartPreviewAsync: Marshaling to UI thread...");
#endif
                bool enqueued = mediaPlayerElement.DispatcherQueue.TryEnqueue(
                    Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal,
                    () =>
                    {
                        mediaPlayerElement.SetMediaPlayer(this.previewMediaPlayer);
                        if (this.previewMediaPlayer.PlaybackSession.PlaybackState != Windows.Media.Playback.MediaPlaybackState.Playing)
                        {
                            this.previewMediaPlayer.Play();
                        }
                    });

                if (!enqueued)
                {
#if DEBUG
                    Trace.WriteLine("[IntVue.Debug] MediaCaptureService.StartPreviewAsync: ERROR - Failed to marshal to UI thread.");
#endif
                    throw new InvalidOperationException("Failed to marshal SetMediaPlayer call to UI thread");
                }
            }
            else
            {
                mediaPlayerElement.SetMediaPlayer(this.previewMediaPlayer);
                if (this.previewMediaPlayer.PlaybackSession.PlaybackState != Windows.Media.Playback.MediaPlaybackState.Playing)
                {
                    this.previewMediaPlayer.Play();
                }
            }

#if DEBUG
            Trace.WriteLine("[IntVue.Debug] MediaCaptureService.StartPreviewAsync: Preview started successfully.");
#endif
        }
        catch (COMException comEx)
        {
#if DEBUG
            Trace.WriteLine($"[IntVue.Debug] MediaCaptureService.StartPreviewAsync: COMException - HResult=0x{comEx.HResult:X8}");
#endif
            throw new InvalidOperationException(
                "Failed to bind MediaPlayer to preview control. This may indicate a graphics driver issue or incompatible display settings.",
                comEx);
        }
        catch (Exception ex)
        {
#if DEBUG
            Trace.WriteLine($"[IntVue.Debug] MediaCaptureService.StartPreviewAsync: ERROR - {ex.GetType().Name}: {ex.Message}");
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
                try
                {
                    this.previewMediaPlayer.Source = null;
                    this.previewMediaPlayer.Dispose();
                }
                catch (Exception disposeEx)
                {
#if DEBUG
                    Debug.WriteLine($"[IntVue.Debug] MediaCaptureService.StopPreviewAsync: ERROR disposing MediaPlayer - {disposeEx.GetType().Name}: {disposeEx.Message}");
#endif
                }
                finally
                {
                    this.previewMediaPlayer = null;
                }
            }

            if (this.previewMediaSource != null)
            {
#if DEBUG
                Trace.WriteLine("[IntVue.Debug] MediaCaptureService.StopPreviewAsync: Disposing MediaSource...");
#endif
                try
                {
                    this.previewMediaSource.Dispose();
                }
                catch (Exception disposeEx)
                {
#if DEBUG
                    Debug.WriteLine($"[IntVue.Debug] MediaCaptureService.StopPreviewAsync: ERROR disposing MediaSource - {disposeEx.GetType().Name}: {disposeEx.Message}");
#endif
                }
                finally
                {
                    this.previewMediaSource = null;
                }
            }

            // Reset the frame source reference (cannot be disposed directly, managed by MediaCapture)
            this.previewFrameSource = null;

            // Dispose and reset MediaCapture to release all preview resources before recording
            // This is necessary because MediaFrameSource and LowLagMediaRecording are mutually exclusive
#if DEBUG
            Trace.WriteLine("[IntVue.Debug] MediaCaptureService.StopPreviewAsync: Disposing MediaCapture to release frame sources...");
#endif
            if (this.mediaCapture != null)
            {
                try
                {
                    this.mediaCapture.Dispose();
                }
                catch (Exception disposeEx)
                {
#if DEBUG
                    Debug.WriteLine($"[IntVue.Debug] MediaCaptureService.StopPreviewAsync: ERROR disposing MediaCapture - {disposeEx.GetType().Name}: {disposeEx.Message}");
#endif
                }
                finally
                {
                    this.mediaCapture = null;
                    this.initialized = false;  // Reset initialized flag so MediaCapture will be reinitialied for recording
                }
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

        try
        {
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
        catch (InvalidOperationException ex)
        {
#if DEBUG
            Debug.WriteLine($"[IntVue.Debug] MediaCaptureService.StartRecordingAsync: InvalidOperationException - {ex.Message}");
            if (ex.InnerException != null)
            {
                Debug.WriteLine($"[IntVue.Debug] MediaCaptureService.StartRecordingAsync:   InnerException: {ex.InnerException.GetType().Name} - {ex.InnerException.Message}");
            }
#endif
            throw;
        }
        catch (Exception ex)
        {
#if DEBUG
            Debug.WriteLine($"[IntVue.Debug] MediaCaptureService.StartRecordingAsync: ERROR - {ex.GetType().Name}: {ex.Message}");
            Debug.WriteLine($"[IntVue.Debug] MediaCaptureService.StartRecordingAsync: StackTrace: {ex.StackTrace}");
#endif
            throw;
        }
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
        finally
        {
            GC.SuppressFinalize(this);
        }
    }
}
