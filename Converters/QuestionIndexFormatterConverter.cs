// Copyright (c) YourProjectName. All rights reserved.

using System;

using Microsoft.UI.Xaml.Data;

namespace IntVue.Converters;

/// <summary>
/// Formats question index for display.
/// Parameter should be total count, value is current index.
/// </summary>
public sealed class QuestionIndexFormatterConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int index && parameter is int total && total > 0)
        {
            return $"Question {index} of {total}";
        }

        return "Question 0 of 0";
    }

    /// <inheritdoc/>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
