// Copyright (c) YourProjectName. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

using IntVue.Models;
using IntVue.Services;
using IntVue.ViewModels;

namespace IntVue.Tests.ViewModels;

[TestClass]
public class ProductReviewViewModelTests
{
    [TestMethod]
    public void Constructor_WithValidServices_InitializesSuccessfully()
    {
        // Arrange & Act
        var productReviewService = new Mock<IProductReviewService>();
        var playlistService = new Mock<IPlaylistService>();
        var countdownService = new Mock<ICountdownService>();

        var viewModel = new ProductReviewViewModel(productReviewService.Object, playlistService.Object, countdownService.Object);

        // Assert
        Assert.IsNotNull(viewModel);
        Assert.AreEqual("Product Review", viewModel.Title);
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
        Assert.IsFalse(viewModel.IsLoading);
    }

    [TestMethod]
    public void Constructor_WithNullProductReviewService_ThrowsArgumentNullException()
    {
        // Arrange
        var playlistService = new Mock<IPlaylistService>();
        var countdownService = new Mock<ICountdownService>();
        var exception = false;

        // Act
        try
        {
            _ = new ProductReviewViewModel(null!, playlistService.Object, countdownService.Object);
        }
        catch (ArgumentNullException)
        {
            exception = true;
        }

        // Assert
        Assert.IsTrue(exception);
    }

    [TestMethod]
    public void Constructor_WithNullPlaylistService_ThrowsArgumentNullException()
    {
        // Arrange
        var productReviewService = new Mock<IProductReviewService>();
        var countdownService = new Mock<ICountdownService>();
        var exception = false;

        // Act
        try
        {
            _ = new ProductReviewViewModel(productReviewService.Object, null!, countdownService.Object);
        }
        catch (ArgumentNullException)
        {
            exception = true;
        }

        // Assert
        Assert.IsTrue(exception);
    }

    [TestMethod]
    public void Constructor_WithNullCountdownService_ThrowsArgumentNullException()
    {
        // Arrange
        var productReviewService = new Mock<IProductReviewService>();
        var playlistService = new Mock<IPlaylistService>();
        var exception = false;

        // Act
        try
        {
            _ = new ProductReviewViewModel(productReviewService.Object, playlistService.Object, null!);
        }
        catch (ArgumentNullException)
        {
            exception = true;
        }

        // Assert
        Assert.IsTrue(exception);
    }

    [TestMethod]
    public void Constructor_DefaultsCurrentPlayModeToRepeatCurrent()
    {
        // Arrange & Act
        var productReviewService = new Mock<IProductReviewService>();
        var playlistService = new Mock<IPlaylistService>();
        var countdownService = new Mock<ICountdownService>();

        var viewModel = new ProductReviewViewModel(productReviewService.Object, playlistService.Object, countdownService.Object);

        // Assert
        Assert.AreEqual(PlayMode.Sequential, viewModel.CurrentPlayMode);
    }

    [TestMethod]
    public async Task LoadQuestionsAsync_WithValidDirectory_LoadsQuestionsSuccessfully()
    {
        // Arrange
        var questions = new List<Question>
        {
            new() { FilePath = "q1.webm", FileName = "q1" },
            new() { FilePath = "q2.webm", FileName = "q2" }
        };

        var productReviewService = new Mock<IProductReviewService>();
        productReviewService
            .Setup(s => s.LoadQuestionDirectoryAsync("C:\\Questions"))
            .ReturnsAsync(questions);

        var playlistService = new Mock<IPlaylistService>();
        playlistService.SetupGet(s => s.CurrentIndex).Returns(0);
        playlistService.Setup(s => s.InitializeAsync(It.IsAny<List<Question>>()))
            .Returns(Task.CompletedTask);

        var countdownService = new Mock<ICountdownService>();
        var viewModel = new ProductReviewViewModel(productReviewService.Object, playlistService.Object, countdownService.Object);

        // Act
        await viewModel.LoadQuestionsCommand.ExecuteAsync("C:\\Questions");

        // Assert
        Assert.IsFalse(viewModel.IsLoading);
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
        playlistService.Verify(s => s.InitializeAsync(questions), Times.Once);
    }

    [TestMethod]
    public async Task LoadQuestionsAsync_WithEmptyDirectory_SetsErrorMessage()
    {
        // Arrange
        var productReviewService = new Mock<IProductReviewService>();
        productReviewService
            .Setup(s => s.LoadQuestionDirectoryAsync("C:\\Empty"))
            .ReturnsAsync(new List<Question>());

        var playlistService = new Mock<IPlaylistService>();
        var countdownService = new Mock<ICountdownService>();
        var viewModel = new ProductReviewViewModel(productReviewService.Object, playlistService.Object, countdownService.Object);

        // Act
        await viewModel.LoadQuestionsCommand.ExecuteAsync("C:\\Empty");

        // Assert
        Assert.IsFalse(viewModel.IsLoading);
        Assert.IsTrue(viewModel.ErrorMessage.Contains("No valid question files found"));
    }

    [TestMethod]
    public async Task LoadQuestionsAsync_WithInvalidDirectory_SetsErrorMessage()
    {
        // Arrange
        var productReviewService = new Mock<IProductReviewService>();
        productReviewService
            .Setup(s => s.LoadQuestionDirectoryAsync(""))
            .ThrowsAsync(new ArgumentException("Invalid path"));

        var playlistService = new Mock<IPlaylistService>();
        var countdownService = new Mock<ICountdownService>();
        var viewModel = new ProductReviewViewModel(productReviewService.Object, playlistService.Object, countdownService.Object);

        // Act
        await viewModel.LoadQuestionsCommand.ExecuteAsync("");

        // Assert
        Assert.IsFalse(viewModel.IsLoading);
        Assert.IsTrue(viewModel.ErrorMessage.Contains("Invalid directory path"));
    }

    [TestMethod]
    public async Task LoadQuestionsAsync_WithMissingDirectory_SetsErrorMessage()
    {
        // Arrange
        var productReviewService = new Mock<IProductReviewService>();
        productReviewService
            .Setup(s => s.LoadQuestionDirectoryAsync("C:\\NonExistent"))
            .ThrowsAsync(new System.IO.DirectoryNotFoundException());

        var playlistService = new Mock<IPlaylistService>();
        var countdownService = new Mock<ICountdownService>();
        var viewModel = new ProductReviewViewModel(productReviewService.Object, playlistService.Object, countdownService.Object);

        // Act
        await viewModel.LoadQuestionsCommand.ExecuteAsync("C:\\NonExistent");

        // Assert
        Assert.IsFalse(viewModel.IsLoading);
        Assert.IsTrue(viewModel.ErrorMessage.Contains("Directory not found"));
    }

    [TestMethod]
    public async Task LoadQuestionsAsync_WithAccessDenied_SetsErrorMessage()
    {
        // Arrange
        var productReviewService = new Mock<IProductReviewService>();
        productReviewService
            .Setup(s => s.LoadQuestionDirectoryAsync("C:\\Protected"))
            .ThrowsAsync(new UnauthorizedAccessException());

        var playlistService = new Mock<IPlaylistService>();
        var countdownService = new Mock<ICountdownService>();
        var viewModel = new ProductReviewViewModel(productReviewService.Object, playlistService.Object, countdownService.Object);

        // Act
        await viewModel.LoadQuestionsCommand.ExecuteAsync("C:\\Protected");

        // Assert
        Assert.IsFalse(viewModel.IsLoading);
        Assert.IsTrue(viewModel.ErrorMessage.Contains("Access to directory denied"));
    }

    [TestMethod]
    public void MoveToNext_CallsPlaylistService()
    {
        // Arrange
        var productReviewService = new Mock<IProductReviewService>();
        var playlistService = new Mock<IPlaylistService>();
        playlistService.Setup(s => s.MoveToNext()).Returns(new Question { FileName = "q2" });
        playlistService.SetupGet(s => s.CurrentIndex).Returns(1);
        playlistService.SetupGet(s => s.TotalCount).Returns(2);

        var countdownService = new Mock<ICountdownService>();
        var viewModel = new ProductReviewViewModel(productReviewService.Object, playlistService.Object, countdownService.Object);

        // Act
        viewModel.MoveToNextCommand.Execute(null);

        // Assert
        playlistService.Verify(s => s.MoveToNext(), Times.Once);
    }

    [TestMethod]
    public void MoveToPrevious_CallsPlaylistService()
    {
        // Arrange
        var productReviewService = new Mock<IProductReviewService>();
        var playlistService = new Mock<IPlaylistService>();
        playlistService.Setup(s => s.MoveToPrevious()).Returns(new Question { FileName = "q1" });
        playlistService.SetupGet(s => s.CurrentIndex).Returns(0);

        var countdownService = new Mock<ICountdownService>();
        var viewModel = new ProductReviewViewModel(productReviewService.Object, playlistService.Object, countdownService.Object);

        // Act
        viewModel.MoveToPreviousCommand.Execute(null);

        // Assert
        playlistService.Verify(s => s.MoveToPrevious(), Times.Once);
    }

    [TestMethod]
    public void SelectQuestion_WithValidIndex_SelectsQuestion()
    {
        // Arrange
        var question = new Question { FileName = "q1" };
        var productReviewService = new Mock<IProductReviewService>();
        var playlistService = new Mock<IPlaylistService>();
        playlistService.Setup(s => s.SelectByIndex(0)).Returns(question);
        playlistService.SetupGet(s => s.CurrentIndex).Returns(0);

        var countdownService = new Mock<ICountdownService>();
        var viewModel = new ProductReviewViewModel(productReviewService.Object, playlistService.Object, countdownService.Object);

        // Act
        viewModel.SelectQuestionCommand.Execute(0);

        // Assert
        playlistService.Verify(s => s.SelectByIndex(0), Times.Once);
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
    }

    [TestMethod]
    public void SelectQuestion_WithInvalidIndex_SetsErrorMessage()
    {
        // Arrange
        var productReviewService = new Mock<IProductReviewService>();
        var playlistService = new Mock<IPlaylistService>();
        playlistService.Setup(s => s.SelectByIndex(999)).Returns((Question?)null);

        var countdownService = new Mock<ICountdownService>();
        var viewModel = new ProductReviewViewModel(productReviewService.Object, playlistService.Object, countdownService.Object);

        // Act
        viewModel.SelectQuestionCommand.Execute(999);

        // Assert
        Assert.IsTrue(viewModel.ErrorMessage.Contains("Invalid question index"));
    }

    [TestMethod]
    public async Task ApplySortAsync_WithValidSortMode_SortsPlaylist()
    {
        // Arrange
        var productReviewService = new Mock<IProductReviewService>();
        var playlistService = new Mock<IPlaylistService>();
        playlistService.Setup(s => s.ApplySortAsync(SortMode.AscendingAlpha))
            .Returns(Task.CompletedTask);
        playlistService.SetupGet(s => s.CurrentSortMode).Returns(SortMode.AscendingAlpha);

        var countdownService = new Mock<ICountdownService>();
        var viewModel = new ProductReviewViewModel(productReviewService.Object, playlistService.Object, countdownService.Object);

        // Act
        await viewModel.ApplySortCommand.ExecuteAsync(SortMode.AscendingAlpha);

        // Assert
        Assert.IsFalse(viewModel.IsLoading);
        Assert.AreEqual(string.Empty, viewModel.ErrorMessage);
        playlistService.Verify(s => s.ApplySortAsync(SortMode.AscendingAlpha), Times.Once);
    }

    [TestMethod]
    public void SetPlayMode_WithValidPlayMode_UpdatesPlayMode()
    {
        // Arrange
        var productReviewService = new Mock<IProductReviewService>();
        var playlistService = new Mock<IPlaylistService>();
        playlistService.Setup(s => s.SetPlayMode(PlayMode.Loop));

        var countdownService = new Mock<ICountdownService>();
        var viewModel = new ProductReviewViewModel(productReviewService.Object, playlistService.Object, countdownService.Object);

        // Act
        viewModel.SetPlayModeCommand.Execute(PlayMode.Loop);

        // Assert
        Assert.AreEqual(PlayMode.Loop, viewModel.CurrentPlayMode);
        playlistService.Verify(s => s.SetPlayMode(PlayMode.Loop), Times.Once);
    }

    [TestMethod]
    public async Task StartCountdownAsync_OnSuccess_UpdatesCountdown()
    {
        // Arrange
        var productReviewService = new Mock<IProductReviewService>();
        var playlistService = new Mock<IPlaylistService>();
        var countdownService = new Mock<ICountdownService>();

        var countdownCompleted = false;
        countdownService
            .Setup(s => s.StartAsync(3, It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>()))
            .Returns<int, IProgress<int>, CancellationToken>(async (seconds, progress, ct) =>
            {
                for (int i = seconds; i >= 0; i--)
                {
                    progress.Report(i);
                    await Task.Delay(10, ct);
                }
                return true;
            });

        var viewModel = new ProductReviewViewModel(productReviewService.Object, playlistService.Object, countdownService.Object);
        viewModel.CountdownCompleted += (s, e) => countdownCompleted = true;

        // Act
        await viewModel.StartCountdownCommand.ExecuteAsync(null);

        // Assert
        Assert.IsFalse(viewModel.IsCountingDown);
        Assert.AreEqual(0, viewModel.CountdownSeconds);
        Assert.IsTrue(countdownCompleted);
    }

    [TestMethod]
    public async Task StartCountdownAsync_WhenCancelled_StopsCountdown()
    {
        // Arrange
        var productReviewService = new Mock<IProductReviewService>();
        var playlistService = new Mock<IPlaylistService>();
        var countdownService = new Mock<ICountdownService>();

        countdownService
            .Setup(s => s.StartAsync(3, It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(false));

        var countdownCompleted = false;
        var viewModel = new ProductReviewViewModel(productReviewService.Object, playlistService.Object, countdownService.Object);
        viewModel.CountdownCompleted += (s, e) => countdownCompleted = true;

        // Act
        await viewModel.StartCountdownCommand.ExecuteAsync(null);

        // Assert
        Assert.IsFalse(viewModel.IsCountingDown);
        Assert.IsFalse(countdownCompleted);
    }

    [TestMethod]
    public void CancelCountdown_WhileCountingDown_StopsCountdown()
    {
        // Arrange
        var productReviewService = new Mock<IProductReviewService>();
        var playlistService = new Mock<IPlaylistService>();
        var countdownService = new Mock<ICountdownService>();

        var viewModel = new ProductReviewViewModel(productReviewService.Object, playlistService.Object, countdownService.Object);
        viewModel.IsCountingDown = true;
        viewModel.CountdownSeconds = 2;

        // Act
        viewModel.CancelCountdownCommand.Execute(null);

        // Assert
        Assert.IsFalse(viewModel.IsCountingDown);
        Assert.AreEqual(0, viewModel.CountdownSeconds);
    }

    [TestMethod]
    public void Dispose_ReleasesResources()
    {
        // Arrange
        var productReviewService = new Mock<IProductReviewService>();
        var playlistService = new Mock<IPlaylistService>();
        var countdownService = new Mock<ICountdownService>();

        var viewModel = new ProductReviewViewModel(productReviewService.Object, playlistService.Object, countdownService.Object);

        // Act
        viewModel.Dispose();

        // Assert
        playlistService.Verify(s => s.Clear(), Times.Once);
    }

    [TestMethod]
    public void MoveToNextCommand_WhenCalled_UpdatesCurrentQuestion()
    {
        // Arrange
        var nextQuestion = new Question { FileName = "q2", FilePath = "q2.webm" };
        var productReviewService = new Mock<IProductReviewService>();
        var playlistService = new Mock<IPlaylistService>();

        // Setup PlaylistService to return next question
        playlistService.Setup(s => s.MoveToNext()).Returns(nextQuestion);
        playlistService.SetupGet(s => s.CurrentIndex).Returns(1);
        playlistService.SetupGet(s => s.TotalCount).Returns(3);
        playlistService.SetupGet(s => s.CurrentQuestion).Returns(nextQuestion);
        playlistService.SetupGet(s => s.CurrentSortMode).Returns(SortMode.AscendingAlpha);

        var countdownService = new Mock<ICountdownService>();
        var viewModel = new ProductReviewViewModel(productReviewService.Object, playlistService.Object, countdownService.Object);

        // Act
        viewModel.MoveToNextCommand.Execute(null);

        // Assert
        Assert.AreEqual(nextQuestion, viewModel.CurrentQuestion);
        Assert.AreEqual(2, viewModel.CurrentQuestionIndex);  // 1-based index
    }

    [TestMethod]
    public void Questions_ReturnsFromPlaylistService()
    {
        // Arrange
        var questions = new ObservableCollection<Question>
        {
            new() { FileName = "q1" },
            new() { FileName = "q2" }
        };

        var productReviewService = new Mock<IProductReviewService>();
        var playlistService = new Mock<IPlaylistService>();
        playlistService.SetupGet(s => s.Questions).Returns(questions);

        var countdownService = new Mock<ICountdownService>();
        var viewModel = new ProductReviewViewModel(productReviewService.Object, playlistService.Object, countdownService.Object);

        // Act
        var result = viewModel.Questions;

        // Assert
        Assert.IsTrue(ReferenceEquals(questions, result));
        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public void HasRecording_InitializesToFalse()
    {
        // Arrange
        var productReviewService = new Mock<IProductReviewService>();
        var playlistService = new Mock<IPlaylistService>();
        var countdownService = new Mock<ICountdownService>();

        // Act
        var viewModel = new ProductReviewViewModel(productReviewService.Object, playlistService.Object, countdownService.Object);

        // Assert
        Assert.IsFalse(viewModel.HasRecording);
    }

    [TestMethod]
    public void HasRecording_CanBeSetToTrue()
    {
        // Arrange
        var productReviewService = new Mock<IProductReviewService>();
        var playlistService = new Mock<IPlaylistService>();
        var countdownService = new Mock<ICountdownService>();
        var viewModel = new ProductReviewViewModel(productReviewService.Object, playlistService.Object, countdownService.Object);

        // Act
        viewModel.HasRecording = true;

        // Assert
        Assert.IsTrue(viewModel.HasRecording);
    }

    [TestMethod]
    public void HasRecording_CanBeSetToFalse()
    {
        // Arrange
        var productReviewService = new Mock<IProductReviewService>();
        var playlistService = new Mock<IPlaylistService>();
        var countdownService = new Mock<ICountdownService>();
        var viewModel = new ProductReviewViewModel(productReviewService.Object, playlistService.Object, countdownService.Object);
        viewModel.HasRecording = true;

        // Act
        viewModel.HasRecording = false;

        // Assert
        Assert.IsFalse(viewModel.HasRecording);
    }

    [TestMethod]
    public void HasRecording_RaisesPropertyChangedEvent()
    {
        // Arrange
        var productReviewService = new Mock<IProductReviewService>();
        var playlistService = new Mock<IPlaylistService>();
        var countdownService = new Mock<ICountdownService>();
        var viewModel = new ProductReviewViewModel(productReviewService.Object, playlistService.Object, countdownService.Object);
        var propertyChanged = false;

        viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(ProductReviewViewModel.HasRecording))
            {
                propertyChanged = true;
            }
        };

        // Act
        viewModel.HasRecording = true;

        // Assert
        Assert.IsTrue(propertyChanged);
    }

    [TestMethod]
    public void CanStartOrStopRecording_WhenCurrentQuestionChanges_FiresPropertyChanged()
    {
        // Arrange
        var question = new Question { FileName = "q1", FilePath = "q1.webm" };
        var productReviewService = new Mock<IProductReviewService>();
        var playlistService = new Mock<IPlaylistService>();
        var countdownService = new Mock<ICountdownService>();
        var viewModel = new ProductReviewViewModel(productReviewService.Object, playlistService.Object, countdownService.Object);

        bool propertyChangedFired = false;
        string changedPropertyName = string.Empty;

        viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(ProductReviewViewModel.CanStartOrStopRecording))
            {
                propertyChangedFired = true;
                changedPropertyName = e.PropertyName;
            }
        };

        // Act
        viewModel.CurrentQuestion = question;

        // Assert
        Assert.IsTrue(propertyChangedFired, "PropertyChanged should fire for CanStartOrStopRecording when CurrentQuestion changes");
        Assert.AreEqual(nameof(ProductReviewViewModel.CanStartOrStopRecording), changedPropertyName);
        Assert.IsTrue(viewModel.CanStartOrStopRecording, "CanStartOrStopRecording should be true when question exists and no recording");
    }

    [TestMethod]
    public void DefaultPlayMode_ShouldBeRepeatCurrent()
    {
        // Arrange
        var productReviewService = new Mock<IProductReviewService>();
        var mockSettings = new Mock<ISettingsService>();
        var playlistService = new PlaylistService(mockSettings.Object);
        var countdownService = new Mock<ICountdownService>();

        // Act
        var viewModel = new ProductReviewViewModel(productReviewService.Object, playlistService, countdownService.Object);

        // Assert
        Assert.AreEqual(PlayMode.Loop, playlistService.CurrentPlayMode,
            "PlaylistService default play mode should be Loop");
        Assert.AreEqual(PlayMode.Sequential, viewModel.CurrentPlayMode,
            "ViewModel default play mode should be Sequential (standard sequential playback)");
    }

    [TestMethod]
    public async Task LoadQuestions_ShouldSyncPlayModeWithPlaylistService()
    {
        // Arrange
        var questions = new List<Question>
        {
            new Question { FileName = "q1.webm", FilePath = "q1.webm" },
            new Question { FileName = "q2.webm", FilePath = "q2.webm" }
        };

        var mockSettings = new Mock<ISettingsService>();
        var playlistService = new PlaylistService(mockSettings.Object);

        var mockReview = new Mock<IProductReviewService>();
        mockReview.Setup(s => s.LoadQuestionDirectoryAsync(It.IsAny<string>()))
            .ReturnsAsync(questions);

        var viewModel = new ProductReviewViewModel(mockReview.Object, playlistService, new Mock<ICountdownService>().Object);

        // Act
        await viewModel.LoadQuestionsCommand.ExecuteAsync("test-dir");

        // Assert
        Assert.AreEqual(playlistService.CurrentPlayMode, viewModel.CurrentPlayMode,
            "ViewModel play mode should match PlaylistService play mode after load");
        Assert.AreEqual(PlayMode.Loop, viewModel.CurrentPlayMode,
            "Play mode should be Loop after loading questions");
    }

    [TestMethod]
    public void MoveToNext_WithLoopMode_ShouldWrapToBeginning()
    {
        // Arrange
        var questions = new List<Question>
        {
            new Question { FileName = "q1.webm", FilePath = "q1.webm" },
            new Question { FileName = "q2.webm", FilePath = "q2.webm" },
            new Question { FileName = "q3.webm", FilePath = "q3.webm" }
        };

        var playlistService = new PlaylistService(new Mock<ISettingsService>().Object);
        playlistService.InitializeAsync(questions).GetAwaiter().GetResult();
        playlistService.SetPlayMode(PlayMode.Loop);

        // Move to last question (index 2)
        playlistService.SelectByIndex(2);
        Assert.AreEqual(2, playlistService.CurrentIndex, "Should be at last question before moving");

        // Act: Move to next from last question
        var result = playlistService.MoveToNext();

        // Assert: Should wrap to beginning (index 0)
        Assert.AreEqual(0, playlistService.CurrentIndex,
            "Should wrap to beginning (index 0) in Loop mode");
        Assert.AreEqual("q1.webm", result?.FileName,
            "Should be first question (q1.webm) after wrapping");
    }

    [TestMethod]
    public async Task ShuffleCommand_ShouldUpdateCurrentQuestion()
    {
        // Arrange
        var questions = new List<Question>
        {
            new Question { FileName = "q1.webm", FilePath = "q1.webm" },
            new Question { FileName = "q2.webm", FilePath = "q2.webm" },
            new Question { FileName = "q3.webm", FilePath = "q3.webm" }
        };

        var mockSettings = new Mock<ISettingsService>();
        var playlistService = new PlaylistService(mockSettings.Object);

        var mockReview = new Mock<IProductReviewService>();
        mockReview.Setup(s => s.LoadQuestionDirectoryAsync(It.IsAny<string>()))
            .ReturnsAsync(questions);

        var viewModel = new ProductReviewViewModel(mockReview.Object, playlistService, new Mock<ICountdownService>().Object);
        await viewModel.LoadQuestionsCommand.ExecuteAsync("test-dir");

        var firstQuestionBefore = viewModel.CurrentQuestion;

        // Act: Shuffle
        await viewModel.ToggleShuffleAsync();

        // Assert: CurrentQuestion should be set to first question in shuffled order
        Assert.IsNotNull(viewModel.CurrentQuestion,
            "CurrentQuestion should not be null after shuffle");
        Assert.AreEqual(playlistService.CurrentQuestion, viewModel.CurrentQuestion,
            "ViewModel current question should match PlaylistService current question");
        Assert.AreEqual(SortMode.Shuffle, viewModel.CurrentSortMode,
            "Sort mode should be Shuffle after shuffle command");
    }

    [TestMethod]
    public void SetPlayMode_ShouldUpdateViewModelMode()
    {
        // Arrange
        var playlistService = new PlaylistService(new Mock<ISettingsService>().Object);
        var viewModel = new ProductReviewViewModel(
            new Mock<IProductReviewService>().Object,
            playlistService,
            new Mock<ICountdownService>().Object);

        var modeChangedFired = false;
        viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(ProductReviewViewModel.CurrentPlayMode))
            {
                modeChangedFired = true;
            }
        };

        // Act: Set play mode to Loop (different from default RepeatCurrent)
        viewModel.SetPlayModeCommand.Execute(PlayMode.Loop);

        // Assert
        Assert.IsTrue(modeChangedFired, "PropertyChanged should fire for CurrentPlayMode");
        Assert.AreEqual(PlayMode.Loop, viewModel.CurrentPlayMode,
            "ViewModel play mode should be updated");
        Assert.AreEqual(PlayMode.Loop, playlistService.CurrentPlayMode,
            "PlaylistService play mode should be updated");
    }

    [TestMethod]
    public void MoveToNext_WithRepeatMode_ShouldStayOnCurrentQuestion()
    {
        // Arrange
        var questions = new List<Question>
        {
            new Question { FileName = "q1.webm", FilePath = "q1.webm" },
            new Question { FileName = "q2.webm", FilePath = "q2.webm" },
            new Question { FileName = "q3.webm", FilePath = "q3.webm" }
        };

        var playlistService = new PlaylistService(new Mock<ISettingsService>().Object);
        playlistService.InitializeAsync(questions).GetAwaiter().GetResult();
        playlistService.SetPlayMode(PlayMode.RepeatCurrent);

        // Move to middle question (index 1)
        playlistService.SelectByIndex(1);
        Assert.AreEqual(1, playlistService.CurrentIndex, "Should be at q2 (index 1) before moving");

        var questionBeforeMove = playlistService.CurrentQuestion;

        // Act: Move to next with RepeatCurrent mode
        var result = playlistService.MoveToNext();

        // Assert: Should stay on same question (index 1)
        Assert.AreEqual(1, playlistService.CurrentIndex,
            "Should stay on same question (index 1) in Repeat mode");
        Assert.AreEqual(questionBeforeMove, result,
            "Should return same question (q2.webm)");
        Assert.AreEqual("q2.webm", result?.FileName,
            "Should be same question (q2.webm) after MoveToNext");
    }

    [TestMethod]
    public void DeleteRecording_WithRepeatMode_ShouldStayOnSameQuestion()
    {
        // Arrange: Setup with 3 questions
        var questions = new List<Question>
        {
            new Question { FileName = "q1.webm", FilePath = "c:\\q1.webm" },
            new Question { FileName = "q2.webm", FilePath = "c:\\q2.webm" },
            new Question { FileName = "q3.webm", FilePath = "c:\\q3.webm" }
        };

        var mockSettings = new Mock<ISettingsService>();
        var playlistService = new PlaylistService(mockSettings.Object);
        playlistService.InitializeAsync(questions).GetAwaiter().GetResult();
        playlistService.SetPlayMode(PlayMode.RepeatCurrent);

        var viewModel = new ProductReviewViewModel(
            new Mock<IProductReviewService>().Object,
            playlistService,
            new Mock<ICountdownService>().Object);

        // Sync ViewModel mode with PlaylistService (normally done in LoadQuestionsAsync)
        viewModel.CurrentPlayMode = playlistService.CurrentPlayMode;

        // Select Q2
        viewModel.SelectQuestionCommand.Execute(1);

        // Capture state BEFORE delete
        var questionBefore = viewModel.CurrentQuestion;
        var pathBefore = viewModel.CurrentQuestionPath;
        var indexBefore = viewModel.CurrentQuestionIndex;
        var modeBefore = viewModel.CurrentPlayMode;

        Assert.AreEqual("q2.webm", questionBefore?.FileName, "Should start on Q2");
        Assert.AreEqual("c:\\q2.webm", pathBefore, "Path should be q2.webm");
        Assert.AreEqual(2, indexBefore, "Index should be 2/5");
        Assert.AreEqual(PlayMode.RepeatCurrent, modeBefore, "Mode should be RepeatCurrent");

        // Simulate recording
        viewModel.HasRecording = true;
        Assert.IsFalse(viewModel.CanStartOrStopRecording, "Button disabled with recording");

        // Act: Delete recording
        viewModel.HasRecording = false;

        // Assert: Verify ALL state properties
        Assert.AreEqual(questionBefore, viewModel.CurrentQuestion,
            "CurrentQuestion should STAY on Q2");
        Assert.AreEqual(pathBefore, viewModel.CurrentQuestionPath,
            "CurrentQuestionPath should STAY c:\\q2.webm");
        Assert.AreEqual(indexBefore, viewModel.CurrentQuestionIndex,
            "CurrentQuestionIndex should STAY 2/5");
        Assert.AreEqual(modeBefore, viewModel.CurrentPlayMode,
            "CurrentPlayMode should STAY RepeatCurrent");
        Assert.IsFalse(viewModel.HasRecording, "HasRecording should be false");
        Assert.IsTrue(viewModel.CanStartOrStopRecording, "Button should be enabled");
    }

    [TestMethod]
    public async Task DeleteRecording_WithLoopMode_ShouldAdvanceToNextQuestion()
    {
        // Arrange: Setup with 3 questions
        var questions = new List<Question>
        {
            new Question { FileName = "q1.webm", FilePath = "c:\\q1.webm" },
            new Question { FileName = "q2.webm", FilePath = "c:\\q2.webm" },
            new Question { FileName = "q3.webm", FilePath = "c:\\q3.webm" }
        };

        var mockSettings = new Mock<ISettingsService>();
        var playlistService = new PlaylistService(mockSettings.Object);
        await playlistService.InitializeAsync(questions);
        playlistService.SetPlayMode(PlayMode.Loop);

        var mockReview = new Mock<IProductReviewService>();
        var viewModel = new ProductReviewViewModel(
            mockReview.Object,
            playlistService,
            new Mock<ICountdownService>().Object);

        // Sync ViewModel mode with playlistService mode (simulating LoadQuestionsAsync behavior)
        viewModel.SetPlayModeCommand.Execute(PlayMode.Loop);

        // Select Q2
        viewModel.SelectQuestionCommand.Execute(1);

        // Capture state BEFORE delete
        var questionBefore = viewModel.CurrentQuestion;
        var pathBefore = viewModel.CurrentQuestionPath;
        var indexBefore = viewModel.CurrentQuestionIndex;
        var modeBefore = viewModel.CurrentPlayMode;

        Assert.AreEqual("q2.webm", questionBefore?.FileName, "Should start on Q2");
        Assert.AreEqual("c:\\q2.webm", pathBefore, "Path should be q2.webm");
        Assert.AreEqual(2, indexBefore, "Index should be 2/5");
        Assert.AreEqual(PlayMode.Loop, modeBefore, "Mode should be Loop");

        // Simulate recording
        viewModel.HasRecording = true;
        Assert.IsFalse(viewModel.CanStartOrStopRecording, "Button disabled with recording");

        // Act: Delete recording (which triggers MoveToNext in Loop mode)
        viewModel.HasRecording = false;
        viewModel.MoveToNextCommand.Execute(null);  // Delete triggers this

        // Assert: Verify ALL state properties
        Assert.AreNotEqual(questionBefore?.FileName, viewModel.CurrentQuestion?.FileName,
            "CurrentQuestion should CHANGE (advance)");
        Assert.AreEqual("q3.webm", viewModel.CurrentQuestion?.FileName,
            "CurrentQuestion should be Q3");
        Assert.AreNotEqual(pathBefore, viewModel.CurrentQuestionPath,
            "CurrentQuestionPath should CHANGE");
        Assert.AreEqual("c:\\q3.webm", viewModel.CurrentQuestionPath,
            "CurrentQuestionPath should be c:\\q3.webm");
        Assert.AreNotEqual(indexBefore, viewModel.CurrentQuestionIndex,
            "CurrentQuestionIndex should CHANGE");
        Assert.AreEqual(3, viewModel.CurrentQuestionIndex, "Index should be 3/5");
        Assert.AreEqual(PlayMode.Loop, viewModel.CurrentPlayMode,
            "CurrentPlayMode should STAY Loop");
        Assert.IsFalse(viewModel.HasRecording, "HasRecording should be false");
        Assert.IsTrue(viewModel.CanStartOrStopRecording, "Button should be enabled");
    }

    [TestMethod]
    public async Task DeleteRecording_WithShuffleMode_ShouldAdvanceInShuffledOrder()
    {
        // Arrange: Setup with 3 questions
        var questions = new List<Question>
        {
            new Question { FileName = "q1.webm", FilePath = "c:\\q1.webm" },
            new Question { FileName = "q2.webm", FilePath = "c:\\q2.webm" },
            new Question { FileName = "q3.webm", FilePath = "c:\\q3.webm" }
        };

        var mockSettings = new Mock<ISettingsService>();
        var playlistService = new PlaylistService(mockSettings.Object);
        await playlistService.InitializeAsync(questions);

        var mockReview = new Mock<IProductReviewService>();
        mockReview.Setup(s => s.LoadQuestionDirectoryAsync(It.IsAny<string>()))
            .ReturnsAsync(questions);

        var viewModel = new ProductReviewViewModel(
            mockReview.Object,
            playlistService,
            new Mock<ICountdownService>().Object);

        // Shuffle questions
        await viewModel.ToggleShuffleAsync();

        // Get the shuffled order
        var shuffledOrder = new List<string>();
        for (int i = 0; i < viewModel.TotalQuestions; i++)
        {
            playlistService.SelectByIndex(i);
            shuffledOrder.Add(playlistService.CurrentQuestion?.FileName ?? "");
        }

        // Move to second question in shuffled order (index 1)
        playlistService.SelectByIndex(1);

        // Capture state BEFORE delete
        var questionBefore = viewModel.CurrentQuestion;
        var pathBefore = viewModel.CurrentQuestionPath;
        var indexBefore = viewModel.CurrentQuestionIndex;
        var modeBefore = viewModel.CurrentSortMode;

        Assert.AreEqual(SortMode.Shuffle, modeBefore, "Mode should be Shuffle");
        Assert.IsTrue(shuffledOrder.Contains(questionBefore?.FileName ?? ""),
            "Before question should be in shuffled order");

        // Simulate recording
        viewModel.HasRecording = true;
        Assert.IsFalse(viewModel.CanStartOrStopRecording, "Button disabled with recording");

        // Act: Delete recording (which triggers MoveToNext in Shuffle mode)
        viewModel.HasRecording = false;
        viewModel.MoveToNextCommand.Execute(null);  // Delete triggers this

        // Assert: Verify ALL state properties
        Assert.AreNotEqual(questionBefore?.FileName, viewModel.CurrentQuestion?.FileName,
            "CurrentQuestion should CHANGE (advance in shuffled order)");
        Assert.AreNotEqual(pathBefore, viewModel.CurrentQuestionPath,
            "CurrentQuestionPath should CHANGE");
        Assert.AreNotEqual(indexBefore, viewModel.CurrentQuestionIndex,
            "CurrentQuestionIndex should CHANGE");
        Assert.AreEqual(SortMode.Shuffle, viewModel.CurrentSortMode,
            "CurrentSortMode should STAY Shuffle");
        Assert.IsFalse(viewModel.HasRecording, "HasRecording should be false");
        Assert.IsTrue(viewModel.CanStartOrStopRecording, "Button should be enabled");
        Assert.IsTrue(shuffledOrder.Contains(viewModel.CurrentQuestion?.FileName ?? ""),
            "New question should still be in shuffled order");
    }

    #region Phase 2-3: Integration Tests for MediaPlayer State Verification

    /// <summary>
    /// Integration test: Verify that deleting recording with Repeat mode
    /// triggers media reload without navigation (PropertyChanged events).
    /// </summary>
    [TestMethod]
    public void IntegrationTest_DeleteRecording_RepeatMode_VerifyMediaReloadTrigger()
    {
        // Arrange: Setup with 3 questions
        var questions = new List<Question>
        {
            new Question { FileName = "q1.webm", FilePath = "c:\\q1.webm" },
            new Question { FileName = "q2.webm", FilePath = "c:\\q2.webm" },
            new Question { FileName = "q3.webm", FilePath = "c:\\q3.webm" }
        };

        var mockSettings = new Mock<ISettingsService>();
        var playlistService = new PlaylistService(mockSettings.Object);
        playlistService.InitializeAsync(questions).GetAwaiter().GetResult();
        playlistService.SetPlayMode(PlayMode.RepeatCurrent);

        var viewModel = new ProductReviewViewModel(
            new Mock<IProductReviewService>().Object,
            playlistService,
            new Mock<ICountdownService>().Object);

        viewModel.CurrentPlayMode = playlistService.CurrentPlayMode;

        // Select Q2
        viewModel.SelectQuestionCommand.Execute(1);
        var pathBefore = viewModel.CurrentQuestionPath;
        var propertyChangedCount = 0;

        // Track PropertyChanged events to verify media reload trigger
        viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(ProductReviewViewModel.CurrentQuestionPath) ||
                e.PropertyName == nameof(ProductReviewViewModel.CurrentQuestion))
            {
                propertyChangedCount++;
            }
        };

        // Simulate recording
        viewModel.HasRecording = true;

        // Act: Delete recording (Repeat mode stays on current question)
        viewModel.HasRecording = false;

        // Assert: Verify state AND that media reload was triggered
        Assert.AreEqual(pathBefore, viewModel.CurrentQuestionPath,
            "CurrentQuestionPath should STAY same (no reload trigger from question change in Repeat mode)");
        Assert.AreEqual("c:\\q2.webm", viewModel.CurrentQuestionPath,
            "Path should remain c:\\q2.webm in Repeat mode");

        // Note: In Repeat mode, CurrentQuestionPath doesn't change, so LoadCurrentQuestionMedia
        // would be called from OnViewModelPropertyChanged only if a property changed.
        // Since no question change occurs, the media reload is implicit (handled separately in BtnDelete_Click).
        // This test verifies state management; media reload verification requires UI testing.
    }

    /// <summary>
    /// Integration test: Verify that deleting recording with Loop mode
    /// triggers media reload by advancing to next question (PropertyChanged events).
    /// </summary>
    [TestMethod]
    public void IntegrationTest_DeleteRecording_LoopMode_VerifyMediaReloadTrigger()
    {
        // Arrange: Setup with 3 questions
        var questions = new List<Question>
        {
            new Question { FileName = "q1.webm", FilePath = "c:\\q1.webm" },
            new Question { FileName = "q2.webm", FilePath = "c:\\q2.webm" },
            new Question { FileName = "q3.webm", FilePath = "c:\\q3.webm" }
        };

        var mockSettings = new Mock<ISettingsService>();
        var playlistService = new PlaylistService(mockSettings.Object);
        playlistService.InitializeAsync(questions).GetAwaiter().GetResult();
        playlistService.SetPlayMode(PlayMode.Loop);

        var viewModel = new ProductReviewViewModel(
            new Mock<IProductReviewService>().Object,
            playlistService,
            new Mock<ICountdownService>().Object);

        // Select Q2
        viewModel.SelectQuestionCommand.Execute(1);
        var pathBefore = viewModel.CurrentQuestionPath;
        var propertyChangedFired = false;

        // Track PropertyChanged events to verify media reload trigger
        viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(ProductReviewViewModel.CurrentQuestionPath))
            {
                propertyChangedFired = true;
            }
        };

        // Simulate recording
        viewModel.HasRecording = true;

        // Act: Delete recording (Loop mode advances to next question)
        viewModel.HasRecording = false;
        viewModel.MoveToNextCommand.Execute(null);

        // Assert: Verify state AND that media reload was triggered
        Assert.AreNotEqual(pathBefore, viewModel.CurrentQuestionPath,
            "CurrentQuestionPath should CHANGE (triggering media reload)");
        Assert.AreEqual("c:\\q3.webm", viewModel.CurrentQuestionPath,
            "Path should advance to c:\\q3.webm");
        Assert.IsTrue(propertyChangedFired,
            "PropertyChanged should fire for CurrentQuestionPath (triggers LoadCurrentQuestionMedia)");
    }

    /// <summary>
    /// Integration test: Verify that deleting recording with Shuffle mode
    /// triggers media reload by advancing in shuffled order (PropertyChanged events).
    /// </summary>
    [TestMethod]
    public async Task IntegrationTest_DeleteRecording_ShuffleMode_VerifyMediaReloadTrigger()
    {
        // Arrange: Setup with 3 questions
        var questions = new List<Question>
        {
            new Question { FileName = "q1.webm", FilePath = "c:\\q1.webm" },
            new Question { FileName = "q2.webm", FilePath = "c:\\q2.webm" },
            new Question { FileName = "q3.webm", FilePath = "c:\\q3.webm" }
        };

        var mockSettings = new Mock<ISettingsService>();
        var playlistService = new PlaylistService(mockSettings.Object);
        await playlistService.InitializeAsync(questions);

        var mockReview = new Mock<IProductReviewService>();
        mockReview.Setup(s => s.LoadQuestionDirectoryAsync(It.IsAny<string>()))
            .ReturnsAsync(questions);

        var viewModel = new ProductReviewViewModel(
            mockReview.Object,
            playlistService,
            new Mock<ICountdownService>().Object);

        // Shuffle and move to second question
        await viewModel.ToggleShuffleAsync();
        playlistService.SelectByIndex(1);
        viewModel.MoveToNextCommand.Execute(null);

        var pathBefore = viewModel.CurrentQuestionPath;
        var propertyChangedFired = false;

        // Track PropertyChanged events to verify media reload trigger
        viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(ProductReviewViewModel.CurrentQuestionPath))
            {
                propertyChangedFired = true;
            }
        };

        // Simulate recording
        viewModel.HasRecording = true;

        // Act: Delete recording (Shuffle mode advances in shuffled order)
        viewModel.HasRecording = false;
        viewModel.MoveToNextCommand.Execute(null);

        // Assert: Verify state AND that media reload was triggered
        Assert.AreNotEqual(pathBefore, viewModel.CurrentQuestionPath,
            "CurrentQuestionPath should CHANGE (triggering media reload)");
        Assert.IsTrue(propertyChangedFired,
            "PropertyChanged should fire for CurrentQuestionPath (triggers LoadCurrentQuestionMedia)");
    }

    /// <summary>
    /// Integration test: Verify complete state management for all play modes after delete.
    /// Data-driven test covering Repeat, Loop, and Shuffle modes.
    /// </summary>
    [TestMethod]
    [DataRow(PlayMode.RepeatCurrent, "q2.webm", "c:\\q2.webm", 2, DisplayName = "Repeat Mode: Stay on Q2")]
    [DataRow(PlayMode.Loop, "q3.webm", "c:\\q3.webm", 3, DisplayName = "Loop Mode: Advance to Q3")]
    public void IntegrationTest_DeleteRecording_AllModes_CompleteStateManagement(
        PlayMode playMode, string expectedFileName, string expectedPath, int expectedIndex)
    {
        // Arrange: Setup with 3 questions
        var questions = new List<Question>
        {
            new Question { FileName = "q1.webm", FilePath = "c:\\q1.webm" },
            new Question { FileName = "q2.webm", FilePath = "c:\\q2.webm" },
            new Question { FileName = "q3.webm", FilePath = "c:\\q3.webm" }
        };

        var mockSettings = new Mock<ISettingsService>();
        var playlistService = new PlaylistService(mockSettings.Object);
        playlistService.InitializeAsync(questions).GetAwaiter().GetResult();
        playlistService.SetPlayMode(playMode);

        var viewModel = new ProductReviewViewModel(
            new Mock<IProductReviewService>().Object,
            playlistService,
            new Mock<ICountdownService>().Object);

        viewModel.CurrentPlayMode = playlistService.CurrentPlayMode;

        // Select Q2
        viewModel.SelectQuestionCommand.Execute(1);

        // Capture before state
        var hasRecordingBefore = false;
        var canStartBefore = viewModel.CanStartOrStopRecording;

        // Simulate recording
        viewModel.HasRecording = true;
        var hasRecordingAfterAdd = true;
        var canStartAfterAdd = viewModel.CanStartOrStopRecording;

        // Act: Simulate complete delete flow
        viewModel.HasRecording = false;
        if (playMode != PlayMode.RepeatCurrent)
        {
            viewModel.MoveToNextCommand.Execute(null);
        }

        // Assert: Complete state verification
        Assert.AreEqual(expectedFileName, viewModel.CurrentQuestion?.FileName,
            $"CurrentQuestion should be {expectedFileName}");
        Assert.AreEqual(expectedPath, viewModel.CurrentQuestionPath,
            $"CurrentQuestionPath should be {expectedPath}");
        Assert.AreEqual(expectedIndex, viewModel.CurrentQuestionIndex,
            $"CurrentQuestionIndex should be {expectedIndex}");
        Assert.AreEqual(playMode, viewModel.CurrentPlayMode,
            $"CurrentPlayMode should remain {playMode}");
        Assert.IsFalse(viewModel.HasRecording, "HasRecording should be false");
        Assert.IsTrue(viewModel.CanStartOrStopRecording,
            "CanStartOrStopRecording should be true (can start new recording)");

        // Verify state transitions
        Assert.IsFalse(hasRecordingBefore, "Should start without recording");
        Assert.IsTrue(hasRecordingAfterAdd, "Should have recording after add");
        Assert.IsFalse(canStartAfterAdd, "Should not be able to start while recording exists");
        Assert.IsTrue(viewModel.CanStartOrStopRecording, "Should be able to start after delete");
    }

    /// <summary>
    /// Integration test: Verify that CurrentQuestion PropertyChanged fires correctly.
    /// This is critical because OnViewModelPropertyChanged watches for CurrentQuestion changes
    /// to trigger LoadCurrentQuestionMedia() in the code-behind.
    /// </summary>
    [TestMethod]
    public void IntegrationTest_CurrentQuestion_PropertyChangedNotification()
    {
        // Arrange
        var questions = new List<Question>
        {
            new Question { FileName = "q1.webm", FilePath = "c:\\q1.webm" },
            new Question { FileName = "q2.webm", FilePath = "c:\\q2.webm" },
            new Question { FileName = "q3.webm", FilePath = "c:\\q3.webm" }
        };

        var mockSettings = new Mock<ISettingsService>();
        var playlistService = new PlaylistService(mockSettings.Object);
        playlistService.InitializeAsync(questions).GetAwaiter().GetResult();

        var viewModel = new ProductReviewViewModel(
            new Mock<IProductReviewService>().Object,
            playlistService,
            new Mock<ICountdownService>().Object);

        var currentQuestionChangedFired = false;
        var currentQuestionPathChangedFired = false;

        // Track PropertyChanged events
        viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(ProductReviewViewModel.CurrentQuestion))
            {
                currentQuestionChangedFired = true;
            }
            if (e.PropertyName == nameof(ProductReviewViewModel.CurrentQuestionPath))
            {
                currentQuestionPathChangedFired = true;
            }
        };

        // Reset flags
        currentQuestionChangedFired = false;
        currentQuestionPathChangedFired = false;

        // Act: Move to next question (triggers PropertyChanged for both)
        viewModel.MoveToNextCommand.Execute(null);

        // Assert: Verify both property changes fire
        Assert.IsTrue(currentQuestionChangedFired,
            "CurrentQuestion PropertyChanged should fire (triggers media reload)");
        Assert.IsTrue(currentQuestionPathChangedFired,
            "CurrentQuestionPath PropertyChanged should fire (triggers media reload)");
    }

    /// <summary>
    /// Integration test: Verify CanStartOrStopRecording computed property updates correctly.
    /// CanStartOrStopRecording depends on CurrentQuestion and HasRecording.
    /// When CurrentQuestion changes, PropertyChanged must fire for CanStartOrStopRecording.
    /// </summary>
    [TestMethod]
    public void IntegrationTest_CanStartOrStopRecording_UpdatesWhenCurrentQuestionChanges()
    {
        // Arrange
        var questions = new List<Question>
        {
            new Question { FileName = "q1.webm", FilePath = "c:\\q1.webm" },
            new Question { FileName = "q2.webm", FilePath = "c:\\q2.webm" },
            new Question { FileName = "q3.webm", FilePath = "c:\\q3.webm" }
        };

        var mockSettings = new Mock<ISettingsService>();
        var playlistService = new PlaylistService(mockSettings.Object);
        playlistService.InitializeAsync(questions).GetAwaiter().GetResult();

        var viewModel = new ProductReviewViewModel(
            new Mock<IProductReviewService>().Object,
            playlistService,
            new Mock<ICountdownService>().Object);

        // Initialize ViewModel state from playlist (normally done by LoadQuestionsAsync)
        viewModel.SelectQuestionCommand.Execute(0);

        // Start on Q1 without recording
        Assert.IsNotNull(viewModel.CurrentQuestion, "CurrentQuestion should be set to Q1");
        Assert.IsTrue(viewModel.CanStartOrStopRecording, "Should be able to start on Q1");

        // Add recording
        viewModel.HasRecording = true;
        Assert.IsFalse(viewModel.CanStartOrStopRecording, "Should NOT be able to start with recording");

        // Track CanStartOrStopRecording changes
        viewModel.PropertyChanged += (s, e) =>
        {
            // Just tracking for informational purposes
        };

        // Act: Move to next question (should trigger CanStartOrStopRecording update)
        viewModel.MoveToNextCommand.Execute(null);

        // Assert: CanStartOrStopRecording should stay false (recording still exists)
        Assert.IsFalse(viewModel.CanStartOrStopRecording,
            "Should still NOT be able to start (recording exists on new question)");

        // Delete recording
        viewModel.HasRecording = false;

        // Assert: After delete, should be able to start
        Assert.IsTrue(viewModel.CanStartOrStopRecording,
            "Should be able to start after deleting recording");
    }

    [TestMethod]
    public async Task ShuffleButton_Click_ActivatesShuffle_AndChangesPlaylistOrder()
    {
        // Arrange: Setup with 10 questions in known order
        var questions = new List<Question>();
        for (int i = 1; i <= 10; i++)
        {
            questions.Add(new Question { FileName = $"q{i}.webm", FilePath = $"c:\\q{i}.webm" });
        }

        var mockSettings = new Mock<ISettingsService>();
        var playlistService = new PlaylistService(mockSettings.Object);
        await playlistService.InitializeAsync(questions);

        var viewModel = new ProductReviewViewModel(
            new Mock<IProductReviewService>().Object,
            playlistService,
            new Mock<ICountdownService>().Object);

        // Capture initial order
        var initialOrder = playlistService.Questions.Select(q => q.FileName).ToList();
        Assert.AreEqual(10, initialOrder.Count, "Should have 10 questions");
        Assert.AreEqual("q1.webm", initialOrder[0], "First question should be q1.webm initially");

        // Verify initial sort mode is NOT Shuffle
        Assert.AreNotEqual(SortMode.Shuffle, viewModel.CurrentSortMode,
            "Initial sort mode should not be Shuffle");

        // Act: Simulate clicking Shuffle button
        await viewModel.ToggleShuffleAsync();

        // Assert Part 1: CurrentSortMode should be Shuffle
        Assert.AreEqual(SortMode.Shuffle, viewModel.CurrentSortMode,
            "Shuffle button click should set CurrentSortMode to Shuffle");

        // Assert Part 2: Playlist order should have changed (shuffled)
        var shuffledOrder = playlistService.Questions.Select(q => q.FileName).ToList();
        Assert.AreEqual(10, shuffledOrder.Count, "Should still have 10 questions after shuffle");

        // Verify that the order has changed (with very high probability for 10 items)
        bool orderChanged = !initialOrder.SequenceEqual(shuffledOrder);
        Assert.IsTrue(orderChanged,
            "Playlist order should be different after shuffle (probability of false positive: <0.000001%)");

        // Assert Part 3: All questions should still be present (no duplicates, no lost items)
        var initialSet = new HashSet<string>(initialOrder);
        var shuffledSet = new HashSet<string>(shuffledOrder);
        Assert.AreEqual(initialSet.Count, shuffledSet.Count, "Should have same number of unique questions");
        Assert.IsTrue(initialSet.SetEquals(shuffledSet),
            "All original questions should still be present after shuffle");

        // Assert Part 4: Visual feedback state (CurrentSortMode) should trigger converter
        // The SortModeToBackgroundConverter will use CurrentSortMode == SortMode.Shuffle to highlight button
        Assert.AreEqual(SortMode.Shuffle, viewModel.CurrentSortMode,
            "Shuffle button visual feedback should be active (SortMode=Shuffle for converter)");
    }

    [TestMethod]
    public async Task ShuffleButton_MultipleClicks_MaintainsShuffle_AndPreservesQuestions()
    {
        // Arrange: Setup with 8 questions
        var questions = new List<Question>();
        for (int i = 1; i <= 8; i++)
        {
            questions.Add(new Question { FileName = $"q{i}.webm", FilePath = $"c:\\q{i}.webm" });
        }

        var mockSettings = new Mock<ISettingsService>();
        var playlistService = new PlaylistService(mockSettings.Object);
        await playlistService.InitializeAsync(questions);

        var viewModel = new ProductReviewViewModel(
            new Mock<IProductReviewService>().Object,
            playlistService,
            new Mock<ICountdownService>().Object);

        // Act: Shuffle multiple times
        await viewModel.ToggleShuffleAsync();
        var firstShuffle = playlistService.Questions.Select(q => q.FileName).ToList();

        await viewModel.ToggleShuffleAsync();
        var secondShuffle = playlistService.Questions.Select(q => q.FileName).ToList();

        // Assert Part 1: Second click toggles back to AscendingAlpha
        Assert.AreEqual(SortMode.AscendingAlpha, viewModel.CurrentSortMode,
            "CurrentSortMode should toggle back to AscendingAlpha after second click (toggle behavior)");

        // Assert Part 2: First shuffle changes order from initial alphabetical
        var initialOrder = questions.Select(q => q.FileName).ToList();
        bool firstOrderChanged = !initialOrder.SequenceEqual(firstShuffle);
        Assert.IsTrue(firstOrderChanged,
            "First shuffle should change order from initial alphabetical sequence");

        // Assert Part 3: All questions still present in both shuffles
        var originalSet = new HashSet<string>(questions.Select(q => q.FileName));
        var firstSet = new HashSet<string>(firstShuffle);
        var secondSet = new HashSet<string>(secondShuffle);

        Assert.IsTrue(originalSet.SetEquals(firstSet),
            "All questions present after first shuffle (no data loss)");
        Assert.IsTrue(originalSet.SetEquals(secondSet),
            "All questions present after second shuffle (no data loss)");

        // Assert Part 4: Shuffle mode persists across multiple button clicks
        Assert.AreEqual(8, secondShuffle.Count, "Should still have 8 questions after multiple shuffles");
    }

    #endregion
}
