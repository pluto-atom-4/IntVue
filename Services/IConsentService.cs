// <copyright file="IConsentService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace IntVue.Services;

using System;
using System.Threading.Tasks;

/// <summary>
/// Service for managing user consent for camera and microphone access.
/// Handles consent dialogs, persistence, and retrieval from local settings.
/// </summary>
public interface IConsentService
{
    /// <summary>
    /// Gets a value indicating whether the user has already given consent for camera/microphone access.
    /// </summary>
    bool HasGivenConsent { get; }

    /// <summary>
    /// Gets the timestamp when consent was given, or null if not given.
    /// </summary>
    DateTime? ConsentTimestamp { get; }

    /// <summary>
    /// Displays a consent dialog and returns true if user accepted, false if declined.
    /// Only shows on first use; subsequent calls return stored consent status.
    /// </summary>
    /// <param name="xamlRoot">The XAML root required for the dialog.</param>
    /// <returns>A <see cref="Task{Boolean}"/> representing the asynchronous operation and returning the consent decision.</returns>
    Task<bool> RequestConsentAsync(object xamlRoot);

    /// <summary>
    /// Revokes previously given consent and resets the consent state.
    /// </summary>
    void RevokeConsent();
}
