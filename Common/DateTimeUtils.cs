using System.Collections.Generic;
using UnityEngine;

namespace NoSlimes.UnityUtils.Common
{
    public static class DateTimeUtils
    {

        /// <summary>
        /// Returns the full name of a month given its index (0 = January, 11 = December).
        /// If the month is outside 0–11, it wraps around using modulo 12.
        /// </summary>"
        /// <param name="month">Month index (0-based).</param>
        /// <returns>The full month name.</returns>
        public static string GetMonthName(int month)
        {
            //month 23 -> 11, for example
            month = ((month % 12) + 12) % 12;

            return month switch
            {
                1 => "February",
                2 => "March",
                3 => "April",
                4 => "May",
                5 => "June",
                6 => "July",
                7 => "August",
                8 => "September",
                9 => "October",
                10 => "November",
                11 => "December",
                _ => "January",
            };
        }

        /// <summary>
        /// Returns the 3-letter abbreviation of a month given its index (0 = Jan, 11 = Dec).
        /// </summary>
        /// <param name="month">Month index (0-based).</param>
        /// <returns>3-letter month abbreviation.</returns>
        public static string GetMonthNameShort(int month)
        {
            return GetMonthName(month).Substring(0, 3);
        }

        /// <summary>
        /// Returns a formatted time string (HH:MM:SS.mmm) based on the given float time in seconds.
        /// Optional parameters control which components are included.
        /// </summary>
        /// <param name="time">Time in seconds.</param>
        /// <param name="hours">Include hours.</param>
        /// <param name="minutes">Include minutes.</param>
        /// <param name="seconds">Include seconds.</param>
        /// <param name="milliseconds">Include milliseconds.</param>
        /// <returns>Formatted time string.</returns>
        public static string GetTimeHMS(float time, bool hours = true, bool minutes = true, bool seconds = true, bool milliseconds = true)
        {
            (string h0, string h1, string m0, string m1, string s0, string s1, string ms0, string ms1, string ms2) = GetTimeCharacterStrings(time);

            List<string> parts = new();

            if (hours) parts.Add(h0 + h1);
            if (minutes) parts.Add(m0 + m1);
            if (seconds) parts.Add(s0 + s1);

            string result = string.Join(":", parts);

            if (milliseconds)
            {
                string ms = ms0 + ms1 + ms2;
                result += (parts.Count > 0 ? "." : "") + ms;
            }

            return result;
        }


        /// <summary>
        /// Converts a float time (seconds) into hours, minutes, seconds, and milliseconds as integers.
        /// </summary>
        /// <param name="time">Time in seconds.</param>
        /// <returns>Tuple of (hours, minutes, seconds, milliseconds).</returns>
        public static (int h, int m, int s, int ms) GetTimeHMS(float time)
        {
            int totalSeconds = Mathf.FloorToInt(time);
            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            int seconds = totalSeconds % 60;
            int milliseconds = (int)((time - totalSeconds) * 1000);
            return (hours, minutes, seconds, milliseconds);
        }
        /// <summary>
        /// Converts a float time (seconds) into individual string digits for HH:MM:SS.mmm display.
        /// Useful for custom text rendering of each digit.
        /// </summary>
        /// <param name="time">Time in seconds.</param>
        /// <returns>Tuple of strings: h0, h1, m0, m1, s0, s1, ms0, ms1, ms2.</returns>
        public static (string h0, string h1, string m0, string m1, string s0, string s1, string ms0, string ms1, string ms2)
            GetTimeCharacterStrings(float time)
        {
            (int h, int m, int s, int ms) = GetTimeHMS(time);

            string Format2Digits(int value) => value.ToString("D2");
            string Format3Digits(int value) => value.ToString("D3");

            string hStr = Format2Digits(h);
            string mStr = Format2Digits(m);
            string sStr = Format2Digits(s);
            string msStr = Format3Digits(ms);

            return (hStr[0].ToString(), hStr[1].ToString(),
                    mStr[0].ToString(), mStr[1].ToString(),
                    sStr[0].ToString(), sStr[1].ToString(),
                    msStr[0].ToString(), msStr[1].ToString(), msStr[2].ToString());
        }

        /// <summary>
        /// Logs the elapsed time in milliseconds since the given start time with an optional prefix.
        /// </summary>
        /// <param name="startTime">Start time in seconds (e.g., Time.realtimeSinceStartup).</param>
        /// <param name="prefix">Optional prefix to include in the log message.</param>
        public static void PrintTimeMilliseconds(float startTime, string prefix = "")
        {
            Debug.Log(prefix + GetTimeMilliseconds(startTime));
        }

        /// <summary>
        /// Returns the elapsed time in milliseconds since the given start time.
        /// </summary>
        /// <param name="startTime">Start time in seconds (e.g., Time.realtimeSinceStartup).</param>
        /// <returns>Elapsed time in milliseconds.</returns>
        public static float GetTimeMilliseconds(float startTime)
        {
            return (Time.realtimeSinceStartup - startTime) * 1000f;
        }
    }
}