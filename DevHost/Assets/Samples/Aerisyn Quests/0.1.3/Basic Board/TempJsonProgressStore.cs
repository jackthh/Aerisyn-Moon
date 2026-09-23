using System;
using System.IO;
using UnityEngine;

namespace Aerisyn.Quests.Samples.BasicBoard
{
    /// <summary>
    /// TEMPORARY sample-only persistence so the demo survives a restart. Writes one JSON file per Board
    /// under Application.persistentDataPath. This is not the official save contract for Aerisyn Quests;
    /// production games hand <see cref="ProgressSnapshot"/> arrays to their own save pipeline
    /// (see ADR-0001 in the Aerisyn-Moon repo).
    /// </summary>
    public static class TempJsonProgressStore
    {
        #region File format

        // JsonUtility cannot serialize a bare array, so wrap it. The field name "Quests" is the JSON key; keep it.
        [Serializable]
        private sealed class SaveFileEnvelope
        {
            public ProgressSnapshot[] Quests = new ProgressSnapshot[0];
        }

        private static string GetSaveFilePath(BoardId boardId) =>
            Path.Combine(Application.persistentDataPath, "aerisyn-quests-sample." + boardId.Value + ".json");

        #endregion

        #region Public API

        /// <summary>Overwrite the Board's save file with <paramref name="snapshots"/>.</summary>
        public static void Save(BoardId boardId, ProgressSnapshot[] snapshots)
        {
            var json = JsonUtility.ToJson(new SaveFileEnvelope { Quests = snapshots }, prettyPrint: true);
            File.WriteAllText(GetSaveFilePath(boardId), json);
        }

        /// <summary>Returns null when nothing was saved yet.</summary>
        public static ProgressSnapshot[] Load(BoardId boardId)
        {
            var saveFilePath = GetSaveFilePath(boardId);
            if (!File.Exists(saveFilePath))
                return null;

            var envelope = JsonUtility.FromJson<SaveFileEnvelope>(File.ReadAllText(saveFilePath));
            return envelope != null ? envelope.Quests : null;
        }

        /// <summary>Remove the Board's save file if it exists.</summary>
        public static void Delete(BoardId boardId)
        {
            var saveFilePath = GetSaveFilePath(boardId);
            if (File.Exists(saveFilePath))
                File.Delete(saveFilePath);
        }

        #endregion
    }
}
