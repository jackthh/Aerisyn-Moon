#nullable disable
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// Builds a pasteable Header Row from a Config Type shape (Field Headers / Column Aliases)
    /// and guidance for !!! Ignore Marker note columns.
    /// </summary>
    public static class HeaderEmitter
    {


        const string IgnoreMarkerGuidance =
            "Place !!! in the row immediately above any notes-only column header. " +
            "Pull skips those columns and never writes their cells into Baked Assets.";


        #region Public API

        /// <summary>
        /// Emits Header Row labels in Vertical Nest column order for the Config Type's root items shape.
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
            CollectEmitHeaders(rootElementType, headers);
            return new HeaderEmitResult(headers, IgnoreMarkerGuidance);
        }

        #endregion


        #region Schema walk

        /// <summary>
        /// Walks the same type-driven nest shape as the parser: scalars, primitive arrays, then child.
        /// </summary>
        static void CollectEmitHeaders(Type elementType, List<string> headers)
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

                headers.Add(FieldHeaderNames.GetEmitName(field));
            }

            if (structListField != null)
                CollectEmitHeaders(structListElementType, headers);
        }

        #endregion


    }
}
