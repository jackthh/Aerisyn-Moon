using System;
using System.IO;
using UnityEngine;

namespace Aerisyn.DataConfigSheet.Editor
{
    /// <summary>Shared project-relative path helpers for bake and OAuth.</summary>
    public static class DataConfigPathUtility
    {
        /// <summary>Maps a project-relative path (Assets/... or UserSettings/...) to an absolute filesystem path.</summary>
        public static string ResolveProjectPath(string projectRelativePath)
        {
            if (string.IsNullOrWhiteSpace(projectRelativePath))
                throw new ArgumentException("Path is empty.", nameof(projectRelativePath));

            string normalized = projectRelativePath.Replace('\\', '/').Trim();
            if (Path.IsPathRooted(normalized))
                return normalized;

            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", normalized));
        }


        public static string NormalizeAssetPath(string path)
        {
            return path.Replace('\\', '/').TrimEnd('/');
        }
    }
}
