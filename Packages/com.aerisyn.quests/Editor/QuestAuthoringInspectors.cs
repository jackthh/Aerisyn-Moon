using System.Collections.Generic;
using Aerisyn.Quests.Authoring;
using UnityEditor;
using UnityEngine;

namespace Aerisyn.Quests.Editor
{
    /// <summary>
    /// Inspectors for the authoring assets. They draw the default fields and surface the same
    /// validation the Tracker applies at OpenBoard, so mistakes show up while editing instead of at runtime.
    /// Read-only: nothing here mutates assets, so no Undo registration is required.
    /// </summary>
    [CustomEditor(typeof(QuestAsset))]
    public sealed class QuestAssetInspector : UnityEditor.Editor
    {
        private readonly List<string> _errors = new List<string>();

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            _errors.Clear();
            var asset = (QuestAsset)target;
            if (asset.Validate(_errors))
            {
                EditorGUILayout.HelpBox("Valid. Objective " + asset.Objective + ".", MessageType.Info);
                return;
            }

            EditorGUILayout.HelpBox(string.Join("\n", _errors), MessageType.Error);
        }
    }

    [CustomEditor(typeof(QuestBoardAsset))]
    public sealed class QuestBoardAssetInspector : UnityEditor.Editor
    {
        private readonly List<string> _errors = new List<string>();

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            _errors.Clear();
            var board = (QuestBoardAsset)target;
            if (board.Validate(_errors))
            {
                EditorGUILayout.HelpBox("Valid board '" + board.BoardIdValue + "' with " + board.Quests.Count + " quest(s).", MessageType.Info);
                return;
            }

            EditorGUILayout.HelpBox(string.Join("\n", _errors), MessageType.Error);
        }
    }
}
