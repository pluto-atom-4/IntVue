// <copyright file="FileHelpers.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace IntVue.Helpers
{
    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Helper utilities for working with file names and paths.
    /// </summary>
    public static class FileHelpers
    {
        // Allowed characters: letters, digits, dash, underscore, dot
        private static readonly Regex InvalidChars = new Regex("[^A-Za-z0-9_.-]+", RegexOptions.Compiled);

        /// <summary>
        /// Sanitize a user-provided filename by removing path separators and invalid characters
        /// and trimming to a safe length. Preserves common extensions when possible.
        /// </summary>
        /// <param name="name">The input filename provided by the user.</param>
        /// <param name="maxLength">Maximum allowed length for the returned filename.</param>
        /// <returns>A sanitized filename safe for use as a storage filename.</returns>
        public static string SanitizeFileName(string name, int maxLength = 128)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                name = "recording";
            }

            // Remove path separators and invalid chars
            name = name.Replace("\\", "_").Replace("/", "_");
            name = InvalidChars.Replace(name, "_");

            // Trim to max length (preserve extension if present)
            if (name.Length > maxLength)
            {
                var extIndex = name.LastIndexOf('.');
                if (extIndex > 0 && name.Length - extIndex <= 10)
                {
                    var ext = name.Substring(extIndex);
                    var baseName = name.Substring(0, maxLength - ext.Length);
                    name = baseName + ext;
                }
                else
                {
                    name = name.Substring(0, maxLength);
                }
            }

            // Final fallback
            if (string.IsNullOrWhiteSpace(name))
            {
                name = "recording" + DateTime.UtcNow.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            }

            return name;
        }
    }
}
