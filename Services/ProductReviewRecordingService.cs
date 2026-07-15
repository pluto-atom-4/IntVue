// Copyright (c) YourProjectName. All rights reserved.

using System;
using System.Threading.Tasks;

using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;

namespace IntVue.Services;

/// <summary>
/// Service for recording user responses to product review questions.
/// Handles MediaCapture initialization, recording, and file management.
/// </summary>
public class ProductReviewRecordingService : IDisposable
{
    private MediaCapture? _mediaCapture;
    private LowLagMediaRecording? _mediaRecording;
    private StorageFile? _recordedFile;
    private bool _isRecording;
    private bool _disposed;

    /// <summary>
    /// Gets a value indicating whether recording is currently active.
    /// </summary>
    public bool IsRecording => _isRecording;

    /// <summary>
    /// Gets the currently recorded file, or null if no recording is available.
    /// </summary>
    public StorageFile? RecordedFile => _recordedFile;

    /// <summary>
    /// Initializes MediaCapture for recording from the specified device.
    /// </summary>
    /// <param name="deviceId">The device ID to use for recording.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task InitializeAsync(string deviceId)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[ProductReviewRecordingService.InitializeAsync] Initializing with device: {deviceId}");

            _mediaCapture = new MediaCapture();

            var settings = new MediaCaptureInitializationSettings
            {
                VideoDeviceId = deviceId,
                StreamingCaptureMode = StreamingCaptureMode.AudioAndVideo,
            };

            await _mediaCapture.InitializeAsync(settings);
            System.Diagnostics.Debug.WriteLine("[ProductReviewRecordingService.InitializeAsync] MediaCapture initialized successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductReviewRecordingService.InitializeAsync] Error: {ex.Message}");
            _mediaCapture?.Dispose();
            _mediaCapture = null;
            throw;
        }
    }

    /// <summary>
    /// Starts recording to a file in the Videos library.
    /// </summary>
    /// <param name="filename">The filename for the recording (without extension).</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task StartRecordingAsync(string filename = "response")
    {
        try
        {
            if (_mediaCapture == null)
            {
                throw new InvalidOperationException("MediaCapture not initialized. Call InitializeAsync first.");
            }

            System.Diagnostics.Debug.WriteLine($"[ProductReviewRecordingService.StartRecordingAsync] Starting recording: {filename}");

            // Prepare file storage
            StorageLibrary myVideos = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Videos);
            _recordedFile = await myVideos.SaveFolder.CreateFileAsync(
                $"{filename}.mp4",
                CreationCollisionOption.GenerateUniqueName);

            System.Diagnostics.Debug.WriteLine($"[ProductReviewRecordingService.StartRecordingAsync] Recording to: {_recordedFile.Path}");

            // Set up encoding profile
            MediaEncodingProfile encodingProfile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Auto);

            // Hook into limitation events
            _mediaCapture.RecordLimitationExceeded += this.OnMediaCaptureRecordLimitationExceeded;

            // Prepare recording
            _mediaRecording = await _mediaCapture.PrepareLowLagRecordToStorageFileAsync(encodingProfile, _recordedFile);

            // Start recording
            await _mediaRecording.StartAsync();

            _isRecording = true;
            System.Diagnostics.Debug.WriteLine("[ProductReviewRecordingService.StartRecordingAsync] Recording started successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductReviewRecordingService.StartRecordingAsync] Error: {ex.Message}");
            _isRecording = false;
            throw;
        }
    }

    /// <summary>
    /// Stops the current recording session.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task StopRecordingAsync()
    {
        try
        {
            if (_mediaRecording == null || !_isRecording)
            {
                System.Diagnostics.Debug.WriteLine("[ProductReviewRecordingService.StopRecordingAsync] Recording not active");
                return;
            }

            System.Diagnostics.Debug.WriteLine("[ProductReviewRecordingService.StopRecordingAsync] Stopping recording...");

            await _mediaRecording.StopAsync();
            await _mediaRecording.FinishAsync();

            _isRecording = false;

            if (_recordedFile != null)
            {
                System.Diagnostics.Debug.WriteLine($"[ProductReviewRecordingService.StopRecordingAsync] Recording saved: {_recordedFile.Path}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductReviewRecordingService.StopRecordingAsync] Error: {ex.Message}");
            _isRecording = false;
            throw;
        }
        finally
        {
            if (_mediaCapture != null)
            {
                _mediaCapture.RecordLimitationExceeded -= this.OnMediaCaptureRecordLimitationExceeded;
            }

            _mediaRecording = null;
        }
    }

    /// <summary>
    /// Deletes the current recording.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task DeleteRecordingAsync()
    {
        try
        {
            if (_recordedFile == null)
            {
                System.Diagnostics.Debug.WriteLine("[ProductReviewRecordingService.DeleteRecordingAsync] No recording to delete");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[ProductReviewRecordingService.DeleteRecordingAsync] Deleting: {_recordedFile.Name}");

            await _recordedFile.DeleteAsync();
            _recordedFile = null;

            System.Diagnostics.Debug.WriteLine("[ProductReviewRecordingService.DeleteRecordingAsync] Recording deleted successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductReviewRecordingService.DeleteRecordingAsync] Error: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Disposes resources owned by the service.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes service resources.
    /// </summary>
    /// <param name="disposing">True if disposing managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _mediaRecording = null;
            _mediaCapture?.Dispose();
        }

        _disposed = true;
    }

    private void OnMediaCaptureRecordLimitationExceeded(MediaCapture sender)
    {
        System.Diagnostics.Debug.WriteLine("[ProductReviewRecordingService] Record limitation threshold breached");
        _ = this.StopRecordingAsync();
    }
}
