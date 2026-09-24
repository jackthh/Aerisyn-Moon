using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using UnityEngine;

namespace Aerisyn.DataConfigSheet.Editor
{
    /// <summary>
    /// Downloads Google Sheet tabs as CSV files into the Luban data directory.
    /// </summary>
    public static class GoogleSheetsCsvExporter
    {
        #region Public API

        /// <summary>
        /// Exports each BakeConfig sheet mapping to a CSV under luban dataDir.
        /// </summary>
        public static async Task ExportAsync(BakeConfig config, string lubanDataDir)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            if (string.IsNullOrWhiteSpace(config.SpreadsheetId))
                throw new InvalidOperationException($"BakeConfig '{config.name}' has an empty Spreadsheet Id.");
            if (config.SheetExports == null || config.SheetExports.Length == 0)
                throw new InvalidOperationException(
                    $"BakeConfig '{config.name}' has no Tab export entries. Add tab title → CSV file mappings.");

            Directory.CreateDirectory(lubanDataDir);

            SheetsService sheets = await GoogleOAuthSession.CreateSheetsServiceAsync(config);
            for (int i = 0; i < config.SheetExports.Length; i++)
            {
                BakeConfig.SheetExportEntry entry = config.SheetExports[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.TabTitle))
                    throw new InvalidOperationException($"Sheet export at index {i} has an empty Tab Title.");
                if (string.IsNullOrWhiteSpace(entry.CsvFileName))
                    throw new InvalidOperationException($"Sheet export '{entry.TabTitle}' has an empty CSV file name.");

                string csvPath = Path.Combine(lubanDataDir, entry.CsvFileName.Trim());
                await ExportTabToCsvAsync(sheets, config.SpreadsheetId.Trim(), entry.TabTitle.Trim(), csvPath);
                Debug.Log($"[DataConfigSheet] Exported tab '{entry.TabTitle}' → '{csvPath}'");
            }
        }

        #endregion


        #region CSV write

        /// <summary>
        /// Reads the full tab via Sheets API and writes RFC-style CSV (quotes when needed).
        /// </summary>
        static async Task ExportTabToCsvAsync(
            SheetsService sheets,
            string spreadsheetId,
            string tabTitle,
            string csvPath)
        {
            // A:ZZ covers typical config width; empty trailing cells are trimmed per row
            string range = $"{EscapeSheetName(tabTitle)}!A:ZZ";
            SpreadsheetsResource.ValuesResource.GetRequest request =
                sheets.Spreadsheets.Values.Get(spreadsheetId, range);
            ValueRange response = await request.ExecuteAsync();

            IList<IList<object>> rows = response?.Values;
            var builder = new StringBuilder(4096);
            if (rows != null)
            {
                for (int r = 0; r < rows.Count; r++)
                {
                    IList<object> row = rows[r];
                    int lastNonEmpty = FindLastNonEmptyIndex(row);
                    for (int c = 0; c <= lastNonEmpty; c++)
                    {
                        if (c > 0)
                            builder.Append(',');

                        string cell = c < row.Count && row[c] != null ? row[c].ToString() : string.Empty;
                        builder.Append(EscapeCsvField(cell));
                    }

                    builder.Append('\n');
                }
            }

            string directory = Path.GetDirectoryName(csvPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(csvPath, builder.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }


        static int FindLastNonEmptyIndex(IList<object> row)
        {
            if (row == null || row.Count == 0)
                return -1;

            for (int i = row.Count - 1; i >= 0; i--)
            {
                if (row[i] != null && !string.IsNullOrEmpty(row[i].ToString()))
                    return i;
            }

            return -1;
        }


        static string EscapeSheetName(string tabTitle)
        {
            // Sheets A1 notation: quote names that need it; escape internal quotes by doubling
            if (tabTitle.IndexOfAny(new[] { ' ', '\'', '!', ':' }) >= 0)
                return "'" + tabTitle.Replace("'", "''") + "'";

            return tabTitle;
        }


        static string EscapeCsvField(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            bool needsQuotes =
                value.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0;
            if (!needsQuotes)
                return value;

            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        #endregion
    }
}
