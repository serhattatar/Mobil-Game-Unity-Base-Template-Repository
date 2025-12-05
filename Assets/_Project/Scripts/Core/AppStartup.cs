using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

/// <summary>
/// The entry point of the application. Placed in the Bootstrap scene.
/// It initializes core services (Save, Audio, Pool, Debug) and loads the main Game Scene.
/// </summary>
public class AppStartup : MonoBehaviour
{
    [Header("Scene Configuration")]
    [Tooltip("The name of the main gameplay scene to load after initialization.")]
    [SerializeField] private string _gameSceneName = "GameScene";

    [Header("System Prefabs")]
    [Tooltip("Drag the PoolManager prefab (with configured pools) here.")]
    [SerializeField] private GameObject _poolManagerPrefab;

    [Tooltip("Drag the SaveManager prefab here.")]
    [SerializeField] private GameObject _saveManagerPrefab;

    [Tooltip("Drag the AudioManager prefab here.")]
    [SerializeField] private GameObject _audioManagerPrefab;

    [Header("Debug (Development Only)")]
    [Tooltip("Drag the DebugCanvas prefab here. Will only be created in Development Builds or Editor.")]
    [SerializeField] private GameObject _debugCanvasPrefab;

    private async void Start()
    {
        // 1. Setup Application Settings
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep; // Prevent screen dimming

        // 2. Initialize Systems (Order is important)
        await InitializeServices();

        // 3. Fake Loading Delay (Optional: useful for showing splash screen logos)
        await UniTask.Delay(1000);

        // 4. Load the Main Game
        Debug.Log($"[AppStartup] Loading scene: {_gameSceneName}");
        SceneManager.LoadScene(_gameSceneName);
    }

    private async UniTask InitializeServices()
    {
        // --- Core Managers ---
        // We use FindFirstObjectByType (Unity 6+) for better performance than FindObjectOfType

        if (Object.FindFirstObjectByType<SaveManager>() == null)
            CreateManager(_saveManagerPrefab, "SaveManager");

        if (Object.FindFirstObjectByType<PoolManager>() == null)
            CreateManager(_poolManagerPrefab, "PoolManager");

        if (Object.FindFirstObjectByType<AudioManager>() == null)
            CreateManager(_audioManagerPrefab, "AudioManager");

        // --- Debug System (Conditional) ---
        // This block is completely stripped out in Release builds for security and performance
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (_debugCanvasPrefab != null && Object.FindFirstObjectByType<Utilities.DebugSystem.DebugUI>() == null)
        {
            var debugObj = Instantiate(_debugCanvasPrefab);
            DontDestroyOnLoad(debugObj);
            Debug.Log("[AppStartup] Debug System Initialized (Development Mode).");
        }
#endif

        // Wait a frame to ensure all managers' Awake() methods have finished execution
        await UniTask.Yield();

        Debug.Log("<color=green>[AppStartup] All Services Initialized Successfully.</color>");
    }

    private void CreateManager(GameObject prefab, string defaultName)
    {
        GameObject obj;

        if (prefab != null)
        {
            obj = Instantiate(prefab);
            // Remove "(Clone)" from the name for cleaner hierarchy
            obj.name = prefab.name;
        }
        else
        {
            // Fallback: Auto-create from code if no prefab is assigned
            obj = new GameObject(defaultName);

            if (defaultName == "SaveManager") obj.AddComponent<SaveManager>();
            else if (defaultName == "PoolManager") obj.AddComponent<PoolManager>();
            else if (defaultName == "AudioManager") obj.AddComponent<AudioManager>();

            Debug.LogWarning($"[AppStartup] {defaultName} created from code (No Prefab assigned). Settings might be default.");
        }

        DontDestroyOnLoad(obj);
    }
}