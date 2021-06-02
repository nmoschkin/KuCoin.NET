using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

using KuCoinApp.Localization.Resources;

namespace KuCoinApp.Localization
{
    [AttributeUsage(AttributeTargets.Property)]
    public class TranslateAttribute : Attribute
    {
        const string ResourceId = "KuCoinApp.Localization.Resources.AppResources";

        static readonly Lazy<ResourceManager> ResMgr = new Lazy<ResourceManager>(
            () => new ResourceManager(ResourceId, IntrospectionExtensions.GetTypeInfo(typeof(AppResources)).Assembly));

        public string ResourceKey { get; private set; }
        public TranslateAttribute(string resourceKey)
        {
            ResourceKey = resourceKey;
        }

        public string GetText(CultureInfo ci = null)
        {
            if (ci == null) ci = CultureInfo.CurrentCulture;

            return ResMgr.Value.GetString(ResourceKey, ci);
        }

    }
}
