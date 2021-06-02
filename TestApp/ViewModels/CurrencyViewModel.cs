using Kucoin.NET.Data.Market;
using Kucoin.NET.Observable;
using Kucoin.NET.Rest;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace KuCoinApp
{
    public class CurrencyViewModel : ObservableBase
    {
        BitmapSource bmp = null;
        MarketCurrency curr;

        static Dictionary<string, BitmapSource> imgCache = new Dictionary<string, BitmapSource>();

        public static IReadOnlyList<MarketCurrency> QuoteCurrencies { get; private set; }

        public static CurrencyViewModel GetCurrency(string currency, bool loadImage = true)
        {
            foreach (var curr in Currencies)
            {
                if (curr.Currency == currency)
                {
                    return new CurrencyViewModel(curr, loadImage);
                }
            }

            return null;
        }

        public static IReadOnlyList<MarketCurrency> Currencies { get; private set; }

        public static async Task UpdateCurrencies()
        {
            var market = new Market();

            await market.RefreshCurrenciesAsync();
            await market.RefreshSymbolsAsync();

            QuoteCurrencies = await market.GetAllQuoteCurrencies();

            var l = new List<MarketCurrency>(market.Currencies);

            l.Sort((a, b) =>
            {
                return string.Compare(a.Currency, b.Currency);
            });

            l = l.Where((c) =>
            {
                return ((IList<TradingSymbol>)market.Symbols).Where((d) =>
                {
                    return d.BaseCurrency == c.Currency;
                })?.FirstOrDefault() != null;
            }).ToList();

            App.Current?.Dispatcher?.Invoke(() =>
            {
                Currencies = l;
            });

        }

        public bool ImageLoaded
        {
            get => bmp != null;
        }

        public string CurrencyName => curr?.Currency;

        public MarketCurrency Currency
        {
            get => curr;
            private set
            {
                if (SetProperty(ref curr, value))
                {
                    OnPropertyChanged(nameof(CurrencyName));
                }
            }
        }

        public BitmapSource Image
        {
            get => bmp;
            private set
            {
                if (SetProperty(ref bmp, value))
                {
                    OnPropertyChanged(nameof(ImageLoaded));
                }
            }
        }

        public CurrencyViewModel(string currency, bool loadImage = true)
        {
            if (Currencies == null)
            {
                throw new ArgumentNullException("Currencies must be initialized before instantiation.");
            }
            else
            {
                foreach (var curr in Currencies)
                {
                    if (curr.Currency == currency)
                    {
                        Currency = curr;
                        return;
                    }
                }
            }
        }

        public CurrencyViewModel(MarketCurrency mc, bool loadImage = true)
        {
            Currency = mc;

            if (loadImage)
            {
                _ = LoadImage();
            }
        }

        public async Task LoadImage()
        {
            await Task.Run(() =>
            {
                try
                {
                    string c = curr.Currency.ToLower();
                    
                    lock(imgCache)
                    {
                        if (imgCache.ContainsKey(c))
                        {
                            App.Current?.Dispatcher?.Invoke(() =>
                            {
                                Image = imgCache[c];
                            });

                            return;
                        }

                        byte[] b = (byte[])CoinResources.ResourceManager.GetObject(c);

                        if (b == null)
                        {
                            return;
                        }

                        App.Current?.Dispatcher?.Invoke(() =>
                        {
                            var stream = new MemoryStream(b);
                            if (stream != null)
                            {
                                Image = BitmapFrame.Create(stream,
                                                    BitmapCreateOptions.None,
                                                    BitmapCacheOption.OnLoad);

                            }
                        });

                        imgCache.Add(c, bmp);
                    }

                }
                catch { }
            });

        }

    }
}
