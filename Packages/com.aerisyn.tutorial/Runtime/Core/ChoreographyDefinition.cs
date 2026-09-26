using System;
using System.Collections.Generic;

namespace Aerisyn.Tutorial
{
    /// <summary>
    /// Opaque Cue ids for a Step, scheduled sequentially (await Cue Done between each).
    /// Concurrent / mixed groups arrive in a later ticket without changing this public shape.
    /// </summary>
    public sealed class ChoreographyDefinition
    {
        #region Fields

        private static readonly ChoreographyDefinition EmptyInstance =
            new ChoreographyDefinition(Array.Empty<string>());

        private readonly string[] _sequentialCueIds;

        #endregion


        #region Properties

        /// <summary>Ordered opaque Cue ids. Empty when the Step has no presentation Choreography.</summary>
        public IReadOnlyList<string> SequentialCueIds => _sequentialCueIds;

        /// <summary>True when no Cues are scheduled.</summary>
        public bool IsEmpty => _sequentialCueIds.Length == 0;

        /// <summary>Shared empty Choreography (no Cues).</summary>
        public static ChoreographyDefinition Empty => EmptyInstance;

        #endregion


        #region Construction

        private ChoreographyDefinition(string[] sequentialCueIds)
        {
            _sequentialCueIds = sequentialCueIds;
        }


        /// <summary>
        /// Builds sequential Choreography: emit each Cue id, wait for Cue Done, then the next.
        /// Throws when any id is null or empty.
        /// </summary>
        public static ChoreographyDefinition Sequential(params string[] cueIds)
        {
            if (cueIds == null || cueIds.Length == 0)
                return EmptyInstance;

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

            return new ChoreographyDefinition(copy);
        }

        #endregion


        public override string ToString() =>
            IsEmpty ? "Choreography (empty)" : "Choreography seq x" + _sequentialCueIds.Length;
    }
}
