#nullable disable
using System;
using System.Threading.Tasks;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// Source Sheet revision fields from Drive (<c>modifiedTime</c>, <c>version</c>); both empty when
    /// unavailable. Pure seam type so tests inject fakes instead of calling Drive.
    /// </summary>
    public readonly struct SheetRevisionMetadata
    {


        public SheetRevisionMetadata(string modifiedTime, string version)
        {
            ModifiedTime = modifiedTime ?? "";
            Version = version ?? "";
        }


        /// <summary>Drive modifiedTime (RFC 3339) or empty.</summary>
        public string ModifiedTime { get; }


        /// <summary>Drive version or empty.</summary>
        public string Version { get; }


        /// <summary>Metadata with no fields, used when Drive is unavailable.</summary>
        public static SheetRevisionMetadata Empty => new SheetRevisionMetadata("", "");


    }


    /// <summary>
    /// Best-effort Drive metadata lookup behind an injectable fetcher. The Editor injects a Drive
    /// <c>files.get</c> call (supportsAllDrives=true); tests inject fakes.
    /// </summary>
    public static class SheetRevisionLookup
    {


        /// <summary>
        /// Fetches revision metadata for the spreadsheet id. A null fetcher or any fetch failure returns
        /// empty metadata: the manifest records revision fields "when available", and a Drive hiccup
        /// must not fail a Pull whose Sheets read already succeeded.
        /// </summary>
        public static async Task<SheetRevisionMetadata> TryFetchAsync(
            Func<string, Task<SheetRevisionMetadata>> fetchAsync,
            string spreadsheetId)
        {
            if (fetchAsync == null)
                return SheetRevisionMetadata.Empty;

            try
            {
                return await fetchAsync(spreadsheetId);
            }
            catch (Exception)
            {
                return SheetRevisionMetadata.Empty;
            }
        }


    }
}
