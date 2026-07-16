// Copyright (c) YourProjectName. All rights reserved.

using System;

using Microsoft.UI.Xaml.Data;

namespace IntVue.Converters;

/// <summary>
/// Converts a recording state boolean to button text.
/// true (recording) → "Stop Recording", false (not recording) → "Start Recording".
/// </summary>
public class RecordingStateToButtonTextConverter : IValueConverter
{
    /// <summary>
    /// Converts a boolean recording state to button text.
    /// </summary>
    /// <param name="value">The boolean IsRecordingNow value.</param>
    /// <param name="targetType">The target type (string).</param>
    /// <param name="parameter">Optional converter parameter (unused).</param>
    /// <param name="language">The language/culture string (unused).</param>
    /// <returns>"Stop Recording" if value is true; otherwise "Start Recording".</returns>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isRecording)
        {
            return isRecording ? "Stop Recording" : "Start Recording";
        }

        return "Start Recording";
    }

    /// <summary>
    /// Not implemented; conversion is one-way only.
    /// </summary>
    /// <param name="value">The button text to convert back (unused).</param>
    /// <param name="targetType">The target type (unused).</param>
    /// <param name="parameter">Optional converter parameter (unused).</param>
    /// <param name="language">The language/culture string (unused).</param>
    /// <returns>Throws NotImplementedException.</returns>
    /// <exception cref="NotImplementedException">This converter does not support back conversion.</exception>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
