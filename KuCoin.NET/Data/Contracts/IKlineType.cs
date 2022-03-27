using System;
using System.Collections.Generic;
using System.Text;

namespace KuCoin.NET.Data.Market
{
    /// <summary>
    /// Enumeration of whether the kline length is represented in seconds, or minutes.
    /// </summary>
    public enum KlineLengthType
    {
        /// <summary>
        /// K-Line is represented in seconds.
        /// </summary>
        Seconds,

        /// <summary>
        /// K-Line is represented in minutes.
        /// </summary>
        Minutes
    }

    /// <summary>
    /// Interface for all K-Line types.
    /// </summary>
    public interface IKlineType : IFormattable
    {
        
        /// <summary>
        /// The absolute length of time as a <see cref="System.TimeSpan"/> object.
        /// </summary>
        TimeSpan TimeSpan { get; }

        /// <summary>
        /// Gets the number of the unit of time.
        /// </summary>
        int Number { get; }

        /// <summary>
        /// Gets the unit of time.
        /// </summary>
        string Unit { get; }

        /// <summary>
        /// Gets all valid values of the implementing type.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IKlineType> GetAllTypes();

        /// <summary>
        /// Gets a value indicating if the present object represents an invalid value.
        /// </summary>
        bool IsInvalid { get; }

        /// <summary>
        /// Gets the start date for the number of requested pieces of data.
        /// </summary>
        /// <param name="pieces">The number of pieces to go back.</param>
        /// <param name="endDate">The time to work back from (now is the default)</param>
        /// <returns>The calculated start date.</returns>
        /// <remarks>
        /// The function computes the date that would need to be passed to get the specified number
        /// of pieces based on the length of the current KlineType value.
        /// </remarks>
        DateTime GetStartDate(int pieces, DateTime? endDate = null);
        
        /// <summary>
        /// Gets the current kline start time calculated by the kline type.
        /// </summary>
        /// <returns></returns>
        DateTime GetCurrentKlineStartTime();

        string ToString(string format);

    }


}
