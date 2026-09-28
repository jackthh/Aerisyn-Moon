using System;
using System.Collections.Generic;

namespace Aerisyn.Tutorial
{
    /// <summary>
    /// Opaque Cue ids for a Step, scheduled as Sequential (await Cue Done), Concurrent
    /// (fire-and-forget, no Cue Done), or a Mix of groups in authoring order.
    /// </summary>
    public sealed class ChoreographyDefinition
    {
        #region Fields

        private static readonly ChoreographyDefinition EmptyInstance =
            new ChoreographyDefinition(Array.Empty<CueGroup>());

        private readonly CueGroup[] _groups;

        #endregion


        #region Properties

        /// <summary>Ordered Cue groups. Empty when the Step has no presentation Choreography.</summary>
        public IReadOnlyList<CueGroup> Groups => _groups;

        /// <summary>True when no Cue groups are scheduled.</summary>
        public bool IsEmpty => _groups.Length == 0;

        /// <summary>Shared empty Choreography (no Cues).</summary>
        public static ChoreographyDefinition Empty => EmptyInstance;

        #endregion


        #region Construction

        private ChoreographyDefinition(CueGroup[] groups)
        {
            _groups = groups;
        }


        /// <summary>
        /// Builds sequential Choreography: emit each Cue id, wait for Cue Done, then the next.
        /// Throws when any id is null or empty.
        /// </summary>
        public static ChoreographyDefinition Sequential(params string[] cueIds) =>
            SingleGroup(CueGroupKind.Sequential, cueIds);


        /// <summary>
        /// Builds concurrent Choreography: emit every Cue id immediately (no Cue Done required).
        /// Throws when any id is null or empty.
        /// </summary>
        public static ChoreographyDefinition Concurrent(params string[] cueIds) =>
            SingleGroup(CueGroupKind.Concurrent, cueIds);


        /// <summary>
        /// Concatenates Cue groups from each part in order (Sequential await + Concurrent fire-and-forget).
        /// Null or Empty parts are skipped.
        /// </summary>
        public static ChoreographyDefinition Mix(params ChoreographyDefinition[] parts)
        {
            if (parts == null || parts.Length == 0)
                return EmptyInstance;

            // Count groups first so we allocate once (no LINQ).
            var total = 0;
            for (var i = 0; i < parts.Length; i++)
            {
                if (parts[i] == null || parts[i].IsEmpty)
                    continue;
                total += parts[i]._groups.Length;
            }

            if (total == 0)
                return EmptyInstance;

            var merged = new CueGroup[total];
            var write = 0;
            for (var i = 0; i < parts.Length; i++)
            {
                if (parts[i] == null || parts[i].IsEmpty)
                    continue;

                var source = parts[i]._groups;
                for (var g = 0; g < source.Length; g++)
                {
                    merged[write] = source[g];
                    write++;
                }
            }

            return new ChoreographyDefinition(merged);
        }


        /// <summary>One Cue group of <paramref name="kind"/>, or Empty when <paramref name="cueIds"/> is null/empty.</summary>
        private static ChoreographyDefinition SingleGroup(CueGroupKind kind, string[] cueIds)
        {
            if (cueIds == null || cueIds.Length == 0)
                return EmptyInstance;

            return new ChoreographyDefinition(new[] { new CueGroup(kind, cueIds) });
        }

        #endregion


        public override string ToString() =>
            IsEmpty ? "Choreography (empty)" : "Choreography groups x" + _groups.Length;
    }
}
