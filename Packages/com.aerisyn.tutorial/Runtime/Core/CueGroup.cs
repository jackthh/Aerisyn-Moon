using System;
using System.Collections.Generic;

namespace Aerisyn.Tutorial
{
    /// <summary>
    /// One scheduling unit inside Choreography: Sequential (await Cue Done) or Concurrent (fire-and-forget) Cue ids.
    /// </summary>
    public sealed class CueGroup
    {
        #region Fields

        private readonly string[] _cueIds;

        #endregion


        #region Properties

        /// <summary>Sequential await or Concurrent fire-and-forget.</summary>
        public CueGroupKind Kind { get; }

        /// <summary>Opaque Cue ids in authoring order.</summary>
        public IReadOnlyList<string> CueIds => _cueIds;

        #endregion


        #region Construction

        /// <summary>
        /// Builds a Cue group. Throws when <paramref name="cueIds"/> is null/empty or any id is blank.
        /// </summary>
        public CueGroup(CueGroupKind kind, params string[] cueIds)
        {
            if (cueIds == null || cueIds.Length == 0)
            {
                throw new ArgumentException(
                    "Cue group must include at least one Cue id.",
                    nameof(cueIds));
            }

            var copy = new string[cueIds.Length];
            for (var i = 0; i < cueIds.Length; i++)
            {
                if (string.IsNullOrEmpty(cueIds[i]))
                {
                    throw new ArgumentException(
                        "Cue id at index " + i + " must be a non-empty string.",
                        nameof(cueIds));
                }

                copy[i] = cueIds[i];
            }

            Kind = kind;
            _cueIds = copy;
        }

        #endregion


        public override string ToString() =>
            Kind + " x" + _cueIds.Length;
    }
}
