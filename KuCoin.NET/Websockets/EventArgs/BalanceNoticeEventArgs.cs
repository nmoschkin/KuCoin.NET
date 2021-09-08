using KuCoin.NET.Data.Websockets.User;

using System;
using System.Collections.Generic;
using System.Text;

namespace KuCoin.NET.Websockets
{
    public class BalanceNoticeEventArgs : EventArgs
    {
        /// <summary>
        /// The event data. 
        /// </summary>
        public BalanceNotice EventData { get; private set; }

        /// <summary>
        /// The type of the event that was received.
        /// </summary>
        public RelationEventType RelationEvent { get; private set; }

        /// <summary>
        /// Instantiate a new instance of this class.
        /// </summary>
        /// <param name="data">The balance notice event data.</param>
        public BalanceNoticeEventArgs(BalanceNotice data)
        {
            EventData = data;
            RelationEvent = data.RelationEvent;
        }

    }
}
