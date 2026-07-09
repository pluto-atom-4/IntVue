// Copyright (c) YourProjectName. All rights reserved.

using System;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace IntVue.Converters;

/// <summary>
/// Converts string to Visibility.
/// empty/null → Collapsed, non-empty → Visible.
/// </summary>
public sealed class StringToVisibilityConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return !string.IsNullOrWhiteSpace(value as string) ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <inheritdoc/>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
