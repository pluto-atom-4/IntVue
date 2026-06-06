// <copyright file="InterviewViewModelTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace IntVue.Tests.ViewModels
{
    using System;
    using System.Threading.Tasks;

    using IntVue.Services;
    using IntVue.ViewModels;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    /// <summary>
    /// Unit tests for InterviewViewModel.
    /// Tests core recording flow, permission handling, and state management.
    /// </summary>
    [TestClass]
    public class InterviewViewModelTests
    {
        private Mock<IMediaCaptureService> mockMediaService = null!;
        private InterviewViewModel viewModel = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockMediaService = new Mock<IMediaCaptureService>();
            this.viewModel = new InterviewViewModel(this.mockMediaService.Object);
        }

        // ==================== Initialization Tests ====================

        [TestMethod]
        public void Constructor_InitializesProperties_WithDefaults()
        {
            // Arrange & Act
            var viewModel = new InterviewViewModel(this.mockMediaService.Object);

            // Assert
            Assert.IsFalse(viewModel.ConsentGiven, "ConsentGiven should default to false");
            Assert.IsFalse(viewModel.IsPreviewing, "IsPreviewing should default to false");
            Assert.IsFalse(viewModel.IsRecording, "IsRecording should default to false");
            Assert.AreEqual(string.Empty, viewModel.RecordedFilePath, "RecordedFilePath should default to empty");
            Assert.IsNotNull(viewModel.QuestionText, "QuestionText should not be null");
        }

        [TestMethod]
        public void QuestionText_CanBeSet_AndRaisesPropertyChanged()
        {
            // Arrange
            var newQuestion = "Why do you want to join our team?";
            var propertyChangedRaised = false;

            this.viewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(InterviewViewModel.QuestionText))
                {
                    propertyChangedRaised = true;
                }
            };

            // Act
            this.viewModel.QuestionText = newQuestion;

            // Assert
            Assert.AreEqual(newQuestion, this.viewModel.QuestionText);
            Assert.IsTrue(propertyChangedRaised, "PropertyChanged should be raised for QuestionText");
        }

        // ==================== Consent Tests ====================

        [TestMethod]
        public void ConsentGiven_CanBeSet_AndRaisesPropertyChanged()
        {
            // Arrange
            var propertyChangedRaised = false;

            this.viewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(InterviewViewModel.ConsentGiven))
                {
                    propertyChangedRaised = true;
                }
            };

            // Act
            this.viewModel.ConsentGiven = true;

            // Assert
            Assert.IsTrue(this.viewModel.ConsentGiven);
            Assert.IsTrue(propertyChangedRaised, "PropertyChanged should be raised for ConsentGiven");
        }

        // ==================== Preview Permission Tests ====================

        [TestMethod]
        public async Task StartPreviewAsync_WhenConsentNotGiven_ReturnsFalse()
        {
            // Arrange
            this.viewModel.ConsentGiven = false;
            var previewHost = new object();

            // Act
            var result = await this.viewModel.StartPreviewAsync(previewHost);

            // Assert
            Assert.IsFalse(result, "Should return false when consent not given");
            this.mockMediaService.Verify(
                s => s.RequestPermissionsAsync(),
                Times.Never,
                "Should not request permissions if consent not given");
        }

        [TestMethod]
        public async Task StartPreviewAsync_WhenPermissionsDenied_ReturnsFalse()
        {
            // Arrange
            this.viewModel.ConsentGiven = true;
            this.mockMediaService
                .Setup(s => s.RequestPermissionsAsync())
                .ReturnsAsync(false);

            var previewHost = new object();

            // Act
            var result = await this.viewModel.StartPreviewAsync(previewHost);

            // Assert
            Assert.IsFalse(result, "Should return false when permissions denied");
            Assert.IsFalse(this.viewModel.IsPreviewing, "IsPreviewing should remain false");
            this.mockMediaService.Verify(
                s => s.InitializeAsync(default),
                Times.Never,
                "Should not initialize if permissions denied");
        }

        [TestMethod]
        public async Task StartPreviewAsync_WithConsentAndPermissions_StartsPreview()
        {
            // Arrange
            this.viewModel.ConsentGiven = true;
            this.mockMediaService
                .Setup(s => s.RequestPermissionsAsync())
                .ReturnsAsync(true);

            var previewHost = new object();
            var isPreviewingChanged = false;

            this.viewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(InterviewViewModel.IsPreviewing))
                {
                    isPreviewingChanged = true;
                }
            };

            // Act
            var result = await this.viewModel.StartPreviewAsync(previewHost);

            // Assert
            Assert.IsTrue(result, "Should return true when preview starts successfully");
            Assert.IsTrue(this.viewModel.IsPreviewing, "IsPreviewing should be true");
            Assert.IsTrue(isPreviewingChanged, "PropertyChanged should be raised for IsPreviewing");

            this.mockMediaService.Verify(
                s => s.RequestPermissionsAsync(),
                Times.Once,
                "Should request permissions");
            this.mockMediaService.Verify(
                s => s.InitializeAsync(default),
                Times.Once,
                "Should initialize MediaCapture");
            this.mockMediaService.Verify(
                s => s.StartPreviewAsync(previewHost),
                Times.Once,
                "Should start preview with provided host");
        }

        [TestMethod]
        public async Task StopPreviewAsync_SetsIsPreviewing_ToFalse()
        {
            // Arrange
            this.viewModel.ConsentGiven = true;
            this.mockMediaService.Setup(s => s.RequestPermissionsAsync()).ReturnsAsync(true);
            await this.viewModel.StartPreviewAsync(new object());

            var isPreviewingChanged = false;
            this.viewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(InterviewViewModel.IsPreviewing))
                {
                    isPreviewingChanged = true;
                }
            };

            // Act
            await this.viewModel.StopPreviewAsync();

            // Assert
            Assert.IsFalse(this.viewModel.IsPreviewing, "IsPreviewing should be false");
            Assert.IsTrue(isPreviewingChanged, "PropertyChanged should be raised for IsPreviewing");
            this.mockMediaService.Verify(
                s => s.StopPreviewAsync(),
                Times.Once,
                "Should call StopPreviewAsync on service");
        }

        // ==================== Recording Flow Tests ====================

        [TestMethod]
        public async Task StartRecordingAsync_WhenNotPreviewing_DoesNotStart()
        {
            // Arrange
            this.viewModel.ConsentGiven = true;
            // Don't start preview, so IsPreviewing is false

            // Act
            await this.viewModel.StartRecordingAsync("test-recording");

            // Assert
            Assert.IsFalse(this.viewModel.IsRecording, "Recording should not start when preview not active");
            Assert.AreEqual(string.Empty, this.viewModel.RecordedFilePath, "RecordedFilePath should remain empty");
            this.mockMediaService.Verify(
                s => s.StartRecordingAsync(It.IsAny<string>()),
                Times.Never,
                "Should not call StartRecordingAsync on service");
        }

        [TestMethod]
        public async Task StartRecordingAsync_WhenPreviewing_StartsRecording()
        {
            // Arrange
            this.viewModel.ConsentGiven = true;
            this.mockMediaService.Setup(s => s.RequestPermissionsAsync()).ReturnsAsync(true);
            await this.viewModel.StartPreviewAsync(new object());

            var testFilePath = @"C:\Users\test\AppData\Local\Packages\IntVue\LocalState\recording.mp4";
            this.mockMediaService
                .Setup(s => s.StartRecordingAsync("interview"))
                .ReturnsAsync(testFilePath);

            var isRecordingChanged = false;
            var recordedFilePathChanged = false;

            this.viewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(InterviewViewModel.IsRecording))
                {
                    isRecordingChanged = true;
                }

                if (e.PropertyName == nameof(InterviewViewModel.RecordedFilePath))
                {
                    recordedFilePathChanged = true;
                }
            };

            // Act
            await this.viewModel.StartRecordingAsync("interview");

            // Assert
            Assert.IsTrue(this.viewModel.IsRecording, "IsRecording should be true");
            Assert.AreEqual(testFilePath, this.viewModel.RecordedFilePath, "RecordedFilePath should be set");
            Assert.IsTrue(isRecordingChanged, "PropertyChanged should be raised for IsRecording");
            Assert.IsTrue(recordedFilePathChanged, "PropertyChanged should be raised for RecordedFilePath");

            this.mockMediaService.Verify(
                s => s.StartRecordingAsync("interview"),
                Times.Once,
                "Should call StartRecordingAsync on service");
        }

        [TestMethod]
        public async Task StopRecordingAsync_SetsIsRecording_ToFalse()
        {
            // Arrange
            this.viewModel.ConsentGiven = true;
            this.mockMediaService.Setup(s => s.RequestPermissionsAsync()).ReturnsAsync(true);
            await this.viewModel.StartPreviewAsync(new object());

            var testFilePath = @"C:\recording.mp4";
            this.mockMediaService.Setup(s => s.StartRecordingAsync(It.IsAny<string>())).ReturnsAsync(testFilePath);
            await this.viewModel.StartRecordingAsync("test");

            var isRecordingChanged = false;
            this.viewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(InterviewViewModel.IsRecording))
                {
                    isRecordingChanged = true;
                }
            };

            // Act
            await this.viewModel.StopRecordingAsync();

            // Assert
            Assert.IsFalse(this.viewModel.IsRecording, "IsRecording should be false");
            Assert.AreEqual(testFilePath, this.viewModel.RecordedFilePath, "RecordedFilePath should persist");
            Assert.IsTrue(isRecordingChanged, "PropertyChanged should be raised for IsRecording");

            this.mockMediaService.Verify(
                s => s.StopRecordingAsync(),
                Times.Once,
                "Should call StopRecordingAsync on service");
        }

        // ==================== Complete Recording Workflow Tests ====================

        [TestMethod]
        public async Task CompleteWorkflow_StartPreview_StartRecording_StopRecording_UpdatesState()
        {
            // Arrange
            this.viewModel.ConsentGiven = true;
            this.mockMediaService.Setup(s => s.RequestPermissionsAsync()).ReturnsAsync(true);
            this.mockMediaService
                .Setup(s => s.StartRecordingAsync("interview"))
                .ReturnsAsync(@"C:\recorded-interview.mp4");

            var stateChanges = new System.Collections.Generic.List<string>();

            this.viewModel.PropertyChanged += (sender, e) =>
            {
                stateChanges.Add(e.PropertyName ?? "null");
            };

            // Act - Start Preview
            var previewStarted = await this.viewModel.StartPreviewAsync(new object());

            // Assert - After Preview Start
            Assert.IsTrue(previewStarted);
            Assert.IsTrue(this.viewModel.IsPreviewing);
            Assert.IsFalse(this.viewModel.IsRecording);

            // Act - Start Recording
            await this.viewModel.StartRecordingAsync("interview");

            // Assert - After Recording Start
            Assert.IsTrue(this.viewModel.IsPreviewing);
            Assert.IsTrue(this.viewModel.IsRecording);
            Assert.AreEqual(@"C:\recorded-interview.mp4", this.viewModel.RecordedFilePath);

            // Act - Stop Recording
            await this.viewModel.StopRecordingAsync();

            // Assert - After Recording Stop
            Assert.IsTrue(this.viewModel.IsPreviewing);
            Assert.IsFalse(this.viewModel.IsRecording);
            Assert.AreEqual(@"C:\recorded-interview.mp4", this.viewModel.RecordedFilePath);

            // Verify property changes were raised
            Assert.IsTrue(stateChanges.Contains(nameof(InterviewViewModel.IsPreviewing)));
            Assert.IsTrue(stateChanges.Contains(nameof(InterviewViewModel.IsRecording)));
            Assert.IsTrue(stateChanges.Contains(nameof(InterviewViewModel.RecordedFilePath)));
        }

        // ==================== Multiple Recording Attempts Tests ====================

        [TestMethod]
        public async Task MultipleRecordingAttempts_UpdatesRecordedFilePath()
        {
            // Arrange
            this.viewModel.ConsentGiven = true;
            this.mockMediaService.Setup(s => s.RequestPermissionsAsync()).ReturnsAsync(true);
            await this.viewModel.StartPreviewAsync(new object());

            var file1Path = @"C:\recording1.mp4";
            var file2Path = @"C:\recording2.mp4";

            // Act & Assert - First Recording
            this.mockMediaService
                .Setup(s => s.StartRecordingAsync("recording1"))
                .ReturnsAsync(file1Path);

            await this.viewModel.StartRecordingAsync("recording1");
            Assert.AreEqual(file1Path, this.viewModel.RecordedFilePath);
            Assert.IsTrue(this.viewModel.IsRecording);

            await this.viewModel.StopRecordingAsync();
            Assert.AreEqual(file1Path, this.viewModel.RecordedFilePath);
            Assert.IsFalse(this.viewModel.IsRecording);

            // Act & Assert - Second Recording
            this.mockMediaService
                .Setup(s => s.StartRecordingAsync("recording2"))
                .ReturnsAsync(file2Path);

            await this.viewModel.StartRecordingAsync("recording2");
            Assert.AreEqual(file2Path, this.viewModel.RecordedFilePath, "RecordedFilePath should be updated to new recording");
            Assert.IsTrue(this.viewModel.IsRecording);

            await this.viewModel.StopRecordingAsync();
            Assert.AreEqual(file2Path, this.viewModel.RecordedFilePath);
            Assert.IsFalse(this.viewModel.IsRecording);
        }

        // ==================== Error Handling Tests ====================

        [TestMethod]
        public async Task StartPreviewAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            this.viewModel.ConsentGiven = true;
            this.mockMediaService.Setup(s => s.RequestPermissionsAsync()).ReturnsAsync(true);
            this.mockMediaService
                .Setup(s => s.InitializeAsync(default))
                .ThrowsAsync(new InvalidOperationException("Camera not found"));

            // Act & Assert
            try
            {
                await this.viewModel.StartPreviewAsync(new object());
                Assert.Fail("Should have thrown InvalidOperationException");
            }
            catch (InvalidOperationException ex)
            {
                Assert.IsTrue(ex.Message.Contains("Camera not found"));
                Assert.IsFalse(this.viewModel.IsPreviewing, "IsPreviewing should remain false");
            }
        }

        [TestMethod]
        public async Task StartRecordingAsync_WhenServiceThrows_DoesNotSetIsRecording()
        {
            // Arrange
            this.viewModel.ConsentGiven = true;
            this.mockMediaService.Setup(s => s.RequestPermissionsAsync()).ReturnsAsync(true);
            await this.viewModel.StartPreviewAsync(new object());

            this.mockMediaService
                .Setup(s => s.StartRecordingAsync(It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("Recording failed"));

            // Act & Assert
            try
            {
                await this.viewModel.StartRecordingAsync("test");
                Assert.Fail("Should have thrown InvalidOperationException");
            }
            catch (InvalidOperationException)
            {
                Assert.IsFalse(this.viewModel.IsRecording, "IsRecording should remain false on exception");
                Assert.AreEqual(string.Empty, this.viewModel.RecordedFilePath, "RecordedFilePath should remain empty on exception");
            }
        }

        // ==================== State Isolation Tests ====================

        [TestMethod]
        public void MultipleInstances_DoNotShareState()
        {
            // Arrange
            var viewModel1 = new InterviewViewModel(this.mockMediaService.Object);
            var viewModel2 = new InterviewViewModel(this.mockMediaService.Object);

            // Act
            viewModel1.ConsentGiven = true;
            viewModel1.QuestionText = "Question 1";

            // Assert
            Assert.IsTrue(viewModel1.ConsentGiven);
            Assert.IsFalse(viewModel2.ConsentGiven, "viewModel2 should have independent ConsentGiven state");
            Assert.AreNotEqual(viewModel1.QuestionText, viewModel2.QuestionText, "ViewModels should have independent QuestionText");
        }

        // ==================== PropertyChanged Event Tests ====================

        [TestMethod]
        public void PropertyChanged_RaisedForAllProperties()
        {
            // Arrange
            var changedProperties = new System.Collections.Generic.HashSet<string>();

            this.viewModel.PropertyChanged += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.PropertyName))
                {
                    changedProperties.Add(e.PropertyName);
                }
            };

            // Act
            this.viewModel.QuestionText = "New Question";
            this.viewModel.ConsentGiven = true;

            // Assert
            Assert.IsTrue(changedProperties.Contains(nameof(InterviewViewModel.QuestionText)));
            Assert.IsTrue(changedProperties.Contains(nameof(InterviewViewModel.ConsentGiven)));
        }
    }
}
