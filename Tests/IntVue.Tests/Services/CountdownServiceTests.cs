// Copyright (c) YourProjectName. All rights reserved.

using IntVue.Services;

namespace IntVue.Tests.Services;

/// <summary>
/// Unit tests for CountdownService.
/// </summary>
[TestClass]
public class CountdownServiceTests
{
    /// <summary>
    /// Test that countdown completes normally and returns true.
    /// </summary>
    [TestMethod]
    public async Task StartAsync_NormalCompletion_ReturnsTrue()
    {
        // Arrange
        var service = new CountdownService(TimeSpan.FromMilliseconds(50));
        var lastReportedValue = 0;
        var progress = new Progress<int>(t => lastReportedValue = t);
        var cts = new CancellationTokenSource();

        // Act
        var result = await service.StartAsync(3, progress, cts.Token);
        await Task.Delay(10); // Allow progress callbacks to complete

        // Assert - verify it completed successfully and reported the final value
        Assert.IsTrue(result, "Countdown should return true on normal completion");
        Assert.AreEqual(0, lastReportedValue, "Last reported value should be 0");
    }

    /// <summary>
    /// Test that countdown returns false when cancelled before starting.
    /// </summary>
    [TestMethod]
    public async Task StartAsync_CancelledBeforeStart_ReturnsFalse()
    {
        // Arrange
        var service = new CountdownService(TimeSpan.FromSeconds(1));
        var progress = new Progress<int>(_ => { });
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await service.StartAsync(3, progress, cts.Token);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Test that countdown returns false when cancelled midway.
    /// </summary>
    [TestMethod]
    public async Task StartAsync_CancelledMidway_ReturnsFalse()
    {
        // Arrange
        var service = new CountdownService(TimeSpan.FromMilliseconds(10));
        var ticks = new List<int>();
        var cts = new CancellationTokenSource();

        var progress = new Progress<int>(t =>
        {
            ticks.Add(t);
            if (ticks.Count == 2)
            {
                cts.Cancel();
            }
        });

        // Act
        var result = await service.StartAsync(3, progress, cts.Token);

        // Assert
        Assert.IsFalse(result);
        Assert.IsTrue(ticks.Count >= 2);
        Assert.IsTrue(ticks.Count < 4);
    }
}
