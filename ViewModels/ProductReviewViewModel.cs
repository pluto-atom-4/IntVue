// Copyright (c) YourProjectName. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using IntVue.Models;
using IntVue.Services;

#pragma warning disable SA1201,SA1202 // Partial methods are intentionally paired with their ObservableProperty for readability

namespace IntVue.ViewModels;

/// <summary>
/// ViewModel for managing product review questions with playlist and countdown.
/// </summary>
public partial class ProductReviewViewModel : ObservableObject, IDisposable
{
    private readonly IProductReviewService _productReviewService;
    private readonly IPlaylistService _playlistService;
    private readonly ICountdownService _countdownService;
    private CancellationTokenSource? _countdownCts;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductReviewViewModel"/> class.
    /// </summary>
    /// <param name="productReviewService">Service for loading and validating questions.</param>
    /// <param name="playlistService">Service for managing question playlist.</param>
    /// <param name="countdownService">Service for countdown timers.</param>
    public ProductReviewViewModel(
        IProductReviewService productReviewService,
        IPlaylistService playlistService,
        ICountdownService countdownService)
    {
        _productReviewService = productReviewService ?? throw new ArgumentNullException(nameof(productReviewService));
        _playlistService = playlistService ?? throw new ArgumentNullException(nameof(playlistService));
        _countdownService = countdownService ?? throw new ArgumentNullException(nameof(countdownService));
    }

    /// <summary>
    /// Fired when countdown completes successfully (not cancelled).
    /// </summary>
    public event EventHandler? CountdownCompleted;

    [ObservableProperty]
    public partial string Title { get; set; } = "Product Review";

    [ObservableProperty]
    public partial string? CurrentQuestionPath { get; set; }

    [ObservableProperty]
    public partial int CurrentQuestionIndex { get; set; }

    [ObservableProperty]
    public partial int TotalQuestions { get; set; }

    [ObservableProperty]
    public partial int CountdownSeconds { get; set; }

    [ObservableProperty]
    public partial bool IsCountingDown { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial SortMode CurrentSortMode { get; set; }

    [ObservableProperty]
    public partial PlayMode CurrentPlayMode { get; set; } = PlayMode.Sequential;

    [ObservableProperty]
    public partial bool IsRecordingNow { get; set; }

    [ObservableProperty]
    public partial bool HasCamera { get; set; } = true;

    [ObservableProperty]
    public partial bool HasRecording { get; set; }

    /// <summary>
    /// Gets or sets the current question, or null if playlist is empty.
    /// Fires PropertyChanged when question changes to trigger UI updates and media reload.
    /// </summary>
    [ObservableProperty]
    public partial Question? CurrentQuestion { get; set; }

    /// <summary>
    /// Gets a value indicating whether the user can start or stop recording.
    /// Can start recording if: current question exists and no recording exists.
    /// Can stop recording if: currently recording.
    /// </summary>
    public bool CanStartOrStopRecording =>
        (CurrentQuestion != null && !HasRecording) || IsRecordingNow;

    /// <summary>
    /// Notifies about CanStartOrStopRecording changes when CurrentQuestion changes.
    /// Required because CanStartOrStopRecording is a computed property that depends on CurrentQuestion.
    /// </summary>
    partial void OnCurrentQuestionChanged(Question? value)
    {
        OnPropertyChanged(nameof(CanStartOrStopRecording));
    }

    /// <summary>
    /// Gets the playlist of questions.
    /// </summary>
    public System.Collections.ObjectModel.ObservableCollection<Question> Questions => _playlistService.Questions;

    /// <summary>
    /// Loads questions from a directory and initializes the playlist.
    /// </summary>
    /// <param name="directoryPath">Path to directory containing question files.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [RelayCommand]
    public async Task LoadQuestionsAsync(string directoryPath)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            // Load questions from directory
            var questions = await _productReviewService.LoadQuestionDirectoryAsync(directoryPath);

            if (questions.Count == 0)
            {
                ErrorMessage = "No valid question files found in directory.";
                return;
            }

            // Initialize playlist with loaded questions
            await _playlistService.InitializeAsync(questions);

            // Update UI state
            UpdatePlaylistState();
        }
        catch (ArgumentException)
        {
            ErrorMessage = "Invalid directory path provided.";
        }
        catch (System.IO.DirectoryNotFoundException)
        {
            ErrorMessage = "Directory not found.";
        }
        catch (UnauthorizedAccessException)
        {
            ErrorMessage = "Access to directory denied.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load questions: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Moves to the next question in the playlist.
    /// </summary>
    [RelayCommand]
    public void MoveToNext()
    {
        try
        {
            var result = _playlistService.MoveToNext();
            UpdatePlaylistState();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to move to next question: {ex.Message}";
        }
    }

    /// <summary>
    /// Moves to the previous question in the playlist.
    /// </summary>
    [RelayCommand]
    public void MoveToPrevious()
    {
        try
        {
            var result = _playlistService.MoveToPrevious();
            UpdatePlaylistState();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to move to previous question: {ex.Message}";
        }
    }

    /// <summary>
    /// Selects a question by index.
    /// </summary>
    /// <param name="index">The index of the question to select.</param>
    [RelayCommand]
    public void SelectQuestion(int index)
    {
        try
        {
            var result = _playlistService.SelectByIndex(index);
            if (result == null)
            {
                ErrorMessage = "Invalid question index.";
                return;
            }

            UpdatePlaylistState();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to select question: {ex.Message}";
        }
    }

    /// <summary>
    /// Applies a sort mode to the playlist.
    /// </summary>
    /// <param name="sortMode">The sort mode to apply.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [RelayCommand]
    public async Task ApplySortAsync(SortMode sortMode)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            await _playlistService.ApplySortAsync(sortMode);
            UpdatePlaylistState();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to apply sort: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Sets the play mode for the playlist.
    /// </summary>
    /// <param name="playMode">The play mode to apply.</param>
    [RelayCommand]
    public void SetPlayMode(PlayMode playMode)
    {
        try
        {
            _playlistService.SetPlayMode(playMode);
            CurrentPlayMode = playMode;
            UpdatePlaylistState();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to set play mode: {ex.Message}";
        }
    }

    /// <summary>
    /// Shuffles the questions in the playlist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [RelayCommand]
    public async Task ShuffleQuestionsAsync()
    {
        try
        {
            await this.ApplySortAsync(SortMode.Shuffle);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to shuffle questions: {ex.Message}";
        }
    }

    /// <summary>
    /// Switches to Loop play mode (wraps to start at end).
    /// </summary>
    [RelayCommand]
    public void ActivateLoopMode()
    {
        try
        {
            this.SetPlayMode(PlayMode.Loop);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to activate loop mode: {ex.Message}";
        }
    }

    /// <summary>
    /// Switches to Repeat play mode (stays on current question).
    /// </summary>
    [RelayCommand]
    public void ActivateRepeatMode()
    {
        try
        {
            this.SetPlayMode(PlayMode.RepeatCurrent);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to activate repeat mode: {ex.Message}";
        }
    }

    /// <summary>
    /// Starts a countdown timer.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [RelayCommand]
    public async Task StartCountdownAsync()
    {
        try
        {
            _countdownCts?.Dispose();
            _countdownCts = new CancellationTokenSource();
            IsCountingDown = true;
            CountdownSeconds = 3;

            var progress = new Progress<int>(s => CountdownSeconds = s);
            bool completed = await _countdownService.StartAsync(3, progress, _countdownCts.Token);

            IsCountingDown = false;
            if (completed)
            {
                CountdownCompleted?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Countdown error: {ex.Message}";
            IsCountingDown = false;
        }
    }

    /// <summary>
    /// Cancels the countdown if one is in progress.
    /// </summary>
    [RelayCommand]
    public void CancelCountdown()
    {
        try
        {
            _countdownCts?.Cancel();
            IsCountingDown = false;
            CountdownSeconds = 0;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to cancel countdown: {ex.Message}";
        }
    }

    /// <summary>
    /// Navigates back to the main screen.
    /// </summary>
    [RelayCommand]
    public void GoBack()
    {
        try
        {
            // Cancel any ongoing countdown
            _countdownCts?.Cancel();
            IsCountingDown = false;

            // Request navigation back via Frame.GoBack()
            // This will be handled in the code-behind (ProductReviewPage.xaml.cs)
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to go back: {ex.Message}";
        }
    }

    /// <summary>
    /// Disposes the view model and releases resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes view model resources.
    /// </summary>
    /// <param name="disposing">True if disposing managed resources; false if finalizing.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _countdownCts?.Dispose();
            _playlistService.Clear();
        }

        _disposed = true;
    }

    /// <summary>
    /// Updates UI state properties from the playlist service.
    /// Synchronizes display, current question, and sort mode with the playlist service state.
    /// </summary>
    private void UpdatePlaylistState()
    {
        CurrentQuestionIndex = _playlistService.CurrentIndex + 1;
        TotalQuestions = _playlistService.TotalCount;
        CurrentQuestionPath = _playlistService.CurrentQuestion?.FilePath ?? string.Empty;
        CurrentQuestion = _playlistService.CurrentQuestion;
        CurrentSortMode = _playlistService.CurrentSortMode;
    }

    /// <summary>
    /// Notifies when HasRecording property changes to update CanStartOrStopRecording binding.
    /// </summary>
    partial void OnHasRecordingChanged(bool value)
    {
        OnPropertyChanged(nameof(CanStartOrStopRecording));
    }

    /// <summary>
    /// Notifies when IsRecordingNow property changes to update CanStartOrStopRecording binding.
    /// </summary>
    partial void OnIsRecordingNowChanged(bool value)
    {
        OnPropertyChanged(nameof(CanStartOrStopRecording));
    }
}
