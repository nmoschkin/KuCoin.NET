using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace KuCoinApp.Extensions
{
    public class DecimalFormatExtension : MarkupExtension, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string format = "0.00";

        private decimal? value;


        public decimal? Value
        {
            get => value;
            set
            {
                if (value != this.value)
                {
                    this.value = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Format
        {
            get => format;
            set
            {
                if (format != value)
                {
                    format = value;
                    OnPropertyChanged();
                }
            }
        }


        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Value?.ToString(Format);
        }

        private void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
