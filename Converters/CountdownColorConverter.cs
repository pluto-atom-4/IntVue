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
    /// <returns></returns>
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
    /// <returns></returns>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
