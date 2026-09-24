#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// Shared reflection for Config Type root <c>items</c> lists (parse + in-place overwrite).
    /// </summary>
    public static class ConfigTypeItemsField
    {


        public const string PreferredName = "items";


        /// <summary>
        /// Resolves the root items list field and its element type on a Config Type shape.
        /// </summary>
        public static bool TryResolve(Type configType, out FieldInfo listField, out Type elementType, out string error)
        {
            listField = null;
            elementType = null;
            error = null;

            List<FieldInfo> candidates = new List<FieldInfo>();
            FieldInfo[] fields = configType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];
                if (!IsSerializableField(field))
                    continue;

                Type candidateElement;
                if (!TryGetCollectionElementType(field.FieldType, out candidateElement))
                    continue;

                if (IsPrimitiveOrString(candidateElement))
                    continue;

                candidates.Add(field);
            }

            if (candidates.Count == 0)
            {
                error = $"Config type '{configType.Name}' needs a root items list of nested serializable elements.";
                return false;
            }

            FieldInfo preferred = null;
            for (int i = 0; i < candidates.Count; i++)
            {
                if (string.Equals(candidates[i].Name, PreferredName, StringComparison.Ordinal))
                {
                    preferred = candidates[i];
                    break;
                }
            }

            if (preferred == null)
            {
                if (candidates.Count > 1)
                {
                    error =
                        $"Config type '{configType.Name}' has multiple list fields; name the root list '{PreferredName}'.";
                    return false;
                }

                preferred = candidates[0];
            }

            listField = preferred;
            TryGetCollectionElementType(preferred.FieldType, out elementType);
            return true;
        }


        public static IList EnsureList(object owner, FieldInfo listField, Type elementType)
        {
            object existing = listField.GetValue(owner);
            IList list = existing as IList;
            if (list != null && !list.IsFixedSize)
                return list;

            Type listType = typeof(List<>).MakeGenericType(elementType);
            list = (IList)Activator.CreateInstance(listType);
            listField.SetValue(owner, list);
            return list;
        }


        public static bool TryGetCollectionElementType(Type type, out Type elementType)
        {
            elementType = null;
            if (type == null)
                return false;

            if (type.IsArray)
            {
                elementType = type.GetElementType();
                return elementType != null;
            }

            if (type.IsGenericType)
            {
                Type def = type.GetGenericTypeDefinition();
                if (def == typeof(List<>) || def == typeof(IList<>) || def == typeof(ICollection<>) || def == typeof(IEnumerable<>))
                {
                    elementType = type.GetGenericArguments()[0];
                    return true;
                }
            }

            return false;
        }


        public static bool IsPrimitiveOrString(Type type)
        {
            Type t = Nullable.GetUnderlyingType(type) ?? type;
            return t.IsPrimitive || t.IsEnum || t == typeof(string) || t == typeof(decimal);
        }


        public static bool IsSerializableField(FieldInfo field)
        {
            if (field.IsStatic)
                return false;

            if (field.IsPublic)
                return true;

            object[] attrs = field.GetCustomAttributes(true);
            for (int i = 0; i < attrs.Length; i++)
            {
                string name = attrs[i].GetType().Name;
                if (name == "SerializeField" || name == "SerializeFieldAttribute")
                    return true;
            }

            return false;
        }


    }
}
