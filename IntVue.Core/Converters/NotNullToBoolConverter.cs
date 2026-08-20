// Copyright (c) YourProjectName. All rights reserved.

using System;

using Microsoft.UI.Xaml.Data;

namespace IntVue.Converters;

/// <summary>
/// Converts object reference to bool for enable/disable logic.
/// null → false, not-null → true.
/// </summary>
public sealed class NotNullToBoolConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is not null;
    }

    /// <inheritdoc/>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
