// Copyright (c) YourProjectName. All rights reserved.

namespace IntVue.Models;

/// <summary>
/// Represents the result of validating a WebM media file.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationResult"/> class with a success or failure state.
    /// </summary>
    /// <param name="isValid">Whether the WebM file passed validation.</param>
    /// <param name="message">Optional error message if validation failed.</param>
    public ValidationResult(bool isValid, string? message = null)
    {
        IsValid = isValid;
        Message = message;
    }

    /// <summary>
    /// Gets a value indicating whether the WebM file is valid and ready for playback.
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// Gets an optional error message describing why validation failed (if <see cref="IsValid"/> is false).
    /// </summary>
    public string? Message { get; }

    /// <summary>
    /// Returns a successful validation result.
    /// </summary>
    /// <returns>A <see cref="ValidationResult"/> indicating success.</returns>
    public static ValidationResult Success() => new ValidationResult(true);

    /// <summary>
    /// Returns a failed validation result with an error message.
    /// </summary>
    /// <param name="message">The error message describing the validation failure.</param>
    /// <returns>A <see cref="ValidationResult"/> indicating failure.</returns>
    public static ValidationResult Failure(string message) => new ValidationResult(false, message);
}
