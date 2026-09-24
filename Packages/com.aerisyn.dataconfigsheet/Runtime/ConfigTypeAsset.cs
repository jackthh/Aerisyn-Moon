using Sirenix.OdinInspector;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// Base for hand-written Config Types. Games subclass this (plus nested serializable types);
    /// Pull creates or overwrites the Baked Asset from matching Source Sheet tabs.
    /// Nest shape comes from the type; the package does not generate these classes.
    /// </summary>
    public abstract class ConfigTypeAsset : SerializedScriptableObject
    {
    }
}
