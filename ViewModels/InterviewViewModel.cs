// Copyright (c) YourProjectName. All rights reserved.

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using IntVue.Services;

using Microsoft.UI.Dispatching;

using Windows.Devices.Enumeration;

namespace IntVue.ViewModels;

/// <summary>
/// ViewModel for the interview flow. Keeps logic thin and testable.
/// </summary>
public class InterviewViewModel : INotifyPropertyChanged
{
    private readonly IMediaCaptureService mediaService;
    private readonly DispatcherQueue? dispatcherQueue;
    private string questionText = "Describe a challenging project you worked on and how you resolved it.";
    private bool isPreviewing;
    private bool isRecording;
    private string recordedFilePath = string.Empty;
    private string recordingError = string.Empty;
    private Microsoft.UI.Xaml.Visibility stopPreviewButtonVisibility = Microsoft.UI.Xaml.Visibility.Collapsed;
    private ObservableCollection<DeviceInformation> cameras = new ();
    private DeviceInformation? selectedCamera;
    private bool isDeviceInitialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="InterviewViewModel"/> class.
    /// </summary>
    /// <param name="mediaService">The media capture service used for camera/microphone operations.</param>
    public InterviewViewModel(IMediaCaptureService mediaService)
    {
        this.mediaService = mediaService;
        this.dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        this.mediaService.RecordLimitationExceeded += this.OnRecordLimitationExceeded;
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
            this.OnPropertyChanged(nameof(this.RecordButtonLabel));
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
    /// Gets any error message from the last recording operation.
    /// </summary>
    public string RecordingError
    {
        get => this.recordingError;
        private set
        {
            this.recordingError = value;
            this.OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the label for the record button based on recording state.
    /// </summary>
    public string RecordButtonLabel => this.IsRecording ? "Stop Recording" : "Start Recording";

    /// <summary>
    /// Gets the collection of available camera devices.
    /// </summary>
    public ObservableCollection<DeviceInformation> Cameras
    {
        get => this.cameras;
    }

    /// <summary>
    /// Gets or sets the currently selected camera device.
    /// </summary>
    public DeviceInformation? SelectedCamera
    {
        get => this.selectedCamera;
        set
        {
            this.selectedCamera = value;
            this.OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets a value indicating whether a camera device has been initialized.
    /// </summary>
    public bool IsDeviceInitialized
    {
        get => this.isDeviceInitialized;
        private set
        {
            this.isDeviceInitialized = value;
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
    /// Loads the list of available camera devices asynchronously.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task LoadCamerasAsync()
    {
        try
        {
            var list = await this.mediaService.GetCamerasAsync().ConfigureAwait(false);
            this.Cameras.Clear();
            foreach (var device in list)
            {
                this.Cameras.Add(device);
            }

            if (this.Cameras.Count > 0)
            {
                this.SelectedCamera = this.Cameras[0];
            }
        }
        catch (Exception ex)
        {
#if DEBUG
            Debug.WriteLine($"[IntVue.Debug] LoadCamerasAsync: Error - {ex.Message}");
#endif
        }
    }

    /// <summary>
    /// Initializes the media capture device with the selected camera asynchronously.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task InitializeDeviceAsync()
    {
        try
        {
            await this.mediaService.InitializeAsync(this.SelectedCamera?.Id).ConfigureAwait(false);
            this.IsDeviceInitialized = true;
        }
        catch (Exception ex)
        {
#if DEBUG
            Debug.WriteLine($"[IntVue.Debug] InitializeDeviceAsync: Error - {ex.Message}");
#endif
            this.IsDeviceInitialized = false;
            throw;
        }
    }

    /// <summary>
    /// Starts the camera preview asynchronously after verifying device initialization.
    /// </summary>
    /// <param name="previewHost">The UI element that will host the camera preview.</param>
    /// <returns>A <see cref="Task{Boolean}"/> representing the asynchronous operation and returning true if successful.</returns>
    public async Task<bool> StartPreviewAsync(object previewHost)
    {
        if (!this.IsDeviceInitialized)
        {
            return false;
        }

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
        try
        {
            Debug.WriteLine("[IntVue.Debug] StartRecordingAsync: Preparing file...");
            var path = await this.mediaService.StartRecordingAsync(baseFileName).ConfigureAwait(false);
            this.RecordedFilePath = path;
            this.IsRecording = true;
            Debug.WriteLine("[IntVue.Debug] StartRecordingAsync: Recording started.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[IntVue.Debug] StartRecordingAsync: Error - {ex.Message}");
            this.IsRecording = false;
            throw;
        }
    }

    /// <summary>
    /// Stops recording the interview asynchronously.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task StopRecordingAsync()
    {
        try
        {
            Debug.WriteLine("[IntVue.Debug] StopRecordingAsync: Stopping...");
            await this.mediaService.StopRecordingAsync().ConfigureAwait(false);
            Debug.WriteLine("[IntVue.Debug] StopRecordingAsync: Finished.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[IntVue.Debug] StopRecordingAsync: Error during finish - {ex.Message}");
            throw;
        }
        finally
        {
            this.IsRecording = false;
        }
    }

    /// <summary>
    /// Toggles recording on or off asynchronously.
    /// If device is not initialized, sets error and returns without recording.
    /// </summary>
    /// <param name="baseFileName">The base filename to use for the recording if starting.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ToggleRecordingAsync(string baseFileName)
    {
        if (!this.IsDeviceInitialized)
        {
            this.RecordingError = "Device is not initialized. Please initialize the camera first.";
            return;
        }

        this.RecordingError = string.Empty;

        if (this.IsRecording)
            await this.StopRecordingAsync().ConfigureAwait(false);
        else
            await this.StartRecordingAsync(baseFileName).ConfigureAwait(false);
    }

    /// <summary>
    /// Called when the OS recording limit (3 hours) is exceeded during recording.
    /// Marshals to the UI thread and stops the recording.
    /// </summary>
    private void OnRecordLimitationExceeded(object? sender, EventArgs e)
    {
        if (this.dispatcherQueue != null && !this.dispatcherQueue.HasThreadAccess)
            _ = this.dispatcherQueue.TryEnqueue(async () => await this.HandleRecordLimitExceededAsync());
        else
            _ = this.HandleRecordLimitExceededAsync();
    }

    /// <summary>
    /// Handles the OS recording limit exceeded event by stopping recording and setting error message.
    /// </summary>
    private async Task HandleRecordLimitExceededAsync()
    {
        await this.StopRecordingAsync().ConfigureAwait(false);
        this.RecordingError = "Recording stopped: OS 3-hour recording limit reached.";
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
