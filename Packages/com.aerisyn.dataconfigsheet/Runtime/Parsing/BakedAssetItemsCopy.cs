#nullable disable
using System;
using System.Collections;
using System.Reflection;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// Copies the root items list from a parsed scratch instance onto an existing Baked Asset
    /// so re-Pull overwrites data without replacing the asset identity/GUID.
    /// </summary>
    public static class BakedAssetItemsCopy
    {


        const string RootItemsFieldName = "items";


        /// <summary>
        /// Replaces destination's root items list contents with a shallow copy of source's items.
        /// </summary>
        public static void CopyRootItems(object source, object destination)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));
            if (source.GetType() != destination.GetType())
            {
                throw new ArgumentException(
                    $"Source type '{source.GetType().Name}' must match destination '{destination.GetType().Name}'.");
            }

            FieldInfo field = ResolveRootItemsField(source.GetType());
            IList sourceList = field.GetValue(source) as IList;
            if (sourceList == null)
                throw new InvalidOperationException($"Source '{source.GetType().Name}.{field.Name}' is not a list.");

            Type elementType = GetListElementType(field.FieldType);
            IList destinationList = field.GetValue(destination) as IList;
            if (destinationList == null || destinationList.IsFixedSize)
            {
                Type listType = typeof(System.Collections.Generic.List<>).MakeGenericType(elementType);
                destinationList = (IList)Activator.CreateInstance(listType);
                field.SetValue(destination, destinationList);
            }

            destinationList.Clear();
            for (int i = 0; i < sourceList.Count; i++)
                destinationList.Add(sourceList[i]);
        }


        static FieldInfo ResolveRootItemsField(Type configType)
        {
            FieldInfo named = configType.GetField(
                RootItemsFieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (named != null)
                return named;

            FieldInfo[] fields = configType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            FieldInfo found = null;
            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];
                if (!field.IsPublic && !HasSerializeField(field))
                    continue;

                if (!IsComplexListField(field))
                    continue;

                if (found != null)
                {
                    throw new InvalidOperationException(
                        $"Type '{configType.Name}' has multiple list fields; name the root list '{RootItemsFieldName}'.");
                }

                found = field;
            }

            if (found == null)
            {
                throw new InvalidOperationException(
                    $"Type '{configType.Name}' has no root items list to copy.");
            }

            return found;
        }


        static bool IsComplexListField(FieldInfo field)
        {
            Type type = field.FieldType;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>))
            {
                Type element = type.GetGenericArguments()[0];
                return element != typeof(string) && !element.IsPrimitive && !element.IsEnum;
            }

            if (type.IsArray)
            {
                Type element = type.GetElementType();
                return element != null && element != typeof(string) && !element.IsPrimitive && !element.IsEnum;
            }

            return false;
        }


        static Type GetListElementType(Type listType)
        {
            if (listType.IsArray)
                return listType.GetElementType();

            return listType.GetGenericArguments()[0];
        }


        static bool HasSerializeField(FieldInfo field)
        {
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
