#nullable disable
using System;
using System.Collections.Generic;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// Header Row text plus designer guidance, ready to paste into Google Sheets.
    /// </summary>
    public sealed class HeaderEmitResult
    {


        /// <summary>Ordered header cells (Column Alias when set, else Field Header name).</summary>
        public IReadOnlyList<string> HeaderRow { get; }


        /// <summary>
        /// How designers mark notes-only columns with !!! above the Header Row,
        /// plus a Local Only omit note when any Local Only fields were left out.
        /// </summary>
        public string IgnoreMarkerGuidance { get; }


        /// <summary>How many Local Only fields were omitted from the pasteable Header Row.</summary>
        public int LocalOnlyOmittedCount { get; }


        public HeaderEmitResult(IReadOnlyList<string> headerRow, string ignoreMarkerGuidance)
            : this(headerRow, ignoreMarkerGuidance, localOnlyOmittedCount: 0)
        {
        }


        public HeaderEmitResult(
            IReadOnlyList<string> headerRow,
            string ignoreMarkerGuidance,
            int localOnlyOmittedCount)
        {
            HeaderRow = headerRow ?? throw new ArgumentNullException(nameof(headerRow));
            IgnoreMarkerGuidance = ignoreMarkerGuidance ?? "";
            LocalOnlyOmittedCount = localOnlyOmittedCount < 0 ? 0 : localOnlyOmittedCount;
        }


        /// <summary>Tab-separated Header Row for paste into a spreadsheet.</summary>
        public string ToTabSeparatedRow()
        {
            return string.Join("\t", HeaderRow);
        }


    }
}
