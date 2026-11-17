#nullable disable
using System;
using System.Collections.Generic;
using System.Text;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// Combined Pull All outcome: per-config results plus Config Manifest findings (completeness
    /// problems). Overall success needs at least one job, no failed job, and no manifest finding.
    /// </summary>
    public sealed class PullAllSummary
    {


        PullAllSummary(IReadOnlyList<PullJobResult> results, IReadOnlyList<string> manifestFindings)
        {
            Results = results;
            ManifestFindings = manifestFindings;

            int passed = 0;
            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].Success)
                    passed++;
            }

            PassedCount = passed;
            FailedCount = results.Count - passed;
        }


        #region Public API

        /// <summary>Per-config results in run order.</summary>
        public IReadOnlyList<PullJobResult> Results { get; }


        /// <summary>Config Manifest completeness problems; empty when the manifest check passed or did not run.</summary>
        public IReadOnlyList<string> ManifestFindings { get; }


        public int PassedCount { get; }


        public int FailedCount { get; }


        /// <summary>
        /// True only when something ran, every job passed, and the manifest check found nothing.
        /// An empty run is never green, so an unpulled project cannot look complete.
        /// </summary>
        public bool OverallSuccess => Results.Count > 0 && FailedCount == 0 && ManifestFindings.Count == 0;

        #endregion


        #region Factories

        /// <summary>Aggregates per-config results and optional manifest findings (null means none).</summary>
        public static PullAllSummary Create(
            IReadOnlyList<PullJobResult> results,
            IReadOnlyList<string> manifestFindings)
        {
            if (results == null)
                throw new ArgumentNullException(nameof(results));

            return new PullAllSummary(
                results,
                manifestFindings ?? Array.Empty<string>());
        }

        #endregion


        #region Format

        /// <summary>
        /// Console/dialog text: headline with counts, each job on its own line (failures first with their
        /// reason), then manifest findings.
        /// </summary>
        public string Format()
        {
            StringBuilder builder = new StringBuilder(256);

            if (Results.Count == 0)
            {
                builder.Append("Pull All failed: No Pull Configs ran.");
                for (int i = 0; i < ManifestFindings.Count; i++)
                    builder.AppendLine().Append("  • ").Append(ManifestFindings[i]);

                return builder.ToString();
            }

            builder.Append(OverallSuccess ? "Pull All complete: " : "Pull All failed: ");
            builder.Append(PassedCount).Append(" passed, ").Append(FailedCount).Append(" failed.");

            for (int i = 0; i < Results.Count; i++)
            {
                if (!Results[i].Success)
                    AppendJob(builder, "FAILED", Results[i]);
            }

            for (int i = 0; i < Results.Count; i++)
            {
                if (Results[i].Success)
                    AppendJob(builder, "passed", Results[i]);
            }

            if (ManifestFindings.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine();
                builder.Append("Config Manifest check failed:");
                for (int i = 0; i < ManifestFindings.Count; i++)
                    builder.AppendLine().Append("  • ").Append(ManifestFindings[i]);
            }

            return builder.ToString();
        }

        #endregion


        #region Private helpers

        static void AppendJob(StringBuilder builder, string label, PullJobResult result)
        {
            builder.AppendLine().AppendLine();
            builder.Append(label).Append(": ").Append(result.Name);

            // Indent multi-line PullReport bodies so each job stays visually grouped.
            string detail = (result.Detail ?? "").TrimEnd();
            if (detail.Length == 0)
                return;

            string[] lines = detail.Replace("\r\n", "\n").Split('\n');
            for (int i = 0; i < lines.Length; i++)
                builder.AppendLine().Append("    ").Append(lines[i]);
        }

        #endregion


    }
}
