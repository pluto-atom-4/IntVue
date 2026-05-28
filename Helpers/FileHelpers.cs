using System;
using System.Text.RegularExpressions;

namespace IntVue.Helpers
{
    public static class FileHelpers
    {
        // Allowed characters: letters, digits, dash, underscore, dot
        private static readonly Regex InvalidChars = new("[^A-Za-z0-9_.-]+", RegexOptions.Compiled);

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
                name = "recording" + DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            }

            return name;
        }
    }
}
