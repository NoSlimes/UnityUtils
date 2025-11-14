using System;

namespace NoSlimes.Utils
{
    public static class StringUtils
    {
        /// <summary>
        /// Converts a float value (0-1) to a percentage string (0%-100%).
        /// </summary>
        /// <param name="value">The value to convert, typically between 0 and 1.</param>
        /// <param name="decimals">Number of decimal places to include in the result.</param>
        /// <param name="includeSign">Whether to include the '%' symbol in the output.</param>
        /// <returns>A string representing the percentage, e.g., "75%" or "75.5%".</returns>
        public static string GetPercentString(float value, int decimals = 0, bool includeSign = true)
        {
            return Math.Round(value * 100f, decimals) + (includeSign ? "%" : "");
        }

        /// <summary>
        /// Converts a portion of a total into a percentage string (0%-100%).
        /// </summary>
        /// <param name="value">The numerator or partial value.</param>
        /// <param name="total">The total or maximum value.</param>
        /// <param name="decimals">Number of decimal places to include in the result.</param>
        /// <param name="includeSign">Whether to include the '%' symbol in the output.</param>
        /// <returns>A string representing the percentage of value relative to total.</returns>
        public static string GetPercentageString(float value, int total, int decimals = 0, bool includeSign = true)
        {
            if (total == 0) return "0" + (includeSign ? "%" : "");
            return GetPercentString(value / total, decimals, includeSign);
        }


        /// <summary>
        /// Returns a string representation of a number with dots as thousand separators (e.g., 1.000.000).
        /// </summary>
        /// <param name="number">The number to format.</param>
        /// <returns>A string with dots as thousand separators.</returns>
        public static string ToDotSeparatedString(int number)
        {
            return number.ToString("#,0", System.Globalization.CultureInfo.InvariantCulture)
                         .Replace(",", ".");
        }
    }
}