using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System.Text;
using System.Collections.Generic;
using Utilities.DebugSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// A centralized Save/Load service using JSON serialization.
/// Features basic XOR encryption and Debug commands.
/// </summary>
[DefaultExecutionOrder(-90)]
public class SaveManager : MonoBehaviour
{
    [System.Serializable]
    public class PlayerProfile
    {
        public string Username = "Player";
        public int Level = 1;
        public double Coins = 0;
        public float MusicVolume = 1f;
        public float SfxVolume = 1f;
        public bool IsHapticsEnabled = true;
        public List<string> UnlockedItems = new List<string>();
    }

    [Header("Settings")]
    [SerializeField] private string _fileName = "player_data.save";
    [SerializeField] private bool _useEncryption = true;
    [SerializeField] private string _encryptionKey = "MST_SECRET_KEY";

    private static SaveManager _instance;
    private PlayerProfile _localData;
    private string _filePath;

    // --- PUBLIC API ---

    public static PlayerProfile Data
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("[SaveManager] Service not initialized!");
                return null;
            }
            return _instance._localData;
        }
    }

    public static void Save() => _instance?.SaveInternal();
    public static void Load() => _instance?.LoadInternal();
    public static void DeleteSave() => _instance?.DeleteSaveInternal();

    // --- UNITY LIFECYCLE ---

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        _filePath = Path.Combine(Application.persistentDataPath, _fileName);
        LoadInternal();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause) SaveInternal();
    }

    private void OnApplicationQuit()
    {
        SaveInternal();
    }

    // --- INTERNAL LOGIC ---

    private void SaveInternal()
    {
        if (_localData == null) _localData = new PlayerProfile();

        string json = JsonConvert.SerializeObject(_localData, Formatting.Indented);

        if (_useEncryption) json = EncryptDecrypt(json);

        try
        {
            File.WriteAllText(_filePath, json);
#if UNITY_EDITOR
            // Debug.Log("[SaveManager] Game Saved."); 
#endif
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to save: {e.Message}");
        }
    }

    private void LoadInternal()
    {
        if (File.Exists(_filePath))
        {
            try
            {
                string content = File.ReadAllText(_filePath);
                if (_useEncryption) content = EncryptDecrypt(content);

                _localData = JsonConvert.DeserializeObject<PlayerProfile>(content);
                Debug.Log($"[SaveManager] Data Loaded. Coins: {_localData.Coins}");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[SaveManager] Corrupted save ({e.Message}). Creating new.");
                CreateNewSave();
            }
        }
        else
        {
            CreateNewSave();
        }
    }

    private void CreateNewSave()
    {
        _localData = new PlayerProfile();
        SaveInternal();
    }

    private void DeleteSaveInternal()
    {
        if (File.Exists(_filePath)) File.Delete(_filePath);
        Debug.Log("[SaveManager] Save file deleted.");
        CreateNewSave();
    }

    private string EncryptDecrypt(string input)
    {
        StringBuilder modified = new StringBuilder();
        for (int i = 0; i < input.Length; i++)
        {
            modified.Append((char)(input[i] ^ _encryptionKey[i % _encryptionKey.Length]));
        }
        return modified.ToString();
    }

    // --- DEBUG COMMANDS ---
#if DEVELOPMENT_BUILD || UNITY_EDITOR
    [DebugCommand("Add 1000 Coins", "Economy")]
    private static void Cheat_AddMoney()
    {
        if (Data != null)
        {
            Data.Coins += 1000;
            Save();
            Debug.Log("Cheat: Added 1000 coins.");
        }
    }

    [DebugCommand("Reset Data", "System")]
    private static void Cheat_Reset()
    {
        DeleteSave();
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(SaveManager))]
public class SaveManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox("Service Mode Active. Auto-saves on Pause/Quit.", MessageType.Info);
        DrawDefaultInspector();

        if (Application.isPlaying)
        {
            EditorGUILayout.Space();
            if (GUILayout.Button("Force Save")) SaveManager.Save();
            if (GUILayout.Button("Delete Save")) SaveManager.DeleteSave();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("System by M.S.T.", EditorStyles.centeredGreyMiniLabel);
    }
}
#endif