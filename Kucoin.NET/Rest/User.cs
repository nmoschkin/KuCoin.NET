using Kucoin.NET.Data.Interfaces;
using Kucoin.NET.Data.User;
using Kucoin.NET.Helpers;
using Kucoin.NET.Json;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Kucoin.NET.Rest
{
    /// <summary>
    /// User actions
    /// </summary>
    public class User : KucoinBaseRestApi
    {
        public User(ICredentialsProvider credProvider, bool isSandbox = false) : base(credProvider, isSandbox)
        {
        }

        public User(string key, string secret, string passphrase, bool isSandbox = false) : base(key, secret, passphrase, isSandbox)
        {
        }


        /// <summary>
        /// Refresh an account.
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public async Task RefreshAccount(Account account)
        {
            var jobj = await MakeRequest(HttpMethod.Get, "/api/v1/accounts/" + account.Id);
            JsonConvert.PopulateObject(jobj.ToString(), account);
        }

        public async Task<Account> GetAccount(string accountId)
        {
            var jobj = await MakeRequest(HttpMethod.Get, "/api/v1/accounts/" + accountId);
            return jobj.ToObject<Account>();
        }

        public async Task<SubAccount[]> GetSubUsers()
        {

            var jobj = await MakeRequest(HttpMethod.Get, "/api/v1/sub/user");
            return jobj.ToObject<SubAccount[]>();

        }

        public async Task<Account> CreateAccount(string currency, AccountType type)
        {
            // /api/v1/accounts


            var dict = new Dictionary<string, object>();

            dict.Add("type", type);
            dict.Add("currency", currency);

            var jobj = await MakeRequest(HttpMethod.Post, "/api/v1/accounts", reqParams: dict);

            var result = new Account();

            result.Id = jobj["id"].ToObject<string>();
            result.Type = type;
            result.Currency = currency;

            return result;
        }

        public async Task<ObservableCollection<Account>> GetAccountList(string currency = null, AccountType? type = null)
        {
            var param = new Dictionary<string, object>();

            if (currency != null)
            {
                param.Add("currency", currency);
            }

            if (type != null)
            {
                param.Add("type", (string)type);
            }

            JToken jobj = null;

            try
            {
                if (param.Count > 0)
                {
                    jobj = await MakeRequest(HttpMethod.Get, "/api/v1/accounts", reqParams: param);
                }
                else
                {
                    jobj = await MakeRequest(HttpMethod.Get, "/api/v1/accounts");
                }
            }
            catch (Exception ex)
            {
                var s = ex;
            }

            return jobj?.ToObject<ObservableCollection<Account>>();
        }

        public async Task UpdateAccountList(ObservableCollection<Account> accounts, string currency = null, AccountType? type = null)
        {
            var accts = await GetAccountList(currency, type);
            int i, c;

            foreach (var acct in accts)
            {
                var aex = accounts.Where((a) => a.Id == acct.Id)?.FirstOrDefault();
                if (aex == null)
                {
                    accounts.Add(aex);
                }
                else
                {
                    i = accounts.IndexOf(aex);
                    accounts[i] = acct;
                }
            }

            c = accounts.Count;

            for(i = c - 1; i >= 0; i--)
            {
                var aex = accts.Where((a) => a.Id == accounts[i].Id)?.FirstOrDefault();

                if (aex == null)
                {
                    accounts.RemoveAt(i);
                }
            }
        }

        public async Task<IList<AccountLedgerItem>> GetAccountLedger(string currency = null, BizType? bizType = null, TransactionDirection? direction = null, DateTime? startTime = null, DateTime? endTime = null, int pageSize = 50)
        {
            var l = new List<AccountLedgerItem>();
            long st, et;
            int cp = 1;
            DateTime d;
            var param = new Dictionary<string, object>();

            param.Add("pageSize", pageSize);
            param.Add("currentPage", cp);

            if (currency != null)
            {
                param.Add("currency", currency);
            }

            if (bizType != null)
            {
                param.Add("bizType", EnumToStringConverter<BizType>.GetEnumName((BizType)bizType));
            }

            if (direction != null)
            {
                param.Add("direction", EnumToStringConverter<TransactionDirection>.GetEnumName((TransactionDirection)bizType));
            }

            if (startTime == null)
            {
                st = 0;
            }
            else
            {
                d = (DateTime)startTime;
                st = EpochTime.DateToSeconds(d);
                param.Add("startAt", st);
            }

            if (endTime == null)
            {
                et = 0;
            }
            else
            {
                d = (DateTime)startTime;
                et = EpochTime.DateToSeconds(d);
                param.Add("endAt", et);
            }

            if (startTime != null && endTime != null && et < st) throw new ArgumentException("End time must be greater than start time");

            return await GetAllPaginatedResults<AccountLedgerItem, AccountLedgerPage>(HttpMethod.Get, $"/api/v1/accounts/ledgers", reqParams: param);

        }

    }
}
