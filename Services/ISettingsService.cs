// Copyright (c) YourProjectName. All rights reserved.

using System.Threading.Tasks;

namespace IntVue.Services;

/// <summary>
/// Service for persisting application settings to local storage.
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Gets a setting value by key.
    /// </summary>
    /// <param name="key">The setting key.</param>
    /// <returns>The setting value, or null if not found.</returns>
    object? GetSetting(string key);

    /// <summary>
    /// Sets a setting value.
    /// </summary>
    /// <param name="key">The setting key.</param>
    /// <param name="value">The value to store.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task SetSettingAsync(string key, object? value);
}
