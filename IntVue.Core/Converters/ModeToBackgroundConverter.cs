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
        // Verify mode and parameter
        if (!(value is PlayMode currentMode && parameter is string modeParam &&
            Enum.TryParse<PlayMode>(modeParam, out var buttonMode)))
        {
            // Invalid input - return transparent
            return CreateTransparentBrush();
        }

        // If mode matches, return red (critical) brush
        if (currentMode == buttonMode)
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

        // Mode doesn't match - return transparent
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
