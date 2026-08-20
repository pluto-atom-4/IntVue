// Copyright (c) YourProjectName. All rights reserved.

using System;

using Microsoft.UI.Xaml.Data;

namespace IntVue.Converters;

/// <summary>
/// Converts countdown seconds (3, 2, 1, 0) to themed Brush colors.
/// 3 → Yellow (Caution), 2 → Orange (Attention), 1 → Red (Critical), 0 → Default text color.
/// </summary>
public class CountdownColorConverter : IValueConverter
{
    /// <summary>
    /// Converts a countdown value to a theme brush.
    /// </summary>
    /// <param name="value">The countdown seconds (3, 2, 1, or 0).</param>
    /// <param name="targetType">The target type (Brush).</param>
    /// <param name="parameter">Optional converter parameter (unused).</param>
    /// <param name="language">The language/culture string (unused).</param>
    /// <returns>A Brush resource: Caution (3), Attention (2), Critical (1), or Primary text color (0+).</returns>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var resources = Microsoft.UI.Xaml.Application.Current.Resources;
        var key = (int)value switch
        {
            3 => "SystemFillColorCautionBrush",
            2 => "SystemFillColorAttentionBrush",
            1 => "SystemFillColorCriticalBrush",
            _ => "TextFillColorPrimaryBrush",
        };

        return resources.TryGetValue(key, out var brush) ? brush : resources["TextFillColorPrimaryBrush"];
    }

    /// <summary>
    /// Not implemented; conversion is one-way only.
    /// </summary>
    /// <param name="value">The Brush value to convert back (unused).</param>
    /// <param name="targetType">The target type (unused).</param>
    /// <param name="parameter">Optional converter parameter (unused).</param>
    /// <param name="language">The language/culture string (unused).</param>
    /// <returns>Throws NotImplementedException.</returns>
    /// <exception cref="NotImplementedException">This converter does not support back conversion.</exception>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
