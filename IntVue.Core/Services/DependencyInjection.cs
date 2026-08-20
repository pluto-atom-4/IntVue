// Copyright (c) YourProjectName. All rights reserved.

using Microsoft.Extensions.DependencyInjection;

namespace IntVue.Services;

/// <summary>
/// Dependency injection configuration for IntVue.Core services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds all cross-platform IntVue.Core services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddIntVueCore(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register platform-agnostic services
        services.AddSingleton<ICountdownService, CountdownService>();
        services.AddSingleton<IFeatureFlagService, FeatureFlagService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IConsentService, ConsentService>();
        services.AddSingleton<IProductReviewService, ProductReviewService>();
        services.AddSingleton<IPlaylistService, PlaylistService>();

        return services;
    }
}
