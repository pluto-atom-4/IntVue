// Copyright (c) YourProjectName. All rights reserved.

using System;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace IntVue.Converters;

/// <summary>
/// Converts a boolean value to Visibility (true → Visible, false → Collapsed).
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Converts a boolean to Visibility.
    /// </summary>
    /// <param name="value">The boolean value to convert.</param>
    /// <param name="targetType">The target type (Visibility).</param>
    /// <param name="parameter">Optional converter parameter (unused).</param>
    /// <param name="language">The language/culture string (unused).</param>
    /// <returns>Visibility.Visible if value is true; otherwise Visibility.Collapsed.</returns>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        return Visibility.Collapsed;
    }

    /// <summary>
    /// Not implemented; conversion is one-way only.
    /// </summary>
    /// <param name="value">The Visibility value to convert back (unused).</param>
    /// <param name="targetType">The target type (unused).</param>
    /// <param name="parameter">Optional converter parameter (unused).</param>
    /// <param name="language">The language/culture string (unused).</param>
    /// <returns>Throws NotImplementedException.</returns>
    /// <exception cref="NotImplementedException">This converter does not support back conversion.</exception>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
