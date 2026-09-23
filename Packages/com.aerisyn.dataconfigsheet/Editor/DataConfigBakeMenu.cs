using System;
using UnityEditor;
using UnityEngine;

namespace Aerisyn.DataConfigSheet.Editor
{
    /// <summary>
    /// Editor menus for one-way Google → ScriptableObject bake via BakingSheet.
    /// </summary>
    public static class DataConfigBakeMenu
    {
        const string BakeSelectedMenu = "Aerisyn/Data Config Sheet/Bake From Google (Selected Config)";
        const string BakeAllMenu = "Aerisyn/Data Config Sheet/Bake From Google (All Configs)";


        #region Bake menus

        [MenuItem(BakeSelectedMenu, false, 100)]
        static async void BakeSelected()
        {
            BakeConfig config = Selection.activeObject as BakeConfig;
            if (config == null)
            {
                EditorUtility.DisplayDialog(
                    "Data Config Sheet",
                    "Select a BakeConfig asset in the Project window, then run this menu again.",
                    "OK");
                return;
            }

            await RunBakeSafe(config);
        }


        [MenuItem(BakeSelectedMenu, true)]
        static bool BakeSelectedValidate()
        {
            return Selection.activeObject is BakeConfig;
        }


        [MenuItem(BakeAllMenu, false, 101)]
        static async void BakeAll()
        {
            string[] guids = AssetDatabase.FindAssets("t:BakeConfig");
            if (guids == null || guids.Length == 0)
            {
                EditorUtility.DisplayDialog(
                    "Data Config Sheet",
                    "No BakeConfig assets found. Create one via Assets → Create → Aerisyn → Data Config Sheet → Bake Config.",
                    "OK");
                return;
            }

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                BakeConfig config = AssetDatabase.LoadAssetAtPath<BakeConfig>(path);
                if (config == null)
                    continue;

                bool ok = await RunBakeSafe(config);
                if (!ok)
                    return;
            }
        }

        #endregion


        #region Helpers

        /// <summary>
        /// Runs bake and surfaces failures in a dialog so menu async voids do not fail silently.
        /// </summary>
        static async System.Threading.Tasks.Task<bool> RunBakeSafe(BakeConfig config)
        {
            try
            {
                EditorUtility.DisplayProgressBar("Data Config Sheet", $"Baking '{config.name}'…", 0.35f);
                await DataConfigBakeRunner.BakeAsync(config);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog(
                    "Data Config Sheet bake failed",
                    exception.Message,
                    "OK");
                return false;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        #endregion


    }
}
