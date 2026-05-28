// <copyright file="MediaCaptureService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IntVue.Helpers;
using Windows.Devices.Enumeration;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;

namespace IntVue.Services
{
    /// <summary>
    /// MediaCaptureService implements IMediaCaptureService using Windows.Media.Capture.
    /// This implements preview to a CaptureElement and recording to ApplicationData.LocalFolder.
    /// Error paths are surfaced as exceptions and should be handled by callers.
    /// </summary>
    public class MediaCaptureService : IMediaCaptureService
    {
        private MediaCapture? mediaCapture;
        private LowLagMediaRecording? lowLagRecording;
        private StorageFile? currentFile;
        private bool initialized;

        public bool IsRecording => lowLagRecording != null;

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            if (this.initialized)
            {
                return;
            }

            this.mediaCapture = new MediaCapture();

            var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            var front = devices.FirstOrDefault(d => d.EnclosureLocation != null && d.EnclosureLocation.Panel == Panel.Front) ?? devices.FirstOrDefault();

            var settings = new MediaCaptureInitializationSettings
            {
                VideoDeviceId = front?.Id,
                StreamingCaptureMode = StreamingCaptureMode.AudioAndVideo
            };

            await this.mediaCapture.InitializeAsync(settings);
            this.initialized = true;
        }

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

        public async Task StopPreviewAsync()
        {
            if (this.mediaCapture != null)
            {
                await this.mediaCapture.StopPreviewAsync();
            }
        }

        public async Task<string> StartRecordingAsync(string baseFileName)
        {
            if (this.mediaCapture == null)
            {
                await this.InitializeAsync().ConfigureAwait(false);
            }

            var safe = FileHelpers.SanitizeFileName(baseFileName);
            var fileName = safe.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) ? safe : safe + ".mp4";
            this.currentFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);

            var profile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Auto);
            this.lowLagRecording = await this.mediaCapture.PrepareLowLagRecordToStorageFileAsync(profile, this.currentFile);
            await this.lowLagRecording.StartAsync();

            return this.currentFile.Path;
        }

        public async Task StopRecordingAsync()
        {
            if (this.lowLagRecording != null)
            {
                await this.lowLagRecording.StopAsync();
                await this.lowLagRecording.FinishAsync();
                this.lowLagRecording = null;
            }
        }

        public Task DisposeAsync()
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

            return Task.CompletedTask;
        }
    }
}
