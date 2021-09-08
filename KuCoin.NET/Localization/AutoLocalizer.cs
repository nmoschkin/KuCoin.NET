using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Localization
{
    public static class AutoLocalizer
    {

        public static string Localize(string value, object callerInstance, [CallerMemberName]string propertyName = null)
        {
            var tn = string.Join(".", callerInstance.GetType().Name, propertyName, value);

            var str = AppResources.ResourceManager.GetString(tn);

            return str;
        }

        public static string Localize(string prefix, string value)
        {
            var tn = string.Join(".", prefix ?? throw new ArgumentNullException(), value ?? "");

            var str = AppResources.ResourceManager.GetString(tn);

            return str;
        }

    }
}
