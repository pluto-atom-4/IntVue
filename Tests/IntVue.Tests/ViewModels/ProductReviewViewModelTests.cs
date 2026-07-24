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
}
