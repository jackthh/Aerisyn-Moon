namespace Aerisyn.Quests.Samples.BasicBoard
{
    /// <summary>
    /// The game owns its verb list. The package only sees these as ints, so each title defines
    /// its own enum like this one and casts when reporting.
    /// </summary>
    public enum SampleObjectiveKind
    {
        ServeCustomer = 1,
        EarnCash = 2,
        UpgradeStall = 3,
        UnlockStall = 4
    }
}