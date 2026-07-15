// Copyright (c) YourProjectName. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using IntVue.Services;
using IntVue.ViewModels;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using Windows.Devices.Enumeration;
using Windows.Media.Core;

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
    /// Handles ViewModel property changes to reload media when CurrentQuestion changes.
    /// </summary>
    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProductReviewViewModel.CurrentQuestion))
        {
            this.LoadCurrentQuestionMedia();
        }
    }

    /// <summary>
    /// Loads the current question's media file into the MediaPlayerElement.
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
    /// Stop Recording button click handler - Stops the active recording session.
    /// </summary>
    private async void BtnStop_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[ProductReviewPage.BtnStop_Click] Stopping recording");

            if (_recordingService?.IsRecording == true)
            {
                await _recordingService.StopRecordingAsync();
                this.ViewModel.IsRecordingNow = false;
                System.Diagnostics.Debug.WriteLine("[ProductReviewPage.BtnStop_Click] Recording stopped successfully");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[ProductReviewPage.BtnStop_Click] No active recording to stop");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductReviewPage.BtnStop_Click] Error: {ex.Message}");
            this.ViewModel.ErrorMessage = $"Failed to stop recording: {ex.Message}";
        }
    }

    /// <summary>
    /// Start Recording button click handler - Starts video playback, then countdown, then recording.
    /// Sequence: Video plays → Countdown (3s) → Recording starts.
    /// </summary>
    private async void BtnRecord_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[ProductReviewPage.BtnRecord_Click] Starting video playback");

            // Start video playback immediately
            if (this.MediaPlayer?.MediaPlayer != null)
            {
                this.MediaPlayer.MediaPlayer.Play();
                System.Diagnostics.Debug.WriteLine("[ProductReviewPage.BtnRecord_Click] Video playback started");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[ProductReviewPage.BtnRecord_Click] Warning: MediaPlayer not available");
            }

            System.Diagnostics.Debug.WriteLine("[ProductReviewPage.BtnRecord_Click] Starting countdown");

            // Subscribe to countdown completion event
            this.ViewModel.CountdownCompleted += this.OnCountdownCompleted;

            // Start the countdown (recording will start when countdown completes)
            await this.ViewModel.StartCountdownCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductReviewPage.BtnRecord_Click] Error: {ex.Message}");
            this.ViewModel.ErrorMessage = $"Failed to start recording: {ex.Message}";
        }
    }

    /// <summary>
    /// Handles countdown completion - starts recording (video already playing).
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
}
