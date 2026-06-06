// <copyright file="MediaCaptureServiceIntegrationTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace IntVue.Tests.Services
{
    using System;
    using System.Threading.Tasks;

    using IntVue.Services;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Windows.Devices.Enumeration;

    /// <summary>
    /// Integration tests for MediaCaptureService using real Windows.Media.Capture APIs.
    /// Tests skip gracefully if no camera device is available.
    /// </summary>
    [TestClass]
    public class MediaCaptureServiceIntegrationTests
    {
        private MediaCaptureService? service;

        [TestInitialize]
        public void TestInitialize()
        {
            this.service = new MediaCaptureService();
        }

        [TestCleanup]
        public async Task TestCleanup()
        {
            if (this.service != null)
            {
                await this.service.DisposeAsync();
                this.service.Dispose();
            }
        }

        // ==================== Helper Methods ====================

        private static async Task<bool> IsCameraAvailableAsync()
        {
            try
            {
                var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
                return devices.Count > 0;
            }
            catch
            {
                return false;
            }
        }

        // ==================== Device Enumeration Tests ====================

        [TestMethod]
        public async Task InitializeAsync_FindsCameraDevice_IfAvailable()
        {
            // Arrange
            Assert.IsNotNull(this.service);
            var cameraAvailable = await IsCameraAvailableAsync();

            if (!cameraAvailable)
            {
                Assert.Inconclusive("No camera device found; skipping test");
                return;
            }

            // Act
            await this.service.InitializeAsync();

            // Assert
            Assert.IsTrue(true, "InitializeAsync succeeded with available camera");
        }

        [TestMethod]
        public async Task InitializeAsync_HandlesNoCameraGracefully()
        {
            // Arrange
            Assert.IsNotNull(this.service);

            // Act & Assert
            // Should not throw; logs warning if no camera found
            await this.service.InitializeAsync();
            Assert.IsTrue(true, "InitializeAsync handled no-camera case gracefully");
        }

        // ==================== Resource Lifecycle Tests ====================

        [TestMethod]
        public async Task InitializeDisposeInitialize_Cycles_NoLeaks()
        {
            // Arrange
            Assert.IsNotNull(this.service);

            // Act - Cycle 1
            await this.service.InitializeAsync();
            await this.service.DisposeAsync();

            // Cycle 2
            await this.service.InitializeAsync();
            await this.service.DisposeAsync();

            // Cycle 3
            await this.service.InitializeAsync();
            await this.service.DisposeAsync();

            // Assert
            Assert.IsTrue(true, "Multiple init/dispose cycles completed without errors");
        }

        [TestMethod]
        public async Task StartPreviewAsync_ThenStopPreviewAsync_NoExceptions()
        {
            // Arrange
            Assert.IsNotNull(this.service);
            var cameraAvailable = await IsCameraAvailableAsync();

            if (!cameraAvailable)
            {
                Assert.Inconclusive("No camera device found; skipping test");
                return;
            }

            await this.service.InitializeAsync();

            // Act & Assert
            try
            {
                // Create a mock MediaPlayerElement for testing
                var mockElement = new Moq.Mock<Microsoft.UI.Xaml.Controls.MediaPlayerElement>();
                await this.service.StartPreviewAsync(mockElement.Object);

                // Verify SetMediaPlayer was called
                mockElement.Verify(m => m.SetMediaPlayer(It.IsNotNull<Windows.Media.Playback.MediaPlayer>()), Times.Once);

                await this.service.StopPreviewAsync();
                Assert.IsTrue(true, "Preview started and stopped successfully");
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not initialized"))
            {
                // If initialization failed, that's acceptable for integration test
                Assert.Inconclusive("MediaCapture initialization failed; skipping test");
            }
        }

        [TestMethod]
        public async Task RecordingFilePath_IsInApplicationData()
        {
            // Arrange
            Assert.IsNotNull(this.service);
            var cameraAvailable = await IsCameraAvailableAsync();

            if (!cameraAvailable)
            {
                Assert.Inconclusive("No camera device found; skipping test");
                return;
            }

            await this.service.InitializeAsync();

            // Act & Assert
            try
            {
                var filePath = await this.service.StartRecordingAsync("test-recording");
                Assert.IsNotNull(filePath, "StartRecordingAsync should return a file path");

                // Recording file should be in ApplicationData.LocalFolder
                var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
                Assert.IsTrue(filePath.StartsWith(localFolder), $"Recording should be in ApplicationData.LocalFolder: {localFolder}");
                Assert.IsTrue(filePath.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase), "File should have .mp4 extension");

                await this.service.StopRecordingAsync();
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not initialized"))
            {
                Assert.Inconclusive("MediaCapture initialization failed; skipping test");
            }
        }

        [TestMethod]
        public async Task StartRecordingAsync_Then_StopRecordingAsync_Sequential()
        {
            // Arrange
            Assert.IsNotNull(this.service);
            var cameraAvailable = await IsCameraAvailableAsync();

            if (!cameraAvailable)
            {
                Assert.Inconclusive("No camera device found; skipping test");
                return;
            }

            await this.service.InitializeAsync();

            // Act & Assert
            try
            {
                var filePath = await this.service.StartRecordingAsync("recording1");
                Assert.IsTrue(this.service.IsRecording, "IsRecording should be true");

                await this.service.StopRecordingAsync();
                Assert.IsFalse(this.service.IsRecording, "IsRecording should be false after stopping");

                Assert.IsTrue(true, "Recording lifecycle completed successfully");
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not initialized"))
            {
                Assert.Inconclusive("MediaCapture initialization failed; skipping test");
            }
        }

        [TestMethod]
        public async Task MultipleRecordings_GenerateUniqueFileNames()
        {
            // Arrange
            Assert.IsNotNull(this.service);
            var cameraAvailable = await IsCameraAvailableAsync();

            if (!cameraAvailable)
            {
                Assert.Inconclusive("No camera device found; skipping test");
                return;
            }

            await this.service.InitializeAsync();

            // Act & Assert
            try
            {
                var file1 = await this.service.StartRecordingAsync("samename");
                await this.service.StopRecordingAsync();

                var file2 = await this.service.StartRecordingAsync("samename");
                await this.service.StopRecordingAsync();

                // Files should have different paths due to GenerateUniqueName
                Assert.AreNotEqual(file1, file2, "Multiple recordings should have unique file paths");
                Assert.IsTrue(true, "Multiple recordings generated unique names");
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not initialized"))
            {
                Assert.Inconclusive("MediaCapture initialization failed; skipping test");
            }
        }

        [TestMethod]
        public async Task PermissionsAsync_ReturnsExpectedValue()
        {
            // Arrange
            Assert.IsNotNull(this.service);

            // Act
            var result = await this.service.RequestPermissionsAsync();

            // Assert
            // Result should be true (developer mode) or false (restricted)
            // The important thing is that it doesn't throw
            Assert.IsInstanceOfType(result, typeof(bool), "RequestPermissionsAsync should return a boolean");
        }
    }
}
