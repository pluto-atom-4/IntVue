// Copyright (c) YourProjectName. All rights reserved.

using System;
using System.Collections.Generic;
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
    private string? selectedVideoDeviceId;

    /// <summary>
    /// Gets a value indicating whether a recording is currently in progress.
    /// </summary>
    public bool IsRecording => this.lowLagRecording != null;

    /// <summary>
    /// Get a list of available camera devices.
    /// </summary>
    /// <returns>A list of available DeviceInformation objects for video capture devices.</returns>
    public async Task<IReadOnlyList<DeviceInformation>> GetCamerasAsync()
    {
        try
        {
            var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            return devices.ToList().AsReadOnly();
        }
        catch (Exception ex)
        {
#if DEBUG
            Debug.WriteLine($"[IntVue.Debug] GetCamerasAsync: ERROR - {ex.GetType().Name}: {ex.Message}");
#endif
            return new List<DeviceInformation>().AsReadOnly();
        }
    }

    /// <summary>
    /// Initialize the underlying MediaCapture resources.
    /// </summary>
    /// <param name="videoDeviceId">Optional device ID to use for video capture. If null, the OS default is used.</param>
    /// <param name="cancellationToken">Cancellation token to abort initialization.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous initialization operation.</returns>
    public async Task InitializeAsync(string? videoDeviceId = null, CancellationToken cancellationToken = default)
    {
        if (this.initialized)
        {
            return;
        }

        if (videoDeviceId != null)
        {
            this.selectedVideoDeviceId = videoDeviceId;
        }

        try
        {
            this.mediaCapture = new MediaCapture();
            var settings = new MediaCaptureInitializationSettings
            {
                StreamingCaptureMode = StreamingCaptureMode.AudioAndVideo,
            };

            if (!string.IsNullOrEmpty(this.selectedVideoDeviceId))
            {
                settings.VideoDeviceId = this.selectedVideoDeviceId;
            }

            await this.mediaCapture.InitializeAsync(settings);
            this.initialized = true;
        }
        catch (Exception ex)
        {
#if DEBUG
            Debug.WriteLine($"[IntVue.Debug] InitializeAsync: ERROR - {ex.GetType().Name}");
#endif
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
        try
        {
            var camInfo = DeviceAccessInformation.CreateFromDeviceClass(DeviceClass.VideoCapture);
            var micInfo = DeviceAccessInformation.CreateFromDeviceClass(DeviceClass.AudioCapture);

            var camAllowed = camInfo?.CurrentStatus == DeviceAccessStatus.Allowed;
            var micAllowed = micInfo?.CurrentStatus == DeviceAccessStatus.Allowed;

            return Task.FromResult(camAllowed && micAllowed);
        }
        catch
        {
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
        if (this.mediaCapture == null)
        {
            await this.InitializeAsync().ConfigureAwait(false);
        }

        if (this.mediaCapture == null)
        {
            throw new InvalidOperationException("MediaCapture not initialized");
        }

        if (previewHost is not MediaPlayerElement mediaPlayerElement)
        {
            throw new ArgumentException("previewHost must be a MediaPlayerElement", nameof(previewHost));
        }

        try
        {
            // Get the first video frame source
            var frameSource = this.mediaCapture.FrameSources.Values.FirstOrDefault();
            if (frameSource == null)
            {
                throw new InvalidOperationException("No video frame source available");
            }

            this.previewFrameSource = frameSource;
            this.previewMediaSource = MediaSource.CreateFromMediaFrameSource(frameSource);

            // Clean up previous MediaPlayer if it exists
            this.TryDispose(ref this.previewMediaPlayer, "MediaPlayer");

            // Create and configure new MediaPlayer
            this.previewMediaPlayer = new MediaPlayer { AutoPlay = true, Source = this.previewMediaSource };

            // SetMediaPlayer is called from UI thread in MVP
            mediaPlayerElement.SetMediaPlayer(this.previewMediaPlayer);
            if (this.previewMediaPlayer.PlaybackSession.PlaybackState != Windows.Media.Playback.MediaPlaybackState.Playing)
            {
                this.previewMediaPlayer.Play();
            }
        }
        catch (COMException comEx)
        {
            throw new InvalidOperationException(
                "Failed to bind MediaPlayer to preview control. This may indicate a graphics driver issue.",
                comEx);
        }
    }

    /// <summary>
    /// Stop the camera preview if active.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task StopPreviewAsync()
    {
        try
        {
            this.TryDispose(ref this.previewMediaPlayer, "MediaPlayer");
            this.TryDispose(ref this.previewMediaSource, "MediaSource");
            this.TryDispose(ref this.mediaCapture, "MediaCapture");
            this.previewFrameSource = null;
            this.initialized = false;
            await Task.CompletedTask;
        }
        catch
        {
            // Swallow exceptions during cleanup
        }
    }

    private void TryDispose<T>(ref T? resource, string name)
        where T : class, IDisposable
    {
        if (resource != null)
        {
            try
            {
                // Clear MediaPlayer source before disposing
                if (resource is MediaPlayer mp)
                {
                    mp.Source = null;
                }

                resource.Dispose();
            }
            catch
            {
                // Swallow disposal exceptions
            }
            finally
            {
                resource = null;
            }
        }
    }

    /// <summary>
    /// Start recording to a file in ApplicationData.LocalFolder and return the file path.
    /// </summary>
    /// <param name="baseFileName">Base file name suggested by the caller; will be sanitized.</param>
    /// <returns>Full path to the recording file.</returns>
    public async Task<string> StartRecordingAsync(string baseFileName)
    {
        if (this.mediaCapture == null)
        {
            await this.InitializeAsync().ConfigureAwait(false);
        }

        if (this.mediaCapture == null)
        {
            throw new InvalidOperationException("MediaCapture not initialized");
        }

        var safe = FileHelpers.SanitizeFileName(baseFileName);
        var fileName = safe.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) ? safe : safe + ".mp4";

        this.currentFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);

        var profile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Auto);
        this.lowLagRecording = await this.mediaCapture.PrepareLowLagRecordToStorageFileAsync(profile, this.currentFile);
        await this.lowLagRecording.StartAsync();

        return this.currentFile.Path;
    }

    /// <summary>
    /// Stop the current recording if one is in progress.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task StopRecordingAsync()
    {
        if (this.lowLagRecording != null)
        {
            await this.lowLagRecording.StopAsync();
            await this.lowLagRecording.FinishAsync();
            this.lowLagRecording = null;
        }
    }

    /// <summary>
    /// Asynchronously dispose managed resources used by the service.
    /// </summary>
    /// <returns>A <see cref="Task"/> that completes when disposal is finished.</returns>
    public async Task DisposeAsync()
    {
        try
        {
            this.TryDispose(ref this.previewMediaPlayer, "MediaPlayer");
            this.TryDispose(ref this.previewMediaSource, "MediaSource");
            if (this.lowLagRecording != null)
            {
                _ = this.lowLagRecording.StopAsync();
                _ = this.lowLagRecording.FinishAsync();
                this.lowLagRecording = null;
            }

            this.TryDispose(ref this.mediaCapture, "MediaCapture");
            this.initialized = false;
        }
        catch
        {
            // Swallow dispose exceptions
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
            this.TryDispose(ref this.previewMediaPlayer, "MediaPlayer");
            this.TryDispose(ref this.previewMediaSource, "MediaSource");
            if (this.lowLagRecording != null)
            {
                this.lowLagRecording.StopAsync().AsTask().GetAwaiter().GetResult();
                this.lowLagRecording.FinishAsync().AsTask().GetAwaiter().GetResult();
                this.lowLagRecording = null;
            }

            this.TryDispose(ref this.mediaCapture, "MediaCapture");
            this.initialized = false;
        }
        catch
        {
            // Swallow exceptions during Dispose
        }
        finally
        {
            GC.SuppressFinalize(this);
        }
    }
}
