// <copyright file="InterviewViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace IntVue.ViewModels
{
    using System.ComponentModel;
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
        /// <param name="mediaService">The media capture service dependency.</param>
        public InterviewViewModel(IMediaCaptureService mediaService)
        {
            this.mediaService = mediaService;
        }

        /// <summary>
        /// Gets or sets the event that occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the interview question text.
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
        /// Gets a value indicating whether preview is currently active.
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
        /// Gets a value indicating whether recording is currently in progress.
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
        /// Gets the file path of the most recent recording.
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
        /// Gets or sets a value indicating whether the user has given consent to record.
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
        /// Request permissions and start the camera preview.
        /// </summary>
        /// <param name="previewHost">The preview control or container object that receives the preview stream.</param>
        /// <returns>A <see cref="Task{T}"/> representing true if preview started successfully, false otherwise.</returns>
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
        /// Stop the camera preview.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task StopPreviewAsync()
        {
            await this.mediaService.StopPreviewAsync().ConfigureAwait(false);
            this.IsPreviewing = false;
        }

        /// <summary>
        /// Start recording audio and video.
        /// </summary>
        /// <param name="baseFileName">The base filename for the recording file (without extension).</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task StartRecordingAsync(string baseFileName)
        {
            if (!this.IsPreviewing)
            {
                return;
            }

            var path = await this.mediaService.StartRecordingAsync(baseFileName).ConfigureAwait(false);
            this.RecordedFilePath = path;
            this.IsRecording = true;
        }

        /// <summary>
        /// Stop recording audio and video.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task StopRecordingAsync()
        {
            await this.mediaService.StopRecordingAsync().ConfigureAwait(false);
            this.IsRecording = false;
        }

        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
