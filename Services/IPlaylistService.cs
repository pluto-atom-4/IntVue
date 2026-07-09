// Copyright (c) YourProjectName. All rights reserved.

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using IntVue.Models;

namespace IntVue.Services;

/// <summary>
/// Service for managing a playlist of interview questions with sorting, navigation, and persistence.
/// </summary>
public interface IPlaylistService
{
    /// <summary>
    /// Gets the current playlist of questions.
    /// </summary>
    ObservableCollection<Question> Questions { get; }

    /// <summary>
    /// Gets the currently selected question, or null if playlist is empty.
    /// </summary>
    Question? CurrentQuestion { get; }

    /// <summary>
    /// Gets the index of the current question in the playlist.
    /// </summary>
    int CurrentIndex { get; }

    /// <summary>
    /// Gets the total number of questions in the playlist.
    /// </summary>
    int TotalCount { get; }

    /// <summary>
    /// Gets the current sort mode (persisted in LocalSettings).
    /// </summary>
    SortMode CurrentSortMode { get; }

    /// <summary>
    /// Gets the current play mode (Sequential, Loop, RepeatCurrent).
    /// </summary>
    PlayMode CurrentPlayMode { get; }

    /// <summary>
    /// Initializes the playlist with a list of questions and applies the saved sort preference.
    /// </summary>
    /// <param name="questions">The questions to load into the playlist.</param>
    /// <remarks>
    /// This method loads the saved sort mode from LocalSettings and applies it to the questions.
    /// It sets the current index to 0 if the playlist is not empty.
    /// </remarks>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task InitializeAsync(System.Collections.Generic.List<Question> questions);

    /// <summary>
    /// Moves to the next question in the playlist.
    /// </summary>
    /// <returns>The next question, or null if at the end (play mode determines behavior).</returns>
    /// <remarks>
    /// Behavior depends on CurrentPlayMode:
    /// - Sequential: Returns next question or null at end
    /// - Loop: Wraps to first question at end
    /// - RepeatCurrent: Returns current question.
    /// </remarks>
    Question? MoveToNext();

    /// <summary>
    /// Moves to the previous question in the playlist.
    /// </summary>
    /// <returns>The previous question, or null if at the beginning.</returns>
    Question? MoveToPrevious();

    /// <summary>
    /// Selects a specific question by index.
    /// </summary>
    /// <param name="index">The index of the question to select (0-based).</param>
    /// <returns>The selected question, or null if index is out of range.</returns>
    Question? SelectByIndex(int index);

    /// <summary>
    /// Applies a sort mode to the playlist and persists it in LocalSettings.
    /// </summary>
    /// <param name="sortMode">The sort mode to apply (AscendingAlpha, DescendingAlpha, Shuffle).</param>
    /// <remarks>
    /// The sort preference is persisted to ApplicationData.LocalSettings["PlaylistSortMode"].
    /// If Shuffle is selected, the current index is reset to 0.
    /// </remarks>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task ApplySortAsync(SortMode sortMode);

    /// <summary>
    /// Sets the play mode for the playlist.
    /// </summary>
    /// <param name="playMode">The play mode to apply (Sequential, Loop, RepeatCurrent).</param>
    void SetPlayMode(PlayMode playMode);

    /// <summary>
    /// Clears the playlist.
    /// </summary>
    void Clear();
}
