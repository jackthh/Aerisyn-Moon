namespace Aerisyn.Tutorial
{
    /// <summary>
    /// Soft-or-Hard mode of a single Step. Soft coaches without blocking; Hard gates play until success.
    /// Ticket 01 only authors Soft Steps; Hard is reserved for later Gate tickets.
    /// </summary>
    public enum Enforcement
    {
        Soft = 0,
        Hard = 1,
    }
}
