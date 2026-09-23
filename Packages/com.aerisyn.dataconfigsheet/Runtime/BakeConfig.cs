using Sirenix.OdinInspector;
using UnityEngine;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// Editor bake settings: Google spreadsheet id(s), service-account credential path,
    /// ScriptableObject output folder, and optional CSV cache. One-way: Google → SO only.
    /// </summary>
    [CreateAssetMenu(
        fileName = "BakeConfig",
        menuName = "Aerisyn/Data Config Sheet/Bake Config",
        order = 0)]
    [InfoBox(
        "One-way bake only: Google Sheet is the official source of truth. " +
        "Local ScriptableObjects are runtime final and must never be pushed back to Google Sheets.",
        InfoMessageType.Warning)]
    public sealed class BakeConfig : ScriptableObject
    {


        #region Google source

        [FoldoutGroup("Google source")]
        [Tooltip("Google Spreadsheet id(s) from the sheet URL (.../d/{id}/...). One id is enough for MVP.")]
        [ListDrawerSettings(ShowIndexLabels = true, DraggableItems = true)]
        [SerializeField]
        string[] _spreadsheetIds = System.Array.Empty<string>();


        [FoldoutGroup("Google source")]
        [Tooltip("Project-relative path to the service-account JSON (Viewer on the sheet). Keep this file gitignored.")]
        [FilePath(Extensions = "json", RequireExistingPath = false)]
        [SerializeField]
        string _credentialPath = "Assets/AerisynDataConfig/Credentials/service-account.json";

        #endregion


        #region Outputs

        [FoldoutGroup("Outputs")]
        [Tooltip("Project-relative folder for ScriptableObjectSheetExporter output (runtime source of truth after bake).")]
        [FolderPath(RequireExistingPath = false)]
        [SerializeField]
        string _scriptableObjectOutputPath = "Assets/AerisynDataConfig/Baked";


        [FoldoutGroup("Outputs")]
        [Tooltip("Optional project-relative folder for a CSV cache after Google bake. Leave empty to skip.")]
        [FolderPath(RequireExistingPath = false)]
        [SerializeField]
        string _csvCachePath = "";

        #endregion


        #region Schema

        [FoldoutGroup("Schema")]
        [Tooltip("Game-owned factory that constructs the SheetContainer for this bake.")]
        [Required]
        [SerializeField]
        SheetContainerFactory _sheetContainerFactory;

        #endregion


        #region Public API

        /// <summary>Spreadsheet document ids to import (order preserved).</summary>
        public string[] SpreadsheetIds => _spreadsheetIds;


        /// <summary>Project-relative path to the service-account credential JSON.</summary>
        public string CredentialPath => _credentialPath;


        /// <summary>Project-relative output folder for ScriptableObject assets.</summary>
        public string ScriptableObjectOutputPath => _scriptableObjectOutputPath;


        /// <summary>Optional CSV cache folder; empty means no CSV store step.</summary>
        public string CsvCachePath => _csvCachePath;


        /// <summary>Factory that creates the game SheetContainer for bake.</summary>
        public SheetContainerFactory SheetContainerFactory => _sheetContainerFactory;

        #endregion


    }
}
