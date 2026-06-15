// Copyright (c) YourProjectName. All rights reserved.

namespace IntVue.ViewModels;

/// <summary>
/// Minimal MainViewModel used for DI and basic binding in XAML.
/// Keep this minimal to satisfy build and unit tests; extend later as needed.
/// </summary>
public class MainViewModel
{
    /// <summary>
    /// Gets or sets the window title used in the UI.
    /// </summary>
    public string Title { get; set; } = "IntVue";
}
