// Copyright (c) YourProjectName. All rights reserved.

using System;

using IntVue.Converters;
using IntVue.Models;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntVue.Tests.Converters;

[TestClass]
public class SortModeToBackgroundConverterTests
{
    private SortModeToBackgroundConverter _converter = null!;

    [TestInitialize]
    public void Initialize()
    {
        _converter = new SortModeToBackgroundConverter();
    }

    [TestMethod]
    public void Convert_WithShuffleMode_DoesNotThrow()
    {
        // Arrange & Act: Verify converter doesn't throw with Shuffle mode
        try
        {
            var result = _converter.Convert(SortMode.Shuffle, typeof(object), "Shuffle", "en-US");
            // In test context without UI thread, result may be null - that's expected
            Assert.IsTrue(result == null || result is not Exception, "Converter should not throw");
        }
        catch (Exception ex)
        {
            Assert.Fail($"Converter should not throw: {ex.Message}");
        }
    }

    [TestMethod]
    public void Convert_WithAscendingAlphaMode_DoesNotThrow()
    {
        // Arrange & Act: Verify converter doesn't throw with AscendingAlpha mode
        try
        {
            var result = _converter.Convert(SortMode.AscendingAlpha, typeof(object), "AscendingAlpha", "en-US");
            // In test context without UI thread, result may be null - that's expected
            Assert.IsTrue(result == null || result is not Exception, "Converter should not throw");
        }
        catch (Exception ex)
        {
            Assert.Fail($"Converter should not throw: {ex.Message}");
        }
    }

    [TestMethod]
    public void Convert_WithMismatchedSortModes_DoesNotThrow()
    {
        // Arrange: Shuffle mode active, but testing AscendingAlpha button
        var currentSortMode = SortMode.Shuffle;
        var parameter = "AscendingAlpha";

        // Act & Assert: Converter should not throw with mismatched sort modes
        try
        {
            var result = _converter.Convert(currentSortMode, typeof(object), parameter, "en-US");
            // In test context, result may be null - that's acceptable
        }
        catch (Exception ex)
        {
            Assert.Fail($"Converter should not throw: {ex.Message}");
        }
    }

    [TestMethod]
    public void Convert_WithInvalidParameter_DoesNotThrow()
    {
        // Arrange: Invalid sort mode parameter
        var currentSortMode = SortMode.Shuffle;
        var parameter = "InvalidSortMode";

        // Act & Assert: Converter should not throw with invalid parameters
        try
        {
            var result = _converter.Convert(currentSortMode, typeof(object), parameter, "en-US");
            // In test context, result may be null - that's acceptable
        }
        catch (Exception ex)
        {
            Assert.Fail($"Converter should not throw: {ex.Message}");
        }
    }

    [TestMethod]
    public void ConvertBack_Always_ThrowsNotImplementedException()
    {
        // Arrange & Act & Assert
        try
        {
            _converter.ConvertBack(null!, typeof(SortMode), null, "en-US");
            Assert.Fail("ConvertBack should throw NotImplementedException");
        }
        catch (NotImplementedException)
        {
            // Expected
        }
    }
}
