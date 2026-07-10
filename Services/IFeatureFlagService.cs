// Copyright (c) YourProjectName. All rights reserved.

using System.Threading.Tasks;

namespace IntVue.Services;

/// <summary>
/// Service for managing feature flags that enable/disable optional application features.
/// </summary>
public interface IFeatureFlagService
{
    /// <summary>
    /// Determines whether the Product Review feature is enabled.
    /// </summary>
    /// <returns>True if Product Review feature is enabled; false otherwise.</returns>
    bool IsProductReviewEnabled();

    /// <summary>
    /// Sets the enabled state of the Product Review feature.
    /// </summary>
    /// <param name="enabled">True to enable Product Review; false to disable.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task SetProductReviewEnabled(bool enabled);
}
