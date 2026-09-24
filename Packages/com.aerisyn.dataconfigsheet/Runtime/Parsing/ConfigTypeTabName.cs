#nullable disable
using System;
using System.Reflection;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// Resolves the Google Source Sheet tab title for a Config Type:
    /// <see cref="SheetTabAttribute"/> when present, otherwise the type name.
    /// </summary>
    public static class ConfigTypeTabName
    {


        /// <summary>Exact tab title Pull will fetch for this Config Type.</summary>
        public static string Resolve(Type configType)
        {
            if (configType == null)
                throw new ArgumentNullException(nameof(configType));

            object[] attrs = configType.GetCustomAttributes(typeof(SheetTabAttribute), inherit: false);
            if (attrs != null && attrs.Length > 0)
            {
                SheetTabAttribute tab = attrs[0] as SheetTabAttribute;
                if (tab != null)
                    return tab.TabTitle;
            }

            return configType.Name;
        }


    }
}
