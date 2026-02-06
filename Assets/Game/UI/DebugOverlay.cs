using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

namespace Game.UI
{
    /// <summary>
    /// Simple debug overlay displaying FPS, mode (Desktop/VR), and app state.
    /// Designed to be easily extended for logging and diagnostics.
    /// </summary>
    public class DebugOverlay : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Text _debugText;

        [Header("FPS Settings")]
        [SerializeField] private float _fpsUpdateInterval = 0.5f;

        private float _fps;
        private float _fpsTimer;
        private int _frameCount;

        private string _mode;
        private string _appState = "InGame";

        private void Start()
        {
            DetectMode();

            if (_debugText == null)
            {
                Debug.LogError("[DebugOverlay] Text reference is missing!");
            }
        }

        private void Update()
        {
            UpdateFPS();
            UpdateDisplay();
        }

        private void UpdateFPS()
        {
            _frameCount++;
            _fpsTimer += Time.unscaledDeltaTime;

            if (_fpsTimer >= _fpsUpdateInterval)
            {
                _fps = _frameCount / _fpsTimer;
                _frameCount = 0;
                _fpsTimer = 0f;
            }
        }

        private void DetectMode()
        {
            var xrDisplaySubsystems = new System.Collections.Generic.List<XRDisplaySubsystem>();
            SubsystemManager.GetSubsystems(xrDisplaySubsystems);

            bool hasActiveXR = xrDisplaySubsystems.Count > 0 && xrDisplaySubsystems[0].running;
            _mode = hasActiveXR ? "VR" : "Desktop";
        }

        private void UpdateDisplay()
        {
            if (_debugText == null) return;

            _debugText.text = $"FPS: {_fps:F1}\n" +
                             $"Mode: {_mode}\n" +
                             $"App State: {_appState}";
        }

        /// <summary>
        /// Updates the app state display. Can be called externally for logging state changes.
        /// </summary>
        public void SetAppState(string state)
        {
            _appState = state;
        }
    }
}
