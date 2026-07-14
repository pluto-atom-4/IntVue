// Copyright (c) YourProjectName. All rights reserved.

using System;
using System.ComponentModel;

using IntVue.ViewModels;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using Windows.Media.Core;

namespace IntVue.Views;

/// <summary>
/// ProductReviewPage - XAML view for playing pre-recorded interview questions with countdown timer.
/// Supports WebM video playback, playlist navigation, and countdown-based recording workflow.
/// </summary>
public sealed partial class ProductReviewPage : Page
{
    private ProductReviewViewModel? _viewModel;

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
    /// Page loaded - Load questions from the CLI-specified directory and set up media playback.
    /// </summary>
    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            // Set DataContext to ensure XAML bindings use same ViewModel instance
            this.DataContext = this.ViewModel;

            // Subscribe to ViewModel property changes to detect when CurrentQuestion changes
            this.ViewModel.PropertyChanged += this.OnViewModelPropertyChanged;

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
            this.ViewModel.ErrorMessage = $"Error loading questions: {ex.Message}";
        }
    }

    /// <summary>
    /// Page unloaded - Clean up resources and dispose ViewModel.
    /// </summary>
    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        try
        {
            // Unsubscribe from ViewModel property changes
            this.ViewModel.PropertyChanged -= this.OnViewModelPropertyChanged;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductReviewPage.OnUnloaded] Error during cleanup: {ex.Message}");
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
    /// Start Recording button click handler - Initiates countdown before recording.
    /// Countdown is 3 seconds; when it completes, playback and recording start automatically.
    /// </summary>
    private async void BtnRecord_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[ProductReviewPage.BtnRecord_Click] Starting countdown");

            // Subscribe to countdown completion event
            this.ViewModel.CountdownCompleted += this.OnCountdownCompleted;

            // Start the countdown
            await this.ViewModel.StartCountdownCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductReviewPage.BtnRecord_Click] Error: {ex.Message}");
            this.ViewModel.ErrorMessage = $"Failed to start recording: {ex.Message}";
        }
    }

    /// <summary>
    /// Handles countdown completion - starts video playback and recording.
    /// </summary>
    private void OnCountdownCompleted(object? sender, EventArgs e)
    {
        try
        {
            // Unsubscribe to prevent multiple registrations
            this.ViewModel.CountdownCompleted -= this.OnCountdownCompleted;

            System.Diagnostics.Debug.WriteLine("[ProductReviewPage.OnCountdownCompleted] Countdown finished, starting playback");

            // Start playing the video
            if (this.MediaPlayer?.MediaPlayer != null)
            {
                this.MediaPlayer.MediaPlayer.Play();
                System.Diagnostics.Debug.WriteLine("[ProductReviewPage.OnCountdownCompleted] Video playback started");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[ProductReviewPage.OnCountdownCompleted] Warning: MediaPlayer not available");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductReviewPage.OnCountdownCompleted] Error: {ex.Message}");
            this.ViewModel.ErrorMessage = $"Failed to start playback: {ex.Message}";
        }
    }
}
