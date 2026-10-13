#nullable disable
using System;
using System.Collections.Generic;
using System.Text;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>Whether a Baked Asset was created new or overwritten in place.</summary>
    public enum PullWriteKind
    {
        Created,
        Updated,
    }


    /// <summary>One Baked Asset write performed by a successful Pull.</summary>
    public readonly struct PullWriteAction
    {


        public PullWriteAction(string configTypeName, string assetPath, PullWriteKind kind)
        {
            ConfigTypeName = configTypeName ?? "";
            AssetPath = assetPath ?? "";
            Kind = kind;
        }


        public string ConfigTypeName { get; }


        public string AssetPath { get; }


        public PullWriteKind Kind { get; }


    }


    /// <summary>
    /// Human-readable Pull outcome: success summary (assets created/updated) or
    /// failures with designer-facing A1 sheet coordinates when available.
    /// </summary>
    public sealed class PullReport
    {


        PullReport(
            bool success,
            string pullConfigName,
            IReadOnlyList<PullWriteAction> writes,
            IReadOnlyList<VerticalNestParseError> errors,
            IReadOnlyList<string> warnings)
        {
            Success = success;
            PullConfigName = pullConfigName ?? "";
            Writes = writes ?? Array.Empty<PullWriteAction>();
            Errors = errors ?? Array.Empty<VerticalNestParseError>();
            Warnings = warnings ?? Array.Empty<string>();
        }


        public bool Success { get; }


        public string PullConfigName { get; }


        public IReadOnlyList<PullWriteAction> Writes { get; }


        public IReadOnlyList<VerticalNestParseError> Errors { get; }


        /// <summary>
        /// Non-fatal notices (e.g. Local Only column present on the Source Sheet). Empty when none.
        /// </summary>
        public IReadOnlyList<string> Warnings { get; }


        #region Factories

        public static PullReport Succeeded(string pullConfigName, IReadOnlyList<PullWriteAction> writes)
        {
            return Succeeded(pullConfigName, writes, Array.Empty<string>());
        }


        public static PullReport Succeeded(
            string pullConfigName,
            IReadOnlyList<PullWriteAction> writes,
            IReadOnlyList<string> warnings)
        {
            return new PullReport(
                true,
                pullConfigName,
                writes,
                Array.Empty<VerticalNestParseError>(),
                warnings ?? Array.Empty<string>());
        }


        public static PullReport Failed(string pullConfigName, IReadOnlyList<VerticalNestParseError> errors)
        {
            if (errors == null || errors.Count == 0)
            {
                return new PullReport(
                    false,
                    pullConfigName,
                    Array.Empty<PullWriteAction>(),
                    new[] { new VerticalNestParseError(-1, -1, "Pull failed with no details.") },
                    Array.Empty<string>());
            }

            return new PullReport(
                false,
                pullConfigName,
                Array.Empty<PullWriteAction>(),
                errors,
                Array.Empty<string>());
        }

        #endregion


        #region Format

        /// <summary>
        /// Copies parse warnings onto the Pull list with the same [tab] prefix as parse errors.
        /// PullFromGrids uses this so Local Only notices reach PullReport.Format / the dialog.
        /// </summary>
        public static void AppendTabPrefixedWarnings(
            string tabTitle,
            IReadOnlyList<string> parseWarnings,
            List<string> destination)
        {
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));
            if (parseWarnings == null || parseWarnings.Count == 0)
                return;

            string prefix = "[" + (tabTitle ?? "") + "] ";
            for (int i = 0; i < parseWarnings.Count; i++)
                destination.Add(prefix + parseWarnings[i]);
        }


        /// <summary>
        /// Builds the console/dialog text: success asset summary, or failures with A1 cells.
        /// </summary>
        public string Format()
        {
            if (Success)
                return FormatSuccess();

            return FormatFailure();
        }


        /// <summary>
        /// Converts 0-based grid indices to Google Sheets A1 (e.g. row 0 col 0 → A1).
        /// Returns null when either index is unknown.
        /// </summary>
        public static string ToA1(int row, int column)
        {
            if (row < 0 || column < 0)
                return null;

            return ColumnLetters(column) + (row + 1).ToString();
        }


        /// <summary>
        /// Designer-facing coordinate: A1 when both indices known; otherwise row N / column X.
        /// </summary>
        public static string FormatCoordinate(int row, int column)
        {
            string a1 = ToA1(row, column);
            if (a1 != null)
                return a1;

            if (row >= 0)
                return "row " + (row + 1);

            if (column >= 0)
                return "column " + ColumnLetters(column);

            return null;
        }

        #endregion


        #region Private helpers

        string FormatSuccess()
        {
            StringBuilder builder = new StringBuilder(256);
            builder.Append("Pull complete for '").Append(PullConfigName).Append("'.");
            builder.AppendLine();

            int writeCount = Writes.Count;
            builder.Append(writeCount).Append(" Baked Asset");
            if (writeCount != 1)
                builder.Append('s');
            builder.AppendLine(":");

            for (int i = 0; i < writeCount; i++)
            {
                PullWriteAction write = Writes[i];
                string verb = write.Kind == PullWriteKind.Created ? "created" : "updated";
                builder.Append("  • ").Append(write.ConfigTypeName).Append(": ").Append(verb).Append(' ')
                    .AppendLine(write.AssetPath);
            }

            // Local Only (and similar) notices: success still writes, but designers must see them
            int warningCount = Warnings.Count;
            if (warningCount > 0)
            {
                builder.AppendLine();
                builder.Append(warningCount).Append(" Warning");
                if (warningCount != 1)
                    builder.Append('s');
                builder.AppendLine(":");

                int limit = Math.Min(warningCount, 20);
                for (int i = 0; i < limit; i++)
                    builder.Append("  • ").AppendLine(Warnings[i]);

                if (warningCount > limit)
                    builder.Append("  (and ").Append(warningCount - limit).Append(" more)");
            }

            return builder.ToString().TrimEnd();
        }


        string FormatFailure()
        {
            StringBuilder builder = new StringBuilder(256);
            builder.Append("Pull failed for '").Append(PullConfigName).Append("'.");
            builder.AppendLine();
            builder.AppendLine("Fix the sheet cells and Pull again (no Baked Assets were written):");

            int limit = Math.Min(Errors.Count, 20);
            for (int i = 0; i < limit; i++)
            {
                VerticalNestParseError error = Errors[i];
                builder.Append("  • ");
                string coord = FormatCoordinate(error.Row, error.Column);
                if (coord != null)
                    builder.Append(coord).Append(' ');
                builder.AppendLine(error.Message);
            }

            if (Errors.Count > limit)
                builder.Append("  (and ").Append(Errors.Count - limit).Append(" more)");

            return builder.ToString().TrimEnd();
        }


        /// <summary>0-based column index → A, B, … Z, AA, AB, …</summary>
        static string ColumnLetters(int zeroBasedColumn)
        {
            // Sheets columns are 1-based letters; convert then peel base-26 digits
            int remaining = zeroBasedColumn + 1;
            char[] buffer = new char[8];
            int length = 0;
            while (remaining > 0)
            {
                remaining--;
                buffer[length++] = (char)('A' + (remaining % 26));
                remaining /= 26;
            }

            Array.Reverse(buffer, 0, length);
            return new string(buffer, 0, length);
        }

        #endregion


    }
}
