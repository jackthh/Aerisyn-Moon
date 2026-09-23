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
        // JsonUtility cannot serialize a bare array, so wrap it.
        [Serializable]
        private sealed class Envelope
        {
            public ProgressSnapshot[] Quests = new ProgressSnapshot[0];
        }

        private static string PathFor(BoardId board) =>
            Path.Combine(Application.persistentDataPath, "aerisyn-quests-sample." + board.Value + ".json");

        public static void Save(BoardId board, ProgressSnapshot[] snapshots)
        {
            var json = JsonUtility.ToJson(new Envelope { Quests = snapshots }, prettyPrint: true);
            File.WriteAllText(PathFor(board), json);
        }

        /// <summary>Returns null when nothing was saved yet.</summary>
        public static ProgressSnapshot[] Load(BoardId board)
        {
            var path = PathFor(board);
            if (!File.Exists(path))
                return null;

            var envelope = JsonUtility.FromJson<Envelope>(File.ReadAllText(path));
            return envelope != null ? envelope.Quests : null;
        }

        public static void Delete(BoardId board)
        {
            var path = PathFor(board);
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
