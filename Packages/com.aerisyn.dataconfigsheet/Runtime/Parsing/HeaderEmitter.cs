#nullable disable
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// Builds a pasteable Header Row from a Config Type shape (Field Headers / Column Aliases)
    /// and guidance for !!! Ignore Marker note columns and omitted Local Only fields.
    /// </summary>
    public static class HeaderEmitter
    {


        const string IgnoreMarkerGuidance =
            "Place !!! in the row immediately above any notes-only column header. " +
            "Pull skips those columns and never writes their cells into Baked Assets.";


        #region Public API

        /// <summary>
        /// Emits Header Row labels in Vertical Nest column order for the Config Type's root items shape.
        /// Local Only fields are omitted; guidance reports how many were left out.
        /// </summary>
        public static HeaderEmitResult Emit(Type configType)
        {
            if (configType == null)
                throw new ArgumentNullException(nameof(configType));

            FieldInfo rootListField;
            Type rootElementType;
            string resolveError;
            if (!ConfigTypeItemsField.TryResolve(configType, out rootListField, out rootElementType, out resolveError))
                throw new InvalidOperationException(resolveError);

            List<string> headers = new List<string>();
            int localOnlyOmitted = 0;
            CollectEmitHeaders(rootElementType, headers, ref localOnlyOmitted);

            string guidance = BuildGuidance(localOnlyOmitted);
            return new HeaderEmitResult(headers, guidance, localOnlyOmitted);
        }

        #endregion


        #region Schema walk

        /// <summary>
        /// Walks the same type-driven nest shape as the parser: scalars, primitive arrays, then child.
        /// Local Only fields are skipped so the paste matches the sheet contract.
        /// </summary>
        static void CollectEmitHeaders(Type elementType, List<string> headers, ref int localOnlyOmitted)
        {
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
                        // Local Only primitive arrays are not part of the Header Row paste
                        if (FieldHeaderNames.IsLocalOnly(field))
                        {
                            localOnlyOmitted++;
                            continue;
                        }

                        headers.Add(FieldHeaderNames.GetEmitName(field));
                        continue;
                    }

                    // One struct-list nest child (v1); collected after scalars on this level
                    if (structListField == null)
                    {
                        structListField = field;
                        structListElementType = collectionElement;
                    }

                    continue;
                }

                // Local Only scalars omit both Field Header names and Column Aliases
                if (FieldHeaderNames.IsLocalOnly(field))
                {
                    localOnlyOmitted++;
                    continue;
                }

                headers.Add(FieldHeaderNames.GetEmitName(field));
            }

            if (structListField != null)
                CollectEmitHeaders(structListElementType, headers, ref localOnlyOmitted);
        }

        #endregion


        #region Guidance

        /// <summary>
        /// Ignore Marker instructions always; Local Only omit note only when the clipboard is incomplete vs C# shape.
        /// </summary>
        static string BuildGuidance(int localOnlyOmitted)
        {
            if (localOnlyOmitted <= 0)
                return IgnoreMarkerGuidance;

            return IgnoreMarkerGuidance +
                   "\n\n" +
                   localOnlyOmitted +
                   " Local Only field(s) omitted from this Header Row " +
                   "(not part of the sheet contract; filled by the game after Pull).";
        }

        #endregion


    }
}
