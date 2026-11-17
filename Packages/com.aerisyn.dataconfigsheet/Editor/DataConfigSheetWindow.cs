using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Aerisyn.DataConfigSheet.Editor
{
    /// <summary>
    /// Aerisyn > Data Config Sheet window: edits the one Data Config Settings asset (created by the explicit menu open or the Create button),
    /// plus Sign In / Sign Out and Pull All. Buttons call the same DataConfigPullWorkflow the menus use.
    /// Tabs: Overview (active environment, setup checklist with one-click fixes, Pull All, last result), Keys, Bindings,
    /// Manifest, Auth, Pull All. Each settings tab draws its fields through an Odin PropertyTree so the key dropdowns and
    /// file/folder pickers on DataConfigSettings keep working. Pull All stays disabled until nothing blocks it.
    /// </summary>
    public sealed class DataConfigSheetWindow : EditorWindow
    {


        #region Constants

        const string WindowMenu = "Aerisyn/Data Config Sheet/Open Window";
        const string ActiveEnvironmentProperty = "_activeEnvironmentKey";
        const string PullAllFolderProperty = "_pullAllFolderPath";
        const string ClientSecretsProperty = "_oauthClientSecretsPath";
        const string ServiceAccountProperty = "_serviceAccountCredentialPath";

        // Status is refreshed at most this often; AssetDatabase and file lookups do not belong in OnGUI.
        const double RefreshIntervalSeconds = 1.0;
        const int MaxResultLines = 14;

        // Section sizing: roomy on purpose.
        const float TabHeight = 34f;
        const float RowMinHeight = 42f;
        const float ButtonHeight = 38f;
        const float SectionPadding = 12f;

        enum Tab
        {
            Overview,
            Keys,
            Bindings,
            Manifest,
            Auth,
            PullAll,
        }

        static readonly string[] TabLabels = { "Overview", "Keys", "Bindings", "Manifest", "Auth", "Pull All" };

        #endregion


        #region State

        [SerializeField] int _tabIndex;

        DataConfigSettings _settings;
        SerializedObject _serialized;
        PropertyTree _tree;
        Vector2 _scroll;
        string _setupError;
        bool _busy;
        string _busyLabel;

        List<SetupItem> _items = new List<SetupItem>();
        string[] _pullConfigPaths = Array.Empty<string>();
        bool _signedIn;
        bool _credentialMissing;
        bool _refreshRequested = true;
        double _lastRefreshTime;

        string _lastResultText;
        bool _lastResultOk;

        GUIStyle _sectionStyle;
        GUIStyle _sectionTitleStyle;
        GUIStyle _detailStyle;
        GUIStyle _resultStyle;

        #endregion


        #region Menu

        [MenuItem(WindowMenu, false, 0)]
        public static void Open()
        {
            DataConfigSheetWindow window = GetWindow<DataConfigSheetWindow>("Data Config Sheet");
            window.minSize = new Vector2(460f, 560f);
            window.Show();
            // Only this explicit open creates the asset; domain reload and layout restore just load.
            window.EnsureSettings(createIfMissing: true);
        }

        #endregion


        #region Lifecycle

        void OnEnable()
        {
            Undo.undoRedoPerformed += RequestRefresh;
            EnsureSettings(createIfMissing: false);
        }


        void OnDisable()
        {
            Undo.undoRedoPerformed -= RequestRefresh;
            DestroyTree();
        }


        void OnProjectChange()
        {
            // The asset may have been deleted or restored outside the window, or Pull Configs moved.
            if (_settings == null)
                EnsureSettings(createIfMissing: false);

            RequestRefresh();
        }


        // ~10x/second while the window is visible. All lookups happen here, never between Layout and Repaint.
        void OnInspectorUpdate()
        {
            if (_settings == null)
                return;

            bool due = EditorApplication.timeSinceStartup - _lastRefreshTime > RefreshIntervalSeconds;
            if (_refreshRequested || due)
            {
                RefreshStatus();
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
                DrawMissingSettings();
                return;
            }

            EnsureStyles();
            _serialized.Update();

            DrawTabBar();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            GUILayout.Space(6f);
            switch ((Tab)Mathf.Clamp(_tabIndex, 0, TabLabels.Length - 1))
            {
                case Tab.Overview:
                    DrawOverview();
                    break;
                case Tab.Keys:
                    DrawKeysTab();
                    break;
                case Tab.Bindings:
                    DrawBindingsTab();
                    break;
                case Tab.Manifest:
                    DrawManifestTab();
                    break;
                case Tab.Auth:
                    DrawAuthTab();
                    break;
                case Tab.PullAll:
                    DrawPullAllTab();
                    break;
            }

            GUILayout.Space(8f);
            EditorGUILayout.EndScrollView();

            _serialized.ApplyModifiedProperties();
        }


        void DrawMissingSettings()
        {
            EnsureStyles();
            GUILayout.Space(12f);
            using (new EditorGUILayout.VerticalScope(_sectionStyle))
            {
                GUILayout.Label("Data Config Sheet", _sectionTitleStyle);
                if (_setupError != null)
                    EditorGUILayout.HelpBox(_setupError, MessageType.Error);
                else
                    EditorGUILayout.HelpBox(
                        $"Data Config Settings not found at '{DataConfigSettings.DefaultAssetPath}'.\n" +
                        "Create it to set up environments, Feature Keys, auth and Pull All.",
                        MessageType.Info);

                if (GUILayout.Button("Create Data Config Settings", GUILayout.Height(ButtonHeight)))
                    EnsureSettings(createIfMissing: true);
            }
        }


        void DrawTabBar()
        {
            string[] labels = new string[TabLabels.Length];
            for (int i = 0; i < labels.Length; i++)
                labels[i] = TabHasBlocking((Tab)i) ? TabLabels[i] + " !" : TabLabels[i];

            GUILayout.Space(4f);
            _tabIndex = GUILayout.Toolbar(_tabIndex, labels, GUILayout.Height(TabHeight));
            GUILayout.Space(2f);
        }


        #endregion


        #region Overview tab

        void DrawOverview()
        {
            DrawEnvironment();
            DrawChecklist();
            DrawActions();
            DrawLastResult();
        }


        void DrawEnvironment()
        {
            SerializedProperty active = _serialized.FindProperty(ActiveEnvironmentProperty);
            if (active == null)
                return;

            using (new EditorGUILayout.VerticalScope(_sectionStyle))
            {
                using (new EditorGUILayout.HorizontalScope(GUILayout.MinHeight(RowMinHeight - 8f)))
                {
                    GUILayout.Label(
                        new GUIContent("Environment", "The active Environment Key. Pull and Pull All share it."),
                        _sectionTitleStyle,
                        GUILayout.Width(110f));

                    string[] keys = ToArray(_settings.EnvironmentKeys);
                    int current = Array.IndexOf(keys, active.stringValue);
                    string[] options = current >= 0
                        ? keys
                        : Prepend(string.IsNullOrWhiteSpace(active.stringValue) ? "(none)" : active.stringValue + " (not in list)", keys);
                    int shown = current >= 0 ? current : 0;

                    using (new EditorGUI.DisabledScope(_busy || keys.Length == 0))
                    {
                        int picked = EditorGUILayout.Popup(shown, options, GUILayout.Height(26f));
                        if (picked != shown)
                        {
                            active.stringValue = keys[current >= 0 ? picked : picked - 1];
                            _serialized.ApplyModifiedProperties();
                            EditorUtility.SetDirty(_settings);
                            RequestRefresh();
                        }
                    }
                }

                if (_settings.EnvironmentKeys.Count == 0)
                    EditorGUILayout.HelpBox("No Environment Keys. Add some on the Keys tab.", MessageType.Warning);
            }
        }


        void DrawChecklist()
        {
            int blocking = SetupChecklist.CountBlocking(_items);
            string header = blocking == 0
                ? "Setup: ready for Pull All"
                : $"Setup: {blocking} to fix before Pull All";

            using (new EditorGUILayout.VerticalScope(_sectionStyle))
            {
                GUILayout.Label(header, _sectionTitleStyle);
                GUILayout.Space(4f);
                for (int i = 0; i < _items.Count; i++)
                {
                    if (i > 0)
                        DrawSeparator();
                    DrawChecklistRow(_items[i]);
                }
            }
        }


        void DrawChecklistRow(SetupItem item)
        {
            using (new EditorGUILayout.HorizontalScope(GUILayout.MinHeight(RowMinHeight)))
            {
                GUILayout.Label(StatusIcon(item.Status), GUILayout.Width(24f), GUILayout.Height(24f));

                using (new EditorGUILayout.VerticalScope())
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(item.Label, EditorStyles.boldLabel);
                    GUILayout.Label(item.Detail, _detailStyle);
                    GUILayout.FlexibleSpace();
                }

                GUILayout.FlexibleSpace();
                using (new EditorGUILayout.VerticalScope())
                {
                    GUILayout.FlexibleSpace();
                    DrawFixButton(item);
                    GUILayout.FlexibleSpace();
                }
            }
        }


        // One-click fix for the rows that have one. Everything else is edited on its tab.
        void DrawFixButton(SetupItem item)
        {
            if (item.Status != SetupItemStatus.Blocking)
                return;

            using (new EditorGUI.DisabledScope(_busy))
            {
                switch (item.Id)
                {
                    case SetupChecklist.EnvironmentId:
                        if (GUILayout.Button("Keys…", GUILayout.Width(78f), GUILayout.Height(26f)))
                            _tabIndex = (int)Tab.Keys;
                        break;

                    case SetupChecklist.PullAllFolderId:
                        if (GUILayout.Button("Choose…", GUILayout.Width(78f), GUILayout.Height(26f)))
                            ChooseFolder();
                        break;

                    case SetupChecklist.AuthId:
                        if (_credentialMissing)
                        {
                            if (GUILayout.Button("Choose…", GUILayout.Width(78f), GUILayout.Height(26f)))
                                ChooseCredentialFile();
                        }
                        else if (_settings.AuthMode == GoogleAuthMode.OAuthUser && !_signedIn)
                        {
                            if (GUILayout.Button("Sign In", GUILayout.Width(78f), GUILayout.Height(26f)))
                                RunSignIn();
                        }
                        break;
                }
            }
        }


        void DrawActions()
        {
            bool ready = SetupChecklist.IsReadyForPullAll(_items);

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(_busy || !ready))
                {
                    string label = _busy
                        ? (_busyLabel ?? "Working…")
                        : $"Pull All ({_pullConfigPaths.Length})";
                    string tip = ready
                        ? "Pull every Pull Config under the Pull All folder."
                        : "Fix the items marked above first.";
                    if (GUILayout.Button(new GUIContent(label, tip), GUILayout.Height(ButtonHeight)))
                        RunPullAll();
                }

                if (_settings.AuthMode == GoogleAuthMode.OAuthUser)
                {
                    using (new EditorGUI.DisabledScope(_busy))
                    {
                        if (_signedIn)
                        {
                            if (GUILayout.Button("Sign Out", GUILayout.Height(ButtonHeight), GUILayout.Width(100f)))
                                RunSignOut();
                        }
                        else if (GUILayout.Button("Sign In", GUILayout.Height(ButtonHeight), GUILayout.Width(100f)))
                        {
                            RunSignIn();
                        }
                    }
                }
            }
        }


        void DrawLastResult()
        {
            if (string.IsNullOrEmpty(_lastResultText))
                return;

            GUILayout.Space(8f);
            using (new EditorGUILayout.VerticalScope(_sectionStyle))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label(_lastResultOk ? "Last Pull All: passed" : "Last Pull All: failed", _sectionTitleStyle);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Copy", GUILayout.Width(60f), GUILayout.Height(22f)))
                        GUIUtility.systemCopyBuffer = _lastResultText;
                    if (GUILayout.Button("Clear", GUILayout.Width(60f), GUILayout.Height(22f)))
                    {
                        _lastResultText = null;
                        return;
                    }
                }

                GUILayout.Space(4f);
                GUILayout.Label(Truncate(_lastResultText, MaxResultLines), _resultStyle);
            }
        }

        #endregion


        #region Settings tabs

        void DrawKeysTab()
        {
            DrawSection(
                "Environment Keys",
                "Stages the project can pull from. Seeded with Dev, Test, Production.",
                "_environmentKeys");
            DrawSection(
                "Feature Keys",
                "Names for the game features Pull Configs point at.",
                "_featureKeys");
        }


        void DrawBindingsTab()
        {
            DrawSection(
                "Spreadsheet bindings",
                "Environment Key × Feature Key → Source Sheet spreadsheet id.",
                "_bindings");
            DrawSection(
                "Title prefix overrides",
                "Optional Source Sheet title prefix per Environment Key. Defaults: Dev → [Dev], Test → [Tester], Production → [Production].",
                "_titlePrefixOverrides");
        }


        void DrawManifestTab()
        {
            DrawSection(
                "Config Manifest",
                "Pull All writes the manifest here after every Pull Config passes. Leave blank to skip it.",
                "_manifestAssetPath");
            DrawSection(
                "Required Config Types",
                "Pull All and the build gate fail when one of these is missing from the manifest.",
                "_requiredConfigTypeNames");
        }


        void DrawAuthTab()
        {
            DrawSection(
                "Google auth",
                "OAuth user = browser sign-in. Service account = robot JSON for CI. One sign-in serves every Pull Config.",
                "_authMode",
                "_oauthClientSecretsPath",
                "_oauthUserTokenPath",
                "_serviceAccountCredentialPath");

            if (_settings.AuthMode == GoogleAuthMode.OAuthUser)
            {
                using (new EditorGUI.DisabledScope(_busy))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Sign In", GUILayout.Height(ButtonHeight)))
                            RunSignIn();
                        using (new EditorGUI.DisabledScope(!_signedIn))
                        {
                            if (GUILayout.Button("Sign Out", GUILayout.Height(ButtonHeight)))
                                RunSignOut();
                        }
                    }
                }
            }
        }


        void DrawPullAllTab()
        {
            DrawSection(
                "Pull All folder",
                "Pull All runs every Pull Config under this folder, recursively. Blank fails.",
                PullAllFolderProperty);

            using (new EditorGUILayout.VerticalScope(_sectionStyle))
            {
                GUILayout.Label($"Pull Configs found ({_pullConfigPaths.Length})", _sectionTitleStyle);
                GUILayout.Space(4f);

                if (_pullConfigPaths.Length == 0)
                {
                    GUILayout.Label("None. Choose a folder that contains Pull Configs.", _detailStyle);
                    return;
                }

                for (int i = 0; i < _pullConfigPaths.Length; i++)
                {
                    string path = _pullConfigPaths[i];
                    using (new EditorGUILayout.HorizontalScope(GUILayout.MinHeight(26f)))
                    {
                        GUILayout.Label(path, _detailStyle);
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Select", GUILayout.Width(60f), GUILayout.Height(22f)))
                        {
                            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                            if (asset != null)
                            {
                                EditorGUIUtility.PingObject(asset);
                                Selection.activeObject = asset;
                            }
                        }
                    }
                }
            }
        }


        // Draws one titled section: the named settings properties through the Odin tree (dropdowns, pickers, ShowIf).
        void DrawSection(string title, string description, params string[] propertyPaths)
        {
            using (new EditorGUILayout.VerticalScope(_sectionStyle))
            {
                GUILayout.Label(title, _sectionTitleStyle);
                if (!string.IsNullOrEmpty(description))
                    GUILayout.Label(description, _detailStyle);
                GUILayout.Space(6f);

                if (_tree == null)
                    return;

                EditorGUI.BeginChangeCheck();
                InspectorUtilities.BeginDrawPropertyTree(_tree, true);
                for (int i = 0; i < propertyPaths.Length; i++)
                {
                    InspectorProperty property = _tree.GetPropertyAtPath(propertyPaths[i]);
                    if (property == null)
                        continue;

                    if (i > 0)
                        GUILayout.Space(4f);
                    property.Draw();
                }
                InspectorUtilities.EndDrawPropertyTree(_tree);

                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(_settings);
                    RequestRefresh();
                }
            }
        }

        #endregion


        #region Actions

        // Disables the buttons while an action runs and always repaints afterwards.
        async void RunBusy(string busyLabel, Func<Task> action)
        {
            _busy = true;
            _busyLabel = busyLabel;
            try
            {
                SaveSettings();
                await action();
            }
            finally
            {
                _busy = false;
                _busyLabel = null;
                RequestRefresh();
                Repaint();
            }
        }


        void RunSignIn()
        {
            RunBusy("Signing in…", DataConfigPullWorkflow.SignInWithUiAsync);
        }


        void RunSignOut()
        {
            SaveSettings();
            DataConfigPullWorkflow.SignOutWithUi();
            RequestRefresh();
            Repaint();
        }


        void RunPullAll()
        {
            RunBusy("Pulling…", async () =>
            {
                PullAllSummary summary = await DataConfigPullWorkflow.PullAllWithUiAsync();
                if (summary != null)
                {
                    _lastResultOk = summary.OverallSuccess;
                    _lastResultText = summary.Format();
                }
            });
        }


        // Pull reads the asset through AssetDatabase; flush window edits to disk first.
        void SaveSettings()
        {
            if (_settings == null)
                return;

            EditorUtility.SetDirty(_settings);
            AssetDatabase.SaveAssets();
        }


        void ChooseFolder()
        {
            string current = _settings.PullAllFolderPath;
            string start = string.IsNullOrWhiteSpace(current) || !AssetDatabase.IsValidFolder(current)
                ? Application.dataPath
                : DataConfigPathUtility.ResolveProjectPath(current);

            string picked = EditorUtility.OpenFolderPanel("Pull All folder", start, "");
            if (string.IsNullOrEmpty(picked))
                return;

            string projectRelative = ToProjectRelative(picked);
            if (projectRelative == null)
            {
                EditorUtility.DisplayDialog(
                    "Data Config Sheet",
                    "Pick a folder inside this project's Assets folder.",
                    "OK");
                return;
            }

            SetString(PullAllFolderProperty, projectRelative);
        }


        void ChooseCredentialFile()
        {
            bool oauth = _settings.AuthMode == GoogleAuthMode.OAuthUser;
            string title = oauth ? "OAuth client secrets (JSON)" : "Service account (JSON)";
            string picked = EditorUtility.OpenFilePanel(title, Application.dataPath, "json");
            if (string.IsNullOrEmpty(picked))
                return;

            SetString(oauth ? ClientSecretsProperty : ServiceAccountProperty, ToProjectRelativeOrAbsolute(picked));
        }


        void SetString(string propertyName, string value)
        {
            SerializedProperty property = _serialized.FindProperty(propertyName);
            if (property == null)
                return;

            property.stringValue = value;
            _serialized.ApplyModifiedProperties();
            EditorUtility.SetDirty(_settings);
            // The Odin tree holds its own copy of the values; rebuild so the tabs show the new path.
            RebuildTree();
            RequestRefresh();
        }

        #endregion


        #region Status

        void RequestRefresh()
        {
            _refreshRequested = true;
        }


        void RefreshStatus()
        {
            _refreshRequested = false;
            _lastRefreshTime = EditorApplication.timeSinceStartup;

            if (_settings == null)
            {
                _items.Clear();
                _pullConfigPaths = Array.Empty<string>();
                return;
            }

            bool oauth = _settings.AuthMode == GoogleAuthMode.OAuthUser;
            string credentialPath = oauth ? _settings.OAuthClientSecretsPath : _settings.ServiceAccountCredentialPath;
            bool credentialExists = FileExistsAtProjectPath(credentialPath);
            _signedIn = oauth && GoogleOAuthSession.IsSignedIn(_settings);
            _credentialMissing = string.IsNullOrWhiteSpace(credentialPath) || !credentialExists;

            string folder = _settings.PullAllFolderPath;
            bool folderExists = !string.IsNullOrWhiteSpace(folder) && AssetDatabase.IsValidFolder(DataConfigPathUtility.NormalizeAssetPath(folder));
            _pullConfigPaths = FindPullConfigPaths(folderExists);

            SetupInputs inputs = new SetupInputs
            {
                EnvironmentKeys = _settings.EnvironmentKeys,
                ActiveEnvironmentKey = _settings.ActiveEnvironmentKey,
                FeatureKeys = _settings.FeatureKeys,
                PullAllFolderPath = folder,
                PullAllFolderExists = folderExists,
                PullConfigCount = _pullConfigPaths.Length,
                UsesOAuth = oauth,
                CredentialPath = credentialPath,
                CredentialExists = credentialExists,
                SignedIn = _signedIn,
                ManifestAssetPath = _settings.ManifestAssetPath,
            };
            _items = SetupChecklist.Evaluate(inputs);
        }


        // Same discovery rule Pull All uses, so the list shown is the list that runs.
        string[] FindPullConfigPaths(bool folderExists)
        {
            if (!folderExists)
                return Array.Empty<string>();

            try
            {
                return DataConfigPullConfigDiscovery.FindPaths(_settings);
            }
            catch (InvalidOperationException)
            {
                return Array.Empty<string>();
            }
        }


        static bool FileExistsAtProjectPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            try
            {
                return File.Exists(DataConfigPathUtility.ResolveProjectPath(path));
            }
            catch (Exception)
            {
                return false;
            }
        }


        // Red marker on the tab that holds the fix for a blocking checklist row.
        bool TabHasBlocking(Tab tab)
        {
            string itemId;
            switch (tab)
            {
                case Tab.Keys:
                    itemId = SetupChecklist.EnvironmentId;
                    break;
                case Tab.Auth:
                    itemId = SetupChecklist.AuthId;
                    break;
                case Tab.PullAll:
                    itemId = SetupChecklist.PullAllFolderId;
                    break;
                default:
                    return false;
            }

            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].Id == itemId && _items[i].Status == SetupItemStatus.Blocking)
                    return true;
            }

            return false;
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

            _serialized = _settings != null ? new SerializedObject(_settings) : null;
            RebuildTree();
            RequestRefresh();
        }


        void RebuildTree()
        {
            DestroyTree();
            if (_settings != null)
                _tree = PropertyTree.Create(_settings);
        }


        void DestroyTree()
        {
            if (_tree != null)
            {
                _tree.Dispose();
                _tree = null;
            }
        }


        void EnsureStyles()
        {
            if (_sectionStyle != null)
                return;

            _sectionStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(
                    (int)SectionPadding, (int)SectionPadding, (int)SectionPadding, (int)SectionPadding),
                margin = new RectOffset(8, 8, 4, 8),
            };
            _sectionTitleStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 13 };
            _detailStyle = new GUIStyle(EditorStyles.miniLabel) { wordWrap = true };
            _resultStyle = new GUIStyle(EditorStyles.label) { wordWrap = true };
        }


        static void DrawSeparator()
        {
            Rect line = GUILayoutUtility.GetRect(1f, 1f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(line, new Color(0.5f, 0.5f, 0.5f, 0.25f));
        }


        static GUIContent StatusIcon(SetupItemStatus status)
        {
            switch (status)
            {
                case SetupItemStatus.Ok:
                    return EditorGUIUtility.IconContent("TestPassed");
                case SetupItemStatus.Warning:
                    return EditorGUIUtility.IconContent("console.warnicon.sml");
                default:
                    return EditorGUIUtility.IconContent("TestFailed");
            }
        }


        static string[] ToArray(IReadOnlyList<string> list)
        {
            string[] array = new string[list.Count];
            for (int i = 0; i < array.Length; i++)
                array[i] = list[i];
            return array;
        }


        static string[] Prepend(string first, string[] rest)
        {
            string[] result = new string[rest.Length + 1];
            result[0] = first;
            Array.Copy(rest, 0, result, 1, rest.Length);
            return result;
        }


        // Absolute folder from the picker -> "Assets/..." path, or null when it is outside the project.
        static string ToProjectRelative(string absolutePath)
        {
            string normalized = absolutePath.Replace('\\', '/').TrimEnd('/');
            string dataPath = Application.dataPath.Replace('\\', '/');
            if (normalized == dataPath)
                return "Assets";

            return normalized.StartsWith(dataPath + "/", StringComparison.Ordinal)
                ? "Assets" + normalized.Substring(dataPath.Length)
                : null;
        }


        // Credentials may live outside Assets (gitignored); keep those absolute.
        static string ToProjectRelativeOrAbsolute(string absolutePath)
        {
            return ToProjectRelative(absolutePath) ?? absolutePath.Replace('\\', '/');
        }


        static string Truncate(string text, int maxLines)
        {
            string[] lines = text.Split('\n');
            if (lines.Length <= maxLines)
                return text;

            return string.Join("\n", lines, 0, maxLines) + $"\n… {lines.Length - maxLines} more line(s). Use Copy for the full result.";
        }

        #endregion


    }
}
