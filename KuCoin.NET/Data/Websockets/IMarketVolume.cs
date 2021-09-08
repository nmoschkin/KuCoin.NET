using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace KuCoin.NET.Data.Websockets
{
    /// <summary>
    /// An object that has a market volume.
    /// </summary>
    public interface IMarketVolume
    {

        /// <summary>
        /// Market volume.
        /// </summary>
        decimal MarketVolume { get; }

        /// <summary>
        /// Gets or sets a value indicating that market volume tracking is enabled.
        /// </summary>
        bool IsVolumeEnabled { get; set; }

    }

}
