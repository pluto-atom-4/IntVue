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

        public InterviewViewModel(IMediaCaptureService mediaService)
        {
            this.mediaService = mediaService;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string QuestionText
        {
            get => this.questionText;
            set
            {
                this.questionText = value;
                this.OnPropertyChanged();
            }
        }

        public bool IsPreviewing
        {
            get => this.isPreviewing;
            private set
            {
                this.isPreviewing = value;
                this.OnPropertyChanged();
            }
        }

        public bool IsRecording
        {
            get => this.isRecording;
            private set
            {
                this.isRecording = value;
                this.OnPropertyChanged();
            }
        }

        public string RecordedFilePath
        {
            get => this.recordedFilePath;
            private set
            {
                this.recordedFilePath = value;
                this.OnPropertyChanged();
            }
        }

        public bool ConsentGiven
        {
            get => this.consentGiven;
            set
            {
                this.consentGiven = value;
                this.OnPropertyChanged();
            }
        }

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

        public async Task StopPreviewAsync()
        {
            await this.mediaService.StopPreviewAsync().ConfigureAwait(false);
            this.IsPreviewing = false;
        }

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
