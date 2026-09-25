using System;

namespace Aerisyn.Tutorial
{
    /// <summary>
    /// Serializable place in a Tutorial: which Tutorial and which Step index.
    /// The package never touches disk; the game owns save I/O and resume policy.
    /// </summary>
    /// <remarks>
    /// Field names are part of the save format (JsonUtility and similar key on them).
    /// </remarks>
    [Serializable]
    public struct ProgressSnapshot
    {
        #region Fields

        /// <summary>Tutorial identity string (<see cref="TutorialId.Value"/>).</summary>
        public string TutorialId;

        /// <summary>Zero-based index of the active (or resume) Step within the Tutorial.</summary>
        public int StepIndex;

        #endregion


        #region Construction

        public ProgressSnapshot(TutorialId tutorialId, int stepIndex)
        {
            TutorialId = tutorialId.Value;
            StepIndex = stepIndex;
        }

        #endregion
    }
}
