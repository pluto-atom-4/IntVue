// <copyright file="IMediaCaptureService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

namespace IntVue.Services;

/// <summary>
/// Media capture service interface used by the app.
/// Defines the minimal contract required by ViewModels and tests.
/// </summary>
public interface IMediaCaptureService
{
    /// <summary>
    /// Initialize the underlying media capture resources.
    /// </summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Request permissions for camera/microphone and return true if granted.
    /// </summary>
    Task<bool> RequestPermissionsAsync();

    /// <summary>
    /// Start showing the camera preview in the provided preview host object.
    /// The concrete type is forwarded by the view (e.g., MediaPlayerElement).
    /// </summary>
    Task StartPreviewAsync(object previewHost);

    /// <summary>
    /// Stop the camera preview.
    /// </summary>
    Task StopPreviewAsync();

    /// <summary>
    /// Start recording and return the saved file path.
    /// </summary>
    Task<string> StartRecordingAsync(string baseFileName);

    /// <summary>
    /// Stop recording.
    /// </summary>
    Task StopRecordingAsync();

    /// <summary>
    /// Dispose and release resources asynchronously.
    /// </summary>
    Task DisposeAsync();

    /// <summary>
    /// Whether a recording is currently in progress.
    /// </summary>
    bool IsRecording { get; }
}
