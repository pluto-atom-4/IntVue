// Copyright (c) YourProjectName. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
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
    /// Uses System.IO for better compatibility with local file paths.
    /// Skips media validation to avoid Windows Runtime API state errors.
    /// </summary>
    /// <param name="filePath">The full file path to a WebM media file.</param>
    /// <returns>A <see cref="Question"/> object with basic info.</returns>
    public Task<Question> LoadQuestionFileAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
        }

        try
        {
            System.Diagnostics.Debug.WriteLine($"[LoadQuestionFileAsync] Processing: {filePath}");

            // Use System.IO for file checking (reliable, no state errors)
            if (!File.Exists(filePath))
            {
                System.Diagnostics.Debug.WriteLine($"[LoadQuestionFileAsync] File not found: {filePath}");
                return Task.FromResult(new Question
                {
                    FilePath = filePath,
                    FileName = Path.GetFileName(filePath),
                    IsValid = false,
                    ValidationMessage = "File not found.",
                    DiscoveredAt = DateTime.UtcNow,
                });
            }

            // File exists, create question object
            // Skip media validation to avoid MediaPlaybackItem state errors
            var fileName = Path.GetFileName(filePath);
            System.Diagnostics.Debug.WriteLine($"[LoadQuestionFileAsync] Creating Question for: {fileName}");

            var question = new Question
            {
                FilePath = filePath,
                FileName = fileName,
                MediaUri = new Uri(filePath),
                DiscoveredAt = DateTime.UtcNow,
                IsValid = true,  // Trust that file is valid if it exists and has .webm extension
                DurationMs = 0,  // Duration set on playback
            };

            System.Diagnostics.Debug.WriteLine($"[LoadQuestionFileAsync] Question created successfully: {fileName}");
            return Task.FromResult(question);
        }
        catch (UnauthorizedAccessException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[LoadQuestionFileAsync] Access denied: {ex.Message}");
            return Task.FromResult(new Question
            {
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                IsValid = false,
                ValidationMessage = "Access denied to file.",
                DiscoveredAt = DateTime.UtcNow,
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[LoadQuestionFileAsync] Exception: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
            return Task.FromResult(new Question
            {
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                IsValid = false,
                ValidationMessage = $"Failed to load file: {ex.GetType().Name}",
                DiscoveredAt = DateTime.UtcNow,
            });
        }
    }

    /// <summary>
    /// Discovers and loads all WebM question files from a directory.
    /// Uses System.IO for better compatibility with local file paths in full-trust WinUI apps.
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
            System.Diagnostics.Debug.WriteLine($"[LoadQuestionDirectoryAsync] Loading from: {directoryPath}");

            // Use System.IO.Directory for better compatibility with local paths
            if (!Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");
            }

            System.Diagnostics.Debug.WriteLine($"[LoadQuestionDirectoryAsync] Directory exists");

            // Get all .webm files, sorted by name
            var webmFiles = Directory.GetFiles(directoryPath, $"*{_webMExtension}", SearchOption.TopDirectoryOnly)
                .OrderBy(f => Path.GetFileName(f))
                .ToList();

            System.Diagnostics.Debug.WriteLine($"[LoadQuestionDirectoryAsync] Found {webmFiles.Count} WebM files");

            // Load questions in parallel (non-blocking)
            var loadTasks = webmFiles.Select(filePath => this.LoadQuestionFileAsync(filePath)).ToList();
            var loadedQuestions = await Task.WhenAll(loadTasks);

            System.Diagnostics.Debug.WriteLine($"[LoadQuestionDirectoryAsync] Loaded {loadedQuestions.Length} questions");

            questions.AddRange(loadedQuestions);

            System.Diagnostics.Debug.WriteLine($"[LoadQuestionDirectoryAsync] Completed successfully");
        }
        catch (DirectoryNotFoundException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[LoadQuestionDirectoryAsync] DirectoryNotFoundException: {ex.Message}");
            throw;
        }
        catch (UnauthorizedAccessException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[LoadQuestionDirectoryAsync] Access denied: {ex.Message}");
            throw new UnauthorizedAccessException($"Access denied to directory: {directoryPath}. Check file system permissions.", ex);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[LoadQuestionDirectoryAsync] Exception: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
            throw new InvalidOperationException($"Failed to load question directory ({ex.GetType().Name}): {ex.Message}", ex);
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
