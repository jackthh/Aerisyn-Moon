using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Aerisyn.Tutorial.Authoring
{
    /// <summary>
    /// ScriptableObject skin over <see cref="TutorialDefinition"/> so authors can edit Steps in the
    /// Inspector (Odin). Call <see cref="Build"/> for the immutable definition the Runner consumes.
    /// One-way SO → definition; code-first <see cref="TutorialBuilder"/> remains first-class.
    /// </summary>
    [CreateAssetMenu(fileName = "Tutorial", menuName = "Aerisyn/Tutorial/Tutorial", order = 0)]
    [InfoBox("@InspectorErrorSummary", InfoMessageType.Error, nameof(HasInspectorErrors))]
    [InfoBox("@InspectorSuccessSummary", InfoMessageType.Info, nameof(IsInspectorValid))]
    public sealed class TutorialAsset : ScriptableObject
    {
        #region Serialized fields

        [FoldoutGroup("Tutorial")]
        [LabelText("Tutorial Id")]
        [Tooltip("Stable Tutorial identity used in Completions and Progress Snapshots.")]
        [Required]
        [SerializeField]
        private string _tutorialId = "";

        [FoldoutGroup("Tutorial")]
        [Tooltip("Ordered Steps. Soft/Hard, Report match, and Cue Choreography per beat — list data, not subclasses.")]
        [ListDrawerSettings(ShowIndexLabels = true, DraggableItems = true, ListElementLabelName = "Id")]
        [SerializeField]
        private AuthoredStep[] _steps = Array.Empty<AuthoredStep>();

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


        // _tutorialId is safe here: success InfoBox only shows when Validate passed.
        private string InspectorSuccessSummary =>
            "Valid Tutorial '" + _tutorialId + "' with " + _steps.Length + " Step(s).";

        #endregion


        #region Public API

        /// <summary>
        /// The Tutorial identity from the authored name. Throws when empty,
        /// so call <see cref="Validate"/> first on assets that may be misconfigured.
        /// </summary>
        public TutorialId TutorialId => new TutorialId(_tutorialId);


        /// <summary>Authored Steps in Inspector order.</summary>
        public IReadOnlyList<AuthoredStep> Steps => _steps;


        /// <summary>
        /// Projects this asset into an immutable Core definition.
        /// Throws on invalid authoring; use <see cref="TryBuild"/> for a soft check.
        /// </summary>
        public TutorialDefinition Build() =>
            TutorialAuthoringProjection.Project(_tutorialId, _steps);


        /// <summary>Non-throwing variant for validators and inspectors. <paramref name="error"/> is null on success.</summary>
        public bool TryBuild(out TutorialDefinition definition, out string error)
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


        /// <summary>
        /// Append human-readable problems with this asset to <paramref name="errors"/> (may be null).
        /// Returns true when none were found.
        /// </summary>
        public bool Validate(List<string> errors)
        {
            if (TryBuild(out _, out var error))
                return true;

            errors?.Add(name + ": " + error);
            return false;
        }

        #endregion


        #region Sample / test helpers

        /// <summary>
        /// Fills this asset from plain authored data (CreateInstance + Populate for Samples~ without a .asset file).
        /// Does not mark the asset dirty; Editor callers should SetDirty when persisting.
        /// </summary>
        public void Populate(string tutorialId, AuthoredStep[] steps)
        {
            _tutorialId = tutorialId ?? "";
            _steps = steps ?? Array.Empty<AuthoredStep>();
        }

        #endregion
    }
}