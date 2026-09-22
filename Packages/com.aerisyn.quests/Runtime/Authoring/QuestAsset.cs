using System.Collections.Generic;
using UnityEngine;

namespace Aerisyn.Quests.Authoring
{
    /// <summary>
    /// ScriptableObject skin over <see cref="QuestDefinition"/> so designers can author Quests in the
    /// Inspector. The asset is only an authoring surface: call <see cref="Build"/> to get the immutable
    /// definition the Tracker consumes. CSV or code-driven games can skip this type entirely.
    /// </summary>
    [CreateAssetMenu(fileName = "Quest", menuName = "Aerisyn/Quests/Quest", order = 0)]
    public sealed class QuestAsset : ScriptableObject
    {
        #region Serialized fields

        [Header("Identity")]
        [Tooltip("Unique within the owning Board.")]
        [SerializeField] private int _localId;

        [Header("Objective")]
        [Tooltip("Game-defined kind (cast your own enum to int).")]
        [SerializeField] private int _kind;

        [Tooltip("Narrows the kind (stall id, currency id, ...). Ignored when Match Any Param is on.")]
        [SerializeField] private int _param;

        [SerializeField] private bool _matchAnyParam;

        [Header("Progress")]
        [SerializeField] private Accumulation _accumulation = Accumulation.Sum;

        [Tooltip("One threshold per Step, strictly ascending and > 0. Flag quests use a single 1.")]
        [SerializeField] private long[] _thresholds = { 1 };

        [Header("Claiming")]
        [SerializeField] private ClaimPolicy _claimPolicy = ClaimPolicy.OncePerStep;

        [Tooltip("Full cycles allowed for Repeat With Reset. Ignored for Once Per Step.")]
        [Min(1)]
        [SerializeField] private int _repeatLimit = 1;

        #endregion

        #region Public API

        public int LocalId => _localId;

        public Objective Objective => _matchAnyParam ? Objective.Any(_kind) : new Objective(_kind, _param);

        /// <summary>Build the immutable definition. Throws on invalid authoring; use <see cref="TryBuild"/> for a soft check.</summary>
        public QuestDefinition Build() =>
            new QuestDefinition(_localId, Objective, _accumulation, _thresholds, _claimPolicy, _repeatLimit);

        /// <summary>Non-throwing variant for validators and inspectors.</summary>
        public bool TryBuild(out QuestDefinition definition, out string error)
        {
            try
            {
                definition = Build();
                error = null;
                return true;
            }
            catch (System.ArgumentException ex)
            {
                definition = null;
                error = ex.Message;
                return false;
            }
        }

        /// <summary>Append human-readable problems with this asset. Returns true when none were found.</summary>
        public bool Validate(List<string> errors)
        {
            if (TryBuild(out _, out var error))
                return true;

            errors?.Add(name + ": " + error);
            return false;
        }

        #endregion
    }
}
