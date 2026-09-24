using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using UnityEngine;

namespace Aerisyn.DataConfigSheet.Editor
{
    /// <summary>
    /// Fetches Source Sheet tabs into in-memory <see cref="SheetGrid"/>s via the Sheets API.
    /// No CSV bridge: cell values (including commas) stay intact for Vertical Nest parse.
    /// </summary>
    public static class GoogleSheetsGridFetcher
    {


        #region Public API

        /// <summary>
        /// Downloads each Pull Config Type's tab (type name or <see cref="SheetTabAttribute"/>)
        /// into a grid keyed by Config Type.
        /// </summary>
        public static async Task<Dictionary<Type, SheetGrid>> FetchGridsAsync(PullConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            SheetsService sheets = await GoogleOAuthSession.CreateSheetsServiceAsync(config);
            string spreadsheetId = config.SpreadsheetId.Trim();
            Dictionary<Type, SheetGrid> grids = new Dictionary<Type, SheetGrid>();

            for (int i = 0; i < config.ConfigTypes.Length; i++)
            {
                Type configType = config.ConfigTypes[i];
                string tabTitle = ConfigTypeTabName.Resolve(configType);
                SheetGrid grid = await FetchTabAsync(sheets, spreadsheetId, tabTitle);
                grids[configType] = grid;
                Debug.Log($"[DataConfigSheet] Fetched tab '{tabTitle}' for Config Type '{configType.Name}' ({grid.RowCount} rows).");
            }

            return grids;
        }

        #endregion


        #region Sheets read

        /// <summary>
        /// Reads the full tab via Sheets API values (A:ZZ) into an in-memory grid.
        /// </summary>
        static async Task<SheetGrid> FetchTabAsync(SheetsService sheets, string spreadsheetId, string tabTitle)
        {
            string range = EscapeSheetName(tabTitle) + "!A:ZZ";
            SpreadsheetsResource.ValuesResource.GetRequest request =
                sheets.Spreadsheets.Values.Get(spreadsheetId, range);
            ValueRange response = await request.ExecuteAsync();

            IList<IList<object>> rows = response != null ? response.Values : null;
            if (rows == null || rows.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Source Sheet tab '{tabTitle}' is empty or missing. " +
                    "Confirm the tab title matches the Config Type name (or [SheetTab] override).");
            }

            return SheetGrid.FromObjectRows(rows);
        }


        /// <summary>
        /// Sheets A1 notation: quote names that need it; escape internal quotes by doubling.
        /// </summary>
        static string EscapeSheetName(string tabTitle)
        {
            if (tabTitle.IndexOfAny(new[] { ' ', '\'', '!', ':' }) >= 0)
                return "'" + tabTitle.Replace("'", "''") + "'";

            return tabTitle;
        }

        #endregion


    }
}
