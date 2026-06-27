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
public partial class MainViewModel : ObservableObject
{
    private readonly ICountdownService _countdownService;
    private CancellationTokenSource? _countdownCts;

    [ObservableProperty]
    public partial string Title { get; set; } = "IntVue";

    [ObservableProperty]
    public partial int CountdownSeconds { get; set; }

    [ObservableProperty]
    public partial bool IsCountingDown { get; set; }

    /// <summary>
    /// Fired when countdown completes successfully (not cancelled).
    /// </summary>
    public event EventHandler? CountdownCompleted;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainViewModel"/> class.
    /// </summary>
    public MainViewModel(ICountdownService countdownService)
        => this._countdownService = countdownService;

    /// <summary>
    /// Starts the countdown timer. Fires CountdownCompleted event when done.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
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
