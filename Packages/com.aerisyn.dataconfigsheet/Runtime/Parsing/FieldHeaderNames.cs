#nullable disable
using System.Collections.Generic;
using System.Reflection;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// Resolves Header Row names for a field: canonical Field Header plus optional Column Alias.
    /// </summary>
    public static class FieldHeaderNames
    {


        /// <summary>Optional Column Alias text, or null when the field has none.</summary>
        public static string GetAlias(FieldInfo field)
        {
            if (field == null)
                return null;

            object[] attrs = field.GetCustomAttributes(typeof(ColumnAliasAttribute), inherit: true);
            if (attrs == null || attrs.Length == 0)
                return null;

            ColumnAliasAttribute alias = attrs[0] as ColumnAliasAttribute;
            return alias != null ? alias.Alias : null;
        }


        /// <summary>
        /// Preferred paste/emit header: Column Alias when set, otherwise the Field Header name.
        /// </summary>
        public static string GetEmitName(FieldInfo field)
        {
            string alias = GetAlias(field);
            return alias ?? field.Name;
        }


        /// <summary>
        /// Looks up a column by Field Header name first, then by Column Alias.
        /// </summary>
        public static bool TryFindColumn(FieldInfo field, Dictionary<string, int> headersByName, out int column)
        {
            column = -1;
            if (field == null || headersByName == null)
                return false;

            if (headersByName.TryGetValue(field.Name, out column))
                return true;

            string alias = GetAlias(field);
            if (alias != null && headersByName.TryGetValue(alias, out column))
                return true;

            return false;
        }


    }
}
