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

        /// <summary>Opaque Cue ids in authoring order. Empty / null means skip this group.</summary>
        public string[] CueIds = Array.Empty<string>();

        #endregion


        #region Projection

        /// <summary>
        /// True when there is at least one non-empty Cue id to schedule.
        /// Blank ids are ignored so a half-filled Inspector row does not throw mid-list.
        /// </summary>
        public bool HasCueIds
        {
            get
            {
                if (CueIds == null || CueIds.Length == 0)
                    return false;

                for (var i = 0; i < CueIds.Length; i++)
                {
                    if (!string.IsNullOrEmpty(CueIds[i]))
                        return true;
                }

                return false;
            }
        }


        /// <summary>
        /// Builds a Core Cue group from non-empty Cue ids.
        /// Throws when <see cref="HasCueIds"/> is false (caller should skip empty groups).
        /// </summary>
        public CueGroup ToCueGroup()
        {
            if (!HasCueIds)
                throw new ArgumentException("Authored Cue group has no Cue ids.");

            // Count first so we allocate once (no LINQ).
            var count = 0;
            for (var i = 0; i < CueIds.Length; i++)
            {
                if (!string.IsNullOrEmpty(CueIds[i]))
                    count++;
            }

            var copy = new string[count];
            var write = 0;
            for (var i = 0; i < CueIds.Length; i++)
            {
                if (string.IsNullOrEmpty(CueIds[i]))
                    continue;
                copy[write] = CueIds[i];
                write++;
            }

            return new CueGroup(Kind, copy);
        }

        #endregion
    }
}
