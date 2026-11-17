#nullable disable
using System;
using System.Collections.Generic;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// First-create folder rule: given a project-relative asset path under Assets, returns the folders
    /// that do not exist yet as (parent, name) pairs, parents before children, ready for
    /// AssetDatabase.CreateFolder.
    /// </summary>
    public static class AssetFolderChain
    {


        /// <summary>
        /// Walks the folders of <paramref name="assetPath"/> (everything before the file name) and returns the
        /// missing ones. Throws InvalidOperationException when the path is not under "Assets".
        /// </summary>
        public static List<(string Parent, string Name)> MissingFolders(string assetPath, Func<string, bool> folderExists)
        {
            if (folderExists == null)
                throw new ArgumentNullException(nameof(folderExists));

            string normalized = (assetPath ?? "").Replace('\\', '/').Trim();
            string[] parts = normalized.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2 || parts[0] != "Assets")
                throw new InvalidOperationException($"Asset path '{assetPath}' must be under 'Assets/'.");

            List<(string Parent, string Name)> missing = new List<(string Parent, string Name)>();
            string parent = "Assets";
            // The last part is the file name; only folders before it matter.
            for (int i = 1; i < parts.Length - 1; i++)
            {
                string folder = parent + "/" + parts[i];
                if (!folderExists(folder))
                    missing.Add((parent, parts[i]));
                parent = folder;
            }

            return missing;
        }


    }
}
