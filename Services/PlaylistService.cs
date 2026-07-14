// Copyright (c) YourProjectName. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using IntVue.Models;

namespace IntVue.Services;

/// <summary>
/// Service for managing a playlist of interview questions with sorting, navigation, and persistence.
/// </summary>
public class PlaylistService : IPlaylistService
{
    private const string _sortModeSettingKey = "PlaylistSortMode";

    private readonly ISettingsService _settingsService;
    private readonly ObservableCollection<Question> _questions = new ObservableCollection<Question>();
    private int _currentIndex = -1;
    private PlayMode _currentPlayMode = PlayMode.RepeatCurrent;
    private SortMode _currentSortMode = SortMode.AscendingAlpha;
    private List<Question> _originalQuestions = new List<Question>();
    private Random? _shuffleRandom;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaylistService"/> class.
    /// </summary>
    /// <param name="settingsService">The settings service for persisting playlist preferences.</param>
    public PlaylistService(ISettingsService settingsService)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
    }

    /// <summary>
    /// Gets the current playlist of questions.
    /// </summary>
    public ObservableCollection<Question> Questions => _questions;

    /// <summary>
    /// Gets the currently selected question, or null if playlist is empty.
    /// </summary>
    public Question? CurrentQuestion => _currentIndex >= 0 && _currentIndex < _questions.Count ? _questions[_currentIndex] : null;

    /// <summary>
    /// Gets the index of the current question in the playlist.
    /// </summary>
    public int CurrentIndex => _currentIndex;

    /// <summary>
    /// Gets the total number of questions in the playlist.
    /// </summary>
    public int TotalCount => _questions.Count;

    /// <summary>
    /// Gets the current sort mode (persisted in LocalSettings).
    /// </summary>
    public SortMode CurrentSortMode => _currentSortMode;

    /// <summary>
    /// Gets the current play mode (Sequential, Loop, RepeatCurrent).
    /// </summary>
    public PlayMode CurrentPlayMode => _currentPlayMode;

    /// <summary>
    /// Initializes the playlist with a list of questions and applies the saved sort preference.
    /// </summary>
    /// <param name="questions">The questions to load into the playlist.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task InitializeAsync(List<Question> questions)
    {
        ArgumentNullException.ThrowIfNull(questions);

        System.Diagnostics.Debug.WriteLine($"[PlaylistService.InitializeAsync] Starting with {questions.Count} questions");

        try
        {
            _questions.Clear();
            _originalQuestions = new List<Question>(questions);

            // Load saved sort mode from settings service
            try
            {
                System.Diagnostics.Debug.WriteLine($"[PlaylistService.InitializeAsync] Getting sort mode from settings...");
                var sortModeValue = _settingsService.GetSetting(_sortModeSettingKey);
                if (sortModeValue != null && Enum.TryParse<SortMode>(sortModeValue.ToString(), out var savedMode))
                {
                    _currentSortMode = savedMode;
                    System.Diagnostics.Debug.WriteLine($"[PlaylistService.InitializeAsync] Loaded saved sort mode: {_currentSortMode}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[PlaylistService.InitializeAsync] Using default sort mode: {_currentSortMode}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PlaylistService.InitializeAsync] Error loading sort mode: {ex.GetType().Name}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[PlaylistService.InitializeAsync] Using default sort mode: {_currentSortMode}");
            }

            // Apply the saved sort mode
            System.Diagnostics.Debug.WriteLine($"[PlaylistService.InitializeAsync] Calling ApplySortAsync({_currentSortMode})");
            await this.ApplySortAsync(_currentSortMode);
            System.Diagnostics.Debug.WriteLine($"[PlaylistService.InitializeAsync] ApplySortAsync completed");

            // Set current index to 0 if not empty
            _currentIndex = _questions.Count > 0 ? 0 : -1;
            System.Diagnostics.Debug.WriteLine($"[PlaylistService.InitializeAsync] Playlist initialized: {_questions.Count} questions, CurrentIndex={_currentIndex}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PlaylistService.InitializeAsync] EXCEPTION: {ex.GetType().Name}: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[PlaylistService.InitializeAsync] StackTrace: {ex.StackTrace}");
            throw;
        }
    }

    /// <summary>
    /// Moves to the next question in the playlist.
    /// </summary>
    /// <returns>The next question, or null if at the end (play mode determines behavior).</returns>
    public Question? MoveToNext()
    {
        if (_questions.Count == 0)
        {
            return null;
        }

        // RepeatCurrent mode: always return current
        if (_currentPlayMode == PlayMode.RepeatCurrent)
        {
            return this.CurrentQuestion;
        }

        // Move to next index
        int nextIndex = _currentIndex + 1;

        // Loop mode: wrap to beginning
        if (_currentPlayMode == PlayMode.Loop)
        {
            _currentIndex = nextIndex % _questions.Count;
        }
        else
        {
            // Sequential mode: stay at end or move forward
            if (nextIndex < _questions.Count)
            {
                _currentIndex = nextIndex;
            }
            else
            {
                return null; // End of playlist
            }
        }

        return this.CurrentQuestion;
    }

    /// <summary>
    /// Moves to the previous question in the playlist.
    /// </summary>
    /// <returns>The previous question, or null if at the beginning.</returns>
    public Question? MoveToPrevious()
    {
        if (_questions.Count == 0 || _currentIndex <= 0)
        {
            return null;
        }

        _currentIndex--;
        return this.CurrentQuestion;
    }

    /// <summary>
    /// Selects a specific question by index.
    /// </summary>
    /// <param name="index">The index of the question to select (0-based).</param>
    /// <returns>The selected question, or null if index is out of range.</returns>
    public Question? SelectByIndex(int index)
    {
        if (index < 0 || index >= _questions.Count)
        {
            return null;
        }

        _currentIndex = index;
        return this.CurrentQuestion;
    }

    /// <summary>
    /// Applies a sort mode to the playlist and persists it in LocalSettings.
    /// </summary>
    /// <param name="sortMode">The sort mode to apply (AscendingAlpha, DescendingAlpha, Shuffle).</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ApplySortAsync(SortMode sortMode)
    {
        System.Diagnostics.Debug.WriteLine($"[PlaylistService.ApplySortAsync] Starting with sort mode: {sortMode}");
        _currentSortMode = sortMode;

        // Clear and re-populate questions based on sort mode
        _questions.Clear();

        var sortedQuestions = sortMode switch
        {
            SortMode.AscendingAlpha => _originalQuestions.OrderBy(q => q.FileName).ToList(),
            SortMode.DescendingAlpha => _originalQuestions.OrderByDescending(q => q.FileName).ToList(),
            SortMode.Shuffle => this.ShuffleQuestions(_originalQuestions),
            _ => _originalQuestions,
        };

        System.Diagnostics.Debug.WriteLine($"[PlaylistService.ApplySortAsync] Sorted into {sortedQuestions.Count} questions");

        foreach (var question in sortedQuestions)
        {
            _questions.Add(question);
        }

        // Reset current index to 0
        _currentIndex = _questions.Count > 0 ? 0 : -1;
        System.Diagnostics.Debug.WriteLine($"[PlaylistService.ApplySortAsync] Questions collection updated: {_questions.Count} items, CurrentIndex={_currentIndex}");

        // Persist sort mode to settings service (non-blocking failure)
        try
        {
            System.Diagnostics.Debug.WriteLine($"[PlaylistService.ApplySortAsync] Persisting sort mode to settings...");
            await _settingsService.SetSettingAsync(_sortModeSettingKey, sortMode.ToString());
            System.Diagnostics.Debug.WriteLine($"[PlaylistService.ApplySortAsync] Sort mode persisted successfully");
        }
        catch (Exception ex)
        {
            // Log but don't throw - playlist is already populated correctly
            System.Diagnostics.Debug.WriteLine($"[PlaylistService.ApplySortAsync] Warning: Failed to persist sort mode: {ex.GetType().Name}: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[PlaylistService.ApplySortAsync] Playlist continues to work; sort preference won't persist across sessions");
        }
    }

    /// <summary>
    /// Sets the play mode for the playlist.
    /// </summary>
    /// <param name="playMode">The play mode to apply (Sequential, Loop, RepeatCurrent).</param>
    public void SetPlayMode(PlayMode playMode)
    {
        _currentPlayMode = playMode;
    }

    /// <summary>
    /// Clears the playlist.
    /// </summary>
    public void Clear()
    {
        _questions.Clear();
        _originalQuestions.Clear();
        _currentIndex = -1;
    }

    /// <summary>
    /// Shuffles the questions using a seeded random generator for reproducibility.
    /// </summary>
    private List<Question> ShuffleQuestions(List<Question> questions)
    {
        if (questions.Count == 0)
        {
            return new List<Question>();
        }

        // Seed based on sorted filename hashes for reproducibility within a session
        int seed = questions
            .OrderBy(q => q.FileName)
            .Aggregate(0, (hash, q) => hash ^ q.FileName.GetHashCode());

        _shuffleRandom = new Random(seed);

        var shuffled = new List<Question>(questions);
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int randomIndex = _shuffleRandom.Next(i + 1);
            (shuffled[i], shuffled[randomIndex]) = (shuffled[randomIndex], shuffled[i]);
        }

        return shuffled;
    }
}
