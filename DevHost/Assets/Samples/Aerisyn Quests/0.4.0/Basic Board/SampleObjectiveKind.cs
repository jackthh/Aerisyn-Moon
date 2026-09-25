namespace Aerisyn.Quests.Samples.BasicBoard
{
    /// <summary>
    /// The game owns its verb list. The package only sees these as ints, so each title defines
    /// its own enum like this one and casts when reporting. Quest assets and CSVs store the raw
    /// int, so never renumber existing entries.
    /// </summary>
    public enum SampleObjectiveKind
    {
        /// <summary>A customer was served. Param: stall id.</summary>
        ServeCustomer = 1,

        /// <summary>Cash was earned. Param unused (0); value: amount earned.</summary>
        EarnCash = 2,

        /// <summary>A stall was upgraded. Param: stall id; value: the stall's new level.</summary>
        UpgradeStall = 3,

        /// <summary>A stall was unlocked. Param: stall id.</summary>
        UnlockStall = 4
    }
}
