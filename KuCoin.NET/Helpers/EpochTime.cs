
using System;
using System.Collections.Generic;
using System.Text;

namespace KuCoin.NET.Helpers
{

    /// <summary>
    /// Time interval length interpretation types
    /// </summary>
    public enum TimeTypes
    {
        /// <summary>
        /// Interpret seconds
        /// </summary>
        InSeconds,
        /// <summary>
        /// Interpret milliseconds
        /// </summary>
        InMilliseconds,

        /// <summary>
        /// Interpret nanoseconds
        /// </summary>
        InNanoseconds
    }

    /// <summary>
    /// Methods for converting between Unix and .NET DateTime stamps.
    /// </summary>
    public static class EpochTime
    {

        /// <summary>
        /// The Unix epoch date (1970-01-01 00:00:00.00000 UTC).
        /// </summary>
        public static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Convert UTC nanoseconds-based Unix-epoch date to local <see cref="DateTime"/>.
        /// </summary>
        /// <param name="time"></param>
        /// <returns>Local <see cref="DateTime"/> represented by the value.</returns>
        public static DateTime NanosecondsToDate(long time)
        {
            return (Epoch + new TimeSpan(time / 100)).ToLocalTime();
        }

        /// <summary>
        /// Converts a <see cref="DateTime"/> value to a UTC nanoseconds-based Unix-epoch timestamp.
        /// </summary>
        /// <param name="date">The date to convert.</param>
        /// <returns>The number of nanoseconds since the Unix epoch.</returns>
        public static long DateToNanoseconds(DateTime date)
        {
            return (date.ToUniversalTime() - Epoch).Ticks * 100;
        }

        /// <summary>
        /// Convert UTC milliseconds-based Unix-epoch date to local <see cref="DateTime"/>.
        /// </summary>
        /// <param name="time"></param>
        /// <returns>Local <see cref="DateTime"/> represented by the value.</returns>
        public static DateTime MillisecondsToDate(long time)
        {
            return Epoch.AddMilliseconds(time).ToLocalTime();
        }

        /// <summary>
        /// Converts a <see cref="DateTime"/> value to a UTC milliseconds-based Unix-epoch timestamp.
        /// </summary>
        /// <param name="date">The date to convert.</param>
        /// <returns>The number of milliseconds since the Unix epoch.</returns>
        public static long DateToMilliseconds(DateTime date)
        {
            var ts = date.ToUniversalTime() - Epoch;
            return (long)ts.TotalMilliseconds;
        }

        /// <summary>
        /// Convert UTC seconds-based Unix-epoch date to local <see cref="DateTime"/>.
        /// </summary>
        /// <param name="time"></param>
        /// <returns>Local <see cref="DateTime"/> represented by the value.</returns>
        public static DateTime SecondsToDate(long time)
        {
            return Epoch.AddSeconds(time).ToLocalTime();
        }

        /// <summary>
        /// Converts a <see cref="DateTime"/> value to a UTC seconds-based Unix-epoch timestamp.
        /// </summary>
        /// <param name="date">The date to convert.</param>
        /// <returns>The number of seconds since the Unix epoch.</returns>
        public static long DateToSeconds(DateTime date)
        {
            var ts = date.ToUniversalTime() - Epoch;
            return (long)ts.TotalSeconds;
        }
    }
}
