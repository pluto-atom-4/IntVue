// <copyright file="MediaCaptureService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace IntVue.Services
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.Versioning;
    using System.Threading;
    using System.Threading.Tasks;

    using IntVue.Helpers;

    using Microsoft.UI.Xaml.Controls;

    using Windows.Devices.Enumeration;
    using Windows.Media.Capture;
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
                return;
            }

            this.mediaCapture = new MediaCapture();

            var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

            if (devices.Count == 0)
            {
                Debug.WriteLine("Warning: No camera device found. Preview mode disabled.");
                this.initialized = true;
                return;
            }

            DeviceInformation? front = null;
            for (var i = 0; i < devices.Count; i++)
            {
                var d = devices[i];
                if (d.EnclosureLocation != null && d.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Front)
                {
                    front = d;
                    break;
                }
            }

            if (front == null && devices.Count > 0)
            {
                front = devices[0];
            }

            var settings = new MediaCaptureInitializationSettings
            {
                VideoDeviceId = front?.Id,
                StreamingCaptureMode = StreamingCaptureMode.AudioAndVideo,
            };

            await this.mediaCapture.InitializeAsync(settings);
            this.initialized = true;
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
                // Get the first available video frame source from MediaCapture
                var frameSource = this.mediaCapture.FrameSources.Values.FirstOrDefault();

                if (frameSource == null)
                {
                    throw new InvalidOperationException("No video frame source available from MediaCapture");
                }

                // Create MediaSource from the frame source
                this.previewMediaSource = MediaSource.CreateFromMediaFrameSource(frameSource);

                // Create MediaPlayer and bind to MediaPlayerElement
                this.previewMediaPlayer = new MediaPlayer();
                this.previewMediaPlayer.Source = this.previewMediaSource;
                mediaPlayerElement.SetMediaPlayer(this.previewMediaPlayer);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error starting preview: {ex.Message}");
                throw;
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

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error stopping preview: {ex.Message}");
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
