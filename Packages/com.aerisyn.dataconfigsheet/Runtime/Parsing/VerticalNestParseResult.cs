#nullable disable
using System;
using System.Collections.Generic;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>Structured Vertical Nest failure with sheet coordinates (0-based indices).</summary>
    public sealed class VerticalNestParseError
    {


        public VerticalNestParseError(int row, int column, string message)
        {
            Row = row;
            Column = column;
            Message = message ?? "";
        }


        /// <summary>0-based grid row (-1 when the error is not cell-local).</summary>
        public int Row { get; }


        /// <summary>0-based grid column (-1 when the error is not cell-local).</summary>
        public int Column { get; }


        public string Message { get; }


    }


    /// <summary>Outcome of a type-driven Vertical Nest parse over one grid.</summary>
    public sealed class VerticalNestParseResult
    {


        VerticalNestParseResult(
            bool success,
            IReadOnlyList<VerticalNestParseError> errors,
            IReadOnlyList<string> warnings)
        {
            Success = success;
            Errors = errors;
            Warnings = warnings;
        }


        public bool Success { get; }


        public IReadOnlyList<VerticalNestParseError> Errors { get; }


        /// <summary>
        /// Non-fatal notices (e.g. Local Only column present on the sheet). Empty when none.
        /// </summary>
        public IReadOnlyList<string> Warnings { get; }


        public static VerticalNestParseResult Ok()
        {
            return Ok(Array.Empty<string>());
        }


        public static VerticalNestParseResult Ok(IReadOnlyList<string> warnings)
        {
            return new VerticalNestParseResult(
                true,
                Array.Empty<VerticalNestParseError>(),
                warnings ?? Array.Empty<string>());
        }


        public static VerticalNestParseResult Fail(IReadOnlyList<VerticalNestParseError> errors)
        {
            if (errors == null || errors.Count == 0)
            {
                return new VerticalNestParseResult(
                    false,
                    new[] { new VerticalNestParseError(-1, -1, "Parse failed with no details.") },
                    Array.Empty<string>());
            }

            return new VerticalNestParseResult(false, errors, Array.Empty<string>());
        }


        public static VerticalNestParseResult Fail(int row, int column, string message)
        {
            return Fail(new[] { new VerticalNestParseError(row, column, message) });
        }


    }
}
