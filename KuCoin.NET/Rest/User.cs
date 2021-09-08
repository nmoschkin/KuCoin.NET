using KuCoin.NET.Data.Order;
using KuCoin.NET.Data.User;
using KuCoin.NET.Helpers;
using KuCoin.NET.Json;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Rest
{
    /// <summary>
    /// User actions
    /// </summary>
    public class User : KucoinBaseRestApi
    {
        public User(ICredentialsProvider credProvider) : base(credProvider)
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

        public async Task<SubAccount[]> GetSubAccounts()
        {

            var jobj = await MakeRequest(HttpMethod.Get, "/api/v1/sub/user");
            return jobj.ToObject<SubAccount[]>();

        }

        public async Task<SubAccountBalances> GetSubAccountBalances(string subUsertId = null)
        {
            var url = "/api/v1/sub/sub-accounts";
            if (subUsertId != null) url += "/" + subUsertId;


            var jobj = await MakeRequest(HttpMethod.Get, url);
            return jobj.ToObject<SubAccountBalances>();

        }

        public async Task<TransferableBalance> GetTransferable(string currency, AccountType type)
        {
            var actt = type.AccountName.ToUpper();

            var dict = new Dictionary<string, object>();

            dict.Add("currency", currency);
            dict.Add("type", actt);

            var jobj = await MakeRequest(HttpMethod.Get, "/api/v1/accounts/transferable", reqParams: dict);
            return jobj.ToObject<TransferableBalance>();

        }

        public async Task<string> SubTransfer(string currency, decimal amount, TransactionDirection direction, string subUserId, AccountType? accountType = null, AccountType? subAccountType = null, string clientOid = null)
        {
            if (clientOid == null) clientOid = Guid.NewGuid().ToString("d");

            var dict = new Dictionary<string, object>();

            dict.Add("clientOid", clientOid);
            dict.Add("currency", currency);
            dict.Add("amount", amount.ToString());
            dict.Add("direction", direction == TransactionDirection.In ? "IN" : "OUT");
            dict.Add("subUserId", subUserId);

            if (accountType is AccountType a1)
            {
                dict.Add("accountType", a1.AccountName.ToUpper());
            }

            if (subAccountType is AccountType a2)
            {
                dict.Add("subAccountType", a2.AccountName.ToUpper());
            }

            var jobj = await MakeRequest(HttpMethod.Post, "/api/v1/accounts/sub-transfer", reqParams: dict);
            return jobj["orderId"].ToObject<string>();
        }

        public async Task<string> InnerTransfer(string currency, decimal amount, AccountType fromAccount, AccountType toAccount, string clientOid = null)
        {
            if (clientOid == null) clientOid = Guid.NewGuid().ToString("d");

            var dict = new Dictionary<string, object>();

            dict.Add("clientOid", clientOid);
            dict.Add("currency", currency);
            dict.Add("amount", amount.ToString());

            dict.Add("from", fromAccount.AccountName);
            dict.Add("to", toAccount.AccountName);

            var jobj = await MakeRequest(HttpMethod.Post, "/api/v1/accounts/inner-transfer", reqParams: dict);
            return jobj["orderId"].ToObject<string>();
        }

        public async Task<DepositAddress> CreateDepositAddress(string currency, string address, string chain = null)
        {
            var dict = new Dictionary<string, object>();

            dict.Add("currency", currency);
            dict.Add("address", address);
            if (chain != null) dict.Add("chain", chain);


            var jobj = await MakeRequest(HttpMethod.Post, "/api/v1/accounts/deposit-addresses", reqParams: dict);
            return jobj.ToObject<DepositAddress>();

        }

        public async Task<DepositAddress[]> GetDepositAddresses(string currency)
        {
            var dict = new Dictionary<string, object>();

            dict.Add("currency", currency);

            var jobj = await MakeRequest(HttpMethod.Get, "/api/v2/accounts/deposit-addresses", reqParams: dict);
            return jobj.ToObject<DepositAddress[]>();
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
                d = (DateTime)endTime;
                et = EpochTime.DateToSeconds(d);
                param.Add("endAt", et);
            }

            if (startTime != null && endTime != null && et < st) throw new ArgumentException("End time must be greater than start time");

            return await GetAllPaginatedResults<AccountLedgerItem, AccountLedgerPage>(HttpMethod.Get, $"/api/v1/accounts/ledgers", reqParams: param);

        }

        public async Task<IList<DepositListItem>> GetDepositList(string currency = null, DateTime? startTime = null, DateTime? endTime = null, DepositStatus? status = null, int pageSize = 50)
        {
            var l = new List<DepositListItem>();
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
                d = (DateTime)endTime;
                et = EpochTime.DateToSeconds(d);
                param.Add("endAt", et);
            }
            if (status != null)
            {
                param.Add("status", EnumToStringConverter<DepositStatus>.GetEnumName((DepositStatus)status));
            }


            if (startTime != null && endTime != null && et < st) throw new ArgumentException("End time must be greater than start time");

            return await GetAllPaginatedResults<DepositListItem, DepositListPage>(HttpMethod.Get, $"/api/v1/deposits", reqParams: param);

        }

        public async Task<IList<Withdrawal>> GetWithdrawalList(string currency = null, DateTime? startTime = null, DateTime? endTime = null, WithdrawalStatus? status = null, int pageSize = 50)
        {
            var l = new List<Withdrawal>();
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
                d = (DateTime)endTime;
                et = EpochTime.DateToSeconds(d);
                param.Add("endAt", et);
            }
            if (status != null)
            {
                param.Add("status", EnumToStringConverter<WithdrawalStatus>.GetEnumName((WithdrawalStatus)status));
            }


            if (startTime != null && endTime != null && et < st) throw new ArgumentException("End time must be greater than start time");

            return await GetAllPaginatedResults<Withdrawal, WithdrawalListPage>(HttpMethod.Get, $"/api/v1/withdrawals", reqParams: param);

        }

        public async Task<WithdrawalQuota> GetWithdrawalQuotas(string currency, string chain = null)
        {
            var dict = new Dictionary<string, object>();

            dict.Add("currency", currency);
            if (chain != null) dict.Add("chain", chain);

            var jobj = await MakeRequest(HttpMethod.Get, "/api/v1/quotas", reqParams: dict);
            return jobj.ToObject<WithdrawalQuota>();

        }

        public async Task<string> ApplyWithdrawal(string currency, decimal amount, AccountAddress address)
        {
            var dict = address.ToDict();

            dict.Add("currency", currency);
            dict.Add("amount", amount.ToString());

            var jobj = await MakeRequest(HttpMethod.Post, "/api/v1/withdrawals", reqParams: dict);
            return jobj["withdrawalId"].ToObject<string>();
        }

        public async Task CancelWithdrawal(string withdrawalId)
        {
            await MakeRequest(HttpMethod.Delete, $"/api/v1/withdrawals/{withdrawalId}");

        }

        public async Task<FeeRate> GetBaseFee()
        {

            var jobj = await MakeRequest(HttpMethod.Get, "/api/v1/base-fee");
            return jobj.ToObject<FeeRate>();
        }

        public async Task<FeeRate[]> GetActualFeeRates(IEnumerable<string> symbols)
        {
            var sb = new StringBuilder();

            foreach(var sym in symbols)
            {
                if (sb.Length > 0) sb.Append(",");
                sb.Append(sym);
            }

            var url = "/api/v1/trade-fees?symbols=" + sb.ToString();

            var jobj = await MakeRequest(HttpMethod.Get, url);
            return jobj.ToObject<FeeRate[]>();


        }

        public async Task<FeeRate> GetActualFeeRate(string symbol)
        {
            return (await GetActualFeeRates(new string[] { symbol }))[0];
        }

    }
}
