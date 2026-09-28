using System.Collections.Generic;
using Aerisyn.Tutorial.Authoring;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Aerisyn.Tutorial.Samples.MySample
{
    /// <summary>
    /// Multi-step MySample facade over <see cref="TutorialRunner"/> with UGUI presentation.
    /// Flow: Soft open panel → Soft choose hero → Hard assign slot → Hard confirm.
    /// Cues recolor/scale UI; status + coach copy explain each beat; clicks drive Cue Done / Reports.
    /// </summary>
    public sealed class MySampleTutorialDriver : MonoBehaviour
    {
        #region Cue / Report / Step constants (must match Tutorial Sample.asset)

        public const string StepOpenPanel = "open_panel";
        public const string StepChooseHero = "choose_hero";
        public const string StepAssignSlot = "assign_slot";
        public const string StepConfirmRecruit = "confirm_recruit";

        public const string CueHighlightRecruit = "highlight.recruit_button";
        public const string CueTextRecruitCoach = "text.recruit_coach";
        public const string CueHighlightHeroCards = "highlight.hero_cards";
        public const string CueTextPickHero = "text.pick_hero";
        public const string CueGlowHeroSlot = "glow.hero_slot";
        public const string CueSfxChime = "sfx.chime";
        public const string CueHighlightSlot = "highlight.slot";
        public const string CueHighlightConfirm = "highlight.confirm";
        public const string CueTextFinalCoach = "text.final_coach";

        public const string TargetRecruit = "recruit_button";
        public const string TargetHeroA = "hero_a";
        public const string TargetHeroB = "hero_b";
        public const string TargetHeroSlot = "hero_slot";
        public const string TargetConfirm = "confirm";
        public const string TargetCoach = "coach";

        public const int ReportOpenRecruit = 21;
        public const int ReportChooseHero = 22;
        public const int ReportAssignSlot = 23;
        public const int ReportConfirmRecruit = 20;
        public const int AssignSlotParam = 1;
        public const int ConfirmRecruitParam = 1;

        #endregion


        #region Fields

        [Header("Authoring")]
        [SerializeField]
        private TutorialAsset _tutorialAsset;

        [Header("UI targets")]
        [SerializeField]
        private RectTransform _recruitButton;

        [SerializeField]
        private RectTransform _heroCardA;

        [SerializeField]
        private RectTransform _heroCardB;

        [SerializeField]
        private RectTransform _heroSlot;

        [SerializeField]
        private RectTransform _confirmButton;

        [SerializeField]
        private RectTransform _coachPanel;

        [SerializeField]
        private TMP_Text _statusText;

        [SerializeField]
        private TMP_Text _progressText;

        [SerializeField]
        private TMP_Text _coachText;

        [SerializeField]
        private TMP_Text _selectedHeroText;

        [Header("Presentation")]
        [SerializeField]
        private float _highlightScale = 1.12f;

        [SerializeField]
        private float _animSpeed = 10f;

        [SerializeField]
        private Color _highlightColor = new Color(0.95f, 0.75f, 0.2f, 1f);

        [SerializeField]
        private Color _glowColor = new Color(0.35f, 0.85f, 1f, 1f);

        [SerializeField]
        private Color _selectedColor = new Color(0.45f, 0.85f, 0.45f, 1f);

        private readonly TutorialRunner _runner = new TutorialRunner();
        private readonly Dictionary<int, UiRest> _restPoses = new Dictionary<int, UiRest>();

        private string _awaitingCueDoneId;
        private string _activeStepId = "";
        private bool _hardGateLocked;
        private int _selectedHeroParam;
        private string _selectedHeroLabel = "";

        // Soft open_panel sequencing
        private bool _openHighlightDone;
        private bool _openCoachDone;

        // Soft choose_hero sequencing
        private bool _chooseCardsCueDone;
        private bool _chooseCoachDone;

        // Hard confirm sequencing
        private bool _confirmHighlightDone;

        private RectTransform _animTarget;
        private Vector3 _animScaleTo = Vector3.one;
        private bool _animating;

        #endregion


        #region Nested types

        private struct UiRest
        {
            public Vector3 LocalScale;
            public Color ImageColor;
            public bool HasImage;
        }

        #endregion


        #region Lifecycle

        private void OnEnable()
        {
            CacheRestPoses();
            AttachRunnerHandlers();
            ResetFlowFlags();
            SetProgress(0, 4, "—");
            SetStatus("Ready");
            SetCoach("");
            SetSelectedHero("None");

            if (_runner.IsActive || _tutorialAsset == null)
                return;

            _runner.Start(_tutorialAsset.Build());
            SetProgress(1, 4, StepOpenPanel);
            SetStatus("Soft 1/4: open the recruit panel");
        }


        private void OnDisable()
        {
            if (_runner.IsActive)
                _runner.Stop();

            RestoreAllPresentation();
            ResetFlowFlags();
        }


        private void Update()
        {
            if (!_animating || _animTarget == null)
                return;

            float step = Time.deltaTime * _animSpeed;
            _animTarget.localScale = Vector3.Lerp(_animTarget.localScale, _animScaleTo, step);

            if ((_animTarget.localScale - _animScaleTo).sqrMagnitude < 0.0001f)
            {
                _animTarget.localScale = _animScaleTo;
                _animating = false;
            }
        }

        #endregion


        #region Public API

        /// <summary>
        /// Routes UI clicks by active Step. Hard Gate ignores off-target controls.
        /// </summary>
        public void NotifyClicked(string targetId)
        {
            if (!_runner.IsActive || string.IsNullOrEmpty(targetId))
                return;

            switch (_activeStepId)
            {
                case StepOpenPanel:
                    HandleOpenPanelClick(targetId);
                    break;
                case StepChooseHero:
                    HandleChooseHeroClick(targetId);
                    break;
                case StepAssignSlot:
                    HandleAssignSlotClick(targetId);
                    break;
                case StepConfirmRecruit:
                    HandleConfirmClick(targetId);
                    break;
            }
        }

        #endregion


        #region Step click handlers

        private void HandleOpenPanelClick(string targetId)
        {
            if (!_openHighlightDone && targetId == TargetRecruit && _awaitingCueDoneId == CueHighlightRecruit)
            {
                FinishAwaitingCue();
                _openHighlightDone = true;
                SetStatus("Soft 1/4: read coach, then OK");
                return;
            }

            if (_openHighlightDone && !_openCoachDone &&
                (targetId == TargetCoach || targetId == TargetRecruit) &&
                _awaitingCueDoneId == CueTextRecruitCoach)
            {
                FinishAwaitingCue();
                _openCoachDone = true;
                _runner.Report(ReportOpenRecruit, 0);
            }
        }


        private void HandleChooseHeroClick(string targetId)
        {
            if (!_chooseCardsCueDone &&
                (targetId == TargetHeroA || targetId == TargetHeroB) &&
                _awaitingCueDoneId == CueHighlightHeroCards)
            {
                FinishAwaitingCue();
                _chooseCardsCueDone = true;
                MarkHeroSelected(targetId);
                SetStatus("Soft 2/4: confirm pick via coach OK");
                return;
            }

            // Allow picking during coach Cue as well.
            if (_chooseCardsCueDone && (targetId == TargetHeroA || targetId == TargetHeroB))
            {
                MarkHeroSelected(targetId);
                return;
            }

            if (_chooseCardsCueDone && !_chooseCoachDone &&
                targetId == TargetCoach &&
                _awaitingCueDoneId == CueTextPickHero)
            {
                if (_selectedHeroParam == 0)
                {
                    SetStatus("Pick Hero A or B first");
                    return;
                }

                FinishAwaitingCue();
                _chooseCoachDone = true;
                _runner.Report(ReportChooseHero, _selectedHeroParam);
            }
        }


        private void HandleAssignSlotClick(string targetId)
        {
            if (!_hardGateLocked)
                return;

            if (targetId != TargetHeroSlot)
            {
                SetStatus("Hard Gate: click the Hero Slot only");
                return;
            }

            if (_awaitingCueDoneId == CueHighlightSlot)
                FinishAwaitingCue();

            _runner.Report(ReportAssignSlot, AssignSlotParam);
        }


        private void HandleConfirmClick(string targetId)
        {
            if (!_hardGateLocked)
                return;

            if (!_confirmHighlightDone && targetId == TargetConfirm && _awaitingCueDoneId == CueHighlightConfirm)
            {
                FinishAwaitingCue();
                _confirmHighlightDone = true;
                SetStatus("Hard 4/4: final coach — click OK");
                return;
            }

            if (_confirmHighlightDone &&
                (targetId == TargetCoach || targetId == TargetConfirm) &&
                _awaitingCueDoneId == CueTextFinalCoach)
            {
                FinishAwaitingCue();
                _runner.Report(ReportConfirmRecruit, ConfirmRecruitParam);
                return;
            }

            // Confirm click while still on highlight Cue also advances.
            if (targetId == TargetConfirm && _awaitingCueDoneId == CueHighlightConfirm)
            {
                FinishAwaitingCue();
                _confirmHighlightDone = true;
            }
        }

        #endregion


        #region Runner handlers

        private void AttachRunnerHandlers()
        {
            _runner.Cue -= OnCue;
            _runner.Gate -= OnGate;
            _runner.StepCompleted -= OnStepCompleted;
            _runner.TutorialCompleted -= OnTutorialCompleted;

            _runner.Cue += OnCue;
            _runner.Gate += OnGate;
            _runner.StepCompleted += OnStepCompleted;
            _runner.TutorialCompleted += OnTutorialCompleted;
        }


        private void OnCue(TutorialId tutorialId, int stepIndex, string stepId, string cueId)
        {
            _activeStepId = stepId;
            ApplyCuePresentation(cueId);

            if (cueId == CueGlowHeroSlot || cueId == CueSfxChime)
                return;

            _awaitingCueDoneId = cueId;
        }


        private void OnGate(TutorialId tutorialId, int stepIndex, string stepId, GatePhase phase)
        {
            _activeStepId = stepId;
            _hardGateLocked = phase == GatePhase.Started;

            if (_hardGateLocked && stepId == StepAssignSlot)
                SetStatus("Hard 3/4 Gate: drop hero into Slot");
            else if (_hardGateLocked && stepId == StepConfirmRecruit)
                SetStatus("Hard 4/4 Gate: confirm recruit");
        }


        private void OnStepCompleted(TutorialId tutorialId, int stepIndex, string stepId)
        {
            RestoreAllPresentation();
            _awaitingCueDoneId = null;
            SetCoach("");

            switch (stepId)
            {
                case StepOpenPanel:
                    _openHighlightDone = true;
                    _openCoachDone = true;
                    SetProgress(2, 4, StepChooseHero);
                    SetStatus("Soft 2/4: choose a hero");
                    break;
                case StepChooseHero:
                    _chooseCardsCueDone = true;
                    _chooseCoachDone = true;
                    SetProgress(3, 4, StepAssignSlot);
                    SetStatus("Hard 3/4: assign to slot");
                    break;
                case StepAssignSlot:
                    SetProgress(4, 4, StepConfirmRecruit);
                    SetStatus("Hard 4/4: confirm");
                    break;
            }
        }


        private void OnTutorialCompleted(TutorialId tutorialId)
        {
            _hardGateLocked = false;
            RestoreAllPresentation();
            ResetFlowFlags();
            SetCoach("");
            SetProgress(4, 4, "done");
            SetStatus("Complete: " + tutorialId.Value + " — " + _selectedHeroLabel + " recruited!");
        }

        #endregion


        #region Presentation

        private void ApplyCuePresentation(string cueId)
        {
            switch (cueId)
            {
                case CueHighlightRecruit:
                    HighlightControl(_recruitButton, _highlightColor, true);
                    SetStatus("Cue: highlight Recruit");
                    break;
                case CueTextRecruitCoach:
                    ShowCoach("Open Recruit to browse available heroes.");
                    HighlightControl(_coachPanel, _highlightColor, true);
                    break;
                case CueHighlightHeroCards:
                    HighlightControl(_heroCardA, _highlightColor, true);
                    HighlightControl(_heroCardB, _highlightColor, true);
                    SetStatus("Cue: pick Hero A or Hero B");
                    break;
                case CueTextPickHero:
                    ShowCoach("Select a hero card, then press OK to lock your choice.");
                    HighlightControl(_coachPanel, _highlightColor, true);
                    break;
                case CueGlowHeroSlot:
                    HighlightControl(_heroSlot, _glowColor, true);
                    break;
                case CueSfxChime:
                    HighlightControl(_coachPanel, _glowColor, false);
                    break;
                case CueHighlightSlot:
                    HighlightControl(_heroSlot, _highlightColor, true);
                    SetStatus("Cue: click Hero Slot to assign");
                    break;
                case CueHighlightConfirm:
                    HighlightControl(_confirmButton, _highlightColor, true);
                    SetStatus("Cue: highlight Confirm");
                    break;
                case CueTextFinalCoach:
                    ShowCoach("Confirm to recruit " + (_selectedHeroLabel.Length > 0 ? _selectedHeroLabel : "your hero") + ".");
                    HighlightControl(_coachPanel, _highlightColor, true);
                    break;
            }
        }


        private void MarkHeroSelected(string targetId)
        {
            _selectedHeroParam = targetId == TargetHeroA ? 1 : 2;
            _selectedHeroLabel = targetId == TargetHeroA ? "Hero A (Knight)" : "Hero B (Mage)";
            SetSelectedHero(_selectedHeroLabel);

            // Green = chosen; other card returns toward rest then mild dim via restore+highlight selected only.
            RestoreOne(_heroCardA);
            RestoreOne(_heroCardB);
            RectTransform chosen = targetId == TargetHeroA ? _heroCardA : _heroCardB;
            HighlightControl(chosen, _selectedColor, true);
        }


        private void HighlightControl(RectTransform target, Color color, bool scaleUp)
        {
            if (target == null)
                return;

            UiRest rest = GetRest(target);
            Image image = target.GetComponent<Image>();
            if (image != null)
                image.color = color;

            AnimateScale(target, scaleUp ? rest.LocalScale * _highlightScale : rest.LocalScale);
        }


        private void AnimateScale(RectTransform target, Vector3 localScale)
        {
            _animTarget = target;
            _animScaleTo = localScale;
            _animating = true;
        }


        private void FinishAwaitingCue()
        {
            if (string.IsNullOrEmpty(_awaitingCueDoneId))
                return;

            string cueId = _awaitingCueDoneId;
            _awaitingCueDoneId = null;
            _runner.CueDone(cueId);
        }


        private void SetStatus(string message)
        {
            if (_statusText != null)
                _statusText.text = message ?? "";
        }


        private void SetProgress(int step, int total, string stepId)
        {
            if (_progressText != null)
                _progressText.text = "Step " + step + "/" + total + "  ·  " + stepId;
        }


        private void ShowCoach(string message)
        {
            SetCoach(message);
        }


        private void SetCoach(string message)
        {
            if (_coachText != null)
                _coachText.text = message ?? "";

            if (_coachPanel != null)
                _coachPanel.gameObject.SetActive(!string.IsNullOrEmpty(message));
        }


        private void SetSelectedHero(string label)
        {
            if (_selectedHeroText != null)
                _selectedHeroText.text = "Selected: " + label;
        }

        #endregion


        #region Pose cache

        private void CacheRestPoses()
        {
            _restPoses.Clear();
            Remember(_recruitButton);
            Remember(_heroCardA);
            Remember(_heroCardB);
            Remember(_heroSlot);
            Remember(_confirmButton);
            Remember(_coachPanel);
        }


        private void Remember(RectTransform target)
        {
            if (target == null)
                return;

            int key = target.GetInstanceID();
            if (_restPoses.ContainsKey(key))
                return;

            Image image = target.GetComponent<Image>();
            _restPoses[key] = new UiRest
            {
                LocalScale = target.localScale,
                HasImage = image != null,
                ImageColor = image != null ? image.color : Color.white,
            };
        }


        private UiRest GetRest(RectTransform target)
        {
            int key = target.GetInstanceID();
            if (_restPoses.TryGetValue(key, out UiRest rest))
                return rest;

            Remember(target);
            return _restPoses[key];
        }


        private void RestoreAllPresentation()
        {
            _animating = false;
            _animTarget = null;
            RestoreOne(_recruitButton);
            RestoreOne(_heroCardA);
            RestoreOne(_heroCardB);
            RestoreOne(_heroSlot);
            RestoreOne(_confirmButton);
            RestoreOne(_coachPanel);

            // Keep chosen hero tint after Soft choose completes.
            if (_selectedHeroParam == 1)
                HighlightControl(_heroCardA, _selectedColor, false);
            else if (_selectedHeroParam == 2)
                HighlightControl(_heroCardB, _selectedColor, false);
        }


        private void RestoreOne(RectTransform target)
        {
            if (target == null)
                return;

            UiRest rest = GetRest(target);
            target.localScale = rest.LocalScale;

            Image image = target.GetComponent<Image>();
            if (image != null && rest.HasImage)
                image.color = rest.ImageColor;
        }


        private void ResetFlowFlags()
        {
            _awaitingCueDoneId = null;
            _activeStepId = "";
            _hardGateLocked = false;
            _selectedHeroParam = 0;
            _selectedHeroLabel = "";
            _openHighlightDone = false;
            _openCoachDone = false;
            _chooseCardsCueDone = false;
            _chooseCoachDone = false;
            _confirmHighlightDone = false;
        }

        #endregion
    }
}
