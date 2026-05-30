// <copyright file="IMediaCaptureService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace IntVue.Services;

using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Media capture service interface used by the app.
/// Defines the minimal contract required by ViewModels and tests.
/// </summary>
public interface IMediaCaptureService
{
    /// <summary>
    /// Gets a value indicating whether whether a recording is currently in progress.
    /// </summary>
    bool IsRecording { get; }

    /// <summary>
    /// Initialize the underlying media capture resources.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Request permissions for camera/microphone and return true if granted.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<bool> RequestPermissionsAsync();

    /// <summary>
    /// Start showing the camera preview in the provided preview host object.
    /// The concrete type is forwarded by the view (e.g., MediaPlayerElement).
    /// </summary>
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
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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
