using System.IO;
using cfg;
using Luban.SimpleJSON;
using UnityEngine;

namespace Aerisyn.DataConfigSheet.DevHost
{
    /// <summary>
    /// DevHost smoke test: loads Luban Tables from GeneratedData JSON on Play.
    /// Attach to any scene object; check Console for item / nested counts.
    /// </summary>
    public sealed class DataConfigLubanSmoke : MonoBehaviour
    {
        #region Serialized

        [Tooltip("Project-relative folder with Luban JSON (tbitem.json, …).")]
        [SerializeField]
        string _jsonDataDir = "Assets/AerisynDataConfig/GeneratedData";

        #endregion


        #region Lifecycle

        void Start()
        {
            string absoluteDir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", _jsonDataDir));
            if (!Directory.Exists(absoluteDir))
            {
                Debug.LogError($"[DataConfig] JSON dir missing: '{absoluteDir}'. Run Bake From Google first.");
                return;
            }

            // Luban Tables loader: file name without extension → JSONNode
            var tables = new Tables(file =>
            {
                string path = Path.Combine(absoluteDir, file + ".json");
                string text = File.ReadAllText(path);
                return JSON.Parse(text);
            });

            int itemCount = tables.TbItem.DataList.Count;
            int nestedCount = tables.TbDemoNested.DataList.Count;
            Debug.Log(
                $"[DataConfig] Luban load OK. TbItem={itemCount}, TbDemoNested={nestedCount}. " +
                $"First item id='{tables.TbItem.DataList[0].Id}'.");
        }

        #endregion
    }
}
