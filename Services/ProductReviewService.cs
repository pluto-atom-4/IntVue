// Copyright (c) YourProjectName. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using IntVue.Models;

using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;

namespace IntVue.Services;

/// <summary>
/// Service for discovering, loading, and validating pre-recorded interview questions in WebM format.
/// </summary>
public class ProductReviewService : IProductReviewService
{
    private const string _webMExtension = ".webm";
    private const int _metadataTimeoutMs = 5000;

    /// <summary>
    /// Loads a single WebM question file from the specified file path.
    /// </summary>
    /// <param name="filePath">The full file path to a WebM media file.</param>
    /// <returns>A <see cref="Question"/> object with metadata and validation status.</returns>
    public async Task<Question> LoadQuestionFileAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
        }

        try
        {
            var file = await StorageFile.GetFileFromPathAsync(filePath);
            var question = new Question
            {
                FilePath = filePath,
                FileName = file.Name,
                MediaUri = new Uri(filePath),
                DiscoveredAt = DateTime.UtcNow,
            };

            return await this.GetQuestionMetadataAsync(question);
        }
        catch (System.IO.FileNotFoundException)
        {
            return new Question
            {
                FilePath = filePath,
                FileName = System.IO.Path.GetFileName(filePath),
                IsValid = false,
                ValidationMessage = "File not found.",
                DiscoveredAt = DateTime.UtcNow,
            };
        }
        catch (UnauthorizedAccessException)
        {
            return new Question
            {
                FilePath = filePath,
                FileName = System.IO.Path.GetFileName(filePath),
                IsValid = false,
                ValidationMessage = "Access denied.",
                DiscoveredAt = DateTime.UtcNow,
            };
        }
        catch (Exception ex)
        {
            return new Question
            {
                FilePath = filePath,
                FileName = System.IO.Path.GetFileName(filePath),
                IsValid = false,
                ValidationMessage = $"Failed to load file: {ex.Message}",
                DiscoveredAt = DateTime.UtcNow,
            };
        }
    }

    /// <summary>
    /// Discovers and loads all WebM question files from a directory.
    /// </summary>
    /// <param name="directoryPath">The directory path to search for WebM files.</param>
    /// <returns>A <see cref="List{Question}"/> containing all discovered questions.</returns>
    public async Task<List<Question>> LoadQuestionDirectoryAsync(string directoryPath)
    {
        if (string.IsNullOrEmpty(directoryPath))
        {
            throw new ArgumentException("Directory path cannot be null or empty.", nameof(directoryPath));
        }

        var questions = new List<Question>();

        try
        {
            var folder = await StorageFolder.GetFolderFromPathAsync(directoryPath);
            var files = await folder.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.OrderByName);

            // Filter .webm files and load them in parallel (non-blocking)
            var webmFiles = files.Where(f => f.FileType.Equals(_webMExtension, StringComparison.OrdinalIgnoreCase)).ToList();

            var loadTasks = webmFiles.Select(file => this.LoadQuestionFileAsync(file.Path)).ToList();
            var loadedQuestions = await Task.WhenAll(loadTasks);

            questions.AddRange(loadedQuestions);
        }
        catch (System.IO.FileNotFoundException ex)
        {
            throw new System.IO.DirectoryNotFoundException($"Directory not found: {directoryPath}", ex);
        }
        catch (System.IO.DirectoryNotFoundException)
        {
            throw;
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            // Log and re-throw; caller decides whether to display error or continue
            throw new System.InvalidOperationException($"Failed to load question directory: {ex.Message}", ex);
        }

        return questions;
    }

    /// <summary>
    /// Validates a WebM media file for playback compatibility.
    /// </summary>
    /// <param name="uri">The URI of the media file to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> indicating success or describing the failure reason.</returns>
    public async Task<ValidationResult> ValidateWebMAsync(Uri uri)
    {
        if (uri == null)
        {
            return ValidationResult.Failure("URI cannot be null.");
        }

        try
        {
            // Attempt to create a media source to probe codec support
            var mediaSource = MediaSource.CreateFromUri(uri);

            // Create a playback item to extract metadata; this also validates the source
            var playbackItem = new MediaPlaybackItem(mediaSource);
            var displayProperties = playbackItem.GetDisplayProperties();

            // If we reach here, the file is accessible and playable
            return ValidationResult.Success();
        }
        catch (Exception ex)
        {
            var errorMessage = ex switch
            {
                System.IO.FileNotFoundException => "Media file not found.",
                System.InvalidOperationException => "Invalid media format or codec not supported.",
                _ => $"Validation failed: {ex.Message}",
            };

            return ValidationResult.Failure(errorMessage);
        }
    }

    /// <summary>
    /// Extracts metadata (duration, codec information) from a question file.
    /// </summary>
    /// <param name="question">The question whose metadata should be extracted.</param>
    /// <returns>The updated <see cref="Question"/> with duration and metadata populated.</returns>
    public async Task<Question> GetQuestionMetadataAsync(Question question)
    {
        ArgumentNullException.ThrowIfNull(question);

        if (question.MediaUri == null)
        {
            question.IsValid = false;
            question.ValidationMessage = "Media URI is not set.";
            return question;
        }

        try
        {
            // Validate the WebM file
            var validationResult = await this.ValidateWebMAsync(question.MediaUri);
            question.IsValid = validationResult.IsValid;
            question.ValidationMessage = validationResult.Message;

            if (!validationResult.IsValid)
            {
                return question;
            }

            // Extract metadata (duration, codec)
            var mediaSource = MediaSource.CreateFromUri(question.MediaUri);
            var playbackItem = new MediaPlaybackItem(mediaSource);

            // MediaPlayerElement will populate duration on playback start; for now, set a placeholder
            question.DurationMs = (int)(mediaSource.Duration?.TotalMilliseconds ?? 0);

            return question;
        }
        catch (Exception ex)
        {
            question.IsValid = false;
            question.ValidationMessage = $"Metadata extraction failed: {ex.Message}";
            return question;
        }
    }
}
