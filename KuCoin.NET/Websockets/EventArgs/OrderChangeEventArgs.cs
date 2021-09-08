using KuCoin.NET.Data.Websockets.User;

using System;
using System.Collections.Generic;
using System.Text;

namespace KuCoin.NET.Websockets
{
    /// <summary>
    /// Event data received for a user's order status changed events.
    /// </summary>
    public class OrderChangeEventArgs : EventArgs
    {
        /// <summary>
        /// The event data. 
        /// </summary>
        /// <remarks>
        /// Consult the API documentation for information on which pieces are relevent for each event type.
        /// </remarks>
        public OrderChange EventData { get; private set; }

        /// <summary>
        /// The type of the event that was received.
        /// </summary>
        public OrderEventType Type { get; private set; }

        /// <summary>
        /// Instantiate a new instance of this class.
        /// </summary>
        /// <param name="data">The order change event data.</param>
        public OrderChangeEventArgs(OrderChange data)
        {
            EventData = data;
            Type = data.Type;
        }

    }
}
