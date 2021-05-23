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

        private decimal? quoteAmount = null;
        private decimal? lastQuotePrice = null;

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

        public decimal Balance
        {
            get => source?.Balance ?? 0M;
            set
            {
                if (source == null || source.Balance == value) return;

                source.Balance = value;
                OnPropertyChanged(nameof(Balance));
                
                QuoteAmount = lastQuotePrice * value;
            }
        }

        public decimal Holds
        {
            get => source?.Holds ?? 0M;
            set
            {
                if (source == null || source.Holds == value) return;

                source.Holds = value;
                OnPropertyChanged(nameof(Holds));
            }
        }
        public decimal Available
        {
            get => source?.Available ?? 0M;
            set
            {
                if (source == null || source.Available == value) return;

                source.Available = value;
                OnPropertyChanged(nameof(Available));
            }
        }

        public decimal? QuoteAmount
        {
            get => quoteAmount;
            set
            {
                SetProperty(ref quoteAmount, value);
            }
        }

        public void UpdateQuoteAmount(decimal? quotePrice)
        {
            lastQuotePrice = quotePrice;
            QuoteAmount = quotePrice * Balance;
        }

        public async Task UpdateQuoteAmount(string quoteCurrency)
        {
            if (quoteCurrency == CurrencyName)
            {
                lastQuotePrice = 1;
                QuoteAmount = Balance;

                return;
            }

            var market = new Market();
            var ticker = await market.GetTicker($"{CurrencyName}-{quoteCurrency}");
            
            lastQuotePrice = ticker.Price;
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
