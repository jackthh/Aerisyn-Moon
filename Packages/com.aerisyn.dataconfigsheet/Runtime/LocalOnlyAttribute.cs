using System;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// Marks a Config Type Field Header as Local Only: not required on the Source Sheet
    /// Header Row and never bound from cells. Prefer this over NonSerialized / IgnorePull naming.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class LocalOnlyAttribute : Attribute
    {
    }
}
