// Copyright (c) YourProjectName. All rights reserved.

using System;
using System.Threading.Tasks;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using Windows.Storage;

namespace IntVue.Services;

/// <summary>
/// Manages user consent for camera and microphone access.
/// Persists consent state and timestamp to ApplicationData.LocalSettings.
/// </summary>
public class ConsentService : IConsentService
{
    private const string _consentGivenKey = "CameraConsentGiven";
    private const string _consentTimestampKey = "CameraConsentTimestamp";
    private const string _consentDialogDismissedKey = "ConsentDialogDismissed";

    /// <inheritdoc/>
    public bool HasGivenConsent
    {
        get
        {
            var settings = ApplicationData.Current.LocalSettings;
            return (bool?)settings.Values[_consentGivenKey] ?? false;
        }
    }

    /// <inheritdoc/>
    public DateTime? ConsentTimestamp
    {
        get
        {
            var settings = ApplicationData.Current.LocalSettings;
            var value = settings.Values[_consentTimestampKey];
            if (value is string timestamp && DateTime.TryParse(timestamp, out var result))
            {
                return result;
            }

            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> RequestConsentAsync(object xamlRoot)
    {
        // If consent was already given, return true without showing dialog
        if (this.HasGivenConsent)
        {
            return true;
        }

        // If dialog was already dismissed this session, don't show again
        var settings = ApplicationData.Current.LocalSettings;
        if ((bool?)settings.Values[_consentDialogDismissedKey] ?? false)
        {
            return false;
        }

        // Show consent dialog
        var dialog = new ContentDialog
        {
            Title = "Privacy Notice",
            Content = BuildConsentMessage(),
            PrimaryButtonText = "I Agree",
            CloseButtonText = "Decline",
            XamlRoot = (XamlRoot)xamlRoot,
        };

        var result = await dialog.ShowAsync();

        // Mark dialog as dismissed this session
        settings.Values[_consentDialogDismissedKey] = true;

        // Check if user clicked "I Agree" button
        if (result == ContentDialogResult.Primary)
        {
            SetConsentGiven();
            return true;
        }

        // User declined or dismissed
        return false;
    }

    /// <inheritdoc/>
    public void RevokeConsent()
    {
        var settings = ApplicationData.Current.LocalSettings;
        settings.Values[_consentGivenKey] = false;
        settings.Values[_consentTimestampKey] = null;
        settings.Values[_consentDialogDismissedKey] = false;
    }

    /// <summary>
    /// Sets consent as given and records the timestamp.
    /// </summary>
    private static void SetConsentGiven()
    {
        var settings = ApplicationData.Current.LocalSettings;
        settings.Values[_consentGivenKey] = true;
        settings.Values[_consentTimestampKey] = DateTime.Now.ToString("O");  // ISO 8601 format
    }

    /// <summary>
    /// Builds the consent message displayed in the dialog.
    /// </summary>
    /// <returns>The consent message text.</returns>
    private static string BuildConsentMessage()
    {
        return "IntVue requires access to your camera and microphone to record interview practice videos.\n\n" +
               "Recordings are saved locally on your device and are not transmitted to any external service.\n\n" +
               "By clicking 'I Agree', you consent to the use of camera and microphone for recording.\n\n" +
               "You can revoke this consent at any time through your device settings.";
    }
}
