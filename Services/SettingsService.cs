// Copyright (c) YourProjectName. All rights reserved.

using System.Threading.Tasks;

using Windows.Storage;

namespace IntVue.Services;

/// <summary>
/// Service for persisting application settings to local storage via ApplicationData.
/// </summary>
public class SettingsService : ISettingsService
{
    /// <summary>
    /// Gets a setting value by key.
    /// </summary>
    /// <param name="key">The setting key.</param>
    /// <returns>The setting value, or null if not found.</returns>
    public object? GetSetting(string key)
    {
        var localSettings = ApplicationData.Current.LocalSettings;
        if (localSettings.Values.TryGetValue(key, out var value))
        {
            return value;
        }

        return null;
    }

    /// <summary>
    /// Sets a setting value.
    /// </summary>
    /// <param name="key">The setting key.</param>
    /// <param name="value">The value to store.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task SetSettingAsync(string key, object? value)
    {
        var localSettings = ApplicationData.Current.LocalSettings;
        localSettings.Values[key] = value;
        await Task.CompletedTask;
    }
}
