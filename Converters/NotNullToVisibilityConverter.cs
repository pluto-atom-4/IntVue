// Copyright (c) YourProjectName. All rights reserved.

using System;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace IntVue.Converters;

/// <summary>
/// Converts object reference to Visibility.
/// null → Collapsed, not-null → Visible.
/// </summary>
public sealed class NotNullToVisibilityConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is not null ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <inheritdoc/>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
