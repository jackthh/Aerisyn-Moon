using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// Bake record written after a successful Pull All: the active Environment Key, when it ran, the
    /// Source Sheet each Feature Key was read from (id, title, Drive revision), and a content hash per
    /// Baked Asset. CI and build helpers compare it to the required Config Type list. Pull All overwrites
    /// it; do not edit by hand.
    /// </summary>
    [CreateAssetMenu(
        fileName = "ConfigManifest",
        menuName = "Aerisyn/Data Config Sheet/Config Manifest",
        order = 4)]
    [InfoBox(
        "Written by Pull All. Records which environment, Source Sheet revisions, and Baked Asset hashes " +
        "were baked. Do not edit by hand.",
        InfoMessageType.Info)]
    public sealed class ConfigManifest : ScriptableObject
    {


        #region Serialized rows

        /// <summary>Serialized twin of <see cref="ManifestFeatureEntry"/> (Unity cannot serialize the immutable pure type).</summary>
        [Serializable]
        public sealed class FeatureRow
        {


            [SerializeField]
            string _featureKey = "";


            [SerializeField]
            string _spreadsheetId = "";


            [SerializeField]
            string _sheetTitle = "";


            [SerializeField]
            string _driveModifiedTime = "";


            [SerializeField]
            string _driveVersion = "";


            public FeatureRow(ManifestFeatureEntry entry)
            {
                _featureKey = entry.FeatureKey;
                _spreadsheetId = entry.SpreadsheetId;
                _sheetTitle = entry.SheetTitle;
                _driveModifiedTime = entry.DriveModifiedTime;
                _driveVersion = entry.DriveVersion;
            }


            public ManifestFeatureEntry ToEntry()
            {
                return new ManifestFeatureEntry(_featureKey, _spreadsheetId, _sheetTitle, _driveModifiedTime, _driveVersion);
            }


        }


        /// <summary>Serialized twin of <see cref="ManifestAssetEntry"/>.</summary>
        [Serializable]
        public sealed class AssetRow
        {


            [SerializeField]
            string _configTypeName = "";


            [SerializeField]
            string _assetPath = "";


            [SerializeField]
            string _contentHash = "";


            public AssetRow(ManifestAssetEntry entry)
            {
                _configTypeName = entry.ConfigTypeName;
                _assetPath = entry.AssetPath;
                _contentHash = entry.ContentHash;
            }


            public ManifestAssetEntry ToEntry()
            {
                return new ManifestAssetEntry(_configTypeName, _assetPath, _contentHash);
            }


        }

        #endregion


        #region Fields

        [ReadOnly]
        [SerializeField]
        string _environmentKey = "";


        [ReadOnly]
        [SerializeField]
        string _generatedAtUtc = "";


        [ReadOnly]
        [SerializeField]
        List<FeatureRow> _features = new List<FeatureRow>();


        [ReadOnly]
        [SerializeField]
        List<AssetRow> _assets = new List<AssetRow>();

        #endregion


        #region Public API

        /// <summary>Replaces every field from the snapshot (Pull All overwrite).</summary>
        public void Apply(ConfigManifestSnapshot snapshot)
        {
            if (snapshot == null)
                throw new ArgumentNullException(nameof(snapshot));

            _environmentKey = snapshot.EnvironmentKey;
            _generatedAtUtc = snapshot.GeneratedAtUtc;

            _features = new List<FeatureRow>(snapshot.Features.Count);
            for (int i = 0; i < snapshot.Features.Count; i++)
                _features.Add(new FeatureRow(snapshot.Features[i]));

            _assets = new List<AssetRow>(snapshot.Assets.Count);
            for (int i = 0; i < snapshot.Assets.Count; i++)
                _assets.Add(new AssetRow(snapshot.Assets[i]));
        }


        /// <summary>Maps the stored rows back into the pure snapshot for completeness checks.</summary>
        public ConfigManifestSnapshot ToSnapshot()
        {
            List<ManifestFeatureEntry> features = new List<ManifestFeatureEntry>(_features.Count);
            for (int i = 0; i < _features.Count; i++)
                features.Add(_features[i].ToEntry());

            List<ManifestAssetEntry> assets = new List<ManifestAssetEntry>(_assets.Count);
            for (int i = 0; i < _assets.Count; i++)
                assets.Add(_assets[i].ToEntry());

            return new ConfigManifestSnapshot(_environmentKey, _generatedAtUtc, features, assets);
        }

        #endregion


    }
}
