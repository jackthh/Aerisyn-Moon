#nullable disable
using System;
using System.Collections;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// Copies the root items list from a parsed scratch instance onto an existing Baked Asset
    /// so re-Pull overwrites data without replacing the asset identity/GUID.
    /// </summary>
    public static class BakedAssetItemsCopy
    {


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

            System.Reflection.FieldInfo field;
            Type elementType;
            string error;
            if (!ConfigTypeItemsField.TryResolve(source.GetType(), out field, out elementType, out error))
                throw new InvalidOperationException(error);

            IList sourceList = field.GetValue(source) as IList;
            if (sourceList == null)
                throw new InvalidOperationException($"Source '{source.GetType().Name}.{field.Name}' is not a list.");

            IList destinationList = ConfigTypeItemsField.EnsureList(destination, field, elementType);
            destinationList.Clear();
            for (int i = 0; i < sourceList.Count; i++)
                destinationList.Add(sourceList[i]);
        }


    }
}
