/* ==================================================================================
 * 📢 GAME LOGGER SYSTEM
 * ==================================================================================
 * Author:        Muhammet Serhat Tatar (M.S.T.)
 * Description:   High-performance wrapper for UnityEngine.Debug.
 * Automatically strips all calls in Release Builds using [Conditional].
 * Provides color-coded categories for better readability.
 * ==================================================================================
 */

using UnityEngine;
using System.Diagnostics; // Required for [Conditional]

namespace Utilities
{
    public static class GameLogger
    {
        // Define colors for consistency
        private const string COLOR_SUCCESS = "#00FF00"; // Green
        private const string COLOR_NETWORK = "#00FFFF"; // Cyan
        private const string COLOR_UI = "#FFA500";      // Orange
        private const string COLOR_COMBAT = "#FF00FF";  // Magenta
        private const string COLOR_SYSTEM = "#FFFF00";  // Yellow

        // --- STANDARD LOGS ---

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Log(object message, Object context = null)
        {
            UnityEngine.Debug.Log(message, context);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Warning(object message, Object context = null)
        {
            UnityEngine.Debug.LogWarning($"⚠️ {message}", context);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Error(object message, Object context = null)
        {
            UnityEngine.Debug.LogError($"🛑 {message}", context);
        }

        // --- CATEGORIZED LOGS (Colored) ---

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Success(object message, Object context = null)
        {
            UnityEngine.Debug.Log($"<color={COLOR_SUCCESS}>✔ [SUCCESS] {message}</color>", context);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Network(object message, Object context = null)
        {
            UnityEngine.Debug.Log($"<color={COLOR_NETWORK}>🌐 [NETWORK] {message}</color>", context);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void UI(object message, Object context = null)
        {
            UnityEngine.Debug.Log($"<color={COLOR_UI}>🖥️ [UI] {message}</color>", context);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Combat(object message, Object context = null)
        {
            UnityEngine.Debug.Log($"<color={COLOR_COMBAT}>⚔️ [COMBAT] {message}</color>", context);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void System(object message, Object context = null)
        {
            UnityEngine.Debug.Log($"<color={COLOR_SYSTEM}>⚙️ [SYSTEM] {message}</color>", context);
        }

        // --- CUSTOM COLOR ---

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Custom(object message, string colorHex, Object context = null)
        {
            UnityEngine.Debug.Log($"<color={colorHex}>{message}</color>", context);
        }
    }
}