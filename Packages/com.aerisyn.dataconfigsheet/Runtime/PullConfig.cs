using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using System.Threading.Tasks;
#endif
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// Editor settings for one Pull job: Feature Key (spreadsheet id and auth come from Data Config Settings),
    /// one shared Baked Asset output folder, and owned Config Type candidates with Include In Pull.
    /// One-way: Google → Vertical Nest parse → Baked Assets. Never push back to Google.
    /// </summary>
    [CreateAssetMenu(
        fileName = "PullConfig",
        menuName = "Aerisyn/Data Config Sheet/Pull Config",
        order = 0)]
    [InfoBox(
        "Pull → ScriptableObject (ADR 0010). List Config Types explicitly; tabs match type name by default " +
        "(optional [SheetTab] override). Optional [ColumnAlias] on fields for designer headers. " +
        "One shared output folder for all Baked Assets. Never push generated data back to Google.",
        InfoMessageType.Info)]
    [InfoBox(
        "Spreadsheet id and Google auth come from Data Config Settings (Aerisyn > Data Config Sheet). " +
        "Share the sheet with your Google email as Viewer.",
        InfoMessageType.Info)]
    public sealed class PullConfig : SerializedScriptableObject
    {


        #region Inspector actions (editor-wired)

#if UNITY_EDITOR
        /// <summary>
        /// Editor module assigns the shared Pull-with-UI workflow here so the Inspector
        /// button and menu items cannot diverge (Runtime cannot reference the Editor asm).
        /// </summary>
        public static Func<PullConfig, Task> RunEditorPullAsync;


        /// <summary>
        /// Editor module assigns a loader for the Data Config Settings asset (null when missing) so the
        /// Feature Key dropdown works without Runtime referencing AssetDatabase.
        /// </summary>
        public static Func<DataConfigSettings> EditorSettingsLoader;


        // Sign In / Sign Out live in the Data Config Sheet window now (auth is on Data Config Settings).
        // A missing settings asset or signed-out state surfaces as a clear failure from the Pull workflow.
        [Button("Pull", ButtonSizes.Large)]
        [PropertyOrder(-100)]
        void InspectorPull()
        {
            if (RunEditorPullAsync == null)
            {
                Debug.LogError(
                    "[DataConfigSheet] Editor Pull workflow is not registered. " +
                    "Is the Data Config Sheet Editor assembly loaded?");
                return;
            }

            _ = RunEditorPullAsync(this);
        }
#endif

        #endregion


        #region Google source

        [FoldoutGroup("Google source")]
        [Tooltip(
            "Feature Key for this Pull. The Source Sheet spreadsheet id is resolved from " +
            "Data Config Settings (active Environment Key x Feature Key) at Pull time.")]
        [ValueDropdown(nameof(FeatureKeyChoices), DropdownTitle = "Feature Key")]
        [SerializeField]
        string _featureKey = "";


        // Default false: Title Check stays on. Only for documented exception sheets whose title does not
        // carry the environment prefix (e.g. a shared CONVENTIONS sheet).
        [FoldoutGroup("Google source")]
        [Tooltip(
            "Skip the Title Check for this Pull Config. Leave off unless the Source Sheet is a documented " +
            "exception whose title does not start with the active environment's prefix (e.g. CONVENTIONS). " +
            "The manifest records an empty title for a skipped sheet.")]
        [SerializeField]
        bool _skipTitleCheck;

        #endregion


        #region Pull targets

        [FoldoutGroup("Pull targets")]
        [Tooltip("Shared folder for all Baked Assets from this Pull Config (project-relative).")]
        [FolderPath(RequireExistingPath = false)]
        [SerializeField]
        string _outputFolder = "";


        // Odin must own this list: Unity [Serializable] entries drop System.Type, so a Unity
        // SerializeField list only ever persisted Include In Pull and left _configType null.
        // CustomAddFunction avoids null slots (Odin default for reference lists → "None" rows).
        [FoldoutGroup("Pull targets")]
        [Tooltip(
            "Owned Config Type candidates. Include In Pull (checkbox) gates fetch and bake; " +
            "unticked types stay listed. Google tab title matches the type name by default, " +
            "or [SheetTab(\"...\")] when titles differ.")]
        [ListDrawerSettings(
            ShowIndexLabels = true,
            DraggableItems = true,
            ShowFoldout = false,
            CustomAddFunction = nameof(CreateConfigTypeEntry))]
        [OdinSerialize, NonSerialized]
        List<PullConfigTypeEntry> _configTypeEntries = new List<PullConfigTypeEntry>();

        #endregion


        #region Picker helpers

        // Feature Key choices come only from Data Config Settings' Feature Key list (no free text).
        // Empty when the settings asset does not exist yet (opening the window creates it) or the Editor loader is unset.
        IEnumerable<string> FeatureKeyChoices()
        {
#if UNITY_EDITOR
            DataConfigSettings settings = EditorSettingsLoader?.Invoke();
            if (settings != null)
                return settings.FeatureKeys;
#endif
            return Array.Empty<string>();
        }

        #endregion


        #region List helpers

        // List + must construct an entry; otherwise Odin inserts null and draws None (Type).
        static PullConfigTypeEntry CreateConfigTypeEntry()
        {
            return new PullConfigTypeEntry();
        }

        #endregion


        #region Public API

        /// <summary>Feature Key picked for this Pull (empty until picked).</summary>
        public string FeatureKey => _featureKey;


        /// <summary>True when this Pull Config opts out of the Title Check (default false = check on).</summary>
        public bool SkipTitleCheck => _skipTitleCheck;


        /// <summary>Shared output folder for Baked Assets from this Pull.</summary>
        public string OutputFolder => _outputFolder;


        /// <summary>Owned Config Type candidates (included and unticked).</summary>
        public IReadOnlyList<PullConfigTypeEntry> ConfigTypeEntries => _configTypeEntries;


        /// <summary>
        /// Builds pure inclusion candidates for PullTargetRules (validation, fetch, inject).
        /// </summary>
        public PullTypeCandidate[] ToPullTypeCandidates()
        {
            List<PullConfigTypeEntry> entries = _configTypeEntries;
            if (entries == null || entries.Count == 0)
                return Array.Empty<PullTypeCandidate>();

            PullTypeCandidate[] candidates = new PullTypeCandidate[entries.Count];
            for (int i = 0; i < entries.Count; i++)
            {
                PullConfigTypeEntry entry = entries[i];
                if (entry == null)
                {
                    candidates[i] = new PullTypeCandidate(null, includeInPull: false);
                    continue;
                }

                candidates[i] = entry.ToCandidate();
            }

            return candidates;
        }

        #endregion


    }
}
