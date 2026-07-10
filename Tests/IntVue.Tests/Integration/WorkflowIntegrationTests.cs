// Copyright (c) YourProjectName. All rights reserved.

using System.Collections.ObjectModel;

using IntVue.Models;
using IntVue.Services;
using IntVue.ViewModels;

namespace IntVue.Tests.Integration;

/// <summary>
/// Integration tests for critical end-to-end workflows.
/// Tests validate complete user workflows (record → play → delete, etc.) with mocked services.
/// </summary>
[TestClass]
public class WorkflowIntegrationTests
{
    /// <summary>
    /// Creates a mock ICountdownService for testing.
    /// </summary>
    private static ICountdownService CreateMockCountdownService()
    {
        var mockService = new Mock<ICountdownService>();
        mockService
            .Setup(s => s.StartAsync(It.IsAny<int>(), It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>()))
            .Returns<int, IProgress<int>, CancellationToken>(async (seconds, progress, _) =>
            {
                for (int i = seconds; i >= 0; i--)
                {
                    progress.Report(i);
                    await Task.Delay(10);
                }

                return true;
            });
        return mockService.Object;
    }

    /// <summary>
    /// Creates a mock IFeatureFlagService for testing.
    /// </summary>
    private static IFeatureFlagService CreateMockFeatureFlagService(bool isProductReviewEnabled = false)
    {
        var mockService = new Mock<IFeatureFlagService>();
        mockService
            .Setup(s => s.IsProductReviewEnabled())
            .Returns(isProductReviewEnabled);
        mockService
            .Setup(s => s.SetProductReviewEnabled(It.IsAny<bool>()))
            .Returns(Task.CompletedTask);
        return mockService.Object;
    }

    /// <summary>
    /// Creates a mock IPlaylistService for testing.
    /// </summary>
    private static IPlaylistService CreateMockPlaylistService(int questionCount = 3)
    {
        var mockService = new Mock<IPlaylistService>();

        // Create sample questions
        var questions = new ObservableCollection<Question>();
        for (int i = 0; i < questionCount; i++)
        {
            questions.Add(new Question
            {
                FilePath = $"C:\\questions\\question{i + 1}.webm",
                FileName = $"question{i + 1}.webm",
                DurationMs = 30000,
                IsValid = true
            });
        }

        mockService
            .Setup(s => s.Questions)
            .Returns(questions);

        mockService
            .Setup(s => s.CurrentQuestion)
            .Returns(() => questions.Count > 0 ? questions[0] : null);

        mockService
            .Setup(s => s.CurrentIndex)
            .Returns(0);

        mockService
            .Setup(s => s.TotalCount)
            .Returns(questions.Count);

        mockService
            .Setup(s => s.CurrentPlayMode)
            .Returns(PlayMode.Sequential);

        mockService
            .Setup(s => s.InitializeAsync(It.IsAny<List<Question>>()))
            .Returns(Task.CompletedTask);

        return mockService.Object;
    }

    /// <summary>
    /// Creates a mock IProductReviewService for testing.
    /// </summary>
    private static IProductReviewService CreateMockProductReviewService(int questionCount = 3)
    {
        var mockService = new Mock<IProductReviewService>();

        // Create sample questions (as List, not ObservableCollection)
        var questions = new List<Question>();
        for (int i = 0; i < questionCount; i++)
        {
            questions.Add(new Question
            {
                FilePath = $"C:\\questions\\question{i + 1}.webm",
                FileName = $"question{i + 1}.webm",
                DurationMs = 30000,
                IsValid = true
            });
        }

        mockService
            .Setup(s => s.LoadQuestionDirectoryAsync(It.IsAny<string>()))
            .ReturnsAsync(questions);

        return mockService.Object;
    }

    // === Record → Play → Delete Workflow Tests ===

    /// <summary>
    /// Test that MainViewModel countdown flow updates IsCountingDown property.
    /// </summary>
    [TestMethod]
    public async Task RecordPlayDeleteWorkflow_CountdownFlow_UpdatesIsCountingDownProperty()
    {
        // Arrange
        var mockCountdownService = CreateMockCountdownService();
        var mockFeatureFlagService = CreateMockFeatureFlagService();
        var viewModel = new MainViewModel(mockCountdownService, mockFeatureFlagService);

        var countdownState = new List<bool>();

        // Act
        var countdownTask = viewModel.StartCountdownAsync();
        await Task.Delay(5);
        countdownState.Add(viewModel.IsCountingDown);
        await countdownTask;
        countdownState.Add(viewModel.IsCountingDown);

        // Assert
        Assert.IsTrue(countdownState[0], "IsCountingDown should be true during countdown");
        Assert.IsFalse(countdownState[1], "IsCountingDown should be false after countdown completes");
    }

    /// <summary>
    /// Test that CountdownCompleted event fires when countdown finishes.
    /// </summary>
    [TestMethod]
    public async Task RecordPlayDeleteWorkflow_Countdown_FiresCompletedEventWhenFinished()
    {
        // Arrange
        var mockCountdownService = CreateMockCountdownService();
        var mockFeatureFlagService = CreateMockFeatureFlagService();
        var viewModel = new MainViewModel(mockCountdownService, mockFeatureFlagService);
        var eventFired = false;

        viewModel.CountdownCompleted += (s, e) => eventFired = true;

        // Act
        await viewModel.StartCountdownAsync();

        // Assert
        Assert.IsTrue(eventFired, "CountdownCompleted event should fire when countdown finishes");
    }

    /// <summary>
    /// Test that CancelCountdownCommand stops countdown immediately.
    /// </summary>
    [TestMethod]
    public async Task RecordPlayDeleteWorkflow_CancelCountdown_StopsImmediately()
    {
        // Arrange
        var mockCountdownService = CreateMockCountdownService();
        var mockFeatureFlagService = CreateMockFeatureFlagService();
        var viewModel = new MainViewModel(mockCountdownService, mockFeatureFlagService);

        var countdownTask = viewModel.StartCountdownAsync();
        await Task.Delay(25); // Let countdown progress

        // Act
        viewModel.CancelCountdownCommand.Execute(null);
        await Task.Delay(50); // Wait longer for cancellation to complete

        // Assert
        Assert.IsFalse(viewModel.IsCountingDown, "IsCountingDown should be false after cancel");

        try
        {
            await countdownTask;
        }
        catch
        {
            // Task cancellation is expected
        }
    }

    // === Product Review Navigation Tests ===

    /// <summary>
    /// Test that ProductReviewViewModel initializes with first question.
    /// </summary>
    [TestMethod]
    public async Task ProductReviewNavigation_LoadDirectory_InitializesWithFirstQuestion()
    {
        // Arrange
        var mockProductReviewService = CreateMockProductReviewService(3);
        var mockPlaylistService = CreateMockPlaylistService(3);
        var mockCountdownService = CreateMockCountdownService();
        var viewModel = new ProductReviewViewModel(mockProductReviewService, mockPlaylistService, mockCountdownService);

        // Act
        await viewModel.LoadQuestionsAsync("dummy-path");

        // Assert
        Assert.IsTrue(viewModel.Questions.Count > 0, "Questions should be loaded");
        Assert.IsNotNull(viewModel.CurrentQuestion, "CurrentQuestion should not be null");
    }

    /// <summary>
    /// Test that Product Review feature flag controls visibility.
    /// </summary>
    [TestMethod]
    public void ProductReviewNavigation_FeatureFlagEnabled_ProductReviewVisibleInMainViewModel()
    {
        // Arrange
        var mockCountdownService = CreateMockCountdownService();
        var mockFeatureFlagService = CreateMockFeatureFlagService(isProductReviewEnabled: true);

        // Act
        var viewModel = new MainViewModel(mockCountdownService, mockFeatureFlagService);

        // Assert
        Assert.IsTrue(viewModel.IsProductReviewEnabled, "Product Review should be visible when feature flag is enabled");
    }

    /// <summary>
    /// Test that Product Review feature flag hides button when disabled.
    /// </summary>
    [TestMethod]
    public void ProductReviewNavigation_FeatureFlagDisabled_ProductReviewHiddenInMainViewModel()
    {
        // Arrange
        var mockCountdownService = CreateMockCountdownService();
        var mockFeatureFlagService = CreateMockFeatureFlagService(isProductReviewEnabled: false);

        // Act
        var viewModel = new MainViewModel(mockCountdownService, mockFeatureFlagService);

        // Assert
        Assert.IsFalse(viewModel.IsProductReviewEnabled, "Product Review should be hidden when feature flag is disabled");
    }

    // === Playlist Navigation Tests ===

    /// <summary>
    /// Test that playlist sequential mode can be set.
    /// </summary>
    [TestMethod]
    public async Task PlaylistNavigation_SequentialMode_CanBeSet()
    {
        // Arrange
        var mockProductReviewService = CreateMockProductReviewService(5);
        var mockPlaylistService = CreateMockPlaylistService(5);
        var mockCountdownService = CreateMockCountdownService();
        var viewModel = new ProductReviewViewModel(mockProductReviewService, mockPlaylistService, mockCountdownService);
        await viewModel.LoadQuestionsAsync("dummy-path");

        // Act
        viewModel.SetPlayModeCommand.Execute(PlayMode.Sequential);

        // Assert
        Assert.IsNotNull(viewModel.CurrentQuestion, "CurrentQuestion should still be set");
    }

    /// <summary>
    /// Test that playlist loop mode can be set.
    /// </summary>
    [TestMethod]
    public async Task PlaylistNavigation_LoopMode_CanBeSet()
    {
        // Arrange
        var mockProductReviewService = CreateMockProductReviewService(3);
        var mockPlaylistService = CreateMockPlaylistService(3);
        var mockCountdownService = CreateMockCountdownService();
        var viewModel = new ProductReviewViewModel(mockProductReviewService, mockPlaylistService, mockCountdownService);
        await viewModel.LoadQuestionsAsync("dummy-path");

        // Act
        viewModel.SetPlayModeCommand.Execute(PlayMode.Loop);

        // Assert
        Assert.IsNotNull(viewModel.CurrentQuestion, "CurrentQuestion should still be set after setting loop mode");
    }

    /// <summary>
    /// Test that playlist repeat current mode can be set.
    /// </summary>
    [TestMethod]
    public async Task PlaylistNavigation_RepeatCurrentMode_CanBeSet()
    {
        // Arrange
        var mockProductReviewService = CreateMockProductReviewService(3);
        var mockPlaylistService = CreateMockPlaylistService(3);
        var mockCountdownService = CreateMockCountdownService();
        var viewModel = new ProductReviewViewModel(mockProductReviewService, mockPlaylistService, mockCountdownService);
        await viewModel.LoadQuestionsAsync("dummy-path");

        // Act
        viewModel.SetPlayModeCommand.Execute(PlayMode.RepeatCurrent);

        // Assert
        Assert.IsNotNull(viewModel.CurrentQuestion, "CurrentQuestion should still be available in repeat current mode");
    }

    // === Error Handling Tests ===

    /// <summary>
    /// Test that ProductReviewViewModel handles empty question list gracefully.
    /// </summary>
    [TestMethod]
    public async Task ErrorHandling_EmptyDirectory_ShowsNoQuestions()
    {
        // Arrange
        var mockProductReviewService = CreateMockProductReviewService(0);
        var mockPlaylistService = CreateMockPlaylistService(0);
        var mockCountdownService = CreateMockCountdownService();
        var viewModel = new ProductReviewViewModel(mockProductReviewService, mockPlaylistService, mockCountdownService);

        // Act
        await viewModel.LoadQuestionsAsync("empty-path");

        // Assert
        Assert.AreEqual(0, viewModel.Questions.Count, "Questions collection should be empty");
    }

    /// <summary>
    /// Test that ProductReviewViewModel handles question loading without errors.
    /// </summary>
    [TestMethod]
    public async Task ErrorHandling_LoadQuestions_HandlesMultipleQuestions()
    {
        // Arrange
        var mockService = new Mock<IProductReviewService>();
        var questions = new List<Question>
        {
            new Question { FilePath = "C:\\q1.webm", FileName = "q1.webm", IsValid = true },
            new Question { FilePath = "C:\\q2.webm", FileName = "q2.webm", IsValid = true },
            new Question { FilePath = "C:\\q3.webm", FileName = "q3.webm", IsValid = true }
        };
        mockService
            .Setup(s => s.LoadQuestionDirectoryAsync(It.IsAny<string>()))
            .ReturnsAsync(questions);

        var mockPlaylistService = CreateMockPlaylistService(3);
        var mockCountdownService = CreateMockCountdownService();
        var viewModel = new ProductReviewViewModel(mockService.Object, mockPlaylistService, mockCountdownService);

        // Act
        await viewModel.LoadQuestionsAsync("path-with-questions");

        // Assert
        Assert.AreEqual(3, viewModel.Questions.Count, "Should load all questions");
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage, "Should have no error message on successful load");
    }

    // === Countdown to Recording Transition Tests ===

    /// <summary>
    /// Test that countdown state transitions correctly (IsCountingDown changes).
    /// </summary>
    [TestMethod]
    public async Task CountdownTransition_StartToComplete_StateChangesCorrectly()
    {
        // Arrange
        var mockCountdownService = CreateMockCountdownService();
        var mockFeatureFlagService = CreateMockFeatureFlagService();
        var viewModel = new MainViewModel(mockCountdownService, mockFeatureFlagService);
        var stateChanges = new List<(string Event, bool IsCountingDown)>();

        // Act
        stateChanges.Add(("Initial", viewModel.IsCountingDown));

        var countdownTask = viewModel.StartCountdownAsync();
        await Task.Delay(5);
        stateChanges.Add(("During", viewModel.IsCountingDown));

        await countdownTask;
        stateChanges.Add(("After", viewModel.IsCountingDown));

        // Assert
        Assert.IsFalse(stateChanges[0].IsCountingDown, "Should start as not counting down");
        Assert.IsTrue(stateChanges[1].IsCountingDown, "Should be counting down during countdown");
        Assert.IsFalse(stateChanges[2].IsCountingDown, "Should finish as not counting down");
    }

    /// <summary>
    /// Test that CountdownSeconds property updates during countdown.
    /// </summary>
    [TestMethod]
    public async Task CountdownTransition_CountdownSeconds_UpdatesDuringCountdown()
    {
        // Arrange
        var mockCountdownService = CreateMockCountdownService();
        var mockFeatureFlagService = CreateMockFeatureFlagService();
        var viewModel = new MainViewModel(mockCountdownService, mockFeatureFlagService);
        var secondsObserved = new List<int>();

        // Act
        var countdownTask = viewModel.StartCountdownAsync();
        for (int i = 0; i < 5; i++)
        {
            secondsObserved.Add(viewModel.CountdownSeconds);
            await Task.Delay(15);
        }

        await countdownTask;

        // Assert
        Assert.IsTrue(secondsObserved.Count > 0, "Should observe countdown seconds changes");
        Assert.IsTrue(secondsObserved[0] >= 0, "Countdown should report non-negative seconds");
    }

    /// <summary>
    /// Test that MoveToNext command can be executed.
    /// </summary>
    [TestMethod]
    public async Task PlaylistNavigation_MoveToNext_CommandExecutes()
    {
        // Arrange
        var mockProductReviewService = CreateMockProductReviewService(3);
        var mockPlaylistService = CreateMockPlaylistService(3);
        var mockCountdownService = CreateMockCountdownService();
        var viewModel = new ProductReviewViewModel(mockProductReviewService, mockPlaylistService, mockCountdownService);
        await viewModel.LoadQuestionsAsync("dummy-path");

        // Act
        viewModel.MoveToNextCommand.Execute(null);

        // Assert
        Assert.IsNotNull(viewModel.CurrentQuestion, "CurrentQuestion should still be available");
    }

    /// <summary>
    /// Test that MoveToPrevious command can be executed.
    /// </summary>
    [TestMethod]
    public async Task PlaylistNavigation_MoveToPrevious_CommandExecutes()
    {
        // Arrange
        var mockProductReviewService = CreateMockProductReviewService(3);
        var mockPlaylistService = CreateMockPlaylistService(3);
        var mockCountdownService = CreateMockCountdownService();
        var viewModel = new ProductReviewViewModel(mockProductReviewService, mockPlaylistService, mockCountdownService);
        await viewModel.LoadQuestionsAsync("dummy-path");

        // Act
        viewModel.MoveToPreviousCommand.Execute(null);

        // Assert
        Assert.IsNotNull(viewModel.CurrentQuestion, "CurrentQuestion should still be available");
    }
}
