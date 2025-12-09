/* ==================================================================================
 * 🛠️ BOSS MODE ATTRIBUTES
 * ==================================================================================
 * Author:        Muhammet Serhat Tatar (M.S.T.)
 * Description:   Attributes to expose fields, properties, and methods to the Boss Mode UI.
 * ==================================================================================
 */

using System;

namespace Utilities.BossMode
{
    /// <summary>
    /// Use this to expose a Static Field, Property, or Method to the Boss Mode Panel.
    /// Example: [BossControl("Player/Health")]
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public class BossControlAttribute : Attribute
    {
        public string Category { get; }
        public string Name { get; }
        public bool IsEconomy { get; }

        /// <param name="path">Format: "Category/Name" (e.g. "Player/Health")</param>
        /// <param name="isEconomy">If true, this appears in the 3rd Tab (Economy/Config)</param>
        public BossControlAttribute(string path, bool isEconomy = false)
        {
            string[] parts = path.Split('/');
            if (parts.Length >= 2)
            {
                Category = parts[0];
                Name = parts[1];
            }
            else
            {
                Category = "General";
                Name = path;
            }
            IsEconomy = isEconomy;
        }
    }
}