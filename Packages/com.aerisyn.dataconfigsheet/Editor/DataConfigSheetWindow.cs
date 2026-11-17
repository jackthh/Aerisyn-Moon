using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Aerisyn.DataConfigSheet.Editor
{
    /// <summary>
    /// Aerisyn > Data Config Sheet window: edits the one Data Config Settings asset (created by the explicit menu open or the Create button),
    /// plus Sign In / Sign Out and Pull All. Buttons call the same DataConfigPullWorkflow the menus use.
    /// The settings Inspector (Odin-drawn when Odin is active) provides the folder and .asset file pickers.
    /// </summary>
    public sealed class DataConfigSheetWindow : EditorWindow
    {


        const string WindowMenu = "Aerisyn/Data Config Sheet/Open Window";

        DataConfigSettings _settings;
        UnityEditor.Editor _settingsEditor;
        Vector2 _scroll;
        string _setupError;
        bool _busy;


        #region Menu

        [MenuItem(WindowMenu, false, 0)]
        public static void Open()
        {
            DataConfigSheetWindow window = GetWindow<DataConfigSheetWindow>("Data Config Sheet");
            window.minSize = new Vector2(380f, 360f);
            window.Show();
            // Only this explicit open creates the asset; domain reload and layout restore just load.
            window.EnsureSettings(createIfMissing: true);
        }

        #endregion


        #region Lifecycle

        void OnEnable()
        {
            EnsureSettings(createIfMissing: false);
        }


        void OnDisable()
        {
            DestroyEditor();
        }


        void OnProjectChange()
        {
            // The asset may have been deleted or restored outside the window.
            if (_settings == null)
            {
                EnsureSettings(createIfMissing: false);
                Repaint();
            }
        }

        #endregion


        #region GUI

        void OnGUI()
        {
            if (_settings == null)
                EnsureSettings(createIfMissing: false);

            if (_settings == null)
            {
                if (_setupError != null)
                    EditorGUILayout.HelpBox(_setupError, MessageType.Error);
                else
                    EditorGUILayout.HelpBox(
                        $"Data Config Settings not found at '{DataConfigSettings.DefaultAssetPath}'.",
                        MessageType.Info);

                if (GUILayout.Button("Create Data Config Settings"))
                    EnsureSettings(createIfMissing: true);
                return;
            }

            DrawToolbar();

            string environmentProblem = KeyListRules.DescribeProblem(
                "Environment Key", _settings.EnvironmentKeys, _settings.ActiveEnvironmentKey);
            if (environmentProblem != null)
                EditorGUILayout.HelpBox(
                    environmentProblem + " Pick the active Environment Key below; Pull fails until then.",
                    MessageType.Warning);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            if (_settingsEditor != null)
                _settingsEditor.OnInspectorGUI();
            EditorGUILayout.EndScrollView();
        }


        void DrawToolbar()
        {
            bool signedIn = GoogleOAuthSession.IsSignedIn(_settings);
            string status = _settings.AuthMode == GoogleAuthMode.OAuthUser
                ? (signedIn ? "Signed in" : "Not signed in")
                : "Service account";

            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label(status, EditorStyles.miniLabel);
                GUILayout.FlexibleSpace();

                using (new EditorGUI.DisabledScope(_busy))
                {
                    if (_settings.AuthMode == GoogleAuthMode.OAuthUser)
                    {
                        if (GUILayout.Button("Sign In", EditorStyles.toolbarButton))
                            RunSignIn();

                        using (new EditorGUI.DisabledScope(!signedIn))
                        {
                            if (GUILayout.Button("Sign Out", EditorStyles.toolbarButton))
                                RunSignOut();
                        }
                    }

                    if (GUILayout.Button("Pull All", EditorStyles.toolbarButton))
                        RunPullAll();
                }
            }

            EditorGUILayout.Space(2f);
        }

        #endregion


        #region Actions

        // Disables the buttons while an action runs and always repaints afterwards.
        async void RunBusy(Func<Task> action)
        {
            _busy = true;
            try
            {
                SaveSettings();
                await action();
            }
            finally
            {
                _busy = false;
                Repaint();
            }
        }


        void RunSignIn()
        {
            RunBusy(DataConfigPullWorkflow.SignInWithUiAsync);
        }


        void RunSignOut()
        {
            SaveSettings();
            DataConfigPullWorkflow.SignOutWithUi();
            Repaint();
        }


        void RunPullAll()
        {
            RunBusy(() => DataConfigPullWorkflow.PullAllWithUiAsync());
        }


        // Pull reads the asset through AssetDatabase; flush window edits to disk first.
        void SaveSettings()
        {
            if (_settings == null)
                return;

            EditorUtility.SetDirty(_settings);
            AssetDatabase.SaveAssets();
        }

        #endregion


        #region Helpers

        void EnsureSettings(bool createIfMissing)
        {
            try
            {
                _settings = createIfMissing
                    ? DataConfigSettingsLoader.LoadOrCreate()
                    : DataConfigSettingsLoader.TryLoad();
                _setupError = null;
            }
            catch (Exception exception)
            {
                _settings = null;
                _setupError = exception.Message;
                Debug.LogException(exception);
            }

            DestroyEditor();
            if (_settings != null)
                _settingsEditor = UnityEditor.Editor.CreateEditor(_settings);
        }


        void DestroyEditor()
        {
            if (_settingsEditor != null)
            {
                DestroyImmediate(_settingsEditor);
                _settingsEditor = null;
            }
        }

        #endregion


    }
}
