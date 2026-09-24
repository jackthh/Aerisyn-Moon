#nullable disable
using System;
using System.Collections.Generic;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// Header Row text plus Ignore Marker guidance, ready to paste into Google Sheets.
    /// </summary>
    public sealed class HeaderEmitResult
    {


        /// <summary>Ordered header cells (Column Alias when set, else Field Header name).</summary>
        public IReadOnlyList<string> HeaderRow { get; }


        /// <summary>How designers mark notes-only columns with !!! above the Header Row.</summary>
        public string IgnoreMarkerGuidance { get; }


        public HeaderEmitResult(IReadOnlyList<string> headerRow, string ignoreMarkerGuidance)
        {
            HeaderRow = headerRow ?? throw new ArgumentNullException(nameof(headerRow));
            IgnoreMarkerGuidance = ignoreMarkerGuidance ?? "";
        }


        /// <summary>Tab-separated Header Row for paste into a spreadsheet.</summary>
        public string ToTabSeparatedRow()
        {
            return string.Join("\t", HeaderRow);
        }


    }
}
