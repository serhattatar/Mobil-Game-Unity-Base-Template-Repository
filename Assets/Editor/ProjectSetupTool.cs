using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Utilities;

/// <summary>
/// Editor tool to generate a standard folder structure for professional projects.
/// Access via Tools > Setup Project Folders.
/// </summary>
public class ProjectSetupTool
{
    [MenuItem("Tools/Setup Project Folders")]
    public static void CreateFolders()
    {
        List<string> folders = new List<string>
        {
            "_Project", // Root folder to separate your assets from 3rd party
            "_Project/Animations",
            "_Project/Audio",
            "_Project/Materials",
            "_Project/Models",
            "_Project/Prefabs",
            "_Project/Prefabs/UI",
            "_Project/Prefabs/Managers",
            "_Project/Resources",
            "_Project/Scenes",
            "_Project/Scripts",
            "_Project/Scripts/Core",
            "_Project/Scripts/Managers",
            "_Project/Scripts/UI",
            "_Project/Scripts/Utilities",
            "_Project/Settings",
            "_Project/Sprites",
            "_Project/Textures",
            "ThirdParty" // Place asset store packages here if possible
        };

        foreach (string folder in folders)
        {
            string path = Path.Combine(Application.dataPath, folder);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        AssetDatabase.Refresh();
        GameLogger.Success("Project Structure Generated Successfully!");
    }
}