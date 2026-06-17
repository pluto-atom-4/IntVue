// Copyright (c) YourProjectName. All rights reserved.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Windows.Devices.Enumeration;

namespace IntVue.Services;

/// <summary>
/// Media capture service interface used by the app.
/// Defines the minimal contract required by ViewModels and tests.
/// </summary>
public interface IMediaCaptureService
{
    /// <summary>
    /// Gets a value indicating whether a recording is currently in progress.
    /// </summary>
    bool IsRecording { get; }

    /// <summary>
    /// Get a list of available camera devices.
    /// </summary>
    /// <returns>A <see cref="Task{IReadOnlyList}"/> representing the asynchronous operation and returning available cameras.</returns>
    Task<IReadOnlyList<DeviceInformation>> GetCamerasAsync();

    /// <summary>
    /// Initialize the underlying media capture resources with an optional specific camera device.
    /// </summary>
    /// <param name="videoDeviceId">Optional device ID to use for video capture. If null, the OS default is used.</param>
    /// <param name="cancellationToken">Cancellation token used to abort initialization.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task InitializeAsync(string? videoDeviceId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Request permissions for camera/microphone and return true if granted.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<bool> RequestPermissionsAsync();

    /// <summary>
    /// Start showing the camera preview in the provided preview host object.
    /// The concrete type is forwarded by the view (e.g., MediaPlayerElement).
    /// </summary>
    /// <param name="previewHost">UI element used to render the camera preview (implementation-specific).</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task StartPreviewAsync(object previewHost);

    /// <summary>
    /// Stop the camera preview.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task StopPreviewAsync();

    /// <summary>
    /// Start recording and return the saved file path.
    /// </summary>
    /// <param name="baseFileName">Suggested base filename to use for the recording; will be sanitized.</param>
    /// <returns>A <see cref="Task{String}"/> representing the asynchronous operation and returning the file path.</returns>
    Task<string> StartRecordingAsync(string baseFileName);

    /// <summary>
    /// Stop recording.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task StopRecordingAsync();

    /// <summary>
    /// Dispose and release resources asynchronously.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task DisposeAsync();
}
