// <copyright file="InterviewViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace IntVue.ViewModels
{
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using IntVue.Services;

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
#if DEBUG
                Trace.WriteLine($"[IntVue.Debug] InterviewViewModel.ConsentGiven: Changed to {value}");
#endif
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Starts the camera preview asynchronously after verifying consent and permissions.
        /// </summary>
        /// <param name="previewHost">The UI element that will host the camera preview.</param>
        /// <returns>A <see cref="Task{Boolean}"/> representing the asynchronous operation and returning true if successful.</returns>
        public async Task<bool> StartPreviewAsync(object previewHost)
        {
#if DEBUG
            Trace.WriteLine("[IntVue.Debug] InterviewViewModel.StartPreviewAsync: Starting preview command...");
#endif

            if (!this.ConsentGiven)
            {
#if DEBUG
                Trace.WriteLine("[IntVue.Debug] InterviewViewModel.StartPreviewAsync: User consent not given, aborting.");
#endif
                return false;
            }

#if DEBUG
            Trace.WriteLine("[IntVue.Debug] InterviewViewModel.StartPreviewAsync: User consent confirmed. Requesting permissions...");
#endif

            var granted = await this.mediaService.RequestPermissionsAsync().ConfigureAwait(false);

#if DEBUG
            Debug.WriteLine($"[IntVue.Debug] InterviewViewModel.StartPreviewAsync: Permissions granted: {granted}");
#endif

            if (!granted)
            {
#if DEBUG
                Trace.WriteLine("[IntVue.Debug] InterviewViewModel.StartPreviewAsync: Permissions not granted, aborting.");
#endif
                return false;
            }

#if DEBUG
            Trace.WriteLine("[IntVue.Debug] InterviewViewModel.StartPreviewAsync: Initializing media capture...");
#endif

            await this.mediaService.InitializeAsync().ConfigureAwait(false);

#if DEBUG
            Trace.WriteLine("[IntVue.Debug] InterviewViewModel.StartPreviewAsync: Starting preview on media service...");
#endif

            await this.mediaService.StartPreviewAsync(previewHost).ConfigureAwait(false);

#if DEBUG
            Trace.WriteLine("[IntVue.Debug] InterviewViewModel.StartPreviewAsync: Preview started successfully.");
#endif

            this.IsPreviewing = true;
            return true;
        }

        /// <summary>
        /// Stops the camera preview asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task StopPreviewAsync()
        {
#if DEBUG
            Trace.WriteLine("[IntVue.Debug] InterviewViewModel.StopPreviewAsync: Stopping preview...");
#endif

            await this.mediaService.StopPreviewAsync().ConfigureAwait(false);
            this.IsPreviewing = false;

#if DEBUG
            Trace.WriteLine("[IntVue.Debug] InterviewViewModel.StopPreviewAsync: Preview stopped.");
#endif
        }

        /// <summary>
        /// Starts recording the interview asynchronously.
        /// </summary>
        /// <param name="baseFileName">The base filename to use for the recording.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task StartRecordingAsync(string baseFileName)
        {
#if DEBUG
            Debug.WriteLine($"[IntVue.Debug] InterviewViewModel.StartRecordingAsync: Starting recording with base name '{baseFileName}'...");
#endif

            if (!this.IsPreviewing)
            {
#if DEBUG
                Trace.WriteLine("[IntVue.Debug] InterviewViewModel.StartRecordingAsync: Preview not active, cannot start recording.");
#endif
                return;
            }

#if DEBUG
            Trace.WriteLine("[IntVue.Debug] InterviewViewModel.StartRecordingAsync: Preview is active. Starting recording on media service...");
#endif

            var path = await this.mediaService.StartRecordingAsync(baseFileName).ConfigureAwait(false);

#if DEBUG
            Debug.WriteLine($"[IntVue.Debug] InterviewViewModel.StartRecordingAsync: Recording started. File path: {path}");
#endif

            this.RecordedFilePath = path;
            this.IsRecording = true;
        }

        /// <summary>
        /// Stops recording the interview asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task StopRecordingAsync()
        {
#if DEBUG
            Trace.WriteLine("[IntVue.Debug] InterviewViewModel.StopRecordingAsync: Stopping recording...");
#endif

            await this.mediaService.StopRecordingAsync().ConfigureAwait(false);
            this.IsRecording = false;

#if DEBUG
            Trace.WriteLine("[IntVue.Debug] InterviewViewModel.StopRecordingAsync: Recording stopped.");
#endif
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
}
