using System;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// Optional designer-friendly Header Row name for a Field Header.
    /// Pull accepts the C# field name or this alias when matching and parsing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class ColumnAliasAttribute : Attribute
    {


        /// <summary>Sheet header text designers may use instead of the field name.</summary>
        public string Alias { get; }


        public ColumnAliasAttribute(string alias)
        {
            if (string.IsNullOrWhiteSpace(alias))
                throw new ArgumentException("Column Alias must be non-empty.", nameof(alias));

            Alias = alias.Trim();
        }


    }
}
