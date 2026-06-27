// Copyright (c) YourProjectName. All rights reserved.

using IntVue.Services;
using IntVue.ViewModels;

namespace IntVue.Tests.ViewModels;

/// <summary>
/// Unit tests for MainViewModel countdown logic.
/// </summary>
[TestClass]
public class MainViewModelTests
{
    /// <summary>
    /// Test that StartCountdownAsync sets IsCountingDown to true during countdown.
    /// </summary>
    [TestMethod]
    public async Task StartCountdownAsync_SetsIsCountingDownTrue()
    {
        // Arrange
        var mockService = new Mock<ICountdownService>();

        mockService
            .Setup(s => s.StartAsync(3, It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>()))
            .Returns<int, IProgress<int>, CancellationToken>(async (_, progress, _) =>
            {
                progress.Report(3);
                await Task.Delay(50);
                progress.Report(2);
                await Task.Delay(50);
                progress.Report(1);
                await Task.Delay(50);
                progress.Report(0);
                return true;
            });

        var viewModel = new MainViewModel(mockService.Object);
        var isCountingDuringCountdown = false;

        // Act
        var countdownTask = viewModel.StartCountdownAsync();
        await Task.Delay(10); // Let it start

        // Assert
        isCountingDuringCountdown = viewModel.IsCountingDown;
        Assert.IsTrue(isCountingDuringCountdown);

        await countdownTask;
    }

    /// <summary>
    /// Test that StartCountdownAsync fires CountdownCompleted event when finished successfully.
    /// </summary>
    [TestMethod]
    public async Task StartCountdownAsync_WhenCompleted_FiresCountdownCompletedEvent()
    {
        // Arrange
        var mockService = new Mock<ICountdownService>();
        mockService
            .Setup(s => s.StartAsync(3, It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>()))
            .Callback<int, IProgress<int>, CancellationToken>((_, progress, _) =>
            {
                progress.Report(3);
                progress.Report(2);
                progress.Report(1);
                progress.Report(0);
            })
            .ReturnsAsync(true);

        var viewModel = new MainViewModel(mockService.Object);
        var eventFired = false;
        viewModel.CountdownCompleted += (s, e) => eventFired = true;

        // Act
        await viewModel.StartCountdownAsync();

        // Assert
        Assert.IsTrue(eventFired);
    }

    /// <summary>
    /// Test that StartCountdownAsync resets IsCountingDown when completed.
    /// </summary>
    [TestMethod]
    public async Task StartCountdownAsync_WhenCompleted_SetsIsCountingDownFalse()
    {
        // Arrange
        var mockService = new Mock<ICountdownService>();
        mockService
            .Setup(s => s.StartAsync(3, It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>()))
            .Callback<int, IProgress<int>, CancellationToken>((_, progress, _) =>
            {
                progress.Report(3);
                progress.Report(2);
                progress.Report(1);
                progress.Report(0);
            })
            .ReturnsAsync(true);

        var viewModel = new MainViewModel(mockService.Object);

        // Act
        await viewModel.StartCountdownAsync();

        // Assert
        Assert.IsFalse(viewModel.IsCountingDown);
    }

    /// <summary>
    /// Test that CountdownSeconds updates during countdown.
    /// </summary>
    [TestMethod]
    public async Task StartCountdownAsync_UpdatesCountdownSeconds()
    {
        // Arrange
        var mockService = new Mock<ICountdownService>();
        var finalCountdownValue = 0;

        mockService
            .Setup(s => s.StartAsync(3, It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>()))
            .Returns<int, IProgress<int>, CancellationToken>(async (_, progress, _) =>
            {
                progress.Report(3);
                await Task.Delay(10);
                progress.Report(2);
                await Task.Delay(10);
                progress.Report(1);
                await Task.Delay(10);
                progress.Report(0);
                return true;
            });

        var viewModel = new MainViewModel(mockService.Object);

        // Act
        await viewModel.StartCountdownAsync();
        finalCountdownValue = viewModel.CountdownSeconds;

        // Assert - verify countdown ended at 0
        Assert.AreEqual(0, finalCountdownValue, "CountdownSeconds should be 0 after completion");
        Assert.IsFalse(viewModel.IsCountingDown, "IsCountingDown should be false after completion");
    }

    /// <summary>
    /// Test that CancelCountdown stops the countdown and resets state.
    /// </summary>
    [TestMethod]
    public async Task CancelCountdown_DuringCountdown_ResetsState()
    {
        // Arrange
        var mockService = new Mock<ICountdownService>();
        var cts = new CancellationTokenSource();

        mockService
            .Setup(s => s.StartAsync(3, It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>()))
            .Callback<int, IProgress<int>, CancellationToken>((_, progress, token) =>
            {
                progress.Report(3);
                // Simulate a long delay that can be cancelled
                try { Task.Delay(5000, token).Wait(token); }
                catch { }
            })
            .ReturnsAsync(false);

        var viewModel = new MainViewModel(mockService.Object);
        var countdownTask = viewModel.StartCountdownAsync();

        // Wait a bit for countdown to start
        await Task.Delay(50);

        // Act
        viewModel.CancelCountdownCommand.Execute(null);

        // Assert
        Assert.IsFalse(viewModel.IsCountingDown);
        Assert.AreEqual(0, viewModel.CountdownSeconds);
    }

    /// <summary>
    /// Test that CountdownCompleted event is not fired when countdown is cancelled.
    /// </summary>
    [TestMethod]
    public async Task StartCountdownAsync_WhenCancelled_DoesNotFireEvent()
    {
        // Arrange
        var mockService = new Mock<ICountdownService>();
        mockService
            .Setup(s => s.StartAsync(3, It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var viewModel = new MainViewModel(mockService.Object);
        var eventFired = false;
        viewModel.CountdownCompleted += (s, e) => eventFired = true;

        // Act
        await viewModel.StartCountdownAsync();

        // Assert
        Assert.IsFalse(eventFired);
    }
}
