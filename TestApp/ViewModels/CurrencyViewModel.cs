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

        public bool ImageLoaded
        {
            get => bmp != null;
        }

        public MarketCurrency Currency
        {
            get => curr;
            private set
            {
                SetProperty(ref curr, value);
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

        public CurrencyViewModel(MarketCurrency mc, bool loadImage = true)
        {
            Currency = mc;

            if (loadImage)
            {
                LoadImage();
            }
        }

        public void LoadImage()
        {
            try
            {
                byte[] b = (byte[])AppResources.ResourceManager.GetObject(curr.Currency.ToLower());
                var stream = new MemoryStream(b);

                if (stream != null)
                {
                    Image = BitmapFrame.Create(stream,
                                        BitmapCreateOptions.None,
                                        BitmapCacheOption.OnLoad);
                }
            }
            catch { }

        }

    }
}
