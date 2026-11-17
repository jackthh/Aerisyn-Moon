#nullable disable
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// One Config Manifest feature row: which Source Sheet a Feature Key was pulled from, its file
    /// title, and the Drive revision fields (empty when Drive metadata was unavailable).
    /// </summary>
    public sealed class ManifestFeatureEntry
    {


        public ManifestFeatureEntry(
            string featureKey,
            string spreadsheetId,
            string sheetTitle,
            string driveModifiedTime,
            string driveVersion)
        {
            FeatureKey = featureKey ?? "";
            SpreadsheetId = spreadsheetId ?? "";
            SheetTitle = sheetTitle ?? "";
            DriveModifiedTime = driveModifiedTime ?? "";
            DriveVersion = driveVersion ?? "";
        }


        /// <summary>Feature Key the Pull Config picked (Feature Key list entry).</summary>
        public string FeatureKey { get; }


        /// <summary>Source Sheet spreadsheet id resolved from Data Config Settings.</summary>
        public string SpreadsheetId { get; }


        /// <summary>Source Sheet file title read by the Title Check; empty when the check was skipped.</summary>
        public string SheetTitle { get; }


        /// <summary>Drive <c>modifiedTime</c> (RFC 3339) or empty when unavailable.</summary>
        public string DriveModifiedTime { get; }


        /// <summary>Drive <c>version</c> or empty when unavailable.</summary>
        public string DriveVersion { get; }


    }


    /// <summary>One Config Manifest asset row: a Baked Asset written by Pull and its content hash.</summary>
    public sealed class ManifestAssetEntry
    {


        public ManifestAssetEntry(string configTypeName, string assetPath, string contentHash)
        {
            ConfigTypeName = configTypeName ?? "";
            AssetPath = assetPath ?? "";
            ContentHash = contentHash ?? "";
        }


        /// <summary>Config Type name; matches the Baked Asset file name and the required list entries.</summary>
        public string ConfigTypeName { get; }


        /// <summary>Project-relative path of the Baked Asset file.</summary>
        public string AssetPath { get; }


        /// <summary>Lowercase hex SHA-256 of the written Baked Asset file.</summary>
        public string ContentHash { get; }


    }


    /// <summary>
    /// Pure Config Manifest contents: active Environment Key, generation time, per-feature Source Sheet
    /// metadata, and a content hash per Baked Asset. The ScriptableObject stores and restores this.
    /// </summary>
    public sealed class ConfigManifestSnapshot
    {


        public ConfigManifestSnapshot(
            string environmentKey,
            string generatedAtUtc,
            IReadOnlyList<ManifestFeatureEntry> features,
            IReadOnlyList<ManifestAssetEntry> assets)
        {
            EnvironmentKey = environmentKey ?? "";
            GeneratedAtUtc = generatedAtUtc ?? "";
            Features = features ?? Array.Empty<ManifestFeatureEntry>();
            Assets = assets ?? Array.Empty<ManifestAssetEntry>();
        }


        #region Public API

        /// <summary>Environment Key that was active when the Pull All ran.</summary>
        public string EnvironmentKey { get; }


        /// <summary>Round-trip ISO 8601 UTC timestamp of the manifest write.</summary>
        public string GeneratedAtUtc { get; }


        public IReadOnlyList<ManifestFeatureEntry> Features { get; }


        public IReadOnlyList<ManifestAssetEntry> Assets { get; }

        #endregion


        #region Factories

        /// <summary>
        /// Builds the manifest from per-config Pull results. Only passed jobs contribute. A Feature Key
        /// shared by several Pull Configs is listed once (first run wins; same key resolves the same
        /// sheet in one environment). Asset rows keep run order.
        /// </summary>
        public static ConfigManifestSnapshot FromJobs(
            string environmentKey,
            DateTime generatedAtUtc,
            IReadOnlyList<PullJobResult> results)
        {
            if (results == null)
                throw new ArgumentNullException(nameof(results));

            List<ManifestFeatureEntry> features = new List<ManifestFeatureEntry>();
            HashSet<string> seenFeatureKeys = new HashSet<string>(StringComparer.Ordinal);
            List<ManifestAssetEntry> assets = new List<ManifestAssetEntry>();

            for (int i = 0; i < results.Count; i++)
            {
                PullJobResult result = results[i];
                if (!result.Success)
                    continue;

                if (result.Feature != null && seenFeatureKeys.Add(result.Feature.FeatureKey))
                    features.Add(result.Feature);

                assets.AddRange(result.Assets);
            }

            // "o" round-trips with a trailing Z for UTC so the text sorts and parses unambiguously.
            string timestamp = generatedAtUtc.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
            return new ConfigManifestSnapshot(environmentKey, timestamp, features, assets);
        }

        #endregion


    }
}
