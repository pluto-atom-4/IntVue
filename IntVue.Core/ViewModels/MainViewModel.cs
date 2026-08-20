// Copyright (c) YourProjectName. All rights reserved.

using System;
using System.Threading;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using IntVue.Services;

namespace IntVue.ViewModels;

/// <summary>
/// Main application view model with countdown and recording logic.
/// </summary>
public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly ICountdownService _countdownService;
    private readonly IFeatureFlagService _featureFlagService;
    private CancellationTokenSource? _countdownCts;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainViewModel"/> class.
    /// </summary>
    /// <param name="countdownService">The countdown service used to manage countdown timers.</param>
    /// <param name="featureFlagService">The feature flag service for enabling/disabling features.</param>
    public MainViewModel(ICountdownService countdownService, IFeatureFlagService featureFlagService)
    {
        this._countdownService = countdownService;
        this._featureFlagService = featureFlagService;
        this.IsProductReviewEnabled = featureFlagService.IsProductReviewEnabled();
    }

    /// <summary>
    /// Fired when countdown completes successfully (not cancelled).
    /// </summary>
    public event EventHandler? CountdownCompleted;

    [ObservableProperty]
    public partial string Title { get; set; } = "IntVue";

    [ObservableProperty]
    public partial int CountdownSeconds { get; set; }

    [ObservableProperty]
    public partial bool IsCountingDown { get; set; }

    [ObservableProperty]
    public partial bool IsProductReviewEnabled { get; set; }

    /// <summary>
    /// Starts the countdown timer. Fires CountdownCompleted event when done.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task StartCountdownAsync()
    {
        this._countdownCts?.Dispose();
        this._countdownCts = new CancellationTokenSource();
        this.IsCountingDown = true;
        this.CountdownSeconds = 3;

        var progress = new Progress<int>(s => this.CountdownSeconds = s);
        bool completed = await this._countdownService.StartAsync(3, progress, this._countdownCts.Token);

        this.IsCountingDown = false;
        if (completed)
        {
            this.CountdownCompleted?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Disposes the view model and releases resources.
    /// </summary>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the view model resources.
    /// </summary>
    /// <param name="disposing">True if disposing managed resources; false if finalizing.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (this._disposed)
        {
            return;
        }

        if (disposing)
        {
            this._countdownCts?.Dispose();
        }

        this._disposed = true;
    }

    /// <summary>
    /// Cancels the countdown if one is in progress.
    /// </summary>
    [RelayCommand]
    private void CancelCountdown()
    {
        this._countdownCts?.Cancel();
        this.IsCountingDown = false;
        this.CountdownSeconds = 0;
    }
}
