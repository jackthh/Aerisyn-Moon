using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Aerisyn.Quests.Authoring
{
    /// <summary>
    /// A Board authored as an asset: a stable name plus the Quests that open and close together.
    /// Hand <see cref="BoardId"/> and <see cref="BuildDefinitions"/> to <see cref="QuestTracker.OpenBoard"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "QuestBoard", menuName = "Aerisyn/Quests/Quest Board", order = 1)]
    public sealed class QuestBoardAsset : ScriptableObject
    {
        #region Serialized fields

        [Tooltip("Stable name used as the BoardId (e.g. stage.1.3, daily). Changing it orphans saved progress.")]
        [FormerlySerializedAs("_boardId")]
        [SerializeField] private string _boardName;

        [Tooltip("Quests on this Board. Local ids must be unique within the list.")]
        [SerializeField] private QuestAsset[] _quests = new QuestAsset[0];

        #endregion

        #region Public API

        /// <summary>The raw Board name as authored. May be empty on a misconfigured asset; see <see cref="Validate"/>.</summary>
        public string BoardName => _boardName;

        /// <summary>Typed Board identity. Throws when <see cref="BoardName"/> is empty.</summary>
        public BoardId BoardId => new BoardId(_boardName);

        public IReadOnlyList<QuestAsset> Quests => _quests;

        /// <summary>Build definitions in asset order. Throws if any Quest slot is empty or invalid.</summary>
        public QuestDefinition[] BuildDefinitions()
        {
            var definitions = new QuestDefinition[_quests.Length];
            for (var slotIndex = 0; slotIndex < _quests.Length; slotIndex++)
            {
                if (_quests[slotIndex] == null)
                    throw new InvalidOperationException(name + ": quest slot " + slotIndex + " is empty.");

                definitions[slotIndex] = _quests[slotIndex].Build();
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

            if (string.IsNullOrEmpty(_boardName))
            {
                errors?.Add(name + ": Board Id is empty.");
                isValid = false;
            }

            // Keep scanning after the first problem so the inspector can list everything at once.
            var seenLocalIds = new HashSet<int>();
            for (var slotIndex = 0; slotIndex < _quests.Length; slotIndex++)
            {
                var questAsset = _quests[slotIndex];
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
                    errors?.Add(name + ": duplicate quest local id " + questAsset.LocalId + " (" + questAsset.name + ").");
                    isValid = false;
                }
            }

            return isValid;
        }

        #endregion

        #region Obsolete aliases (removed in 0.2.0)

        [Obsolete("Renamed to QuestBoardAsset.BoardName for clarity. This alias is removed in 0.2.0.")]
        public string BoardIdValue => BoardName;

        [Obsolete("Renamed to QuestBoardAsset.BoardId for clarity. This alias is removed in 0.2.0.")]
        public BoardId Id => BoardId;

        #endregion
    }
}
