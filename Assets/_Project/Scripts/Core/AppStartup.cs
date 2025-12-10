/* ==================================================================================
 * 🚀 APP STARTUP (BOOTSTRAPPER)
 * ==================================================================================
 * Author:        Muhammet Serhat Tatar (M.S.T.)
 * Description:   Entry point of the application. Initializes all core services.
 * ==================================================================================
 */

using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using Utilities; 

public class AppStartup : MonoBehaviour
{
    [Header("Scene Configuration")]
    [Tooltip("The name of the main gameplay scene to load after initialization.")]
    [SerializeField] private string _gameSceneName = "GameScene";

    [Header("System Prefabs")]
    [Tooltip("PoolManager with configured pools.")]
    [SerializeField] private GameObject _poolManagerPrefab;

    [Tooltip("SaveManager prefab.")]
    [SerializeField] private GameObject _saveManagerPrefab;

    [Tooltip("AudioManager prefab.")]
    [SerializeField] private GameObject _audioManagerPrefab;

    [Tooltip("InputManager prefab.")]
    [SerializeField] private GameObject _inputManagerPrefab; 

    private async void Start()
    {
        // 1. Setup Application Settings
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // 2. Initialize Systems
        await InitializeServices();

        // 3. Fake Loading Delay (Optional)
        await UniTask.Delay(1000);

        // 4. Load the Main Game
        GameLogger.System($"[AppStartup] Loading scene: {_gameSceneName}");
        SceneManager.LoadScene(_gameSceneName);
    }

    private async UniTask InitializeServices()
    {
        // --- Core Managers ---
        // Using FindFirstObjectByType (Unity 6+) for performance

        if (Object.FindFirstObjectByType<SaveManager>() == null)
            CreateManager(_saveManagerPrefab, "SaveManager");

        if (Object.FindFirstObjectByType<PoolManager>() == null)
            CreateManager(_poolManagerPrefab, "PoolManager");

        if (Object.FindFirstObjectByType<AudioManager>() == null)
            CreateManager(_audioManagerPrefab, "AudioManager");

        if (Object.FindFirstObjectByType<InputManager>() == null)
            CreateManager(_inputManagerPrefab, "InputManager"); 

        // Wait a frame to ensure all Awake() methods are finished
        await UniTask.Yield();

        GameLogger.Success("[AppStartup] All Services Initialized Successfully.");
    }

    private void CreateManager(GameObject prefab, string defaultName)
    {
        GameObject obj;

        if (prefab != null)
        {
            obj = Instantiate(prefab);
            obj.name = prefab.name; // Clean name
        }
        else
        {
            // Fallback: Create from code
            obj = new GameObject(defaultName);

            if (defaultName == "SaveManager") obj.AddComponent<SaveManager>();
            else if (defaultName == "PoolManager") obj.AddComponent<PoolManager>();
            else if (defaultName == "AudioManager") obj.AddComponent<AudioManager>();
            else if (defaultName == "InputManager") obj.AddComponent<InputManager>(); 

            GameLogger.Warning($"[AppStartup] {defaultName} created without Prefab. Using defaults.");
        }

        DontDestroyOnLoad(obj);
    }
}