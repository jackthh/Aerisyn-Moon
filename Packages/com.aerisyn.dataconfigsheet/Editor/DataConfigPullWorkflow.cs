using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Aerisyn.DataConfigSheet.Editor
{
    /// <summary>
    /// Single user-facing Pull path into DataConfigPullRunner for menu items and the
    /// Pull Config Inspector button (progress bar, dialogs, logging).
    /// </summary>
    [InitializeOnLoad]
    public static class DataConfigPullWorkflow
    {


        #region Registration

        static DataConfigPullWorkflow()
        {
            // Inspector [Button] on PullConfig cannot reference this Editor assembly;
            // register the shared workflow so both entry points stay identical.
            PullConfig.RunEditorPullAsync = PullWithUiAsync;
        }

        #endregion


        #region Public API

        /// <summary>
        /// Runs live Google Pull for one Pull Config and surfaces the PullReport
        /// (or thrown validation/auth errors) in the Editor UI.
        /// </summary>
        /// <returns>True when the PullReport succeeds; false on failure or exception.</returns>
        public static async Task<bool> PullWithUiAsync(PullConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            try
            {
                EditorUtility.DisplayProgressBar("Data Config Sheet", $"Pulling '{config.name}'…", 0.2f);
                PullReport report = await DataConfigPullRunner.PullAsync(config);
                string body = report.Format();
                Debug.Log("[DataConfigSheet]\n" + body);

                if (!report.Success)
                {
                    EditorUtility.DisplayDialog("Data Config Sheet Pull failed", body, "OK");
                    return false;
                }

                EditorUtility.DisplayDialog("Data Config Sheet", body, "OK");
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog(
                    "Data Config Sheet Pull failed",
                    exception.Message,
                    "OK");
                return false;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }


        /// <summary>
        /// Finds every Pull Config in the project and runs PullWithUiAsync on each,
        /// stopping at the first failure. Each config's Include In Pull ticks apply.
        /// </summary>
        public static async Task PullAllWithUiAsync()
        {
            string[] guids = AssetDatabase.FindAssets("t:PullConfig");
            if (guids == null || guids.Length == 0)
            {
                EditorUtility.DisplayDialog(
                    "Data Config Sheet",
                    "No PullConfig assets found. Create one via Assets → Create → Aerisyn → Data Config Sheet → Pull Config.",
                    "OK");
                return;
            }

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                PullConfig config = AssetDatabase.LoadAssetAtPath<PullConfig>(path);
                if (config == null)
                    continue;

                bool ok = await PullWithUiAsync(config);
                if (!ok)
                    return;
            }
        }

        #endregion


    }
}
