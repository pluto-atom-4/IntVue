// Copyright (c) YourProjectName. All rights reserved.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace IntVue.Services;

/// <summary>
/// Countdown timer service that counts down from a specified number of seconds.
/// </summary>
public class CountdownService : ICountdownService
{
    private readonly TimeSpan _tickInterval;

    /// <summary>
    /// Initializes a new instance of the <see cref="CountdownService"/> class.
    /// </summary>
    /// <param name="tickInterval">Optional interval between countdown ticks. Defaults to 1 second.</param>
    public CountdownService(TimeSpan? tickInterval = null)
        => this._tickInterval = tickInterval ?? TimeSpan.FromSeconds(1);

    /// <summary>
    /// Starts the countdown timer.
    /// </summary>
    /// <param name="seconds">The number of seconds to count down from.</param>
    /// <param name="progress">An IProgress callback that reports countdown seconds (seconds down to 0).</param>
    /// <param name="cancellationToken">A cancellation token to stop the countdown early.</param>
    /// <returns>A <see cref="Task{Boolean}"/> representing the asynchronous operation. Returns true if countdown completed; false if cancelled.</returns>
    public async Task<bool> StartAsync(int seconds, IProgress<int> progress, CancellationToken cancellationToken)
    {
        for (int i = seconds; i >= 0; i--)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return false;
            }

            progress.Report(i);

            if (i > 0)
            {
                try
                {
                    await Task.Delay(this._tickInterval, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    return false;
                }
            }
        }

        return true;
    }
}
