#nullable disable
using System.Collections.Generic;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>How a setup item affects Pull All.</summary>
    public enum SetupItemStatus
    {
        /// <summary>Done; nothing to do.</summary>
        Ok,

        /// <summary>Advisory; Pull All still runs.</summary>
        Warning,

        /// <summary>Pull All would fail until this is fixed.</summary>
        Blocking,
    }


    /// <summary>One prerequisite row in the Data Config Sheet window.</summary>
    public sealed class SetupItem
    {
        public SetupItem(string id, string label, SetupItemStatus status, string detail)
        {
            Id = id;
            Label = label;
            Status = status;
            Detail = detail;
        }


        public string Id { get; }

        public string Label { get; }

        public SetupItemStatus Status { get; }

        /// <summary>One short line: what is set, or what to do next.</summary>
        public string Detail { get; }
    }


    /// <summary>
    /// Plain facts the Editor gathers (settings fields plus file and asset lookups) so
    /// <see cref="SetupChecklist"/> stays free of Unity and the file system.
    /// </summary>
    public struct SetupInputs
    {
        public IReadOnlyList<string> EnvironmentKeys;
        public string ActiveEnvironmentKey;
        public IReadOnlyList<string> FeatureKeys;

        public string PullAllFolderPath;
        public bool PullAllFolderExists;

        /// <summary>Pull Configs found under the folder (0 when the folder is blank or missing).</summary>
        public int PullConfigCount;

        /// <summary>True for OAuth user auth, false for service account.</summary>
        public bool UsesOAuth;

        /// <summary>client_secrets path (OAuth) or service-account JSON path.</summary>
        public string CredentialPath;
        public bool CredentialExists;

        /// <summary>OAuth user token present. Ignored for service accounts.</summary>
        public bool SignedIn;

        public string ManifestAssetPath;
    }


    /// <summary>
    /// What the Data Config Sheet window shows as done, advisory or blocking, and whether Pull All is
    /// ready to run. Mirrors the failures Pull All reports, but up front instead of after a click.
    /// </summary>
    public static class SetupChecklist
    {


        #region Item ids

        public const string EnvironmentId = "environment";
        public const string FeatureKeysId = "feature-keys";
        public const string PullAllFolderId = "pull-all-folder";
        public const string AuthId = "auth";
        public const string ManifestId = "manifest";

        #endregion


        #region Evaluate

        /// <summary>The checklist rows in display order.</summary>
        public static List<SetupItem> Evaluate(SetupInputs inputs)
        {
            return new List<SetupItem>
            {
                EvaluateEnvironment(inputs),
                EvaluateFeatureKeys(inputs),
                EvaluatePullAllFolder(inputs),
                EvaluateAuth(inputs),
                EvaluateManifest(inputs),
            };
        }


        /// <summary>True when no item is <see cref="SetupItemStatus.Blocking"/>.</summary>
        public static bool IsReadyForPullAll(IReadOnlyList<SetupItem> items)
        {
            return CountBlocking(items) == 0;
        }


        public static int CountBlocking(IReadOnlyList<SetupItem> items)
        {
            int count = 0;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Status == SetupItemStatus.Blocking)
                    count++;
            }

            return count;
        }

        #endregion


        #region Rows

        static SetupItem EvaluateEnvironment(SetupInputs inputs)
        {
            const string label = "Active environment";
            if (string.IsNullOrWhiteSpace(inputs.ActiveEnvironmentKey))
                return new SetupItem(EnvironmentId, label, SetupItemStatus.Blocking, "Pick the active Environment Key.");

            string problem = KeyListRules.DescribeProblem(
                "Environment Key", inputs.EnvironmentKeys, inputs.ActiveEnvironmentKey);
            return problem != null
                ? new SetupItem(EnvironmentId, label, SetupItemStatus.Blocking, problem)
                : new SetupItem(EnvironmentId, label, SetupItemStatus.Ok, inputs.ActiveEnvironmentKey);
        }


        static SetupItem EvaluateFeatureKeys(SetupInputs inputs)
        {
            const string label = "Feature Keys";
            int count = inputs.FeatureKeys == null ? 0 : inputs.FeatureKeys.Count;
            return count == 0
                ? new SetupItem(FeatureKeysId, label, SetupItemStatus.Warning, "None yet. Add the keys your Pull Configs use.")
                : new SetupItem(FeatureKeysId, label, SetupItemStatus.Ok, $"{count} defined.");
        }


        static SetupItem EvaluatePullAllFolder(SetupInputs inputs)
        {
            const string label = "Pull All folder";
            if (string.IsNullOrWhiteSpace(inputs.PullAllFolderPath))
                return new SetupItem(PullAllFolderId, label, SetupItemStatus.Blocking, "Choose the folder Pull All scans.");

            if (!inputs.PullAllFolderExists)
                return new SetupItem(PullAllFolderId, label, SetupItemStatus.Blocking, $"Folder '{inputs.PullAllFolderPath}' does not exist.");

            if (inputs.PullConfigCount <= 0)
                return new SetupItem(PullAllFolderId, label, SetupItemStatus.Blocking, $"No Pull Configs under '{inputs.PullAllFolderPath}'.");

            return new SetupItem(
                PullAllFolderId,
                label,
                SetupItemStatus.Ok,
                $"{inputs.PullConfigCount} Pull Config(s) under '{inputs.PullAllFolderPath}'.");
        }


        static SetupItem EvaluateAuth(SetupInputs inputs)
        {
            const string label = "Google auth";
            string credentialKind = inputs.UsesOAuth ? "OAuth client secrets" : "Service account JSON";

            if (string.IsNullOrWhiteSpace(inputs.CredentialPath))
                return new SetupItem(AuthId, label, SetupItemStatus.Blocking, $"Set the {credentialKind} file.");

            if (!inputs.CredentialExists)
                return new SetupItem(AuthId, label, SetupItemStatus.Blocking, $"{credentialKind} file not found at '{inputs.CredentialPath}'.");

            if (!inputs.UsesOAuth)
                return new SetupItem(AuthId, label, SetupItemStatus.Ok, "Service account.");

            return inputs.SignedIn
                ? new SetupItem(AuthId, label, SetupItemStatus.Ok, "Signed in.")
                : new SetupItem(AuthId, label, SetupItemStatus.Blocking, "Not signed in. Use Sign In.");
        }


        static SetupItem EvaluateManifest(SetupInputs inputs)
        {
            const string label = "Config Manifest";
            return string.IsNullOrWhiteSpace(inputs.ManifestAssetPath)
                ? new SetupItem(ManifestId, label, SetupItemStatus.Ok, "Skipped (no path set).")
                : new SetupItem(ManifestId, label, SetupItemStatus.Ok, inputs.ManifestAssetPath);
        }

        #endregion


    }
}
