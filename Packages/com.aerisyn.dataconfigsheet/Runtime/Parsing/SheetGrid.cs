#nullable disable
using System;
using System.Collections.Generic;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// In-memory rectangular cell grid (Sheets API values or fixtures).
    /// Null and whitespace-only cells are blank for Vertical Nest.
    /// </summary>
    public sealed class SheetGrid
    {


        #region Fields

        readonly string[,] _cells;
        readonly int _rowCount;
        readonly int _columnCount;

        #endregion


        #region Construction

        public SheetGrid(string[,] cells)
        {
            if (cells == null)
                throw new ArgumentNullException(nameof(cells));

            _rowCount = cells.GetLength(0);
            _columnCount = cells.GetLength(1);
            _cells = new string[_rowCount, _columnCount];

            for (int row = 0; row < _rowCount; row++)
            {
                for (int col = 0; col < _columnCount; col++)
                    _cells[row, col] = Normalize(cells[row, col]);
            }
        }


        /// <summary>
        /// Builds a grid from jagged rows; shorter rows are padded with blanks.
        /// </summary>
        public static SheetGrid FromRows(IReadOnlyList<IReadOnlyList<string>> rows)
        {
            if (rows == null)
                throw new ArgumentNullException(nameof(rows));

            int rowCount = rows.Count;
            int columnCount = 0;
            for (int r = 0; r < rowCount; r++)
            {
                IReadOnlyList<string> row = rows[r];
                if (row != null && row.Count > columnCount)
                    columnCount = row.Count;
            }

            string[,] cells = new string[rowCount, columnCount];
            for (int r = 0; r < rowCount; r++)
            {
                IReadOnlyList<string> row = rows[r];
                for (int c = 0; c < columnCount; c++)
                {
                    if (row != null && c < row.Count)
                        cells[r, c] = row[c];
                    else
                        cells[r, c] = "";
                }
            }

            return new SheetGrid(cells);
        }


        /// <summary>
        /// Builds a grid from Sheets API-style object rows (cell values stay intact; commas are not delimiters).
        /// </summary>
        public static SheetGrid FromObjectRows(IList<IList<object>> rows)
        {
            if (rows == null || rows.Count == 0)
                return new SheetGrid(new string[0, 0]);

            int rowCount = rows.Count;
            int columnCount = 0;
            for (int r = 0; r < rowCount; r++)
            {
                IList<object> row = rows[r];
                if (row != null && row.Count > columnCount)
                    columnCount = row.Count;
            }

            string[,] cells = new string[rowCount, columnCount];
            for (int r = 0; r < rowCount; r++)
            {
                IList<object> row = rows[r];
                for (int c = 0; c < columnCount; c++)
                {
                    if (row != null && c < row.Count && row[c] != null)
                        cells[r, c] = row[c].ToString();
                    else
                        cells[r, c] = "";
                }
            }

            return new SheetGrid(cells);
        }

        #endregion


        #region Public API

        public int RowCount => _rowCount;


        public int ColumnCount => _columnCount;


        /// <summary>Cell text at 0-based coordinates; blank cells are empty string.</summary>
        public string GetCell(int row, int column)
        {
            if (row < 0 || row >= _rowCount || column < 0 || column >= _columnCount)
                return "";

            return _cells[row, column];
        }


        public bool IsBlank(int row, int column)
        {
            return GetCell(row, column).Length == 0;
        }

        #endregion


        #region Helpers

        static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "";

            return value.Trim();
        }

        #endregion


    }
}
