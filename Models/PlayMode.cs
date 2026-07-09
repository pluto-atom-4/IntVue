// Copyright (c) YourProjectName. All rights reserved.

namespace IntVue.Models;

/// <summary>
/// Specifies the playback behavior for the question playlist.
/// </summary>
public enum PlayMode
{
    /// <summary>
    /// Play questions sequentially; stop at the end of the playlist.
    /// </summary>
    Sequential = 0,

    /// <summary>
    /// Loop back to the first question when reaching the end of the playlist.
    /// </summary>
    Loop = 1,

    /// <summary>
    /// Repeat the current question indefinitely until user moves to next question.
    /// </summary>
    RepeatCurrent = 2,
}
