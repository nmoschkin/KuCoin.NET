
using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace KuCoin.NET.Data.User
{
    /// <summary>
    /// Paginated deposit list
    /// </summary>
    class DepositListPage : PaginatedData<DepositListItem>
    {

    }


    class WithdrawalListPage : PaginatedData<Withdrawal>
    {
    }
}
