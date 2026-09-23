namespace Aerisyn.Quests
{
    /// <summary>How a report changes a Quest's progress value. Declared on the Quest, never chosen by the caller.</summary>
    public enum Accumulation : byte
    {
        /// <summary>progress += reported value (never below 0). "Serve 10 customers."</summary>
        Sum = 0,

        /// <summary>progress = max(progress, reported value). Report the current level, not the delta. "Reach stall level 25."</summary>
        HighWater = 1,

        /// <summary>progress = 1 the first time any matching report arrives; the reported value is ignored. "Unlock stall 2."</summary>
        Flag = 2
    }

    /// <summary>How claiming works once a Step's threshold is reached.</summary>
    public enum ClaimPolicy : byte
    {
        /// <summary>Each Step is claimable once; progress keeps growing toward later Steps.</summary>
        OncePerStep = 0,

        /// <summary>
        /// After every Step has been claimed, progress resets to 0 and the Quest can be earned again,
        /// up to <see cref="QuestDefinition.RepeatLimit"/> full cycles.
        /// </summary>
        RepeatWithReset = 1
    }

    /// <summary>Summary state of a whole Quest, derived from progress and claimed Steps.</summary>
    public enum QuestState : byte
    {
        /// <summary>No Step is waiting to be claimed.</summary>
        InProgress = 0,

        /// <summary>At least one reached Step has not been claimed yet.</summary>
        Claimable = 1,

        /// <summary>Nothing left to earn: every Step claimed (OncePerStep) or repeat limit exhausted (RepeatWithReset).</summary>
        Completed = 2
    }

    /// <summary>State of a single Step on a Quest.</summary>
    public enum StepState : byte
    {
        /// <summary>Threshold not reached yet (or the Quest is already Completed).</summary>
        Locked = 0,

        /// <summary>Threshold reached and reward not claimed.</summary>
        Claimable = 1,

        /// <summary>Reward already claimed for the current cycle.</summary>
        Claimed = 2
    }
}
