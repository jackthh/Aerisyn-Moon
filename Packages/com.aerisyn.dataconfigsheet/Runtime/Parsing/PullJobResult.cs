#nullable disable
using System;
using System.Collections.Generic;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// Outcome of one Pull Config run inside Pull All: pass/fail plus the text designers read
    /// (PullReport body or thrown error message). A passed job may also carry the Config Manifest
    /// rows it produced. Pure seam type; the Editor maps PullReport into it.
    /// </summary>
    public sealed class PullJobResult
    {


        PullJobResult(
            string name,
            bool success,
            string detail,
            ManifestFeatureEntry feature,
            IReadOnlyList<ManifestAssetEntry> assets)
        {
            Name = name ?? "";
            Success = success;
            Detail = detail ?? "";
            Feature = feature;
            Assets = assets ?? Array.Empty<ManifestAssetEntry>();
        }


        #region Public API

        /// <summary>Pull Config name shown in the summary.</summary>
        public string Name { get; }


        /// <summary>True when the Pull wrote its Baked Assets.</summary>
        public bool Success { get; }


        /// <summary>Success summary or failure reason for this job.</summary>
        public string Detail { get; }


        /// <summary>Source Sheet metadata for the manifest; null for failed jobs or Pulls without it.</summary>
        public ManifestFeatureEntry Feature { get; }


        /// <summary>Baked Assets this job wrote, with content hashes; empty for failed jobs.</summary>
        public IReadOnlyList<ManifestAssetEntry> Assets { get; }

        #endregion


        #region Factories

        /// <summary>A passed job with no manifest metadata (Pulls that report no Source Sheet or Baked Asset rows).</summary>
        public static PullJobResult Passed(string name, string detail)
        {
            return new PullJobResult(name, true, detail, null, null);
        }


        /// <summary>A passed job carrying the Source Sheet row and Baked Asset hashes the Config Manifest records.</summary>
        public static PullJobResult Passed(
            string name,
            string detail,
            ManifestFeatureEntry feature,
            IReadOnlyList<ManifestAssetEntry> assets)
        {
            return new PullJobResult(name, true, detail, feature, assets);
        }


        /// <summary>A failed job; detail is the reason shown in the Pull All summary. Contributes nothing to the manifest.</summary>
        public static PullJobResult Failed(string name, string detail)
        {
            return new PullJobResult(name, false, detail, null, null);
        }

        #endregion


    }
}
