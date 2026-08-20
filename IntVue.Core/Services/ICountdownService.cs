// Copyright (c) YourProjectName. All rights reserved.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace IntVue.Services;

/// <summary>
/// Service for managing countdown timers before recording starts.
/// </summary>
public interface ICountdownService
{
    /// <summary>
    /// Starts a countdown timer that reports progress and returns on completion or cancellation.
    /// </summary>
    /// <param name="seconds">Number of seconds to count down from.</param>
    /// <param name="progress">Progress reporter that receives countdown values (seconds down to 0).</param>
    /// <param name="cancellationToken">Token for cancelling the countdown.</param>
    /// <returns>true if countdown completed normally; false if cancelled.</returns>
    Task<bool> StartAsync(int seconds, IProgress<int> progress, CancellationToken cancellationToken);
}
