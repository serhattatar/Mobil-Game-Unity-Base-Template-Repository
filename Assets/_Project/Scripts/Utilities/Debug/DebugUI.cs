#if DEVELOPMENT_BUILD || UNITY_EDITOR

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities.DebugSystem;

namespace Utilities.DebugSystem
{
    public class DebugUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Transform _buttonContainer; // The Content of ScrollView
        [SerializeField] private Button _buttonPrefab; // A template button
        [SerializeField] private Text _fpsText;

        private bool _isOpen = false;
        private Dictionary<string, GameObject> _categoryHeaders = new Dictionary<string, GameObject>();

        private void Start()
        {
            GenerateUI();
            _panelRoot.SetActive(false);
            DontDestroyOnLoad(gameObject);
        }

        private void GenerateUI()
        {
            // 1. Clear existing (if any)
            foreach (Transform child in _buttonContainer)
            {
                Destroy(child.gameObject);
            }

            // 2. Get Commands via Reflection
            var commands = DebugSystem.GetAllCommands();

            // 3. Create Buttons
            foreach (var cmd in commands)
            {
                // Instantiate button
                Button btn = Instantiate(_buttonPrefab, _buttonContainer);
                btn.gameObject.SetActive(true);

                // Setup Text
                TextMeshPro btnText = btn.GetComponentInChildren<TextMeshPro>();
                if (btnText != null) btnText.text = $"[{cmd.Category}] {cmd.Name}";

                // Setup Click Event (Closure capture)
                var commandRef = cmd;
                btn.onClick.AddListener(() =>
                {
                    DebugSystem.ExecuteCommand(commandRef);
                    // Optional: Close menu after click?
                    // ToggleMenu(); 
                });
            }
        }

        private void Update()
        {
            HandleInput();
            UpdateFPS();
        }

        private void HandleInput()
        {
            // Toggle with 3 fingers or F1 key
            bool toggle = Input.GetKeyDown(KeyCode.F1) ||
                          (Input.touchCount == 3 && Input.GetTouch(2).phase == TouchPhase.Began);

            if (toggle) ToggleMenu();
        }

        private void ToggleMenu()
        {
            _isOpen = !_isOpen;
            _panelRoot.SetActive(_isOpen);
        }

        private void UpdateFPS()
        {
            if (_fpsText != null && _isOpen)
            {
                float fps = 1.0f / Time.unscaledDeltaTime;
                _fpsText.text = $"{Mathf.Ceil(fps)} FPS";
            }
        }
    }
}
#endif