using DataTools.PinEntry.Observable;

using Kucoin.NET.Data.Market;
using Kucoin.NET.Data.User;
using Kucoin.NET.Rest;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace KuCoinApp.ViewModels
{
    public class AccountItemViewModel : ObservableBase
    {

        private Account source;
        private CurrencyViewModel currency;

        private decimal quoteAmount;

        public Account Source
        {
            get => source;
            set
            {
                if (SetProperty(ref source, value))
                {

                    Currency = CurrencyViewModel.GetCurrency(source.Currency);

                    var pis = typeof(AccountItemViewModel).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                    foreach (var pi in pis)
                    {
                        if (pi.Name != nameof(Source) && pi.Name != nameof(Currency))
                        {
                            OnPropertyChanged(pi.Name);
                        }
                    }
                }
            }
        }

        public AccountItemViewModel()
        {

        }

        public AccountItemViewModel(Account source)
        {
            Source = source;
        }

        public CurrencyViewModel Currency
        {
            get => currency;
            set
            {
                SetProperty(ref currency, value);
            }
        }

       
        public string TypeDescription => source?.TypeDescription;

        public string CurrencyName => currency?.Currency?.Currency;

        public string CurrencyFullName => currency?.Currency?.FullName;

        public decimal Balance => source?.Balance ?? 0M;
        public decimal Holds => source?.Holds ?? 0M;
        public decimal Available => source?.Available ?? 0M;

        public decimal QuoteAmount
        {
            get => quoteAmount;
            set
            {
                SetProperty(ref quoteAmount, value);
            }
        }

        public void UpdateQuoteAmount(decimal quoteAmount)
        {
            QuoteAmount = Balance * quoteAmount;
        }

        public async Task UpdateQuoteAmount(string quoteCurrency)
        {
            var market = new Market();
            var ticker = await market.GetTicker($"{CurrencyName}-{quoteCurrency}");

            QuoteAmount = Balance * ticker.Price;
        }

        public ImageSource Image
        {
            get
            {
                if (currency != null)
                {
                    if (!currency.ImageLoaded)
                    {
                        _ = currency.LoadImage().ContinueWith((t) => OnPropertyChanged(nameof(Image)));
                        return null;
                    }

                    return currency.Image;
                }
                else
                {
                    return null;
                }
            }
        }



    }
}
