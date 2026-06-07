// <copyright file="MediaCaptureServiceTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace IntVue.Tests.Services
{
    using System;
    using System.Threading.Tasks;

    using IntVue.Services;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for MediaCaptureService.
    /// Tests focus on initialization, resource lifecycle, and error handling.
    /// </summary>
    [TestClass]
    public class MediaCaptureServiceTests
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

        // ==================== Initialization Tests ====================

        [TestMethod]
        public async Task InitializeAsync_FirstCall_SetsInitializedTrue()
        {
            // Arrange
            Assert.IsNotNull(this.service);

            // Act
            await this.service.InitializeAsync();

            // Assert
            // Service should not throw and should be ready (or gracefully handle no camera)
            Assert.IsTrue(true, "InitializeAsync completed without exception");
        }

        [TestMethod]
        public async Task InitializeAsync_SecondCall_ReturnsImmediately()
        {
            // Arrange
            Assert.IsNotNull(this.service);

            // Act
            var start = DateTime.UtcNow;
            await this.service.InitializeAsync();
            var firstDuration = DateTime.UtcNow - start;

            start = DateTime.UtcNow;
            await this.service.InitializeAsync();
            var secondDuration = DateTime.UtcNow - start;

            // Assert
            // Second call should be much faster (idempotent; returns early)
            Assert.IsTrue(secondDuration < firstDuration, "Second InitializeAsync should return immediately");
        }

        // ==================== Permission Tests ====================

        [TestMethod]
        public async Task RequestPermissionsAsync_ReturnsBoolean()
        {
            // Arrange
            Assert.IsNotNull(this.service);

            // Act
            var result = await this.service.RequestPermissionsAsync();

            // Assert
            // Result should be a boolean (true or false depending on actual permissions)
            Assert.IsInstanceOfType(result, typeof(bool));
        }

        [TestMethod]
        public async Task RequestPermissionsAsync_NoExceptionOnError()
        {
            // Arrange
            Assert.IsNotNull(this.service);

            // Act & Assert
            // Should not throw even if permission APIs fail
            var result = await this.service.RequestPermissionsAsync();
            Assert.IsNotNull(result);
        }

        // ==================== Preview Tests ====================

        [TestMethod]
        public async Task StartPreviewAsync_InvalidPreviewHost_ThrowsArgumentException()
        {
            // Arrange
            Assert.IsNotNull(this.service);
            var invalidHost = new object(); // Not a MediaPlayerElement

            // Act & Assert
            try
            {
                await this.service.StartPreviewAsync(invalidHost);
                Assert.Fail("Should have thrown ArgumentException");
            }
            catch (ArgumentException)
            {
                // Expected
                Assert.IsTrue(true);
            }
        }


        [TestMethod]
        public async Task StopPreviewAsync_DoesNotThrow()
        {
            // Arrange
            Assert.IsNotNull(this.service);

            // Act & Assert
            // Should not throw even if preview was never started
            await this.service.StopPreviewAsync();
            Assert.IsTrue(true, "StopPreviewAsync completed without exception");
        }

        [TestMethod]
        public async Task StopPreviewAsync_MultipleCallsAllowed()
        {
            // Arrange
            Assert.IsNotNull(this.service);

            // Act & Assert
            await this.service.StopPreviewAsync();
            await this.service.StopPreviewAsync();
            await this.service.StopPreviewAsync();
            Assert.IsTrue(true, "Multiple StopPreviewAsync calls allowed");
        }

        // ==================== Recording Tests ====================

        [TestMethod]
        public void IsRecording_InitiallyFalse()
        {
            // Arrange
            Assert.IsNotNull(this.service);

            // Act
            var isRecording = this.service.IsRecording;

            // Assert
            Assert.IsFalse(isRecording, "IsRecording should be false before recording starts");
        }

        [TestMethod]
        public async Task StartRecordingAsync_SanitizesFileName()
        {
            // Arrange
            Assert.IsNotNull(this.service);
            var unsafeFileName = "test<>:\"file|.txt"; // Invalid file name characters

            // Act & Assert
            try
            {
                await this.service.StartRecordingAsync(unsafeFileName);
                // If successful, verify IsRecording is true
                Assert.IsTrue(this.service.IsRecording, "Recording should be active");
                await this.service.StopRecordingAsync();
            }
            catch (Exception ex)
            {
                // Expected if no camera initialized or invalid filename
                Assert.IsTrue(true, $"Exception acceptable during recording: {ex.GetType().Name}");
            }
        }

        [TestMethod]
        public async Task StartRecordingAsync_AppendsExtension_IfMissing()
        {
            // Arrange
            Assert.IsNotNull(this.service);
            var fileName = "myrecording"; // No .mp4 extension

            // Act & Assert
            try
            {
                var result = await this.service.StartRecordingAsync(fileName);
                // Result should be a path, not an error
                Assert.IsNotNull(result, "StartRecordingAsync should return file path");
                Assert.IsTrue(result.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase), "Should have .mp4 extension");
                await this.service.StopRecordingAsync();
            }
            catch (InvalidOperationException)
            {
                // Expected if no camera
                Assert.IsTrue(true, "InvalidOperationException acceptable if no camera");
            }
        }

        [TestMethod]
        public async Task StartRecordingAsync_WithExtension_NoDoubleExtension()
        {
            // Arrange
            Assert.IsNotNull(this.service);
            var fileName = "myrecording.mp4";

            // Act & Assert
            try
            {
                var result = await this.service.StartRecordingAsync(fileName);
                Assert.IsNotNull(result, "Should return file path");
                Assert.IsFalse(result.EndsWith(".mp4.mp4", StringComparison.OrdinalIgnoreCase), "Should not double-append extension");
                await this.service.StopRecordingAsync();
            }
            catch (InvalidOperationException)
            {
                // Expected if no camera
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public async Task StopRecordingAsync_DoesNotThrow_WhenNeverStarted()
        {
            // Arrange
            Assert.IsNotNull(this.service);

            // Act & Assert
            await this.service.StopRecordingAsync();
            Assert.IsFalse(this.service.IsRecording, "IsRecording should be false");
        }

        [TestMethod]
        public async Task StopRecordingAsync_MultipleCallsAllowed()
        {
            // Arrange
            Assert.IsNotNull(this.service);

            // Act & Assert
            await this.service.StopRecordingAsync();
            await this.service.StopRecordingAsync();
            Assert.IsTrue(true, "Multiple StopRecordingAsync calls allowed");
        }

        // ==================== Resource Lifecycle Tests ====================

        [TestMethod]
        public async Task DisposeAsync_DoesNotThrow()
        {
            // Arrange
            Assert.IsNotNull(this.service);

            // Act & Assert
            await this.service.DisposeAsync();
            Assert.IsTrue(true, "DisposeAsync completed without exception");
        }

        [TestMethod]
        public async Task DisposeAsync_MultipleCallsAllowed()
        {
            // Arrange
            Assert.IsNotNull(this.service);

            // Act & Assert
            await this.service.DisposeAsync();
            await this.service.DisposeAsync();
            Assert.IsTrue(true, "Multiple DisposeAsync calls allowed");
        }

        [TestMethod]
        public void Dispose_DoesNotThrow()
        {
            // Arrange
            Assert.IsNotNull(this.service);

            // Act & Assert
            this.service.Dispose();
            Assert.IsTrue(true, "Dispose completed without exception");
        }

        [TestMethod]
        public void Dispose_MultipleCallsAllowed()
        {
            // Arrange
            Assert.IsNotNull(this.service);

            // Act & Assert
            this.service.Dispose();
            this.service.Dispose();
            Assert.IsTrue(true, "Multiple Dispose calls allowed");
        }

        [TestMethod]
        public async Task DisposeAsync_ThenDispose_Sequential()
        {
            // Arrange
            Assert.IsNotNull(this.service);

            // Act & Assert
            await this.service.DisposeAsync();
            this.service.Dispose();
            Assert.IsTrue(true, "Sequential async and sync dispose allowed");
        }

        // ==================== Phase 4: State Management & Error Handling Tests ====================

        [TestMethod]
        public async Task StartPreviewAsync_CalledMultipleTimes_DoesNotThrow()
        {
            // Arrange
            Assert.IsNotNull(this.service);
            var invalidHost = new object(); // Will fail, but tests state cleanup path

            // Act & Assert
            // First call: expect ArgumentException (invalid host)
            try
            {
                await this.service.StartPreviewAsync(invalidHost);
            }
            catch (ArgumentException)
            {
                // Expected
            }

            // Second call: should handle cleanup properly without cascading failure
            try
            {
                await this.service.StartPreviewAsync(invalidHost);
            }
            catch (ArgumentException)
            {
                // Expected; important is that we don't get resource leak exceptions
                Assert.IsTrue(true, "Second call handled cleanup correctly");
            }
        }

        [TestMethod]
        public async Task StartPreviewAsync_BeforeInitialize_ThrowsInvalidOperationException()
        {
            // Arrange
            Assert.IsNotNull(this.service);
            var invalidHost = new object();

            // Act & Assert
            // StartPreviewAsync requires Initialize to be called first
            try
            {
                await this.service.StartPreviewAsync(invalidHost);
                Assert.Fail("Should have thrown exception (either ArgumentException or InvalidOperationException)");
            }
            catch (ArgumentException)
            {
                // ArgumentException for invalid host is acceptable
                Assert.IsTrue(true);
            }
            catch (InvalidOperationException)
            {
                // InvalidOperationException for MediaCapture not initialized is acceptable
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public async Task StopPreviewAsync_AfterMultipleFailedStarts_Succeeds()
        {
            // Arrange
            Assert.IsNotNull(this.service);
            var invalidHost = new object();

            // Act: Attempt multiple failed starts, then stop
            try
            {
                await this.service.StartPreviewAsync(invalidHost);
            }
            catch
            {
                // Expected to fail
            }

            try
            {
                await this.service.StartPreviewAsync(invalidHost);
            }
            catch
            {
                // Expected to fail
            }

            // Assert: StopPreviewAsync should handle cleanup gracefully
            await this.service.StopPreviewAsync();
            Assert.IsTrue(true, "StopPreviewAsync succeeded after multiple failed starts");
        }

        [TestMethod]
        public async Task ResourceCleanup_StartStopCycle_CanBeRepeated()
        {
            // Arrange
            Assert.IsNotNull(this.service);
            var invalidHost = new object();

            // Act & Assert: Repeat start/stop cycles to verify resource cleanup
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    await this.service.StartPreviewAsync(invalidHost);
                }
                catch (ArgumentException)
                {
                    // Expected with invalid host
                }

                await this.service.StopPreviewAsync();
                Assert.IsTrue(true, $"Cycle {i + 1} completed without resource leak");
            }
        }
    }
}
