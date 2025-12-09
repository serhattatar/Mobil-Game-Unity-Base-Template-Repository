/* ==================================================================================
 * 🛠️ UI DEBUG OVERLAY
 * ==================================================================================
 * Author:        Muhammet Serhat Tatar (M.S.T.)
 * Description:   Draws a debug window listing all registered UI Views in the current scene.
 * Allows Showing/Hiding panels without creating manual DebugCommands.
 * ==================================================================================
 */

#if DEVELOPMENT_BUILD || UNITY_EDITOR

using UnityEngine;
using System.Collections.Generic;
using UI.Core;

namespace Utilities.DebugSystem
{
    public class UIDebugOverlay : MonoBehaviour
    {
        private List<UIView> _views;
        private Vector2 _scrollPosition;
        private bool _isVisible = true;

        // Window Rect (Start centered)
        private Rect _windowRect = new Rect(Screen.width * 0.1f, Screen.height * 0.1f, Screen.width * 0.8f, Screen.height * 0.8f);

        public void Initialize(List<UIView> views)
        {
            _views = views;
        }

        private void OnGUI()
        {
            if (!_isVisible || _views == null) return;

            // Scale UI for high DPI screens (Mobile)
            float scale = Screen.dpi / 96f;
            // Fallback if dpi is weird
            if (scale < 1) scale = 1;

            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * scale);

            // Calculate scaled rect
            Rect scaledRect = new Rect(_windowRect.x / scale, _windowRect.y / scale, _windowRect.width / scale, _windowRect.height / scale);

            // Draw Window
            scaledRect = GUILayout.Window(999, scaledRect, DrawWindowContent, "UI INSPECTOR (M.S.T.)");
        }

        private void DrawWindowContent(int windowID)
        {
            GUILayout.BeginVertical();

            // Close Button
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("CLOSE INSPECTOR", GUILayout.Height(40)))
            {
                Destroy(gameObject); // Close this overlay
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            // List all views
            foreach (var view in _views)
            {
                if (view == null) continue;

                GUILayout.BeginHorizontal("box");

                // Name
                string status = view.IsActive ? "<color=lime>[OPEN]</color>" : "<color=grey>[CLOSED]</color>";
                GUILayout.Label($"{status} <b>{view.GetType().Name}</b>", CreateLabelStyle(), GUILayout.Width(200));

                GUILayout.FlexibleSpace();

                // Show Button
                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("SHOW", GUILayout.Width(60), GUILayout.Height(30)))
                {
                    UIManager.Show(view.GetType());
                }

                // Hide Button
                GUI.backgroundColor = Color.yellow;
                if (GUILayout.Button("HIDE", GUILayout.Width(60), GUILayout.Height(30)))
                {
                    UIManager.Hide(view.GetType());
                }

                GUI.backgroundColor = Color.white;
                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            // Allow dragging
            GUI.DragWindow();
        }

        private GUIStyle CreateLabelStyle()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.richText = true;
            style.alignment = TextAnchor.MiddleLeft;
            style.fontSize = 14;
            return style;
        }
    }
}
#endif