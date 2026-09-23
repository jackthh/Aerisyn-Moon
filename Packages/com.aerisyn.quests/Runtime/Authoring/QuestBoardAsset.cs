using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Aerisyn.Quests.Authoring
{
    /// <summary>
    /// A Board authored as an asset (Odin): a stable name plus the Quests that open and close together.
    /// Hand <see cref="BoardId"/> and <see cref="BuildDefinitions"/> to <see cref="QuestTracker.OpenBoard"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "QuestBoard", menuName = "Aerisyn/Quests/Quest Board", order = 1)]
    [InfoBox("@InspectorErrorSummary", InfoMessageType.Error, nameof(HasInspectorErrors))]
    [InfoBox("@InspectorSuccessSummary", InfoMessageType.Info, nameof(IsInspectorValid))]
    public sealed class QuestBoardAsset : ScriptableObject
    {
        #region Serialized fields

        [FoldoutGroup("Board")]
        [LabelText("Board Id")]
        [Tooltip("Stable name used as the BoardId (e.g. stage.1.3, daily). Changing it orphans saved progress.")]
        [Required]
        [SerializeField]
        private string boardName;

        [FoldoutGroup("Board")]
        [Tooltip("Quests on this Board. Local ids must be unique within the list.")]
        [ListDrawerSettings(ShowIndexLabels = true, DraggableItems = true)]
        [AssetsOnly]
        [SerializeField]
        private QuestAsset[] quests = Array.Empty<QuestAsset>();

        #endregion


        #region Inspector status (Odin)

        // Reused when Odin evaluates InfoBox members so repaint does not allocate a new list each time.
        [NonSerialized] private readonly List<string> _inspectorErrors = new List<string>();


        private bool IsInspectorValid => Validate(null);


        private bool HasInspectorErrors => !IsInspectorValid;


        private string InspectorErrorSummary
        {
            get
            {
                _inspectorErrors.Clear();
                Validate(_inspectorErrors);
                return string.Join("\n", _inspectorErrors);
            }
        }


        // boardName is safe here: success InfoBox only shows when Validate passed.
        private string InspectorSuccessSummary =>
            "Valid board '" + boardName + "' with " + quests.Length + " quest(s).";

        #endregion


        #region Public API

        /// <summary>
        /// The Board identity, built from the authored name. Throws when the name is empty,
        /// so call <see cref="Validate"/> first on assets that may be misconfigured.
        /// </summary>
        public BoardId BoardId => new BoardId(boardName);


        public IReadOnlyList<QuestAsset> Quests => quests;


        /// <summary>Build definitions in asset order. Throws if any Quest slot is empty or invalid.</summary>
        public QuestDefinition[] BuildDefinitions()
        {
            var definitions = new QuestDefinition[quests.Length];
            for (var slotIndex = 0; slotIndex < quests.Length; slotIndex++)
            {
                if (quests[slotIndex] == null)
                    throw new InvalidOperationException(name + ": quest slot " + slotIndex + " is empty.");

                definitions[slotIndex] = quests[slotIndex].Build();
            }

            return definitions;
        }


        /// <summary>
        /// Append human-readable problems to <paramref name="errors"/> (may be null): empty board name,
        /// empty slots, invalid Quests, duplicate local ids. Returns true when none were found.
        /// Duplicate detection mirrors what the Tracker enforces on open.
        /// </summary>
        public bool Validate(List<string> errors)
        {
            var isValid = true;

            if (string.IsNullOrEmpty(boardName))
            {
                errors?.Add(name + ": Board Id is empty.");
                isValid = false;
            }

            // Keep scanning after the first problem so the inspector can list everything at once.
            var seenLocalIds = new HashSet<int>();
            for (var slotIndex = 0; slotIndex < quests.Length; slotIndex++)
            {
                var questAsset = quests[slotIndex];
                if (questAsset == null)
                {
                    errors?.Add(name + ": quest slot " + slotIndex + " is empty.");
                    isValid = false;
                    continue;
                }

                if (!questAsset.Validate(errors))
                    isValid = false;

                if (!seenLocalIds.Add(questAsset.LocalId))
                {
                    errors?.Add(name + ": duplicate quest local id " + questAsset.LocalId + " (" + questAsset.name +
                                ").");
                    isValid = false;
                }
            }

            return isValid;
        }

        #endregion
    }
}
