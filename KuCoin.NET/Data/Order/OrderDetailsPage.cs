
using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

namespace KuCoin.NET.Data.Order
{
    /// <summary>
    /// Paginated order details
    /// </summary>
    public class OrderDetailsPage : PaginatedData<OrderDetails>
    {
    }

    /// <summary>
    /// Paginated order fills
    /// </summary>
    public class FillPage : PaginatedData<Fill>
    {
    }
}
