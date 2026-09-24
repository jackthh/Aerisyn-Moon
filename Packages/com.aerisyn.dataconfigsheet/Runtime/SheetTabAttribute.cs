using System;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// Optional Google tab title when it must differ from the Config Type name.
    /// Default tab resolution is the type name; this override is exact match only (no suffix guessing).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class SheetTabAttribute : Attribute
    {


        /// <summary>Exact Source Sheet tab title for this Config Type.</summary>
        public string TabTitle { get; }


        public SheetTabAttribute(string tabTitle)
        {
            if (string.IsNullOrWhiteSpace(tabTitle))
                throw new ArgumentException("Sheet tab title must be non-empty.", nameof(tabTitle));

            TabTitle = tabTitle.Trim();
        }


    }
}
