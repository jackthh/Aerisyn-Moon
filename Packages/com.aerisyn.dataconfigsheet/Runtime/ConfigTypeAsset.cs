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


        /// <summary>
        /// After Pull: runs after every included tab parses successfully and before any
        /// Baked Asset create/save. Override to fill Local Only derivatives. Default is a no-op.
        /// A throw fails the whole Pull with no writes.
        /// </summary>
        public virtual void OnAfterPull()
        {
        }


    }
}
