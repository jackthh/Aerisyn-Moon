using System;
using UnityEditor;
using UnityEngine;

namespace Aerisyn.DataConfigSheet.Editor
{
    /// <summary>
    /// Editor menus for OAuth sign-in and Google → Baked Asset Pull.
    /// </summary>
    public static class DataConfigPullMenu
    {


        const string SignInMenu = "Aerisyn/Data Config Sheet/Sign In With Google";
        const string SignOutMenu = "Aerisyn/Data Config Sheet/Sign Out";
        const string PullSelectedMenu = "Aerisyn/Data Config Sheet/Pull From Google (Selected Config)";
        const string PullAllMenu = "Aerisyn/Data Config Sheet/Pull From Google (All Configs)";


        #region Auth menus

        [MenuItem(SignInMenu, false, 50)]
        static async void SignIn()
        {
            PullConfig config = RequireSelectedPullConfig(
                "Select a PullConfig asset (Auth Mode = OAuth User), then Sign In again.");
            if (config == null)
                return;

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
            }
        }


        [MenuItem(SignOutMenu, false, 51)]
        static void SignOut()
        {
            PullConfig config = RequireSelectedPullConfig(
                "Select a PullConfig asset, then Sign Out again.");
            if (config == null)
                return;

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
        }

        #endregion


        #region Pull menus

        [MenuItem(PullSelectedMenu, false, 100)]
        static async void PullSelected()
        {
            PullConfig config = Selection.activeObject as PullConfig;
            if (config == null)
            {
                EditorUtility.DisplayDialog(
                    "Data Config Sheet",
                    "Select a PullConfig asset in the Project window, then run this menu again.",
                    "OK");
                return;
            }

            await RunPullSafe(config);
        }


        [MenuItem(PullSelectedMenu, true)]
        static bool PullSelectedValidate()
        {
            return Selection.activeObject is PullConfig;
        }


        [MenuItem(PullAllMenu, false, 101)]
        static async void PullAll()
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

                bool ok = await RunPullSafe(config);
                if (!ok)
                    return;
            }
        }

        #endregion


        #region Helpers

        static PullConfig RequireSelectedPullConfig(string missingMessage)
        {
            PullConfig config = Selection.activeObject as PullConfig;
            if (config != null)
                return config;

            EditorUtility.DisplayDialog("Data Config Sheet", missingMessage, "OK");
            return null;
        }


        /// <summary>
        /// Runs Pull and surfaces failures in a dialog so menu async voids do not fail silently.
        /// </summary>
        static async System.Threading.Tasks.Task<bool> RunPullSafe(PullConfig config)
        {
            try
            {
                EditorUtility.DisplayProgressBar("Data Config Sheet", $"Pulling '{config.name}'…", 0.2f);
                await DataConfigPullRunner.PullAsync(config);
                EditorUtility.DisplayDialog(
                    "Data Config Sheet",
                    $"Pull complete for '{config.name}'.",
                    "OK");
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

        #endregion


    }
}
