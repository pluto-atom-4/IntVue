// <copyright file="MainViewModels.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace IntVue.ViewModels;

/// <summary>
/// Minimal MainViewModel used for DI and basic binding in XAML.
/// Keep this minimal to satisfy build and unit tests; extend later as needed.
/// </summary>
public class MainViewModel
{
    public string Title { get; set; } = "IntVue";
}
