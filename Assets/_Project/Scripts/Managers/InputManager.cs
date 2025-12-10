/* ==================================================================================
 * 🎮 M.S.T. INPUT MANAGER (STATIC API)
 * ==================================================================================
 * Author:        Muhammet Serhat Tatar (M.S.T.)
 * Description:   Centralized input handling.
 * Usage:         Access directly via InputManager.IsTouching (No .Instance required).
 * ==================================================================================
 */

using UnityEngine;
using System;
using Utilities;

[DefaultExecutionOrder(-90)]
public class InputManager : MonoBehaviour
{
    // --- STATIC API (No .Instance needed) ---

    public static bool IsTouching { get; private set; }
    public static Vector2 TouchPosition { get; private set; }

    // Joystick data is set by VirtualJoystick.cs
    public static Vector2 JoystickInput { get; private set; }

    // Events
    public static event Action OnTap;
    public static event Action OnTouchDown;
    public static event Action OnTouchUp;
    public static event Action<Vector2> OnSwipe;
    public static event Action<Vector2> OnDrag;

    // --- CONFIG ---
    [Header("Settings")]
    [SerializeField] private bool _enableInput = true;
    [SerializeField] private float _swipeThreshold = 50f;

    // --- INTERNAL STATE ---
    private static InputManager _instance;
    private Vector2 _touchStartPos;
    private bool _isSwiped;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    private void Update()
    {
        if (!_enableInput) return;
        HandleInput();
    }

    private void HandleInput()
    {
        // 1. Touch Down
        if (Input.GetMouseButtonDown(0))
        {
            IsTouching = true;
            _touchStartPos = Input.mousePosition;
            TouchPosition = _touchStartPos;
            _isSwiped = false;

            OnTouchDown?.Invoke();
        }
        // 2. Touch Hold / Drag
        else if (Input.GetMouseButton(0))
        {
            IsTouching = true;
            Vector2 currentPos = Input.mousePosition;
            Vector2 delta = currentPos - TouchPosition;

            TouchPosition = currentPos;

            if (delta.magnitude > 0.1f)
            {
                OnDrag?.Invoke(delta);
            }

            // Swipe Logic
            if (!_isSwiped)
            {
                Vector2 swipeDelta = currentPos - _touchStartPos;
                if (swipeDelta.magnitude >= _swipeThreshold)
                {
                    OnSwipe?.Invoke(swipeDelta.normalized);
                    _isSwiped = true;
                }
            }
        }
        // 3. Touch Up
        else if (Input.GetMouseButtonUp(0))
        {
            IsTouching = false;
            OnTouchUp?.Invoke();

            if (!_isSwiped && (Vector2)Input.mousePosition == _touchStartPos)
            {
                OnTap?.Invoke();
            }
        }
    }

    // --- INTERNAL SETTERS ---

    // Called by VirtualJoystick.cs
    public static void SetJoystickInput(Vector2 input)
    {
        JoystickInput = input;
    }

    public static void SetInputState(bool isEnabled)
    {
        if (_instance == null) return;
        _instance._enableInput = isEnabled;

        if (!isEnabled)
        {
            IsTouching = false;
            JoystickInput = Vector2.zero;
        }
        GameLogger.Log($"Input System: {(isEnabled ? "Enabled" : "Disabled")}");
    }
}