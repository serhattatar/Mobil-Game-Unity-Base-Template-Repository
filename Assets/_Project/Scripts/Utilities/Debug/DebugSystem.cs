#if DEVELOPMENT_BUILD || UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Utilities.DebugSystem
{
    public static class DebugSystem
    {
        public struct CommandData
        {
            public string Name;
            public string Category;
            public MethodInfo Method;
        }

        private static List<CommandData> _cachedCommands;

        /// <summary>
        /// Scans the current assembly using Reflection to find all methods marked with [DebugCommand].
        /// This is an expensive operation, so results are cached.
        /// </summary>
        public static List<CommandData> GetAllCommands()
        {
            if (_cachedCommands != null) return _cachedCommands;

            _cachedCommands = new List<CommandData>();

            // Get all types in the executing assembly (your project scripts)
            var assembly = Assembly.GetExecutingAssembly();
            var types = assembly.GetTypes();

            foreach (var type in types)
            {
                // We only look for Static methods to avoid dependency on specific instances.
                // This ensures clean architecture and prevents tightly coupled references.
                var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

                foreach (var method in methods)
                {
                    var attribute = method.GetCustomAttribute<DebugCommandAttribute>();
                    if (attribute != null)
                    {
                        _cachedCommands.Add(new CommandData
                        {
                            Name = attribute.Name,
                            Category = attribute.Category,
                            Method = method
                        });
                    }
                }
            }

            Debug.Log($"[DebugSystem] Discovered {_cachedCommands.Count} commands via Reflection.");
            return _cachedCommands;
        }

        public static void ExecuteCommand(CommandData command)
        {
            try
            {
                // Invoke the static method (null instance)
                command.Method.Invoke(null, null);
                Debug.Log($"<color=cyan>[DebugSystem] Executed: {command.Name}</color>");
            }
            catch (Exception e)
            {
                Debug.LogError($"[DebugSystem] Failed to execute {command.Name}: {e.Message}");
            }
        }
    }
}
#endif