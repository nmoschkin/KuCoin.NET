


namespace KuCoin.NET.Websockets.Distribution
{
    /// <summary>
    /// Diagnostic data provider.
    /// </summary>
    public interface IFeedDiagnostics
    {
        /// <summary>
        /// Gets the current queue length.
        /// </summary>
        int QueueLength { get; }

        /// <summary>
        /// Gets or sets a value indicating that diagnostics are enabled for this object.
        /// </summary>
        bool DiagnosticsEnabled { get; set; }
        
        /// <summary>
        /// Running count of total number of objects that have been processed.
        /// </summary>
        long GrandTotal { get; }

        /// <summary>
        /// Gets the matches per second.
        /// </summary>
        long MatchesPerSecond { get; }

        /// <summary>
        /// Gets the running count of the total number of matches.
        /// </summary>
        long MatchTotal { get; }

        /// <summary>
        /// Gets the transactions per second.
        /// </summary>
        long TransactionsPerSecond { get; }

        /// <summary>
        /// Gets the throughput in bits per second.
        /// </summary>
        long Throughput { get; }

    }
}