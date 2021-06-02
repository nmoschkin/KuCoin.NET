using Kucoin.NET.Data.Market;
using Kucoin.NET.Observable;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KuCoinApp.ViewModels
{
    public class SymbolViewModel : ObservableBase
    {

        private TradingSymbol data;

        private CurrencyViewModel basevm;

        private CurrencyViewModel quotevm;

        public SymbolViewModel(TradingSymbol sym)
        {
            data = sym;

            basevm = CurrencyViewModel.GetCurrency(sym.BaseCurrency, true);
            quotevm = CurrencyViewModel.GetCurrency(sym.QuoteCurrency, true);
        }

        public TradingSymbol Data => data;

        public string Symbol => data?.Symbol;

        public CurrencyViewModel BaseCurrency => basevm;
        public CurrencyViewModel QuoteCurrency => quotevm;

        public override string ToString()
        {
            return Symbol;
        }

    }
}
