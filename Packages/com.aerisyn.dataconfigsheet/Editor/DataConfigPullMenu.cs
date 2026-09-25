using System;
using UnityEditor;
using UnityEngine;

namespace Aerisyn.DataConfigSheet.Editor
{
    /// <summary>
    /// Editor menus for OAuth sign-in, Google → Baked Asset Pull, and Header Emitter.
    /// </summary>
    public static class DataConfigPullMenu
    {


        const string SignInMenu = "Aerisyn/Data Config Sheet/Sign In With Google";
        const string SignOutMenu = "Aerisyn/Data Config Sheet/Sign Out";
        const string PullSelectedMenu = "Aerisyn/Data Config Sheet/Pull From Google (Selected Config)";
        const string PullAllMenu = "Aerisyn/Data Config Sheet/Pull From Google (All Configs)";
        const string EmitHeaderMenu = "Aerisyn/Data Config Sheet/Copy Header Row (Selected Config Type)";


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

            // Same shared workflow as the Pull Config Inspector Pull button.
            await DataConfigPullWorkflow.PullWithUiAsync(config);
        }


        [MenuItem(PullSelectedMenu, true)]
        static bool PullSelectedValidate()
        {
            return Selection.activeObject is PullConfig;
        }


        [MenuItem(PullAllMenu, false, 101)]
        static async void PullAll()
        {
            // Each Pull Config's Include In Pull flags apply inside the runner.
            await DataConfigPullWorkflow.PullAllWithUiAsync();
        }

        #endregion


        #region Header Emitter

        [MenuItem(EmitHeaderMenu, false, 150)]
        static void CopyHeaderRow()
        {
            Type configType = ResolveSelectedConfigType();
            if (configType == null)
            {
                EditorUtility.DisplayDialog(
                    "Data Config Sheet",
                    "Select a Config Type ScriptableObject asset (or its MonoScript), then run this menu again.",
                    "OK");
                return;
            }

            try
            {
                HeaderEmitResult result = HeaderEmitter.Emit(configType);
                EditorGUIUtility.systemCopyBuffer = result.ToTabSeparatedRow();

                string tabHint = ConfigTypeTabName.Resolve(configType);
                EditorUtility.DisplayDialog(
                    "Data Config Sheet: Header Row copied",
                    "Header Row for '" + configType.Name + "' is on the clipboard (tab-separated).\n" +
                    "Paste into Google tab '" + tabHint + "'.\n\n" +
                    result.IgnoreMarkerGuidance,
                    "OK");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog("Data Config Sheet Header Emitter failed", exception.Message, "OK");
            }
        }


        [MenuItem(EmitHeaderMenu, true)]
        static bool CopyHeaderRowValidate()
        {
            return ResolveSelectedConfigType() != null;
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
        /// Resolves a concrete ConfigTypeAsset type from the Project selection (asset or script).
        /// </summary>
        static Type ResolveSelectedConfigType()
        {
            UnityEngine.Object selected = Selection.activeObject;
            if (selected == null)
                return null;

            ConfigTypeAsset asset = selected as ConfigTypeAsset;
            if (asset != null)
                return asset.GetType();

            MonoScript script = selected as MonoScript;
            if (script != null)
            {
                Type type = script.GetClass();
                if (type != null && !type.IsAbstract && typeof(ConfigTypeAsset).IsAssignableFrom(type))
                    return type;
            }

            return null;
        }

        #endregion


    }
}
