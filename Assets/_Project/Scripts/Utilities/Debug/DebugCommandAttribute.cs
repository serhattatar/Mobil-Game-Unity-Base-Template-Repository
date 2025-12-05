using System;

namespace Utilities.DebugSystem
{
    /// <summary>
    /// Mark static methods with this attribute to automatically generate a debug button in the UI.
    /// Works only in Development Builds or Editor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class DebugCommandAttribute : Attribute
    {
        public string Name { get; }
        public string Category { get; }

        public DebugCommandAttribute(string name, string category = "General")
        {
            Name = name;
            Category = category;
        }
    }
}