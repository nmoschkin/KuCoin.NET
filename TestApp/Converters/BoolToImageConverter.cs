using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace KuCoinApp.Converters
{
    public class BoolToImageConverter : IValueConverter
    {

        public string TrueImage { get; set; }

        public string FalseImage { get; set; }


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object bytes;

            if (value is bool b && b == true) bytes = CoinResources.ResourceManager.GetObject(TrueImage);

            else bytes = CoinResources.ResourceManager.GetObject(FalseImage);


            var stream = new MemoryStream((byte[])bytes);
            
            var img = BitmapFrame.Create(stream,
                                    BitmapCreateOptions.None,
                                    BitmapCacheOption.OnLoad);

            return img;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
