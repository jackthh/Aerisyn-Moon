using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Aerisyn.DataConfigSheet.Editor
{
    /// <summary>
    /// Single user-facing Pull and OAuth path for menu items and Pull Config Inspector
    /// buttons (progress bar, dialogs, logging).
    /// </summary>
    [InitializeOnLoad]
    public static class DataConfigPullWorkflow
    {


        #region Registration

        static DataConfigPullWorkflow()
        {
            // Inspector buttons on PullConfig cannot reference this Editor assembly;
            // register shared workflows so Inspector and menus stay identical.
            PullConfig.RunEditorPullAsync = PullWithUiAsync;
            PullConfig.RunEditorSignInAsync = SignInWithUiAsync;
            PullConfig.RunEditorSignOut = SignOutWithUi;
            PullConfig.EditorIsSignedIn = GoogleOAuthSession.IsSignedIn;
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


        /// <summary>
        /// Browser OAuth sign-in for one Pull Config with the same progress bar and
        /// dialogs used by the Inspector Sign In button and the Sign In menu.
        /// </summary>
        public static async Task SignInWithUiAsync(PullConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            try
            {
                EditorUtility.DisplayProgressBar(
                    "Data Config Sheet",
                    "Waiting for Google sign-in in your browser…",
                    0.4f);
                await GoogleOAuthSession.SignInAsync(config);
                EditorUtility.DisplayDialog(
                    "Data Config Sheet",
                    "Signed in. Share the spreadsheet with your Google email as Viewer, then Pull.",
                    "OK");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog("Data Config Sheet sign-in failed", exception.Message, "OK");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                // Refresh PullConfig Inspector "Google auth" status after token write.
                RepaintInspectors();
            }
        }


        /// <summary>
        /// Clears the local OAuth token for one Pull Config with the same dialogs used by
        /// the Inspector Sign Out button and the Sign Out menu.
        /// </summary>
        public static void SignOutWithUi(PullConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            try
            {
                GoogleOAuthSession.SignOut(config);
                EditorUtility.DisplayDialog("Data Config Sheet", "Signed out.", "OK");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog("Data Config Sheet sign-out failed", exception.Message, "OK");
            }
            finally
            {
                // Refresh PullConfig Inspector "Google auth" status after token delete.
                RepaintInspectors();
            }
        }

        #endregion


        #region Helpers

        static void RepaintInspectors()
        {
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }

        #endregion


    }
}
