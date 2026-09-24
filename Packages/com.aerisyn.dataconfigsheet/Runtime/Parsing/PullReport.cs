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
            IReadOnlyList<VerticalNestParseError> errors)
        {
            Success = success;
            PullConfigName = pullConfigName ?? "";
            Writes = writes ?? Array.Empty<PullWriteAction>();
            Errors = errors ?? Array.Empty<VerticalNestParseError>();
        }


        public bool Success { get; }


        public string PullConfigName { get; }


        public IReadOnlyList<PullWriteAction> Writes { get; }


        public IReadOnlyList<VerticalNestParseError> Errors { get; }


        #region Factories

        public static PullReport Succeeded(string pullConfigName, IReadOnlyList<PullWriteAction> writes)
        {
            return new PullReport(true, pullConfigName, writes, Array.Empty<VerticalNestParseError>());
        }


        public static PullReport Failed(string pullConfigName, IReadOnlyList<VerticalNestParseError> errors)
        {
            if (errors == null || errors.Count == 0)
            {
                return new PullReport(
                    false,
                    pullConfigName,
                    Array.Empty<PullWriteAction>(),
                    new[] { new VerticalNestParseError(-1, -1, "Pull failed with no details.") });
            }

            return new PullReport(false, pullConfigName, Array.Empty<PullWriteAction>(), errors);
        }

        #endregion


        #region Format

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
