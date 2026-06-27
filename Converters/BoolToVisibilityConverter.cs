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
    /// <returns></returns>
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
    /// <returns></returns>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
