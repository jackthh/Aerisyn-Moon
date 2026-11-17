#nullable disable
using System;
using System.Collections.Generic;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// Pull All folder scope: given the Data Config Settings folder path and candidate Pull Config asset
    /// paths, returns the ones under that folder (recursive) in stable ordinal order. A blank folder path
    /// fails; there is no whole-project fallback.
    /// </summary>
    public static class PullConfigFolderScope
    {


        /// <summary>
        /// Returns the asset paths under <paramref name="folderPath"/> (any depth), sorted ordinally.
        /// Matching is case-sensitive; backslashes and a trailing slash on either side are normalized.
        /// Throws InvalidOperationException when the folder path is null, empty or whitespace.
        /// </summary>
        public static string[] Select(string folderPath, IReadOnlyList<string> assetPaths)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
                throw new InvalidOperationException(
                    "Pull All folder is blank. Pick the folder that holds your Pull Configs in Data Config Settings.");

            if (assetPaths == null)
                throw new ArgumentNullException(nameof(assetPaths));

            string prefix = Normalize(folderPath).TrimEnd('/') + "/";
            List<string> selected = new List<string>();

            for (int i = 0; i < assetPaths.Count; i++)
            {
                string path = assetPaths[i];
                if (string.IsNullOrWhiteSpace(path))
                    continue;

                string normalized = Normalize(path);
                if (normalized.StartsWith(prefix, StringComparison.Ordinal))
                    selected.Add(normalized);
            }

            selected.Sort(StringComparer.Ordinal);
            return selected.ToArray();
        }


        /// <summary>
        /// Throws InvalidOperationException when <paramref name="selectedPaths"/> is empty, so a Pull All over
        /// a folder with no Pull Configs is a clear failure instead of an empty success.
        /// </summary>
        public static void RequireAny(string folderPath, IReadOnlyList<string> selectedPaths)
        {
            if (selectedPaths == null)
                throw new ArgumentNullException(nameof(selectedPaths));

            if (selectedPaths.Count == 0)
                throw new InvalidOperationException(
                    $"No Pull Configs found under Pull All folder '{folderPath?.Trim()}'. " +
                    "Create a Pull Config there (Assets → Create → Aerisyn → Data Config Sheet → Pull Config) " +
                    "or change the Pull All folder in Data Config Settings.");
        }


        static string Normalize(string path)
        {
            return path.Trim().Replace('\\', '/');
        }


    }
}
