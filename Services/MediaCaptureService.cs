// <copyright file="MediaCaptureService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace IntVue.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using IntVue.Helpers;

    using Windows.Devices.Enumeration;
    using Windows.Media.Capture;
    using Windows.Media.MediaProperties;
    using Windows.Storage;

    /// <summary>
    /// MediaCaptureService implements IMediaCaptureService using Windows.Media.Capture.
    /// This implements preview to a CaptureElement and recording to ApplicationData.LocalFolder.
    /// Error paths are surfaced as exceptions and should be handled by callers.
    /// </summary>
    public class MediaCaptureService : IMediaCaptureService, IAsyncDisposable, IDisposable
    {
        private MediaCapture? mediaCapture;
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
            DeviceInformation? front = null;
            for (var i = 0; i < devices.Count; i++)
            {
                var d = devices[i];
                if (d.EnclosureLocation != null && d.EnclosureLocation.Panel == Panel.Front)
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
            // Use DeviceAccessInformation to infer current permission status for camera and microphone.
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
        /// Start camera preview. The previewHost is a UI element that can render preview frames.
        /// For this skeleton implementation preview is best-effort and may be a no-op.
        /// </summary>
        /// <param name="previewHost">UI element used for preview rendering (implementation-specific).</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task StartPreviewAsync(object previewHost)
        {
            if (this.mediaCapture == null)
            {
                await this.InitializeAsync().ConfigureAwait(false);
            }

            // Preview support not wired to a UI element in Phase 2 skeleton.
            // The app's UI may pass a MediaPlayerElement or other control; preview is a best-effort no-op here.
            await Task.CompletedTask;
        }

        /// <summary>
        /// Stop the camera preview if active.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task StopPreviewAsync()
        {
            if (this.mediaCapture != null)
            {
                await this.mediaCapture.StopPreviewAsync();
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
        /// Asynchronously dispose managed resources used by the service. Stops active preview/recording and releases MediaCapture.
        /// </summary>
        /// <returns>A <see cref="Task"/> that completes when disposal is finished.</returns>
        public async Task DisposeAsync()
        {
            try
            {
                if (this.lowLagRecording != null)
                {
                    // best-effort stop
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
                if (this.lowLagRecording != null)
                {
                    // Synchronously stop/finish recordings (block briefly)
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
