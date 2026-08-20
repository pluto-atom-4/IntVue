// Copyright (c) YourProjectName. All rights reserved.

namespace IntVue.Models;

/// <summary>
/// Specifies how questions in the playlist should be sorted.
/// </summary>
public enum SortMode
{
    /// <summary>
    /// Sort questions by filename in ascending alphabetical order.
    /// </summary>
    AscendingAlpha = 0,

    /// <summary>
    /// Sort questions by filename in descending alphabetical order.
    /// </summary>
    DescendingAlpha = 1,

    /// <summary>
    /// Randomize question order (shuffle).
    /// </summary>
    Shuffle = 2,
}
