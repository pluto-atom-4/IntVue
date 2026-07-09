// Copyright (c) YourProjectName. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using IntVue.Models;

namespace IntVue.Services;

/// <summary>
/// Service for discovering, loading, and validating pre-recorded interview questions.
/// </summary>
public interface IProductReviewService
{
    /// <summary>
    /// Loads a single WebM question file from the specified file path.
    /// </summary>
    /// <param name="filePath">The full file path to a WebM media file.</param>
    /// <returns>A <see cref="Question"/> object with metadata and validation status.</returns>
    /// <remarks>
    /// This method validates the file format asynchronously. If the file is invalid,
    /// the returned <see cref="Question.IsValid"/> property will be false with a corresponding error message.
    /// </remarks>
    Task<Question> LoadQuestionFileAsync(string filePath);

    /// <summary>
    /// Discovers and loads all WebM question files from a directory.
    /// </summary>
    /// <param name="directoryPath">The directory path to search for WebM files.</param>
    /// <returns>A <see cref="List{Question}"/> containing all discovered questions.</returns>
    /// <remarks>
    /// This method recursively searches the directory for .webm files and validates each one.
    /// Files are returned in an undefined order; use a playlist service to sort them.
    /// Invalid files are included in the results with <see cref="Question.IsValid"/> set to false.
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown if directoryPath is null or empty.</exception>
    /// <exception cref="System.IO.DirectoryNotFoundException">Thrown if the directory does not exist.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if access to the directory is denied.</exception>
    Task<List<Question>> LoadQuestionDirectoryAsync(string directoryPath);

    /// <summary>
    /// Validates a WebM media file for playback compatibility.
    /// </summary>
    /// <param name="uri">The URI of the media file to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> indicating success or describing the failure reason.</returns>
    /// <remarks>
    /// This method checks the WebM container format and codec support (VP9).
    /// It runs asynchronously and does not block the UI thread.
    /// </remarks>
    Task<ValidationResult> ValidateWebMAsync(Uri uri);

    /// <summary>
    /// Extracts metadata (duration, codec information) from a question file.
    /// </summary>
    /// <param name="question">The question whose metadata should be extracted.</param>
    /// <returns>The updated <see cref="Question"/> with duration and metadata populated.</returns>
    /// <remarks>
    /// If metadata extraction fails, the question's <see cref="Question.IsValid"/> property is set to false.
    /// </remarks>
    Task<Question> GetQuestionMetadataAsync(Question question);
}
