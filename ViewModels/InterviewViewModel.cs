// Copyright (c) YourProjectName. All rights reserved.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using IntVue.Services;

namespace IntVue.ViewModels;

/// <summary>
/// ViewModel for the interview flow. Keeps logic thin and testable.
/// </summary>
public class InterviewViewModel : INotifyPropertyChanged
{
    private readonly IMediaCaptureService mediaService;
    private string questionText = "Describe a challenging project you worked on and how you resolved it.";
    private bool isPreviewing;
    private bool isRecording;
    private string recordedFilePath = string.Empty;
    private bool consentGiven;
    private Microsoft.UI.Xaml.Visibility stopPreviewButtonVisibility = Microsoft.UI.Xaml.Visibility.Collapsed;

    /// <summary>
    /// Initializes a new instance of the <see cref="InterviewViewModel"/> class.
    /// </summary>
    /// <param name="mediaService">The media capture service used for camera/microphone operations.</param>
    public InterviewViewModel(IMediaCaptureService mediaService)
    {
        this.mediaService = mediaService;
    }

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets or sets the interview question text displayed to the user.
    /// </summary>
    public string QuestionText
    {
        get => this.questionText;
        set
        {
            this.questionText = value;
            this.OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets a value indicating whether the camera preview is currently active.
    /// </summary>
    public bool IsPreviewing
    {
        get => this.isPreviewing;
        private set
        {
            this.isPreviewing = value;

            // Update Stop Preview button visibility based on preview state
            this.StopPreviewButtonVisibility = value
                ? Microsoft.UI.Xaml.Visibility.Visible
                : Microsoft.UI.Xaml.Visibility.Collapsed;
            this.OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets a value indicating whether a recording is currently in progress.
    /// </summary>
    public bool IsRecording
    {
        get => this.isRecording;
        private set
        {
            this.isRecording = value;
            this.OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the file path of the most recently recorded interview.
    /// </summary>
    public string RecordedFilePath
    {
        get => this.recordedFilePath;
        private set
        {
            this.recordedFilePath = value;
            this.OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the user has given consent for recording.
    /// </summary>
    public bool ConsentGiven
    {
        get => this.consentGiven;
        set
        {
            this.consentGiven = value;
            this.OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the visibility state of the Stop Preview button (bound to UI).
    /// </summary>
    public Microsoft.UI.Xaml.Visibility StopPreviewButtonVisibility
    {
        get => this.stopPreviewButtonVisibility;
        private set
        {
            if (this.stopPreviewButtonVisibility != value)
            {
                this.stopPreviewButtonVisibility = value;
                this.OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Starts the camera preview asynchronously after verifying consent and permissions.
    /// </summary>
    /// <param name="previewHost">The UI element that will host the camera preview.</param>
    /// <returns>A <see cref="Task{Boolean}"/> representing the asynchronous operation and returning true if successful.</returns>
    public async Task<bool> StartPreviewAsync(object previewHost)
    {
        if (!this.ConsentGiven)
        {
            return false;
        }

        var granted = await this.mediaService.RequestPermissionsAsync().ConfigureAwait(false);
        if (!granted)
        {
            return false;
        }

        await this.mediaService.InitializeAsync().ConfigureAwait(false);
        await this.mediaService.StartPreviewAsync(previewHost).ConfigureAwait(false);

        this.IsPreviewing = true;
        return true;
    }

    /// <summary>
    /// Stops the camera preview asynchronously.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task StopPreviewAsync()
    {
        try
        {
            await this.mediaService.StopPreviewAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
#if DEBUG
            Debug.WriteLine($"[IntVue.Debug] StopPreview: Error - {ex.Message}");
#endif
        }

        this.IsPreviewing = false;
    }

    /// <summary>
    /// Starts recording the interview asynchronously.
    /// </summary>
    /// <param name="baseFileName">The base filename to use for the recording.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task StartRecordingAsync(string baseFileName)
    {
        var path = await this.mediaService.StartRecordingAsync(baseFileName).ConfigureAwait(false);
        this.RecordedFilePath = path;
        this.IsRecording = true;
    }

    /// <summary>
    /// Stops recording the interview asynchronously.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task StopRecordingAsync()
    {
        await this.mediaService.StopRecordingAsync().ConfigureAwait(false);
        this.IsRecording = false;
    }

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event for the specified property name.
    /// </summary>
    /// <param name="name">The name of the property that changed.</param>
    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
