using Kucoin.NET.Data.User;
using Kucoin.NET.Futures.Data.User;
using Kucoin.NET.Helpers;
using Kucoin.NET.Rest;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Kucoin.NET.Futures.Rest
{
    /// <summary>
    /// Futures user account information
    /// </summary>
    public class FuturesUser : FuturesBaseRestApi
    {
        public FuturesUser(ICredentialsProvider cred) : base(cred)
        {
        }

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
        /* 
         * 
         *  startAt	long	[Optional] Start time (millisecond)
            endAt	long	[Optional] End time (millisecond)
            type	String	[Optional] Type RealisedPNL-Realised profit and loss, Deposit-Deposit, Withdrawal-withdraw, Transferin-Transfer in, TransferOut-Transfer out
            offset	long	[Optional] Start offset
            maxCount	long	[Optional] Displayed size per page. The default size is 50
            currency	String	[Optional] Currency of transaction history XBT or USDT
            forward	boolean	[optional] This parameter functions to judge whether the lookup is forward or not. True means “yes” and False means “no”. This parameter is set as true by default
         * 
         */

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
