namespace Aerisyn.Tutorial
{
    /// <summary>
    /// How a Cue group is scheduled: await Cue Done between each, or fire-and-forget all at once.
    /// </summary>
    public enum CueGroupKind
    {
        Sequential = 0,
        Concurrent = 1,
    }
}
