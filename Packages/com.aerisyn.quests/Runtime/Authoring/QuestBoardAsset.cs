using System.Collections.Generic;
using UnityEngine;

namespace Aerisyn.Quests.Authoring
{
    /// <summary>
    /// A Board authored as an asset: a stable id plus the Quests that open and close together.
    /// Hand <see cref="BuildDefinitions"/> to <see cref="QuestTracker.OpenBoard"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "QuestBoard", menuName = "Aerisyn/Quests/Quest Board", order = 1)]
    public sealed class QuestBoardAsset : ScriptableObject
    {
        #region Serialized fields

        [Tooltip("Stable name used as the BoardId (e.g. stage.1.3, daily). Changing it orphans saved progress.")]
        [SerializeField] private string _boardId;

        [SerializeField] private QuestAsset[] _quests = new QuestAsset[0];

        #endregion

        #region Public API

        public string BoardIdValue => _boardId;

        public BoardId Id => new BoardId(_boardId);

        public IReadOnlyList<QuestAsset> Quests => _quests;

        /// <summary>Build definitions in asset order. Throws if any Quest is null or invalid.</summary>
        public QuestDefinition[] BuildDefinitions()
        {
            var result = new QuestDefinition[_quests.Length];
            for (var i = 0; i < _quests.Length; i++)
            {
                if (_quests[i] == null)
                    throw new System.InvalidOperationException(name + ": quest slot " + i + " is empty.");

                result[i] = _quests[i].Build();
            }

            return result;
        }

        /// <summary>
        /// Append human-readable problems: empty board id, null slots, invalid Quests, duplicate local ids.
        /// Returns true when none were found. Duplicate detection mirrors what the Tracker enforces on open.
        /// </summary>
        public bool Validate(List<string> errors)
        {
            var ok = true;

            if (string.IsNullOrEmpty(_boardId))
            {
                errors?.Add(name + ": Board Id is empty.");
                ok = false;
            }

            var seen = new HashSet<int>();
            for (var i = 0; i < _quests.Length; i++)
            {
                var quest = _quests[i];
                if (quest == null)
                {
                    errors?.Add(name + ": quest slot " + i + " is empty.");
                    ok = false;
                    continue;
                }

                if (!quest.Validate(errors))
                    ok = false;

                if (!seen.Add(quest.LocalId))
                {
                    errors?.Add(name + ": duplicate quest local id " + quest.LocalId + " (" + quest.name + ").");
                    ok = false;
                }
            }

            return ok;
        }

        #endregion
    }
}
