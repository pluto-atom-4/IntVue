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
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    public async Task<bool> StartAsync(int seconds, IProgress<int> progress, CancellationToken cancellationToken)
    {
        for (int i = seconds; i >= 1; i--)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return false;
            }

            progress.Report(i);

            try
            {
                await Task.Delay(this._tickInterval, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }

        progress.Report(0);
        return true;
    }
}
