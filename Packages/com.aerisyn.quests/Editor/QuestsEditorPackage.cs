// Editor assembly marker so Aerisyn.Quests.Editor compiles before tooling exists.

namespace Aerisyn.Quests.Editor
{
    /// <summary>
    /// Editor assembly marker for <c>com.aerisyn.quests</c>.
    /// Add menus, drawers, and importers here as the package grows.
    /// </summary>
    public static class QuestsEditorPackage
    {
        #region Public API

        /// <summary>Mirrors runtime package id for editor tooling checks.</summary>
        public const string PackageId = QuestsPackage.PackageId;

        #endregion
    }
}
