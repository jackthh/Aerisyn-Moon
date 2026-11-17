using System;
using System.Threading.Tasks;
using Google.Apis.Drive.v3;

namespace Aerisyn.DataConfigSheet.Editor
{
    /// <summary>
    /// Drive <c>files.get</c> implementation of the revision-metadata seam used by the Config Manifest.
    /// Viewer access to the Source Sheet is enough.
    /// </summary>
    public static class GoogleDriveRevisionFetcher
    {


        /// <summary>
        /// Reads <c>modifiedTime</c> and <c>version</c> for the spreadsheet id. supportsAllDrives=true so
        /// sheets that live in a shared drive resolve instead of returning 404.
        /// </summary>
        public static async Task<SheetRevisionMetadata> FetchAsync(DriveService drive, string spreadsheetId)
        {
            if (drive == null)
                throw new ArgumentNullException(nameof(drive));

            FilesResource.GetRequest request = drive.Files.Get(spreadsheetId.Trim());
            request.Fields = "modifiedTime,version";
            request.SupportsAllDrives = true;
            Google.Apis.Drive.v3.Data.File file = await request.ExecuteAsync();

            // ModifiedTimeRaw keeps the RFC 3339 text Drive returned (no local-time conversion).
            string modifiedTime = file?.ModifiedTimeRaw ?? "";
            string version = file?.Version != null ? file.Version.Value.ToString() : "";
            return new SheetRevisionMetadata(modifiedTime, version);
        }


    }
}
