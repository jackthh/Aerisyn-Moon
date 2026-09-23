using System.Collections.Generic;
using Aerisyn.Quests.Authoring;
using UnityEditor;
using UnityEngine;

namespace Aerisyn.Quests.Editor
{
    /// <summary>
    /// Inspector for <see cref="QuestAsset"/>. Draws the default fields and surfaces the same
    /// validation the Tracker applies at OpenBoard, so mistakes show up while editing instead of at runtime.
    /// Read-only: nothing here mutates assets, so no Undo registration is required.
    /// </summary>
    [CustomEditor(typeof(QuestAsset))]
    public sealed class QuestAssetInspector : UnityEditor.Editor
    {
        // Reused every repaint to avoid per-frame allocations.
        private readonly List<string> _validationErrors = new List<string>();

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            _validationErrors.Clear();
            var questAsset = (QuestAsset)target;
            if (questAsset.Validate(_validationErrors))
            {
                EditorGUILayout.HelpBox("Valid. Objective " + questAsset.Objective + ".", MessageType.Info);
                return;
            }

            EditorGUILayout.HelpBox(string.Join("\n", _validationErrors), MessageType.Error);
        }
    }

    /// <summary>
    /// Inspector for <see cref="QuestBoardAsset"/>. Same idea as <see cref="QuestAssetInspector"/>, plus
    /// board-level checks (empty name, empty slots, duplicate local ids). Read-only, so no Undo needed.
    /// </summary>
    [CustomEditor(typeof(QuestBoardAsset))]
    public sealed class QuestBoardAssetInspector : UnityEditor.Editor
    {
        // Reused every repaint to avoid per-frame allocations.
        private readonly List<string> _validationErrors = new List<string>();

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            _validationErrors.Clear();
            var boardAsset = (QuestBoardAsset)target;
            if (boardAsset.Validate(_validationErrors))
            {
                EditorGUILayout.HelpBox("Valid board '" + boardAsset.BoardId + "' with " + boardAsset.Quests.Count + " quest(s).", MessageType.Info);
                return;
            }

            EditorGUILayout.HelpBox(string.Join("\n", _validationErrors), MessageType.Error);
        }
    }
}
