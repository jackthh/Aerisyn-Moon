using UnityEditor;
using UnityEngine;

namespace Aerisyn.DataConfigSheet.Editor
{
    /// <summary>
    /// Aerisyn > Data Config Sheet window: edits the one Data Config Settings asset (created on first open),
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
        }

        #endregion


        #region Lifecycle

        void OnEnable()
        {
            EnsureSettings();
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
                EnsureSettings();
                Repaint();
            }
        }

        #endregion


        #region GUI

        void OnGUI()
        {
            if (_settings == null)
                EnsureSettings();

            if (_settings == null)
            {
                EditorGUILayout.HelpBox(
                    _setupError ?? "Data Config Settings could not be loaded.",
                    MessageType.Error);
                if (GUILayout.Button("Retry"))
                    EnsureSettings();
                return;
            }

            DrawToolbar();

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

        async void RunSignIn()
        {
            _busy = true;
            try
            {
                SaveSettings();
                await DataConfigPullWorkflow.SignInWithUiAsync();
            }
            finally
            {
                _busy = false;
                Repaint();
            }
        }


        void RunSignOut()
        {
            SaveSettings();
            DataConfigPullWorkflow.SignOutWithUi();
            Repaint();
        }


        async void RunPullAll()
        {
            _busy = true;
            try
            {
                SaveSettings();
                await DataConfigPullWorkflow.PullAllWithUiAsync();
            }
            finally
            {
                _busy = false;
                Repaint();
            }
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

        void EnsureSettings()
        {
            try
            {
                _settings = DataConfigSettingsLoader.LoadOrCreate();
                _setupError = null;
            }
            catch (System.Exception exception)
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
