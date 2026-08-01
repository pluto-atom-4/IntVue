// Copyright (c) YourProjectName. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using IntVue.Models;
using IntVue.Services;
using IntVue.ViewModels;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using Windows.Devices.Enumeration;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace IntVue.Views;

/// <summary>
/// ProductReviewPage - XAML view for playing pre-recorded interview questions with countdown timer.
/// Supports WebM video playback, playlist navigation, and countdown-based recording workflow.
/// </summary>
#pragma warning disable CA1001 // Page disposes _recordingService in OnUnloaded; WinUI pattern
public sealed partial class ProductReviewPage : Page
#pragma warning restore CA1001
{
    private ProductReviewViewModel? _viewModel;
    private ProductReviewRecordingService? _recordingService;
    private List<DeviceInformation>? _deviceList;
    private DispatcherTimer? _videoPlaybackTimer;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductReviewPage"/> class.
    /// </summary>
    public ProductReviewPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Gets viewModel instance providing UI state, commands, and business logic.
    /// </summary>
    public ProductReviewViewModel ViewModel
    {
        get
        {
            if (this._viewModel == null)
            {
                this._viewModel = App.Services.GetService<ProductReviewViewModel>()
                    ?? throw new InvalidOperationException("ProductReviewViewModel not registered");
            }

            return this._viewModel;
        }
    }

    /// <summary>
    /// Page loaded - Load questions, initialize recording service, and set up media playback.
    /// </summary>
    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            // Set DataContext to ensure XAML bindings use same ViewModel instance
            this.DataContext = this.ViewModel;

            // Subscribe to ViewModel property changes to detect when CurrentQuestion changes
            this.ViewModel.PropertyChanged += this.OnViewModelPropertyChanged;

            // Subscribe to media player events
            if (this.MediaPlayer?.MediaPlayer != null)
            {
                var mediaPlayer = this.MediaPlayer.MediaPlayer;
                mediaPlayer.MediaEnded += this.OnMediaEnded;
                System.Diagnostics.Debug.WriteLine("[ProductReviewPage.OnLoaded] Subscribed to MediaPlayer.MediaEnded event");

                // Also subscribe to PlaybackStateChanged as backup
                if (mediaPlayer.PlaybackSession != null)
                {
                    mediaPlayer.PlaybackSession.PlaybackStateChanged += this.OnPlaybackStateChanged;
                    System.Diagnostics.Debug.WriteLine("[ProductReviewPage.OnLoaded] Subscribed to PlaybackSession.PlaybackStateChanged event");
                }
            }

            // Initialize recording service
            await this.InitializeRecordingAsync();

            // Load questions from the directory specified via CLI (--questions-dir)
            if (!string.IsNullOrEmpty(App.QuestionsDirectory))
            {
                await this.ViewModel.LoadQuestionsCommand.ExecuteAsync(App.QuestionsDirectory);

                // Load the first question's media after loading completes
                this.LoadCurrentQuestionMedia();
            }
            else
            {
                this.ViewModel.ErrorMessage = "No questions directory specified via --questions-dir";
            }
        }
        catch (Exception ex)
        {
            this.ViewModel.ErrorMessage = $"Error loading page: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"[ProductReviewPage.OnLoaded] Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Page unloaded - Clean up resources and dispose ViewModel and recording service.
    /// </summary>
    private async void OnUnloaded(object sender, RoutedEventArgs e)
    {
        try
        {
            // Stop recording if active
            if (_recordingService?.IsRecording == true)
            {
                System.Diagnostics.Debug.WriteLine("[ProductReviewPage.OnUnloaded] Stopping active recording");
                await _recordingService.StopRecordingAsync();
            }

            // Unsubscribe from ViewModel property changes
            this.ViewModel.PropertyChanged -= this.OnViewModelPropertyChanged;
            this.ViewModel.CountdownCompleted -= this.OnCountdownCompleted;

            // Stop video playback monitor timer
            if (this._videoPlaybackTimer != null)
            {
                this._videoPlaybackTimer.Stop();
                this._videoPlaybackTimer.Tick -= this.OnVideoPlaybackTimerTick;
                this._videoPlaybackTimer = null;
                System.Diagnostics.Debug.WriteLine("[ProductReviewPage.OnUnloaded] Video playback timer stopped");
            }

            // Unsubscribe from media player events
            if (this.MediaPlayer?.MediaPlayer != null)
            {
                this.MediaPlayer.MediaPlayer.MediaEnded -= this.OnMediaEnded;

                if (this.MediaPlayer.MediaPlayer.PlaybackSession != null)
                {
                    this.MediaPlayer.MediaPlayer.PlaybackSession.PlaybackStateChanged -= this.OnPlaybackStateChanged;
                }
            }

            // Dispose recording service
            _recordingService?.Dispose();

            System.Diagnostics.Debug.WriteLine("[ProductReviewPage.OnUnloaded] Cleanup complete");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductReviewPage.OnUnloaded] Error during cleanup: {ex.Message}");
        }
    }

    /// <summary>
    /// Initializes the recording service and enumerates video capture devices.
    /// </summary>
    private async Task InitializeRecordingAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[ProductReviewPage.InitializeRecordingAsync] Starting");

            // Create recording service
            _recordingService = new ProductReviewRecordingService();

            // Enumerate video capture devices
            System.Diagnostics.Debug.WriteLine("[ProductReviewPage.InitializeRecordingAsync] Enumerating devices...");
            _deviceList = (await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture)).ToList();

            if (_deviceList.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("[ProductReviewPage.InitializeRecordingAsync] No camera devices found");
                this.ViewModel.ErrorMessage = "No camera devices found. Recording will not be available.";
                this.ViewModel.HasCamera = false;
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[ProductReviewPage.InitializeRecordingAsync] Found {_deviceList.Count} camera(s)");

            // Initialize first device
            var firstDevice = _deviceList[0];
            System.Diagnostics.Debug.WriteLine($"[ProductReviewPage.InitializeRecordingAsync] Initializing device: {firstDevice.Name}");
            await _recordingService.InitializeAsync(firstDevice.Id);

            System.Diagnostics.Debug.WriteLine("[ProductReviewPage.InitializeRecordingAsync] Recording service initialized successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductReviewPage.InitializeRecordingAsync] Error: {ex.Message}");
            this.ViewModel.ErrorMessage = $"Failed to initialize recording: {ex.Message}";
        }
    }

    /// <summary>
    /// Handles ViewModel property changes to reload media when CurrentQuestion or CurrentQuestionPath changes.
    /// </summary>
    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProductReviewViewModel.CurrentQuestion) ||
            e.PropertyName == nameof(ProductReviewViewModel.CurrentQuestionPath))
        {
            this.LoadCurrentQuestionMedia();
        }
    }

    /// <summary>
    /// Loads the current question's media file into the MediaPlayerElement.
    /// Resets MediaPlayer state before loading new source to ensure clean playback state.
    /// </summary>
    private void LoadCurrentQuestionMedia()
    {
        try
        {
            var currentQuestion = this.ViewModel.CurrentQuestion;

            if (currentQuestion == null || string.IsNullOrEmpty(currentQuestion.FilePath))
            {
                System.Diagnostics.Debug.WriteLine("[ProductReviewPage.LoadCurrentQuestionMedia] No current question or file path");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[ProductReviewPage.LoadCurrentQuestionMedia] Loading: {currentQuestion.FilePath}");

            // Reset MediaPlayer state before loading new source
            // This ensures playback state is clean when switching media (e.g., from recording playback back to question media)
            if (this.MediaPlayer?.MediaPlayer != null)
            {
                var mediaPlayer = this.MediaPlayer.MediaPlayer;
                mediaPlayer.Pause();
                mediaPlayer.PlaybackSession?.Position = TimeSpan.Zero;
                System.Diagnostics.Debug.WriteLine("[ProductReviewPage.LoadCurrentQuestionMedia] MediaPlayer state reset (pause + position=0)");
            }

            // Create media source from file path and load into MediaPlayerElement
            var mediaSource = MediaSource.CreateFromUri(new Uri(currentQuestion.FilePath));
            this.MediaPlayer.Source = mediaSource;

            System.Diagnostics.Debug.WriteLine($"[ProductReviewPage.LoadCurrentQuestionMedia] Media source loaded successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductReviewPage.LoadCurrentQuestionMedia] Error: {ex.GetType().Name}: {ex.Message}");
            this.ViewModel.ErrorMessage = $"Failed to load media: {ex.Message}";
        }
    }

    /// <summary>
    /// Back button click handler - Navigate back to previous page.
    /// </summary>
    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (Frame?.CanGoBack == true)
            {
                Frame.GoBack();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Navigation back failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Recording action button click handler - Consolidates start and stop recording.
    /// If not recording: Starts video playback sequence (Video plays → Countdown → Recording starts).
    /// If recording: Stops the active recording session.
    /// </summary>
    private async void BtnRecord_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (this.ViewModel.IsRecordingNow)
            {
                System.Diagnostics.Debug.WriteLine("[ProductReviewPage.BtnRecord_Click] Stopping recording");
                await this.StopRecordingAsync();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[ProductReviewPage.BtnRecord_Click] Starting video playback");
                this.StartVideoPlayback();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductReviewPage.BtnRecord_Click] Exception: {ex.GetType().Name}: {ex.Message}");
            this.ViewModel.ErrorMessage = $"Failed to handle recording action: {ex.Message}";
        }
    }

    /// <summary>
    /// Shuffle button click handler - Toggles shuffle mode on/off.
    /// </summary>
    private async void BtnShuffle_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[ProductReviewPage.BtnShuffle_Click] Toggling shuffle mode");
            await this.ViewModel.ToggleShuffleAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductReviewPage.BtnShuffle_Click] Error: {ex.Message}");
            this.ViewModel.ErrorMessage = $"Failed to toggle shuffle: {ex.Message}";
        }
    }

    /// <summary>
    /// Loop button click handler - Toggles loop mode on/off (mutually exclusive with Repeat).
    /// </summary>
    private void BtnLoop_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[ProductReviewPage.BtnLoop_Click] Toggling loop mode");
            this.ViewModel.ToggleLoopMode();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductReviewPage.BtnLoop_Click] Error: {ex.Message}");
            this.ViewModel.ErrorMessage = $"Failed to toggle loop mode: {ex.Message}";
        }
    }

    /// <summary>
    /// Repeat button click handler - Toggles repeat mode on/off (mutually exclusive with Loop).
    /// </summary>
    private void BtnRepeat_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[ProductReviewPage.BtnRepeat_Click] Toggling repeat mode");
            this.ViewModel.ToggleRepeatMode();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductReviewPage.BtnRepeat_Click] Error: {ex.Message}");
            this.ViewModel.ErrorMessage = $"Failed to toggle repeat mode: {ex.Message}";
        }
    }

    /// <summary>
    /// Helper method to stop recording.
    /// </summary>
    private async Task StopRecordingAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[ProductReviewPage.StopRecordingAsync] Stopping recording");

            if (_recordingService?.IsRecording == true)
            {
                await _recordingService.StopRecordingAsync();
                this.ViewModel.IsRecordingNow = false;
                this.ViewModel.HasRecording = true;
                System.Diagnostics.Debug.WriteLine("[ProductReviewPage.StopRecordingAsync] Recording stopped successfully");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[ProductReviewPage.StopRecordingAsync] No active recording to stop");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductReviewPage.StopRecordingAsync] Error: {ex.Message}");
            this.ViewModel.ErrorMessage = $"Failed to stop recording: {ex.Message}";
        }
    }

    /// <summary>
    /// Helper method to start video playback.
    /// </summary>
    private void StartVideoPlayback()
    {
        try
        {
            // Start video playback immediately
            if (this.MediaPlayer?.MediaPlayer != null)
            {
                var mediaPlayer = this.MediaPlayer.MediaPlayer;
                var playbackSession = mediaPlayer.PlaybackSession;

                System.Diagnostics.Debug.WriteLine($"[ProductReviewPage.StartVideoPlayback] MediaPlayer state: PlaybackState={playbackSession?.PlaybackState}, Duration={playbackSession?.NaturalDuration.TotalSeconds}s");

                mediaPlayer.Play();
                System.Diagnostics.Debug.WriteLine("[ProductReviewPage.StartVideoPlayback] Play() called, video playback started");

                // Start timer to monitor when video finishes
                this.StartVideoPlaybackMonitor();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[ProductReviewPage.StartVideoPlayback] ERROR: MediaPlayer is null");
                this.ViewModel.ErrorMessage = "MediaPlayer not available";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductReviewPage.StartVideoPlayback] Exception: {ex.GetType().Name}: {ex.Message}");
            this.ViewModel.ErrorMessage = $"Failed to start playback: {ex.Message}";
        }
    }

    /// <summary>
    /// Starts a timer to monitor video playback and detect when it finishes.
    /// </summary>
    private void StartVideoPlaybackMonitor()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[ProductReviewPage.StartVideoPlaybackMonitor] Starting video playback monitor timer");

            // Stop any existing timer
            if (this._videoPlaybackTimer != null)
            {
                this._videoPlaybackTimer.Stop();
                this._videoPlaybackTimer.Tick -= this.OnVideoPlaybackTimerTick;
                this._videoPlaybackTimer = null;
            }

            // Create and start new timer (check every 500ms)
            this._videoPlaybackTimer = new DispatcherTimer();
            this._videoPlaybackTimer.Interval = TimeSpan.FromMilliseconds(500);
            this._videoPlaybackTimer.Tick += this.OnVideoPlaybackTimerTick;
            this._videoPlaybackTimer.Start();

            System.Diagnostics.Debug.WriteLine("[ProductReviewPage.StartVideoPlaybackMonitor] Timer started, checking every 500ms");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductReviewPage.StartVideoPlaybackMonitor] Error: {ex.GetType().Name}: {ex.Message}");
        }
    }

    /// <summary>
    /// Timer tick handler - Checks if video playback has finished.
    /// </summary>
    private async void OnVideoPlaybackTimerTick(object? sender, object e)
    {
        try
        {
            var mediaPlayer = this.MediaPlayer?.MediaPlayer;
            if (mediaPlayer == null)
            {
                System.Diagnostics.Debug.WriteLine("[ProductReviewPage.OnVideoPlaybackTimerTick] MediaPlayer is null, stopping timer");
                if (this._videoPlaybackTimer != null)
                {
                    this._videoPlaybackTimer.Stop();
                }

                return;
            }

            var playbackSession = mediaPlayer.PlaybackSession;
            if (playbackSession == null)
            {
                return;
            }

            var currentPosition = playbackSession.Position.TotalSeconds;
            var duration = playbackSession.NaturalDuration.TotalSeconds;
            var playbackState = playbackSession.PlaybackState;

            System.Diagnostics.Debug.WriteLine($"[ProductReviewPage.OnVideoPlaybackTimerTick] Position: {currentPosition:F2}s / {duration:F2}s, State: {playbackState}");

            // Check if video has finished (position >= duration - 0.5 second buffer)
            if (duration > 0 && currentPosition >= (duration - 0.5))
            {
                System.Diagnostics.Debug.WriteLine($"[ProductReviewPage.OnVideoPlaybackTimerTick] Video finished! Position={currentPosition:F2}s, Duration={duration:F2}s");

                // Stop timer
                if (this._videoPlaybackTimer != null)
                {
                    this._videoPlaybackTimer.Stop();
                    this._videoPlaybackTimer = null;
                }

                // Start countdown
                await this.StartCountdownAfterVideo();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductReviewPage.OnVideoPlaybackTimerTick] Error: {ex.GetType().Name}: {ex.Message}");
        }
    }

    /// <summary>
    /// Playback state changed event - Fires when playback state changes. Detects when video finishes.
    /// </summary>
    private async void OnPlaybackStateChanged(MediaPlaybackSession sender, object args)
    {
        try
        {
            var state = sender?.PlaybackState;
            System.Diagnostics.Debug.WriteLine($"[ProductReviewPage.OnPlaybackStateChanged] PlaybackState changed to: {state}");

            // Check if playback has ended
            if (state == MediaPlaybackState.None)
            {
                System.Diagnostics.Debug.WriteLine("[ProductReviewPage.OnPlaybackStateChanged] Playback ended (state=None), starting countdown");
                await this.StartCountdownAfterVideo();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductReviewPage.OnPlaybackStateChanged] Error: {ex.GetType().Name}: {ex.Message}");
        }
    }

    /// <summary>
    /// Helper method to start countdown after video finishes.
    /// </summary>
    private async Task StartCountdownAfterVideo()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[ProductReviewPage.StartCountdownAfterVideo] Starting countdown sequence");

            if (this.ViewModel == null)
            {
                System.Diagnostics.Debug.WriteLine("[ProductReviewPage.StartCountdownAfterVideo] ERROR: ViewModel is null");
                return;
            }

            // Subscribe to countdown completion event
            this.ViewModel.CountdownCompleted += this.OnCountdownCompleted;
            System.Diagnostics.Debug.WriteLine("[ProductReviewPage.StartCountdownAfterVideo] Subscribed to CountdownCompleted event");

            // Start the countdown (will trigger OnCountdownCompleted when done)
            System.Diagnostics.Debug.WriteLine("[ProductReviewPage.StartCountdownAfterVideo] Executing StartCountdownCommand");
            if (this.ViewModel.StartCountdownCommand.CanExecute(null))
            {
                await this.ViewModel.StartCountdownCommand.ExecuteAsync(null);
                System.Diagnostics.Debug.WriteLine("[ProductReviewPage.StartCountdownAfterVideo] Countdown command executed successfully");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[ProductReviewPage.StartCountdownAfterVideo] ERROR: StartCountdownCommand cannot execute");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductReviewPage.StartCountdownAfterVideo] Error: {ex.GetType().Name}: {ex.Message}");
            this.ViewModel.ErrorMessage = $"Countdown error: {ex.Message}";
        }
    }

    /// <summary>
    /// Media player event - Fires when video finishes playing. Starts countdown timer.
    /// </summary>
    private async void OnMediaEnded(MediaPlayer sender, object args)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[ProductReviewPage.OnMediaEnded] MediaEnded event fired");
            await this.StartCountdownAfterVideo();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductReviewPage.OnMediaEnded] Error: {ex.GetType().Name}: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles countdown completion - starts recording after countdown finishes.
    /// </summary>
    private async void OnCountdownCompleted(object? sender, EventArgs e)
    {
        try
        {
            // Unsubscribe to prevent multiple registrations
            this.ViewModel.CountdownCompleted -= this.OnCountdownCompleted;

            System.Diagnostics.Debug.WriteLine("[ProductReviewPage.OnCountdownCompleted] Countdown finished, starting recording");

            // Start recording (video is already playing from BtnRecord_Click)
            if (_recordingService != null)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("[ProductReviewPage.OnCountdownCompleted] Starting recording...");
                    await _recordingService.StartRecordingAsync("product_review_response");
                    this.ViewModel.IsRecordingNow = true;
                    System.Diagnostics.Debug.WriteLine("[ProductReviewPage.OnCountdownCompleted] Recording started successfully");
                }
                catch (Exception recordEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[ProductReviewPage.OnCountdownCompleted] Recording error: {recordEx.Message}");
                    this.ViewModel.ErrorMessage = $"Failed to start recording: {recordEx.Message}";
                    this.ViewModel.IsRecordingNow = false;
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[ProductReviewPage.OnCountdownCompleted] Warning: Recording service not available");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductReviewPage.OnCountdownCompleted] Error: {ex.Message}");
            this.ViewModel.ErrorMessage = $"Countdown completion error: {ex.Message}";
        }
    }

    /// <summary>
    /// Delete Recording button click handler - Deletes the recorded file and advances based on play mode.
    /// Behavior after delete depends on current play mode:
    /// - Loop mode: Advance to next question (continue in sequence).
    /// - Shuffle mode: Advance to next shuffled question (continue through shuffled list).
    /// - Repeat mode: Stay on current question (allow re-record or explicit navigation).
    /// </summary>
    private async void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[ProductReviewPage.BtnDelete_Click] Deleting recording");
            if (_recordingService?.RecordedFile != null)
            {
                await _recordingService.DeleteRecordingAsync();
                this.ViewModel.HasRecording = false;
                System.Diagnostics.Debug.WriteLine("[ProductReviewPage.BtnDelete_Click] Recording deleted");

                // Respect play mode when deleting
                // Loop/Shuffle modes: Advance to next question
                // Repeat mode: Stay on current question
                if (this.ViewModel.CurrentPlayMode != PlayMode.RepeatCurrent)
                {
                    System.Diagnostics.Debug.WriteLine($"[ProductReviewPage.BtnDelete_Click] Advancing to next question ({this.ViewModel.CurrentPlayMode} mode)");
                    if (this.ViewModel.MoveToNextCommand.CanExecute(null))
                    {
                        this.ViewModel.MoveToNextCommand.Execute(null);
                        System.Diagnostics.Debug.WriteLine("[ProductReviewPage.BtnDelete_Click] Advanced to next question");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[ProductReviewPage.BtnDelete_Click] Staying on current question (Repeat mode)");
                }

                // Load media for the current question (after potential navigation)
                this.LoadCurrentQuestionMedia();
                System.Diagnostics.Debug.WriteLine("[ProductReviewPage.BtnDelete_Click] Question media loaded");

                System.Diagnostics.Debug.WriteLine("[ProductReviewPage.BtnDelete_Click] Recording deleted successfully");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[ProductReviewPage.BtnDelete_Click] No recording to delete");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductReviewPage.BtnDelete_Click] Error: {ex.Message}");
            this.ViewModel.ErrorMessage = $"Failed to delete recording: {ex.Message}";
        }
    }

    /// <summary>
    /// Play Recording button click handler - Plays the recorded file.
    /// </summary>
    private void BtnPlay_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[ProductReviewPage.BtnPlay_Click] Playing recording");
            if (_recordingService?.RecordedFile != null)
            {
                var recordingPath = _recordingService.RecordedFile.Path;
                System.Diagnostics.Debug.WriteLine($"[ProductReviewPage.BtnPlay_Click] Loading recording: {recordingPath}");

                // Load the recorded file into the MediaPlayerElement
                var mediaSource = MediaSource.CreateFromUri(new Uri(recordingPath));
                this.MediaPlayer.Source = mediaSource;

                // Start playback
                if (this.MediaPlayer?.MediaPlayer != null)
                {
                    this.MediaPlayer.MediaPlayer.Play();
                    System.Diagnostics.Debug.WriteLine("[ProductReviewPage.BtnPlay_Click] Playback started");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[ProductReviewPage.BtnPlay_Click] ERROR: MediaPlayer is null");
                    this.ViewModel.ErrorMessage = "MediaPlayer not available";
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[ProductReviewPage.BtnPlay_Click] No recording to play");
                this.ViewModel.ErrorMessage = "No recording available to play";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductReviewPage.BtnPlay_Click] Error: {ex.Message}");
            this.ViewModel.ErrorMessage = $"Failed to play recording: {ex.Message}";
        }
    }
}
