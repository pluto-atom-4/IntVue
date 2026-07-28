// Copyright (c) YourProjectName. All rights reserved.

using System;

using IntVue.Models;

using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace IntVue.Converters;

/// <summary>
/// Converts CurrentPlayMode to a background Brush for mode buttons.
/// Compares the provided PlayMode value with the converter parameter (button's mode).
/// Returns SystemFillColorCriticalBrush if modes match; Transparent otherwise.
/// </summary>
public class ModeToBackgroundConverter : IValueConverter
{
    /// <summary>
    /// Converts CurrentPlayMode to a background Brush for a mode button.
    /// </summary>
    /// <param name="value">The current PlayMode (Loop or RepeatCurrent).</param>
    /// <param name="targetType">The target type (Brush).</param>
    /// <param name="parameter">The button's PlayMode as a string ("Loop" or "RepeatCurrent").</param>
    /// <param name="language">The language/culture string (unused).</param>
    /// <returns>SystemFillColorCriticalBrush if modes match; Transparent otherwise.</returns>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var resources = Microsoft.UI.Xaml.Application.Current.Resources;

        if (value is PlayMode currentMode && parameter is string modeParam &&
            Enum.TryParse<PlayMode>(modeParam, out var buttonMode))
        {
            if (currentMode == buttonMode)
            {
                return resources.TryGetValue("SystemFillColorCriticalBrush", out var brush)
                    ? brush
                    : resources["SolidBackgroundFillColorBaseBrush"];
            }
        }

        // Return transparent background for non-matching modes
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
