using System;

namespace Aerisyn.Tutorial.Authoring
{
    /// <summary>
    /// Serializable Cue group for Inspector / SO authoring.
    /// Projects 1:1 into a Core <see cref="CueGroup"/>.
    /// </summary>
    [Serializable]
    public sealed class AuthoredCueGroup
    {
        #region Fields

        /// <summary>Sequential (await Cue Done) or Concurrent (fire-and-forget).</summary>
        public CueGroupKind Kind = CueGroupKind.Sequential;

        /// <summary>Opaque Cue ids in authoring order. Null/empty or blank ids fail Core validation on project.</summary>
        public string[] CueIds = Array.Empty<string>();

        #endregion


        #region Projection

        /// <summary>
        /// Builds a Core Cue group from Cue ids in order.
        /// Throws when ids are null/empty or any id is blank (same rules as Core <see cref="CueGroup"/>).
        /// </summary>
        public CueGroup ToCueGroup()
        {
            // Delegate validation to Core so Authoring never softens blank ids into a different Choreography.
            return new CueGroup(Kind, CueIds);
        }


        /// <summary>
        /// One authored Cue group → Sequential or Concurrent Choreography via Core public factories
        /// (Authoring never depends on Core private constructors).
        /// </summary>
        public ChoreographyDefinition ToChoreography()
        {
            CueGroup group = ToCueGroup();
            var ids = new string[group.CueIds.Count];
            for (var i = 0; i < group.CueIds.Count; i++)
                ids[i] = group.CueIds[i];

            return group.Kind == CueGroupKind.Sequential
                ? ChoreographyDefinition.Sequential(ids)
                : ChoreographyDefinition.Concurrent(ids);
        }

        #endregion
    }
}
