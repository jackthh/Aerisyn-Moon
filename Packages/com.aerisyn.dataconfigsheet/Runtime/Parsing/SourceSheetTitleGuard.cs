#nullable disable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// Title Check ordering seam for live Pull: read the Source Sheet title, require the active
    /// environment's prefix, and only then fetch grids. Title and grid fetchers are injected so
    /// tests run without Google and the Editor uses Sheets API implementations.
    /// </summary>
    public static class SourceSheetTitleGuard
    {


        #region Guarded fetch

        /// <summary>
        /// Resolves the expected prefix (so an unguarded custom environment fails before any network
        /// call), fetches the title for the spreadsheet id, throws InvalidOperationException on a
        /// prefix mismatch, and otherwise returns the result of fetchGridsAsync for the same id.
        /// A mismatch never reaches fetchGridsAsync, so nothing is parsed or written.
        /// </summary>
        public static async Task<TGrids> FetchAfterTitleCheckAsync<TGrids>(
            string spreadsheetId,
            string activeEnvironmentKey,
            IReadOnlyList<EnvironmentTitlePrefix> prefixOverrides,
            Func<string, Task<string>> fetchTitleAsync,
            Func<string, Task<TGrids>> fetchGridsAsync)
        {
            if (fetchTitleAsync == null)
                throw new ArgumentNullException(nameof(fetchTitleAsync));
            if (fetchGridsAsync == null)
                throw new ArgumentNullException(nameof(fetchGridsAsync));

            string expectedPrefix = TitleCheckRules.ResolveExpectedPrefix(activeEnvironmentKey, prefixOverrides);
            string actualTitle = await fetchTitleAsync(spreadsheetId);

            if (!TitleCheckRules.Matches(actualTitle, expectedPrefix))
            {
                throw new InvalidOperationException(
                    $"Title Check failed for Environment Key '{activeEnvironmentKey}': expected the Source Sheet " +
                    $"title to start with '{expectedPrefix}' but it is '{actualTitle}'. " +
                    $"Check the spreadsheet id bound in the Environment Profile (id '{spreadsheetId}'). " +
                    "No Baked Assets were written.");
            }

            return await fetchGridsAsync(spreadsheetId);
        }

        #endregion


    }
}
