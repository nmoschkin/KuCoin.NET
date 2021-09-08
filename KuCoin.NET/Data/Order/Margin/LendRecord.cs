
using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace KuCoin.NET.Data.Order.Margin
{
    /// <summary>
    /// Paginated lend records
    /// </summary>
    public class LendRecord : PaginatedData<LendOrder>
    {
    }

    /// <summary>
    /// Paginated historical lend records
    /// </summary>
    public class HistoricalLendRecord : PaginatedData<HistoricalLendOrder>
    {
    }

    /// <summary>
    /// Paginated unsettled lend records
    /// </summary>
    public class UnsettledLendRecord : PaginatedData<UnsettledLendOrder>
    {
    }

    /// <summary>
    /// Paginated settled lend records
    /// </summary>
    public class SettledLendRecord : PaginatedData<SettledLendOrder>
    {
    }

    /// <summary>
    /// Paginated account lend records
    /// </summary>
    public class AccountLendRecord : PaginatedData<AccountLendRecordItem>
    {
    }




}
