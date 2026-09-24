#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// Type-driven Vertical Nest parse: Preamble skip, Header Row by Field Headers,
    /// !!! Ignore Marker columns, blank-parent struct nests, and primitive array columns.
    /// </summary>
    public static class VerticalNestParser
    {


        const string IgnoreMarker = "!!!";


        #region Public API

        /// <summary>
        /// Clears and fills the target Config Type's root items list from the grid.
        /// Nest shape comes from reflection over serializable fields (type-driven).
        /// </summary>
        public static VerticalNestParseResult ParseInto(object target, SheetGrid grid)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));

            List<VerticalNestParseError> errors = new List<VerticalNestParseError>();

            FieldInfo rootListField;
            Type rootElementType;
            string resolveError;
            if (!ConfigTypeItemsField.TryResolve(target.GetType(), out rootListField, out rootElementType, out resolveError))
            {
                errors.Add(new VerticalNestParseError(-1, -1, resolveError));
                return VerticalNestParseResult.Fail(errors);
            }

            NestLevelSchema rootSchema;
            if (!TryBuildSchema(rootElementType, rootListField, out rootSchema, errors))
                return VerticalNestParseResult.Fail(errors);

            List<ColumnBinding> columns;
            int headerRow;
            if (!TryBindHeader(grid, rootSchema, out headerRow, out columns, errors))
                return VerticalNestParseResult.Fail(errors);

            IList itemsList = ConfigTypeItemsField.EnsureList(target, rootListField, rootElementType);
            itemsList.Clear();
            object[] currentByLevel = new object[CountLevels(rootSchema)];

            for (int row = headerRow + 1; row < grid.RowCount; row++)
            {
                if (IsRowBlank(grid, row, columns))
                    continue;

                if (!TryParseDataRow(grid, row, columns, rootSchema, itemsList, currentByLevel, errors))
                    return VerticalNestParseResult.Fail(errors);
            }

            return errors.Count > 0
                ? VerticalNestParseResult.Fail(errors)
                : VerticalNestParseResult.Ok();
        }

        #endregion


        #region Schema

        /// <summary>
        /// One nest level: scalar Field Headers, primitive-array columns, and at most one struct-list child.
        /// </summary>
        sealed class NestLevelSchema
        {
            public FieldInfo ListFieldOnParent;
            public Type ElementType;
            public List<FieldInfo> ScalarFields = new List<FieldInfo>();
            public List<FieldInfo> PrimitiveArrayFields = new List<FieldInfo>();
            public NestLevelSchema Child;
        }


        sealed class ColumnBinding
        {
            public int Column;
            public NestLevelSchema Level;
            public int LevelIndex;
            public FieldInfo Field;
            public bool IsPrimitiveArray;
        }


        static bool TryBuildSchema(
            Type elementType,
            FieldInfo listFieldOnParent,
            out NestLevelSchema schema,
            List<VerticalNestParseError> errors)
        {
            schema = new NestLevelSchema
            {
                ElementType = elementType,
                ListFieldOnParent = listFieldOnParent,
            };

            FieldInfo structListField = null;
            Type structListElementType = null;

            FieldInfo[] fields = elementType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];
                if (!ConfigTypeItemsField.IsSerializableField(field))
                    continue;

                Type collectionElement;
                if (ConfigTypeItemsField.TryGetCollectionElementType(field.FieldType, out collectionElement))
                {
                    if (ConfigTypeItemsField.IsPrimitiveOrString(collectionElement))
                    {
                        schema.PrimitiveArrayFields.Add(field);
                        continue;
                    }

                    if (structListField != null)
                    {
                        errors.Add(new VerticalNestParseError(
                            -1,
                            -1,
                            $"Type '{elementType.Name}' has multiple struct list fields; Vertical Nest v1 allows one nest child."));
                        return false;
                    }

                    structListField = field;
                    structListElementType = collectionElement;
                    continue;
                }

                schema.ScalarFields.Add(field);
            }

            if (structListField != null)
            {
                NestLevelSchema child;
                if (!TryBuildSchema(structListElementType, structListField, out child, errors))
                    return false;

                schema.Child = child;
            }

            return true;
        }


        static int CountLevels(NestLevelSchema root)
        {
            int count = 0;
            NestLevelSchema cursor = root;
            while (cursor != null)
            {
                count++;
                cursor = cursor.Child;
            }

            return count;
        }


        static void CollectExpectedHeaders(NestLevelSchema level, List<string> headers)
        {
            for (int i = 0; i < level.ScalarFields.Count; i++)
                headers.Add(level.ScalarFields[i].Name);

            for (int i = 0; i < level.PrimitiveArrayFields.Count; i++)
                headers.Add(level.PrimitiveArrayFields[i].Name);

            if (level.Child != null)
                CollectExpectedHeaders(level.Child, headers);
        }

        #endregion


        #region Header binding

        /// <summary>
        /// Finds the Header Row by matching Field Headers; row above supplies !!! Ignore Marker columns.
        /// </summary>
        static bool TryBindHeader(
            SheetGrid grid,
            NestLevelSchema rootSchema,
            out int headerRow,
            out List<ColumnBinding> columns,
            List<VerticalNestParseError> errors)
        {
            headerRow = -1;
            columns = null;

            List<string> expected = new List<string>();
            CollectExpectedHeaders(rootSchema, expected);
            if (expected.Count == 0)
            {
                errors.Add(new VerticalNestParseError(-1, -1, "Config type shape has no Field Headers to match."));
                return false;
            }

            for (int row = 0; row < grid.RowCount; row++)
            {
                List<ColumnBinding> bound;
                if (!TryMatchHeaderRow(grid, row, rootSchema, expected, out bound))
                    continue;

                headerRow = row;
                columns = bound;
                return true;
            }

            errors.Add(new VerticalNestParseError(
                -1,
                -1,
                "Could not find Header Row matching Field Headers: " + string.Join(", ", expected) + "."));
            return false;
        }


        static bool TryMatchHeaderRow(
            SheetGrid grid,
            int row,
            NestLevelSchema rootSchema,
            List<string> expectedHeaders,
            out List<ColumnBinding> columns)
        {
            columns = null;

            // Marker row sits immediately above the Header Row when present
            bool[] ignored = new bool[grid.ColumnCount];
            if (row > 0)
            {
                for (int col = 0; col < grid.ColumnCount; col++)
                {
                    if (string.Equals(grid.GetCell(row - 1, col), IgnoreMarker, StringComparison.Ordinal))
                        ignored[col] = true;
                }
            }

            // All headers participate in Header Row discovery; !!! columns are omitted from parse bindings
            Dictionary<string, int> allHeaders = new Dictionary<string, int>(StringComparer.Ordinal);
            Dictionary<string, int> parseHeaders = new Dictionary<string, int>(StringComparer.Ordinal);
            for (int col = 0; col < grid.ColumnCount; col++)
            {
                string header = grid.GetCell(row, col);
                if (header.Length == 0)
                    continue;

                if (allHeaders.ContainsKey(header))
                    return false;

                allHeaders[header] = col;
                if (!ignored[col])
                    parseHeaders[header] = col;
            }

            for (int i = 0; i < expectedHeaders.Count; i++)
            {
                if (!allHeaders.ContainsKey(expectedHeaders[i]))
                    return false;
            }

            columns = new List<ColumnBinding>();
            BindLevelColumns(rootSchema, 0, parseHeaders, columns);
            return true;
        }


        static void BindLevelColumns(
            NestLevelSchema level,
            int levelIndex,
            Dictionary<string, int> parseHeaders,
            List<ColumnBinding> columns)
        {
            for (int i = 0; i < level.ScalarFields.Count; i++)
            {
                FieldInfo field = level.ScalarFields[i];
                int column;
                if (!parseHeaders.TryGetValue(field.Name, out column))
                    continue;

                columns.Add(new ColumnBinding
                {
                    Column = column,
                    Level = level,
                    LevelIndex = levelIndex,
                    Field = field,
                    IsPrimitiveArray = false,
                });
            }

            for (int i = 0; i < level.PrimitiveArrayFields.Count; i++)
            {
                FieldInfo field = level.PrimitiveArrayFields[i];
                int column;
                if (!parseHeaders.TryGetValue(field.Name, out column))
                    continue;

                columns.Add(new ColumnBinding
                {
                    Column = column,
                    Level = level,
                    LevelIndex = levelIndex,
                    Field = field,
                    IsPrimitiveArray = true,
                });
            }

            if (level.Child != null)
                BindLevelColumns(level.Child, levelIndex + 1, parseHeaders, columns);
        }

        #endregion


        #region Row parse

        static bool IsRowBlank(SheetGrid grid, int row, List<ColumnBinding> columns)
        {
            for (int i = 0; i < columns.Count; i++)
            {
                if (!grid.IsBlank(row, columns[i].Column))
                    return false;
            }

            return true;
        }


        /// <summary>
        /// Applies one data row: non-blank scalars open a new nest element; primitive cells append.
        /// </summary>
        static bool TryParseDataRow(
            SheetGrid grid,
            int row,
            List<ColumnBinding> columns,
            NestLevelSchema rootSchema,
            IList rootItems,
            object[] currentByLevel,
            List<VerticalNestParseError> errors)
        {
            NestLevelSchema[] levels = FlattenLevels(rootSchema);
            bool[] levelHasScalar = new bool[levels.Length];

            for (int i = 0; i < columns.Count; i++)
            {
                ColumnBinding binding = columns[i];
                if (binding.IsPrimitiveArray)
                    continue;

                if (!grid.IsBlank(row, binding.Column))
                    levelHasScalar[binding.LevelIndex] = true;
            }

            for (int levelIndex = 0; levelIndex < levels.Length; levelIndex++)
            {
                if (!levelHasScalar[levelIndex])
                    continue;

                object instance = Activator.CreateInstance(levels[levelIndex].ElementType);
                if (instance == null)
                {
                    errors.Add(new VerticalNestParseError(
                        row,
                        -1,
                        $"Could not create instance of '{levels[levelIndex].ElementType.Name}'."));
                    return false;
                }

                if (levelIndex == 0)
                {
                    rootItems.Add(instance);
                }
                else
                {
                    object parent = currentByLevel[levelIndex - 1];
                    if (parent == null)
                    {
                        errors.Add(new VerticalNestParseError(
                            row,
                            -1,
                            $"Nested '{levels[levelIndex].ElementType.Name}' values appear before a parent row."));
                        return false;
                    }

                    IList parentList = ConfigTypeItemsField.EnsureList(
                        parent,
                        levels[levelIndex].ListFieldOnParent,
                        levels[levelIndex].ElementType);
                    parentList.Add(instance);
                }

                currentByLevel[levelIndex] = instance;
                for (int deeper = levelIndex + 1; deeper < currentByLevel.Length; deeper++)
                    currentByLevel[deeper] = null;
            }

            for (int i = 0; i < columns.Count; i++)
            {
                ColumnBinding binding = columns[i];
                string cell = grid.GetCell(row, binding.Column);
                if (cell.Length == 0)
                    continue;

                object current = currentByLevel[binding.LevelIndex];
                if (current == null)
                {
                    errors.Add(new VerticalNestParseError(
                        row,
                        binding.Column,
                        $"Value for '{binding.Field.Name}' has no open parent nest element."));
                    return false;
                }

                if (binding.IsPrimitiveArray)
                {
                    if (!TryAppendPrimitive(current, binding.Field, cell, row, binding.Column, errors))
                        return false;
                }
                else if (!TryAssignScalar(current, binding.Field, cell, row, binding.Column, errors))
                {
                    return false;
                }
            }

            return true;
        }


        static NestLevelSchema[] FlattenLevels(NestLevelSchema root)
        {
            List<NestLevelSchema> levels = new List<NestLevelSchema>();
            NestLevelSchema cursor = root;
            while (cursor != null)
            {
                levels.Add(cursor);
                cursor = cursor.Child;
            }

            return levels.ToArray();
        }

        #endregion


        #region Field assign

        static bool TryAssignScalar(
            object instance,
            FieldInfo field,
            string cell,
            int row,
            int column,
            List<VerticalNestParseError> errors)
        {
            object converted;
            if (!TryConvert(cell, field.FieldType, out converted))
            {
                errors.Add(new VerticalNestParseError(
                    row,
                    column,
                    $"Cannot convert '{cell}' to {field.FieldType.Name} for field '{field.Name}'."));
                return false;
            }

            field.SetValue(instance, converted);
            return true;
        }


        static bool TryAppendPrimitive(
            object instance,
            FieldInfo field,
            string cell,
            int row,
            int column,
            List<VerticalNestParseError> errors)
        {
            Type elementType;
            if (!ConfigTypeItemsField.TryGetCollectionElementType(field.FieldType, out elementType))
            {
                errors.Add(new VerticalNestParseError(row, column, $"Field '{field.Name}' is not a primitive collection."));
                return false;
            }

            object converted;
            if (!TryConvert(cell, elementType, out converted))
            {
                errors.Add(new VerticalNestParseError(
                    row,
                    column,
                    $"Cannot convert '{cell}' to {elementType.Name} for field '{field.Name}'."));
                return false;
            }

            IList list = ConfigTypeItemsField.EnsureList(instance, field, elementType);
            list.Add(converted);
            return true;
        }


        static bool TryConvert(string cell, Type targetType, out object value)
        {
            value = null;
            Type type = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (type == typeof(string))
            {
                value = cell;
                return true;
            }

            if (type == typeof(int))
            {
                int parsed;
                if (!int.TryParse(cell, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed))
                    return false;
                value = parsed;
                return true;
            }

            if (type == typeof(long))
            {
                long parsed;
                if (!long.TryParse(cell, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed))
                    return false;
                value = parsed;
                return true;
            }

            if (type == typeof(float))
            {
                float parsed;
                if (!float.TryParse(cell, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
                    return false;
                value = parsed;
                return true;
            }

            if (type == typeof(double))
            {
                double parsed;
                if (!double.TryParse(cell, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
                    return false;
                value = parsed;
                return true;
            }

            if (type == typeof(bool))
            {
                bool parsed;
                if (!bool.TryParse(cell, out parsed))
                    return false;
                value = parsed;
                return true;
            }

            if (type.IsEnum)
            {
                try
                {
                    value = Enum.Parse(type, cell, ignoreCase: true);
                    return true;
                }
                catch (ArgumentException)
                {
                    return false;
                }
            }

            return false;
        }

        #endregion


        }
}
