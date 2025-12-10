/* ==================================================================================
 * 🕹️ VIRTUAL JOYSTICK
 * ==================================================================================
 * Author:        Muhammet Serhat Tatar (M.S.T.)
 * Description:   UI Component for on-screen joystick.
 * Pushes data directly to InputManager's static API.
 * ==================================================================================
 */

using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("References")]
    [SerializeField] private RectTransform _background;
    [SerializeField] private RectTransform _handle;

    [Header("Settings")]
    [SerializeField] private bool _hideOnRelease = false;

    private CanvasGroup _canvasGroup;
    private Vector2 _inputVector;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_hideOnRelease) SetVisibility(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_hideOnRelease) SetVisibility(true);
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_background, eventData.position, eventData.pressEventCamera, out pos))
        {
            pos.x = (pos.x / _background.sizeDelta.x);
            pos.y = (pos.y / _background.sizeDelta.y);

            _inputVector = new Vector2(pos.x * 2 - 1, pos.y * 2 - 1);
            _inputVector = (_inputVector.magnitude > 1.0f) ? _inputVector.normalized : _inputVector;

            _handle.anchoredPosition = new Vector2(
                _inputVector.x * (_background.sizeDelta.x / 2),
                _inputVector.y * (_background.sizeDelta.y / 2));

            // STATIC CALL
            InputManager.SetJoystickInput(_inputVector);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _inputVector = Vector2.zero;
        _handle.anchoredPosition = Vector2.zero;

        // STATIC CALL
        InputManager.SetJoystickInput(Vector2.zero);

        if (_hideOnRelease) SetVisibility(false);
    }

    private void SetVisibility(bool visible)
    {
        if (_canvasGroup == null) return;
        _canvasGroup.alpha = visible ? 1 : 0;
        _canvasGroup.blocksRaycasts = visible;
    }
}