📘 M.S.T. UNITY BASE TEMPLATE
Master Documentation (README)
Author: Muhammet Serhat Tatar (M.S.T.) Unity Version: 6000.0.x (Unity 6) Architecture: Service-Based Wrappers, Clean Architecture, Scene-Based UI

📖 1. ARCHITECTURE OVERVIEW
This template enforces a strict Bootstrap Pattern.

🔄 Lifecycle
Bootstrap Scene (Index 0)

The game must start here.

Contains the AppStartup script.

Service Initialization AppStartup instantiates persistent managers as DontDestroyOnLoad:

PoolManager

SaveManager

AudioManager

InputManager (New)

Game Load Once all services are ready, the GameScene loads automatically.

⚠️ RULE: Never place Manager scripts manually inside gameplay scenes. They are injected via AppStartup.

🎱 2. OBJECT POOLING SYSTEM
A zero-allocation pooling wrapper built on UnityEngine.Pool.

⚙️ Configuration
Navigate to _Project/Prefabs/Managers.

Select PoolManager_Prefab.

Add your prefab types to the Initial Pools list.

Adjust Prewarm Count to pre-instantiate objects.

🚀 Usage API
C#

// Spawn
_bulletPrefab.Spawn(transform.position, rotation);

// Despawn (Extension Method)
gameObject.ReturnToPool(3f); // Auto-return after 3s
💾 3. SAVE & LOAD SYSTEM
Secure persistence layer using JSON serialization + XOR encryption.

⭐ Features
Auto-Save on OnApplicationPause and Quit.

Encryption enabled by default.

🧩 Usage API
C#

// Read
int coins = SaveManager.Data.Coins;

// Write
SaveManager.Data.Coins += 100;
SaveManager.Save(); // Manual save
🔊 4. AUDIO SYSTEM
Handles music & SFX with pitch variation support.

🎧 Usage API
C#

AudioManager.PlayMusic(_bgMusic);
AudioManager.PlaySFX(_shootClip, volume: 1f, randomPitch: true);
🎮 5. INPUT SYSTEM (MOBILE & EDITOR)
A unified, static API for Touch, Joystick, and Swipe controls.

⚙️ Setup
Add InputManager_Prefab to AppStartup.

For on-screen joystick: Add the VirtualJoystick script to your UI Image.

🕹️ Usage API (Static)
C#

void Update()
{
    // 1. Joystick Vector
    Vector3 move = new Vector3(InputManager.JoystickInput.x, 0, InputManager.JoystickInput.y);

    // 2. Touch Detection
    if (InputManager.IsTouching)
    {
        Vector2 pos = InputManager.TouchPosition;
    }
}

// 3. Events
void Start()
{
    InputManager.OnTap += Jump;
    InputManager.OnSwipe += HandleSwipe; // Returns Vector2 direction
}
🖥️ 6. UI SYSTEM (SCENE-BASED)
A type-safe, scene-local UI architecture. UI elements are destroyed when the scene unloads to save memory.

⚙️ Setup
Create a Canvas in your Level scene.

Add UIManager script to it.

Assign your Popup Prefabs to the "Scene Views" list in Inspector.

🧩 Usage API
C#

// Show a Popup
UIManager.Show<SettingsPopup>();

// Show with Data (Payload)
UIManager.Show<WinPopup>(new WinData { Score = 100 });

// Hide
UIManager.Hide<SettingsPopup>();
📢 7. GAME LOGGER (CONDITIONAL)
A performance-oriented wrapper for Debug.Log. All calls are automatically stripped from Release Builds.

📝 Usage API
C#

// Standard
GameLogger.Log("Game Started");

// Categorized (Colored)
GameLogger.Combat("Player took 10 damage"); // Magenta
GameLogger.Network("Connected to server");  // Cyan
GameLogger.UI("Popup Opened");              // Orange

// Warning/Error
GameLogger.Warning("Low Ammo");
GameLogger.Error("Null Reference Detected");
👑 8. BOSS MODE (DEBUG CONSOLE)
A powerful, zero-setup developer console generated at runtime.

🔹 Access
Trigger: Triple-tap the semi-transparent "DEV" icon (Top-Right).

Alerts: Icon flashes Yellow (Warning) or Red (Error) during gameplay.

🔹 Features
Console: Real-time logs with collapse, filter, and sticky header features.

Inspector: Live variable tweaking and method execution.

Economy: Dedicated tab for game balancing.

🔹 Usage (Attributes)
Add [BossControl] to any field, property, or method.

C#

using Utilities.BossMode;

public class PlayerController : MonoBehaviour
{
    // Tweakable Variable (Live Update)
    [BossControl("Player/Speed")]
    public float MoveSpeed = 5f;

    // Action Button
    [BossControl("Cheats/Kill All")]
    private void KillAll() { ... }

    // Economy Tab
    [BossControl("Economy/Gold", true)]
    public static int Gold = 100;
}
🛠️ 9. EDITOR TOOLS
Custom tools to speed up workflow.

🔹 Scene Switcher (Overlay)
Located in the Scene View Toolbar.

Allows instant switching between scenes.

Play Mode Lock: Prevents accidental scene changes while playing.

🔹 Force Bootstrapper
Menu: Tools > M.S.T. > Enable Auto-Bootstrap.

Forces the editor to always start from Scene 0 (Bootstrap), ensuring Managers are initialized even if you press Play in "Level 3".

📦 10. INSTALLATION & FOLDERS
Open _Project/Scenes/Bootstrap.

Check AppStartup inspector references.

Ensure TextMeshPro Essentials are imported.

📂 Folder Structure Rules
_Project/: All custom assets.

_Project/Scripts/Core: AppStartup, Bootstrapper.

_Project/Scripts/Managers: Singleton Managers (Pool, Save, Input).

_Project/Scripts/Utilities: Helpers, BossMode, Logger.

ThirdParty/: Imported assets (Do not modify).