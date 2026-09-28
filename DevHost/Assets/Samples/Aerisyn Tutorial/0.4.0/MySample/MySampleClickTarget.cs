using UnityEngine;
using UnityEngine.UI;

namespace Aerisyn.Tutorial.Samples.MySample
{
    /// <summary>
    /// Click bridge for MySample: UI Button onClick and/or 3D OnMouseDown both notify the driver.
    /// </summary>
    public sealed class MySampleClickTarget : MonoBehaviour
    {
        #region Fields

        [Tooltip("Opaque id the driver matches (e.g. recruit_button, hero_slot, confirm, coach).")]
        [SerializeField]
        private string _targetId = "";

        [SerializeField]
        private MySampleTutorialDriver _driver;

        private Button _button;

        #endregion


        #region Public API

        /// <summary>Target id used by the driver when deciding Cue Done / Report.</summary>
        public string TargetId => _targetId;


        /// <summary>Wires the driver reference (used by scene setup / Inspector).</summary>
        public void Bind(MySampleTutorialDriver driver, string targetId)
        {
            _driver = driver;
            _targetId = targetId ?? "";
        }

        #endregion


        #region Lifecycle

        private void Awake()
        {
            _button = GetComponent<Button>();
            if (_button != null)
                _button.onClick.AddListener(Notify);
        }


        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(Notify);
        }

        #endregion


        #region Input

        // 3D world targets (Collider + Physics raycast) still work alongside UI.
        private void OnMouseDown() => Notify();


        private void Notify()
        {
            if (_driver == null)
                return;

            _driver.NotifyClicked(_targetId);
        }

        #endregion
    }
}
