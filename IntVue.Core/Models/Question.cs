// Copyright (c) YourProjectName. All rights reserved.

using System;

namespace IntVue.Models;

/// <summary>
/// Represents a pre-recorded interview question for playback in the Product Review feature.
/// </summary>
public class Question
{
    /// <summary>
    /// Gets or sets the full file path to the WebM media file.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the filename (without directory path) of the question.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the media URI for playback via MediaPlayerElement.
    /// </summary>
    public Uri? MediaUri { get; set; }

    /// <summary>
    /// Gets or sets the duration of the media in milliseconds.
    /// </summary>
    public int DurationMs { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the WebM file is valid and ready for playback.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets or sets a validation error message if <see cref="IsValid"/> is false.
    /// </summary>
    public string? ValidationMessage { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the question file was discovered.
    /// </summary>
    public DateTime DiscoveredAt { get; set; } = DateTime.UtcNow;
}
