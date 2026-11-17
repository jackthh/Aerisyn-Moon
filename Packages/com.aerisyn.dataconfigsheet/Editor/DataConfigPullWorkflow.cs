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
        /// Finds every Pull Config in the project (stable asset-path order) and Pulls each one. A failing
        /// config never stops the run, so one summary shows every passed and failed job. Each config's
        /// Include In Pull ticks apply. Pull All uses one Environment Profile (the first Pull Config's): a Pull
        /// Config that points at another profile is reported as failed and not pulled. When every job passes it
        /// writes the Config Manifest at that profile's path and checks it against the required Config Type
        /// list. When any job failed the manifest is left untouched, but the required list is still checked
        /// against what the passed jobs produced, so the summary shows job failures and missing types together.
        /// A missing type or wrong environment fails the overall run.
        /// </summary>
        /// <returns>The combined summary (OverallSuccess false when no Pull Config exists).</returns>
        public static async Task<PullAllSummary> PullAllWithUiAsync()
        {
            List<PullConfig> configs = FindPullConfigsInStableOrder();
            if (configs.Count == 0)
            {
                EditorUtility.DisplayDialog(
                    "Data Config Sheet",
                    "No PullConfig assets found. Create one via Assets → Create → Aerisyn → Data Config Sheet → Pull Config.",
                    "OK");
                return PullAllSummary.Create(Array.Empty<PullJobResult>(), null);
            }

            // One manifest, one active environment: the first Pull Config's profile is the project's path.
            EnvironmentProfile manifestProfile = null;
            string[] configNames = new string[configs.Count];
            string[] profileIds = new string[configs.Count];
            for (int i = 0; i < configs.Count; i++)
            {
                configNames[i] = configs[i].name;
                EnvironmentProfile profile = configs[i].EnvironmentProfile;
                if (profile == null)
                    continue;

                if (manifestProfile == null)
                    manifestProfile = profile;

                string profilePath = AssetDatabase.GetAssetPath(profile);
                profileIds[i] = string.IsNullOrEmpty(profilePath) ? profile.name : profilePath;
            }

            string[] profileMismatches = PullAllProfileRule.FindMismatches(configNames, profileIds);

            // Job index == config index; a mismatched config becomes a failed result without pulling.
            List<PullAllJob> jobs = new List<PullAllJob>(configs.Count);
            for (int i = 0; i < configs.Count; i++)
            {
                PullConfig config = configs[i];
                string mismatch = profileMismatches[i];
                if (mismatch != null)
                    jobs.Add(new PullAllJob(config.name, () => Task.FromResult(PullJobResult.Failed(config.name, mismatch))));
                else
                    jobs.Add(new PullAllJob(config.name, () => RunPullJobAsync(config)));
            }

            IReadOnlyList<PullJobResult> results;
            List<string> manifestFindings = new List<string>();
            try
            {
                EditorUtility.DisplayProgressBar("Data Config Sheet", $"Pull All ({configs.Count} Pull Configs)…", 0.2f);
                results = await PullAllRunner.RunAllAsync(jobs);

                if (manifestProfile != null)
                {
                    try
                    {
                        CheckManifest(manifestProfile, results, manifestFindings);
                    }
                    catch (Exception exception)
                    {
                        // Per-job exceptions are already failed results; this is manifest IO (path, hashing) failing.
                        Debug.LogException(exception);
                        manifestFindings.Add($"Config Manifest write failed: {exception.Message}");
                    }
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


        /// <summary>
        /// Pull Configs sorted by asset path (ordinal) so run order, logs, and the summary do not depend
        /// on AssetDatabase GUID enumeration order.
        /// </summary>
        static List<PullConfig> FindPullConfigsInStableOrder()
        {
            string[] guids = AssetDatabase.FindAssets("t:PullConfig");
            List<string> paths = new List<string>(guids.Length);
            for (int i = 0; i < guids.Length; i++)
                paths.Add(AssetDatabase.GUIDToAssetPath(guids[i]));

            paths.Sort(StringComparer.Ordinal);

            List<PullConfig> configs = new List<PullConfig>(paths.Count);
            for (int i = 0; i < paths.Count; i++)
            {
                PullConfig config = AssetDatabase.LoadAssetAtPath<PullConfig>(paths[i]);
                if (config != null)
                    configs.Add(config);
            }

            return configs;
        }


        /// <summary>
        /// One Pull All job: live Pull with no per-config dialog, mapped into a pure result carrying the
        /// manifest rows. A failed PullReport becomes a failed result; thrown errors are caught by the runner.
        /// </summary>
        static async Task<PullJobResult> RunPullJobAsync(PullConfig config)
        {
            LivePullOutcome outcome = await DataConfigPullRunner.PullWithSourceAsync(config);
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
        /// All jobs passed: writes the Config Manifest (when the profile sets a path) and checks the saved
        /// asset. Any job failed: keeps the previous manifest (a partial one would claim completeness) and
        /// checks the required Config Types against what the passed jobs produced, so missing types still surface.
        /// </summary>
        static void CheckManifest(
            EnvironmentProfile profile,
            IReadOnlyList<PullJobResult> results,
            List<string> findings)
        {
            bool everyJobPassed = true;
            for (int i = 0; i < results.Count; i++)
                everyJobPassed &= results[i].Success;

            IReadOnlyList<string> problems;
            if (everyJobPassed)
            {
                if (!string.IsNullOrWhiteSpace(profile.ManifestAssetPath))
                {
                    ConfigManifestSnapshot snapshot = ConfigManifestSnapshot.FromJobs(
                        profile.ActiveEnvironmentKey,
                        DateTime.UtcNow,
                        results);
                    ConfigManifestStore.Write(profile.ManifestAssetPath, snapshot);
                }

                problems = ConfigManifestStore.CheckProfile(profile);
            }
            else
            {
                problems = ManifestCompleteness.CheckRun(
                    profile.ActiveEnvironmentKey,
                    profile.RequiredConfigTypeNames,
                    results);
            }

            findings.AddRange(problems);
        }

        #endregion


    }
}
