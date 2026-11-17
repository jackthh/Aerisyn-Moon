using System;
using System.Collections.Generic;
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
            // The Inspector button on PullConfig cannot reference this Editor assembly;
            // register the shared workflow so Inspector and menus stay identical.
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
        /// Loads Data Config Settings, finds every Pull Config under its Pull All folder (recursive, stable
        /// asset-path order) and Pulls each one. A blank folder, a folder with no Pull Config, or missing
        /// settings fails the run with a clear summary. A failing config never stops the run, so one summary
        /// shows every passed and failed job. Each config's Include In Pull ticks apply. When every job
        /// passes it writes the Config Manifest at the settings' path and checks it against the required
        /// Config Type list. When any job failed the manifest is left untouched, but the required list is
        /// still checked against what the passed jobs produced, so the summary shows job failures and
        /// missing types together. A missing type or wrong environment fails the overall run.
        /// </summary>
        /// <returns>The combined summary (OverallSuccess false when no Pull Config ran).</returns>
        public static async Task<PullAllSummary> PullAllWithUiAsync()
        {
            DataConfigSettings settings;
            List<PullConfig> configs;
            try
            {
                settings = DataConfigSettingsLoader.Load();
                configs = DataConfigPullConfigDiscovery.Find(settings);
            }
            catch (InvalidOperationException exception)
            {
                // Missing settings, blank folder, or empty folder: nothing ran, so never a green result.
                PullAllSummary failed = PullAllSummary.Create(
                    Array.Empty<PullJobResult>(),
                    new[] { exception.Message });
                Debug.LogError("[DataConfigSheet]\n" + failed.Format());
                EditorUtility.DisplayDialog("Data Config Sheet Pull All failed", failed.Format(), "OK");
                return failed;
            }

            // Job index == config index.
            List<PullAllJob> jobs = new List<PullAllJob>(configs.Count);
            for (int i = 0; i < configs.Count; i++)
            {
                PullConfig config = configs[i];
                jobs.Add(new PullAllJob(config.name, () => RunPullJobAsync(config, settings)));
            }

            IReadOnlyList<PullJobResult> results;
            List<string> manifestFindings = new List<string>();
            try
            {
                EditorUtility.DisplayProgressBar("Data Config Sheet", $"Pull All ({configs.Count} Pull Configs)…", 0.2f);
                results = await PullAllRunner.RunAllAsync(jobs);

                try
                {
                    CheckManifest(settings, results, manifestFindings);
                }
                catch (Exception exception)
                {
                    // Per-job exceptions are already failed results; this is manifest IO (path, hashing) failing.
                    Debug.LogException(exception);
                    manifestFindings.Add($"Config Manifest write failed: {exception.Message}");
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            PullAllSummary summary = PullAllSummary.Create(results, manifestFindings);
            string body = summary.Format();
            Debug.Log("[DataConfigSheet]\n" + body);
            EditorUtility.DisplayDialog(
                summary.OverallSuccess ? "Data Config Sheet" : "Data Config Sheet Pull All failed",
                body,
                "OK");
            return summary;
        }


        /// <summary>
        /// Browser OAuth sign-in using the auth fields on Data Config Settings, with a progress bar and
        /// dialogs. Shared by the Sign In menu and the Data Config Sheet window. Fails with a dialog when
        /// the settings asset is missing.
        /// </summary>
        public static async Task SignInWithUiAsync()
        {
            try
            {
                DataConfigSettings settings = DataConfigSettingsLoader.Load();
                EditorUtility.DisplayProgressBar(
                    "Data Config Sheet",
                    "Waiting for Google sign-in in your browser…",
                    0.4f);
                await GoogleOAuthSession.SignInAsync(settings);
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
                // Refresh any open Data Config UI after the token write.
                RepaintInspectors();
            }
        }


        /// <summary>
        /// Clears the local OAuth token (path from Data Config Settings) with dialogs. Shared by the
        /// Sign Out menu and the Data Config Sheet window.
        /// </summary>
        public static void SignOutWithUi()
        {
            try
            {
                GoogleOAuthSession.SignOut(DataConfigSettingsLoader.Load());
                EditorUtility.DisplayDialog("Data Config Sheet", "Signed out.", "OK");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog("Data Config Sheet sign-out failed", exception.Message, "OK");
            }
            finally
            {
                // Refresh any open Data Config UI after the token delete.
                RepaintInspectors();
            }
        }

        #endregion


        #region Helpers

        static void RepaintInspectors()
        {
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }


        /// <summary>
        /// One Pull All job: live Pull with no per-config dialog, mapped into a pure result carrying the
        /// manifest rows. A failed PullReport becomes a failed result; thrown errors are caught by the runner.
        /// </summary>
        static async Task<PullJobResult> RunPullJobAsync(PullConfig config, DataConfigSettings settings)
        {
            LivePullOutcome outcome = await DataConfigPullRunner.PullWithSourceAsync(config, settings);
            string body = outcome.Report.Format();
            Debug.Log("[DataConfigSheet]\n" + body);

            if (!outcome.Report.Success)
                return PullJobResult.Failed(config.name, body);

            return PullJobResult.Passed(
                config.name,
                body,
                outcome.Source,
                ConfigManifestStore.BuildAssetRows(outcome.Report));
        }


        /// <summary>
        /// All jobs passed: writes the Config Manifest (when the settings set a path) and checks the saved
        /// asset. Any job failed: keeps the previous manifest (a partial one would claim completeness) and
        /// checks the required Config Types against what the passed jobs produced, so missing types still surface.
        /// </summary>
        static void CheckManifest(
            DataConfigSettings settings,
            IReadOnlyList<PullJobResult> results,
            List<string> findings)
        {
            bool everyJobPassed = true;
            for (int i = 0; i < results.Count; i++)
                everyJobPassed &= results[i].Success;

            IReadOnlyList<string> problems;
            if (everyJobPassed)
            {
                if (!string.IsNullOrWhiteSpace(settings.ManifestAssetPath))
                {
                    ConfigManifestSnapshot snapshot = ConfigManifestSnapshot.FromJobs(
                        settings.ActiveEnvironmentKey,
                        DateTime.UtcNow,
                        results);
                    ConfigManifestStore.Write(settings.ManifestAssetPath, snapshot);
                }

                problems = ConfigManifestStore.CheckSettings(settings);
            }
            else
            {
                problems = ManifestCompleteness.CheckRun(
                    settings.ActiveEnvironmentKey,
                    settings.RequiredConfigTypeNames,
                    results);
            }

            findings.AddRange(problems);
        }

        #endregion


    }
}
