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
        var resources = Microsoft.UI.Xaml.Application.Current.Resources;

        if (value is SortMode currentSort && parameter is string sortParam &&
            Enum.TryParse<SortMode>(sortParam, out var buttonSort))
        {
            if (currentSort == buttonSort)
            {
                return resources.TryGetValue("SystemFillColorCriticalBrush", out var brush)
                    ? brush
                    : resources["SolidBackgroundFillColorBaseBrush"];
            }
        }

        // Return transparent background for non-matching sort modes
        if (resources.TryGetValue("SystemControlTransparentBrush", out var transparentBrush))
        {
            return transparentBrush;
        }

        // Fallback: Create a transparent SolidColorBrush
        var transparentColor = Microsoft.UI.Colors.Transparent;
        return new SolidColorBrush(transparentColor);
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
}
