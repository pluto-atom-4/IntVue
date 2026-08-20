// Copyright (c) YourProjectName. All rights reserved.

using System;

using IntVue.Models;

using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace IntVue.Converters;

/// <summary>
/// Converts CurrentSortMode to a background Brush for sort mode buttons.
/// Compares the provided SortMode value with the converter parameter (button's sort mode).
/// Returns SystemFillColorCriticalBrush if sort modes match; Transparent otherwise.
/// </summary>
public class SortModeToBackgroundConverter : IValueConverter
{
    /// <summary>
    /// Converts CurrentSortMode to a background Brush for a sort mode button.
    /// </summary>
    /// <param name="value">The current SortMode (Shuffle or other sort modes).</param>
    /// <param name="targetType">The target type (Brush).</param>
    /// <param name="parameter">The button's SortMode as a string ("Shuffle").</param>
    /// <param name="language">The language/culture string (unused).</param>
    /// <returns>SystemFillColorCriticalBrush if modes match; Transparent otherwise.</returns>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        // Verify sort mode and parameter
        if (!(value is SortMode currentSort && parameter is string sortParam &&
            Enum.TryParse<SortMode>(sortParam, out var buttonSort)))
        {
            // Invalid input - return transparent
            return CreateTransparentBrush();
        }

        // If sort mode matches, return red (critical) brush
        if (currentSort == buttonSort)
        {
            // Try to get theme resource; fallback to creating red brush
            var app = Microsoft.UI.Xaml.Application.Current;
            if (app != null && app.Resources.TryGetValue("SystemFillColorCriticalBrush", out var brush))
            {
                return brush;
            }

            // Fallback: Create red SolidColorBrush (Color: #E81B23 for light, #FF8A80 for dark)
            return CreateRedBrush();
        }

        // Sort mode doesn't match - return transparent
        return CreateTransparentBrush();
    }

    /// <summary>
    /// Not implemented; conversion is one-way only.
    /// </summary>
    /// <param name="value">The Brush value to convert back (unused).</param>
    /// <param name="targetType">The target type (unused).</param>
    /// <param name="parameter">Optional converter parameter (unused).</param>
    /// <param name="language">The language/culture string (unused).</param>
    /// <returns>Throws NotImplementedException.</returns>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();

    private static object CreateRedBrush()
    {
        try
        {
            return new SolidColorBrush(Microsoft.UI.Colors.Red);
        }
        catch
        {
            // In test context without UI thread, return null (safe in XAML)
            return null!;
        }
    }

    private static object CreateTransparentBrush()
    {
        try
        {
            return new SolidColorBrush(Microsoft.UI.Colors.Transparent);
        }
        catch
        {
            // In test context without UI thread, return null (safe in XAML)
            return null!;
        }
    }
}
