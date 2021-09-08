using KuCoin.NET.Data.User;
using KuCoin.NET.Futures.Data.User;
using KuCoin.NET.Helpers;
using KuCoin.NET.Rest;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Futures.Rest
{
    /// <summary>
    /// Futures user account information
    /// </summary>
    public class FuturesUser : FuturesBaseRestApi
    {
        /// <summary>
        /// Create a new Futures User Manager class with the specified credentials.
        /// </summary>
        /// <param name="credProvider">An object that implements <see cref="ICredentialsProvider"/>.</param>
        public FuturesUser(ICredentialsProvider credProvider) : base(credProvider)
        {
        }

        /// <summary>
        /// Create a new Futures User Manager class with the specified credentials.
        /// </summary>
        /// <param name="key">API Key</param>
        /// <param name="secret">API Secret</param>
        /// <param name="passphrase">API Passphrase</param>
        /// <param name="isSandbox">Is sandbox / not real-time.</param>
        public FuturesUser(
            string key, 
            string secret, 
            string passphrase, 
            bool isSandbox = false) 
            : base(
                key, 
                secret, 
                passphrase, 
                isSandbox)
        {
        }

        /// <summary>
        /// Get accounts overview
        /// </summary>
        /// <param name="currency">The currency of the account to retrieve (default is XBT.)</param>
        /// <returns></returns>
        public async Task<FuturesAccount> GetAccountOverview(FuturesCurrency? currency = FuturesCurrency.XBT)
        {
            var dict = new Dictionary<string, object>();

            if (currency != null)
            {
                dict.Add("currency", currency);

            }

            var url = "/api/v1/account-overview";

            var jobj = await MakeRequest(HttpMethod.Get, url, reqParams: dict);

            return jobj.ToObject<FuturesAccount>();
        }

        /// <summary>
        /// Get deposit address information for the specified currency
        /// </summary>
        /// <param name="currency">The currency (XBT or USDT)</param>
        /// <returns></returns>
        public async Task<AccountAddress> GetDepositAddress(FuturesCurrency? currency = FuturesCurrency.XBT)
        {
            var dict = new Dictionary<string, object>();

            if (currency != null)
            {
                dict.Add("currency", currency);

            }

            var url = "/api/v1/deposit-address";

            var jobj = await MakeRequest(HttpMethod.Get, url, reqParams: dict);

            return jobj.ToObject<AccountAddress>();

        }

        /// <summary>
        /// List transactions with the specified optional parameters
        /// </summary>
        /// <param name="currency">The trading currency (XBT or USDT).</param>
        /// <param name="forward">True if returning data in forward order.</param>
        /// <param name="offset">Offset</param>
        /// <param name="type">The transaction type</param>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <param name="maxCount">Max returned objects</param>
        /// <returns></returns>
        public async Task<TransactionHistory> GetTransactions(
            string currency = null, 
            bool forward = true, 
            long? offset = null,
            TransactionType? type = null, 
            DateTime? startTime = null, 
            DateTime? endTime = null, 
            int maxCount = 50)
        {
            var param = new Dictionary<string, object>();

            param.Add("forward", forward);
            param.Add("maxCount", maxCount);

            long st, et;
            DateTime d;

            if (currency != null)
            {
                param.Add("currency", currency);
            }

            if (type != null)
            {
                param.Add("type", type);
            }

            if (offset != null)
            {
                param.Add("offset", offset);
            }

            if (startTime == null)
            {
                st = 0;
            }
            else
            {
                d = (DateTime)startTime;
                st = EpochTime.DateToMilliseconds(d);
                param.Add("startAt", st);
            }

            if (endTime == null)
            {
                et = 0;
            }
            else
            {
                d = (DateTime)endTime;
                et = EpochTime.DateToMilliseconds(d);
                param.Add("endAt", et);
            }

            if (startTime != null && endTime != null && et < st) throw new ArgumentException("End time must be greater than start time");

            var url = "/api/v1/transaction-history";

            var jobj = await MakeRequest(HttpMethod.Get, url, reqParams: param);

            return jobj.ToObject<TransactionHistory>();

        }

        /// <summary>
        /// Gets a list of deposits based on the specified parameters.
        /// </summary>
        /// <param name="currency">The trading currency (XBT or USDT).</param>
        /// <param name="status">Deposit status</param>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <returns></returns>
        public async Task<IList<Deposit>> GetDepositsList(
            string currency = null,
            DepositStatus? status = null,
            DateTime? startTime = null,
            DateTime? endTime = null)
        {
            var param = new Dictionary<string, object>();

            long st, et;
            DateTime d;

            if (currency != null)
            {
                param.Add("currency", currency);
            }

            if (status != null)
            {
                param.Add("status", status);
            }

            if (startTime == null)
            {
                st = 0;
            }
            else
            {
                d = (DateTime)startTime;
                st = EpochTime.DateToMilliseconds(d);
                param.Add("startAt", st);
            }

            if (endTime == null)
            {
                et = 0;
            }
            else
            {
                d = (DateTime)endTime;
                et = EpochTime.DateToMilliseconds(d);
                param.Add("endAt", et);
            }

            if (startTime != null && endTime != null && et < st) throw new ArgumentException("End time must be greater than start time");

            var url = "/api/v1/deposit-list";

            return await GetAllPaginatedResults<Deposit, DepositList>(HttpMethod.Get, url, reqParams: param);

        }
    }
}
