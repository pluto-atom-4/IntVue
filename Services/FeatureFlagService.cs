// Copyright (c) YourProjectName. All rights reserved.

using System.Threading.Tasks;

namespace IntVue.Services;

/// <summary>
/// Service for managing feature flags that enable/disable optional application features.
/// Flags are stored in-memory and are session-only (not persisted across app runs).
/// </summary>
public class FeatureFlagService : IFeatureFlagService
{
    private bool _productReviewEnabled;

    /// <summary>
    /// Initializes a new instance of the <see cref="FeatureFlagService"/> class.
    /// All feature flags default to disabled.
    /// </summary>
    public FeatureFlagService()
    {
        _productReviewEnabled = false;
    }

    /// <summary>
    /// Determines whether the Product Review feature is enabled.
    /// </summary>
    /// <returns>True if Product Review feature is enabled; false otherwise.</returns>
    public bool IsProductReviewEnabled() => _productReviewEnabled;

    /// <summary>
    /// Sets the enabled state of the Product Review feature.
    /// </summary>
    /// <param name="enabled">True to enable Product Review; false to disable.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task SetProductReviewEnabled(bool enabled)
    {
        _productReviewEnabled = enabled;
        await Task.CompletedTask;
    }
}
