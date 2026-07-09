// Copyright (c) YourProjectName. All rights reserved.

using IntVue.Models;
using IntVue.Services;

namespace IntVue.Tests.Services;

/// <summary>
/// Unit tests for PlaylistService.
/// </summary>
[TestClass]
public class PlaylistServiceTests
{
    /// <summary>
    /// Creates a mock settings service for testing.
    /// </summary>
    private static ISettingsService CreateMockSettingsService()
    {
        var settings = new System.Collections.Generic.Dictionary<string, object?>();
        var mockSettings = new Moq.Mock<ISettingsService>();

        mockSettings
            .Setup(s => s.GetSetting(Moq.It.IsAny<string>()))
            .Returns<string>(key => settings.TryGetValue(key, out var value) ? value : null);

        mockSettings
            .Setup(s => s.SetSettingAsync(Moq.It.IsAny<string>(), Moq.It.IsAny<object?>()))
            .Returns<string, object?>((key, value) =>
            {
                settings[key] = value;
                return System.Threading.Tasks.Task.CompletedTask;
            });

        return mockSettings.Object;
    }

    /// <summary>
    /// Test that InitializeAsync populates the playlist with questions.
    /// </summary>
    [TestMethod]
    public async Task InitializeAsync_WithQuestions_PopulatesPlaylist()
    {
        // Arrange
        var service = new PlaylistService(CreateMockSettingsService());
        var questions = new List<Question>
        {
            new() { FileName = "q1.webm", FilePath = "/q1.webm" },
            new() { FileName = "q2.webm", FilePath = "/q2.webm" },
            new() { FileName = "q3.webm", FilePath = "/q3.webm" },
        };

        // Act
        await service.InitializeAsync(questions);

        // Assert
        Assert.AreEqual(3, service.TotalCount);
        Assert.IsNotNull(service.CurrentQuestion);
        Assert.AreEqual(0, service.CurrentIndex);
    }

    /// <summary>
    /// Test that InitializeAsync throws ArgumentNullException for null questions.
    /// </summary>
    [TestMethod]
    public async Task InitializeAsync_NullQuestions_ThrowsArgumentNullException()
    {
        // Arrange
        var service = new PlaylistService(CreateMockSettingsService());
        var exceptionThrown = false;

        try
        {
            // Act
            await service.InitializeAsync(null!);
        }
        catch (ArgumentNullException)
        {
            exceptionThrown = true;
        }

        // Assert
        Assert.IsTrue(exceptionThrown);
    }

    /// <summary>
    /// Test that InitializeAsync sets current index to -1 for empty list.
    /// </summary>
    [TestMethod]
    public async Task InitializeAsync_EmptyQuestions_SetCurrentIndexToNegative()
    {
        // Arrange
        var service = new PlaylistService(CreateMockSettingsService());

        // Act
        await service.InitializeAsync(new List<Question>());

        // Assert
        Assert.AreEqual(0, service.TotalCount);
        Assert.AreEqual(-1, service.CurrentIndex);
        Assert.IsNull(service.CurrentQuestion);
    }

    /// <summary>
    /// Test MoveToNext in Sequential mode stops at end.
    /// </summary>
    [TestMethod]
    public async Task MoveToNext_Sequential_StopsAtEnd()
    {
        // Arrange
        var service = new PlaylistService(CreateMockSettingsService());
        var questions = new List<Question>
        {
            new() { FileName = "q1.webm", FilePath = "/q1.webm" },
            new() { FileName = "q2.webm", FilePath = "/q2.webm" },
        };
        await service.InitializeAsync(questions);
        service.SetPlayMode(PlayMode.Sequential);

        // Act
        service.MoveToNext(); // Move from 0 to 1
        var result = service.MoveToNext(); // Attempt to move past end

        // Assert
        Assert.IsNull(result);
        Assert.AreEqual(1, service.CurrentIndex);
    }

    /// <summary>
    /// Test MoveToNext in Loop mode wraps to beginning.
    /// </summary>
    [TestMethod]
    public async Task MoveToNext_Loop_WrapsToBeginning()
    {
        // Arrange
        var service = new PlaylistService(CreateMockSettingsService());
        var questions = new List<Question>
        {
            new() { FileName = "q1.webm", FilePath = "/q1.webm" },
            new() { FileName = "q2.webm", FilePath = "/q2.webm" },
        };
        await service.InitializeAsync(questions);
        service.SetPlayMode(PlayMode.Loop);

        // Act
        service.MoveToNext(); // Move from 0 to 1
        service.MoveToNext(); // Wrap from 1 to 0

        // Assert
        Assert.AreEqual(0, service.CurrentIndex);
        Assert.AreEqual("q1.webm", service.CurrentQuestion?.FileName);
    }

    /// <summary>
    /// Test MoveToNext in RepeatCurrent mode returns same question.
    /// </summary>
    [TestMethod]
    public async Task MoveToNext_RepeatCurrent_ReturnsSameQuestion()
    {
        // Arrange
        var service = new PlaylistService(CreateMockSettingsService());
        var questions = new List<Question>
        {
            new() { FileName = "q1.webm", FilePath = "/q1.webm" },
            new() { FileName = "q2.webm", FilePath = "/q2.webm" },
        };
        await service.InitializeAsync(questions);
        service.SetPlayMode(PlayMode.RepeatCurrent);

        // Act
        var result = service.MoveToNext();

        // Assert
        Assert.AreEqual(0, service.CurrentIndex);
        Assert.AreEqual("q1.webm", result?.FileName);
    }

    /// <summary>
    /// Test MoveToPrevious returns previous question.
    /// </summary>
    [TestMethod]
    public async Task MoveToPrevious_InMiddle_ReturnsPreviousQuestion()
    {
        // Arrange
        var service = new PlaylistService(CreateMockSettingsService());
        var questions = new List<Question>
        {
            new() { FileName = "q1.webm", FilePath = "/q1.webm" },
            new() { FileName = "q2.webm", FilePath = "/q2.webm" },
            new() { FileName = "q3.webm", FilePath = "/q3.webm" },
        };
        await service.InitializeAsync(questions);
        service.SelectByIndex(2);

        // Act
        var result = service.MoveToPrevious();

        // Assert
        Assert.AreEqual(1, service.CurrentIndex);
        Assert.AreEqual("q2.webm", result?.FileName);
    }

    /// <summary>
    /// Test MoveToPrevious at beginning returns null.
    /// </summary>
    [TestMethod]
    public async Task MoveToPrevious_AtBeginning_ReturnsNull()
    {
        // Arrange
        var service = new PlaylistService(CreateMockSettingsService());
        var questions = new List<Question>
        {
            new() { FileName = "q1.webm", FilePath = "/q1.webm" },
        };
        await service.InitializeAsync(questions);

        // Act
        var result = service.MoveToPrevious();

        // Assert
        Assert.IsNull(result);
        Assert.AreEqual(0, service.CurrentIndex);
    }

    /// <summary>
    /// Test SelectByIndex selects question at index.
    /// </summary>
    [TestMethod]
    public async Task SelectByIndex_ValidIndex_SelectsQuestion()
    {
        // Arrange
        var service = new PlaylistService(CreateMockSettingsService());
        var questions = new List<Question>
        {
            new() { FileName = "q1.webm", FilePath = "/q1.webm" },
            new() { FileName = "q2.webm", FilePath = "/q2.webm" },
            new() { FileName = "q3.webm", FilePath = "/q3.webm" },
        };
        await service.InitializeAsync(questions);

        // Act
        var result = service.SelectByIndex(2);

        // Assert
        Assert.AreEqual(2, service.CurrentIndex);
        Assert.AreEqual("q3.webm", result?.FileName);
    }

    /// <summary>
    /// Test SelectByIndex with invalid index returns null.
    /// </summary>
    [TestMethod]
    public async Task SelectByIndex_InvalidIndex_ReturnsNull()
    {
        // Arrange
        var service = new PlaylistService(CreateMockSettingsService());
        var questions = new List<Question>
        {
            new() { FileName = "q1.webm", FilePath = "/q1.webm" },
        };
        await service.InitializeAsync(questions);

        // Act
        var result = service.SelectByIndex(5);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Test ApplySortAsync with AscendingAlpha sorts alphabetically.
    /// </summary>
    [TestMethod]
    public async Task ApplySortAsync_AscendingAlpha_SortsAlphabetically()
    {
        // Arrange
        var service = new PlaylistService(CreateMockSettingsService());
        var questions = new List<Question>
        {
            new() { FileName = "z_question.webm", FilePath = "/z.webm" },
            new() { FileName = "a_question.webm", FilePath = "/a.webm" },
            new() { FileName = "m_question.webm", FilePath = "/m.webm" },
        };
        await service.InitializeAsync(questions);

        // Act
        await service.ApplySortAsync(SortMode.AscendingAlpha);

        // Assert
        Assert.AreEqual("a_question.webm", service.Questions[0].FileName);
        Assert.AreEqual("m_question.webm", service.Questions[1].FileName);
        Assert.AreEqual("z_question.webm", service.Questions[2].FileName);
    }

    /// <summary>
    /// Test ApplySortAsync with DescendingAlpha sorts reverse alphabetically.
    /// </summary>
    [TestMethod]
    public async Task ApplySortAsync_DescendingAlpha_SortsReverseAlphabetically()
    {
        // Arrange
        var service = new PlaylistService(CreateMockSettingsService());
        var questions = new List<Question>
        {
            new() { FileName = "a_question.webm", FilePath = "/a.webm" },
            new() { FileName = "z_question.webm", FilePath = "/z.webm" },
            new() { FileName = "m_question.webm", FilePath = "/m.webm" },
        };
        await service.InitializeAsync(questions);

        // Act
        await service.ApplySortAsync(SortMode.DescendingAlpha);

        // Assert
        Assert.AreEqual("z_question.webm", service.Questions[0].FileName);
        Assert.AreEqual("m_question.webm", service.Questions[1].FileName);
        Assert.AreEqual("a_question.webm", service.Questions[2].FileName);
    }

    /// <summary>
    /// Test ApplySortAsync with Shuffle randomizes order.
    /// </summary>
    [TestMethod]
    public async Task ApplySortAsync_Shuffle_RandomizesOrder()
    {
        // Arrange
        var service = new PlaylistService(CreateMockSettingsService());
        var questions = new List<Question>
        {
            new() { FileName = "q1.webm", FilePath = "/q1.webm" },
            new() { FileName = "q2.webm", FilePath = "/q2.webm" },
            new() { FileName = "q3.webm", FilePath = "/q3.webm" },
            new() { FileName = "q4.webm", FilePath = "/q4.webm" },
            new() { FileName = "q5.webm", FilePath = "/q5.webm" },
        };
        await service.InitializeAsync(questions);
        var originalOrder = questions.Select(q => q.FileName).ToList();

        // Act
        await service.ApplySortAsync(SortMode.Shuffle);

        // Assert - all questions should still be present
        Assert.AreEqual(5, service.TotalCount);
        foreach (var question in originalOrder)
        {
            Assert.IsTrue(service.Questions.Any(q => q.FileName == question));
        }
    }

    /// <summary>
    /// Test ApplySortAsync resets current index to 0.
    /// </summary>
    [TestMethod]
    public async Task ApplySortAsync_ResetsCurrentIndex()
    {
        // Arrange
        var service = new PlaylistService(CreateMockSettingsService());
        var questions = new List<Question>
        {
            new() { FileName = "q1.webm", FilePath = "/q1.webm" },
            new() { FileName = "q2.webm", FilePath = "/q2.webm" },
        };
        await service.InitializeAsync(questions);
        service.SelectByIndex(1);

        // Act
        await service.ApplySortAsync(SortMode.DescendingAlpha);

        // Assert
        Assert.AreEqual(0, service.CurrentIndex);
    }

    /// <summary>
    /// Test SetPlayMode changes the play mode.
    /// </summary>
    [TestMethod]
    public void SetPlayMode_ChangesMode()
    {
        // Arrange
        var service = new PlaylistService(CreateMockSettingsService());

        // Act
        service.SetPlayMode(PlayMode.Loop);

        // Assert
        Assert.AreEqual(PlayMode.Loop, service.CurrentPlayMode);
    }

    /// <summary>
    /// Test Clear removes all questions from playlist.
    /// </summary>
    [TestMethod]
    public async Task Clear_RemovesAllQuestions()
    {
        // Arrange
        var service = new PlaylistService(CreateMockSettingsService());
        var questions = new List<Question>
        {
            new() { FileName = "q1.webm", FilePath = "/q1.webm" },
            new() { FileName = "q2.webm", FilePath = "/q2.webm" },
        };
        await service.InitializeAsync(questions);

        // Act
        service.Clear();

        // Assert
        Assert.AreEqual(0, service.TotalCount);
        Assert.AreEqual(-1, service.CurrentIndex);
        Assert.IsNull(service.CurrentQuestion);
    }

    /// <summary>
    /// Test CurrentSortMode reflects applied sort mode.
    /// </summary>
    [TestMethod]
    public async Task CurrentSortMode_ReflectsAppliedMode()
    {
        // Arrange
        var service = new PlaylistService(CreateMockSettingsService());
        var questions = new List<Question>
        {
            new() { FileName = "q1.webm", FilePath = "/q1.webm" },
        };
        await service.InitializeAsync(questions);

        // Act
        await service.ApplySortAsync(SortMode.Shuffle);

        // Assert
        Assert.AreEqual(SortMode.Shuffle, service.CurrentSortMode);
    }
}
