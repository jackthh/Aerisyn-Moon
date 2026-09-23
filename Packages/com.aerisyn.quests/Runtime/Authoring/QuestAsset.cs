using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Aerisyn.Quests.Authoring
{
    /// <summary>
    /// ScriptableObject skin over <see cref="QuestDefinition"/> so designers can author Quests in the
    /// Inspector (Odin). Call <see cref="Build"/> for the immutable definition the Tracker consumes.
    /// CSV or code-driven games can skip this type entirely.
    /// </summary>
    [CreateAssetMenu(fileName = "Quest", menuName = "Aerisyn/Quests/Quest", order = 0)]
    [InfoBox("@InspectorErrorSummary", InfoMessageType.Error, nameof(HasInspectorErrors))]
    [InfoBox("@InspectorSuccessSummary", InfoMessageType.Info, nameof(IsInspectorValid))]
    public sealed class QuestAsset : ScriptableObject
    {
        #region Serialized fields

        [FoldoutGroup("Identity")]
        [Tooltip("Unique within the owning Board.")]
        [SerializeField] private int _localId;


        [FoldoutGroup("Objective")]
        [Tooltip("Game-defined kind (cast your own enum to int).")]
        [FormerlySerializedAs("_kind")]
        [SerializeField] private int _objectiveKind;

        [FoldoutGroup("Objective")]
        [Tooltip("Narrows the kind (stall id, currency id, ...). Ignored when Match Any Param is on.")]
        [FormerlySerializedAs("_param")]
        [HideIf(nameof(_matchAnyParam))]
        [SerializeField] private int _objectiveParam;

        [FoldoutGroup("Objective")]
        [Tooltip("React to every report of this kind, whatever its param.")]
        [SerializeField] private bool _matchAnyParam;


        [FoldoutGroup("Progress")]
        [Tooltip("Sum adds reports, High Water keeps the max, Flag sets 1 once.")]
        [SerializeField] private Accumulation _accumulation = Accumulation.Sum;

        [FoldoutGroup("Progress")]
        [Tooltip("One threshold per Step, strictly ascending and > 0. Flag quests use a single 1.")]
        [FormerlySerializedAs("_thresholds")]
        [ListDrawerSettings(ShowIndexLabels = true, DraggableItems = true)]
        [SerializeField] private long[] _stepThresholds = { 1 };


        [FoldoutGroup("Claiming")]
        [SerializeField] private ClaimPolicy _claimPolicy = ClaimPolicy.OncePerStep;

        [FoldoutGroup("Claiming")]
        [Tooltip("Full cycles allowed for Repeat With Reset. Ignored for Once Per Step.")]
        [ShowIf(nameof(_claimPolicy), ClaimPolicy.RepeatWithReset)]
        [MinValue(1)]
        [SerializeField] private int _repeatLimit = 1;

        #endregion


        #region Inspector status (Odin)

        // Reused when Odin evaluates InfoBox members so repaint does not allocate a new list each time.
        [NonSerialized] private readonly List<string> _inspectorErrors = new List<string>();


        private bool IsInspectorValid => Validate(null);


        private bool HasInspectorErrors => !IsInspectorValid;


        private string InspectorErrorSummary
        {
            get
            {
                _inspectorErrors.Clear();
                Validate(_inspectorErrors);
                return string.Join("\n", _inspectorErrors);
            }
        }


        private string InspectorSuccessSummary => "Valid. Objective " + Objective + ".";

        #endregion


        #region Public API

        /// <summary>Id of this Quest inside its Board.</summary>
        public int LocalId => _localId;


        /// <summary>The Objective built from the kind / param / match-any fields.</summary>
        public Objective Objective => _matchAnyParam ? Objective.AnyParam(_objectiveKind) : new Objective(_objectiveKind, _objectiveParam);


        /// <summary>Build the immutable definition. Throws on invalid authoring; use <see cref="TryBuild"/> for a soft check.</summary>
        public QuestDefinition Build() =>
            new QuestDefinition(_localId, Objective, _accumulation, _stepThresholds, _claimPolicy, _repeatLimit);


        /// <summary>Non-throwing variant for validators and inspectors. <paramref name="error"/> is null on success.</summary>
        public bool TryBuild(out QuestDefinition definition, out string error)
        {
            try
            {
                definition = Build();
                error = null;
                return true;
            }
            catch (ArgumentException exception)
            {
                definition = null;
                error = exception.Message;
                return false;
            }
        }


        /// <summary>Append human-readable problems with this asset to <paramref name="errors"/> (may be null). Returns true when none were found.</summary>
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
