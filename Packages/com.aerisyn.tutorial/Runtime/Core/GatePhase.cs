namespace Aerisyn.Tutorial
{
    /// <summary>
    /// Gate signal phase for a Hard Step: Started when entering, Ended when leaving.
    /// Soft Steps never emit Gate signals.
    /// </summary>
    public enum GatePhase
    {
        Started = 0,
        Ended = 1,
    }
}
