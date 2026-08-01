// Copyright (c) YourProjectName. All rights reserved.

using System;

using IntVue.Converters;
using IntVue.Models;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntVue.Tests.Converters;

[TestClass]
public class ModeToBackgroundConverterTests
{
    private ModeToBackgroundConverter _converter = null!;

    [TestInitialize]
    public void Initialize()
    {
        _converter = new ModeToBackgroundConverter();
    }

    [TestMethod]
    public void Convert_WithValidLoopMode_DoesNotThrow()
    {
        // Arrange & Act: Verify converter doesn't throw with valid parameters
        try
        {
            var result = _converter.Convert(PlayMode.Loop, typeof(object), "Loop", "en-US");
            // In test context without UI thread, result may be null - that's expected
            Assert.IsTrue(result == null || result is not Exception, "Converter should not throw");
        }
        catch (Exception ex)
        {
            Assert.Fail($"Converter should not throw: {ex.Message}");
        }
    }

    [TestMethod]
    public void Convert_WithValidRepeatMode_DoesNotThrow()
    {
        // Arrange & Act: Verify converter doesn't throw with valid parameters
        try
        {
            var result = _converter.Convert(PlayMode.RepeatCurrent, typeof(object), "RepeatCurrent", "en-US");
            // In test context without UI thread, result may be null - that's expected
            Assert.IsTrue(result == null || result is not Exception, "Converter should not throw");
        }
        catch (Exception ex)
        {
            Assert.Fail($"Converter should not throw: {ex.Message}");
        }
    }

    [TestMethod]
    public void Convert_WithMismatchedModes_DoesNotThrow()
    {
        // Arrange: Loop mode active, but testing Repeat button
        var currentMode = PlayMode.Loop;
        var parameter = "RepeatCurrent";

        // Act & Assert: Converter should not throw with mismatched modes
        try
        {
            var result = _converter.Convert(currentMode, typeof(object), parameter, "en-US");
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
        // Arrange: Invalid mode parameter that can't be parsed
        var currentMode = PlayMode.Loop;
        var parameter = "InvalidMode";

        // Act & Assert: Converter should not throw with invalid parameters
        try
        {
            var result = _converter.Convert(currentMode, typeof(object), parameter, "en-US");
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
            _converter.ConvertBack(null!, typeof(PlayMode), null, "en-US");
            Assert.Fail("ConvertBack should throw NotImplementedException");
        }
        catch (NotImplementedException)
        {
            // Expected
        }
    }
}
