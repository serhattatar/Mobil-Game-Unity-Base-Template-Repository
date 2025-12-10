# 📘 M.S.T. UNITY BASE TEMPLATE

### **Master Documentation (README)**

---

Author: **Muhammet Serhat Tatar (M.S.T.)**
Unity Version: **6000.0.x (Unity 6)**
Architecture: **Service-Based Wrappers (No Public Singletons), Clean Architecture**

---

# 📖 1. ARCHITECTURE OVERVIEW

This template enforces a strict **Bootstrap Pattern**.

## 🔄 Lifecycle

1. **Bootstrap Scene (Index 0)**

   * The game **must** start here.
   * Contains the `AppStartup` script.

2. **Service Initialization**
   `AppStartup` instantiates persistent managers as **DontDestroyOnLoad**:

   * PoolManager
   * SaveManager
   * AudioManager

3. **Game Load**
   Once all services are ready, the **GameScene** loads automatically.

⚠️ **RULE:** Never place Manager scripts manually inside the GameScene. They are injected via `AppStartup`.

---

# 🎱 2. OBJECT POOLING SYSTEM

A zero-allocation pooling wrapper built on `UnityEngine.Pool`.

## ⚙️ Configuration

1. Navigate to **_Project/Prefabs/Managers**
2. Select **PoolManager_Prefab**
3. Add your prefab types (Bullets, Enemies, etc.) to the **Initial Pools** list
4. Adjust **Prewarm Count** to pre-instantiate objects during loading

## 🚀 Spawning Objects

Use `.Spawn()` instead of `Instantiate()`:

```csharp
[SerializeField] private GameObject _bulletPrefab;

public void Fire()
{
    _bulletPrefab.Spawn(transform.position, transform.rotation);
}
```

## 🔄 Despawning Objects

Objects manage their own lifecycle:

```csharp
private void OnEnable()
{
    gameObject.ReturnToPool(3f); // auto-despawn after 3 seconds
}

private void OnCollisionEnter(Collision col)
{
    gameObject.ReturnToPool(); // instant return
}
```

---

# 💾 3. SAVE & LOAD SYSTEM

Secure persistence layer using **JSON serialization + XOR encryption**.

## ⭐ Features

* **Auto-Save** on `OnApplicationPause` and `OnApplicationQuit`
* **Encryption** enabled by default (configurable in SaveManager Inspector)

## 🧩 Usage API

```csharp
// Read data
int coins = SaveManager.Data.Coins;

// Modify data
SaveManager.Data.Coins += 100;
SaveManager.Data.UnlockedItems.Add("Shotgun");

// Manual save (optional)
SaveManager.Save();

// Reset save
SaveManager.DeleteSave();
```

---

# 🔊 4. AUDIO SYSTEM

Handles music & SFX with optional pitch variation.

## 🎧 Usage API

```csharp
[SerializeField] private AudioClip _shootSfx;
[SerializeField] private AudioClip _bgMusic;

void Start()
{
    AudioManager.PlayMusic(_bgMusic);
}

void Attack()
{
    AudioManager.PlaySFX(_shootSfx); // random pitch
    AudioManager.PlaySFX(_shootSfx, volume: 1f, randomPitch: false); // fixed pitch
}
```

---

# 🛠️ 5. DEBUG SYSTEM (REFLECTION-BASED)

A powerful mobile-friendly developer console.
Debug code is automatically stripped from Release builds.

## 🔓 Access Controls

* **Editor:** Press `F1`
* **Mobile:** Tap with **3 fingers**

## 🎮 Creating Cheats

Add `[DebugCommand]` to any **static** method:

```csharp
using Utilities.DebugSystem;

public class GameCheats
{
    [DebugCommand("Add 1000 Gold", "Economy")]
    public static void Cheat_AddGold()
    {
        SaveManager.Data.Coins += 1000;
        Debug.Log("Cheat Applied!");
    }
}
```

---

# 📦 6. INSTALLATION & SETUP

1. Clone the repository
2. Open with Unity 6 (6000.0.x)
3. Open scene: **_Project/Scenes/Bootstrap**
4. Select the **AppStartup** object
5. Ensure all manager prefab references are correctly assigned
6. To enable Debug Menu → **Development Build** must be checked

---

# 📝 7. FOLDER STRUCTURE RULES

* **_Project/** → All custom assets, scripts, prefabs
* **ThirdParty/** → Imported packages (do not modify)
* **Resources/** → Avoid using unless absolutely required (increases memory usage)


# 👑 10. Boss Mode (Debug Console)

A zero-setup, mobile-friendly development console generated entirely via code. It creates a runtime UI to view logs, tweak variables, and execute methods without needing inspector access.

### 🔹 Features
* **Sticky Header & Auto-Scroll:** Console behaves like Unity Editor.
* **Smart Alert Icon:** Flashes Red (Error) or Yellow (Warning) during gameplay.
* **Live Editing:** Tweak values in real-time.
* **Performance:** Uses TextMeshPro and strips completely from Release builds.

### 🔹 Setup
No setup required. The system initializes automatically.
**Requirement:** Ensure `TextMeshPro Essentials` are imported in the project.

### 🔹 How to Use
Add the `[BossControl]` attribute to any field, property, or method.

```csharp
using Utilities.BossMode;

public class PlayerController : MonoBehaviour
{
    // 1. Tweakable Variable
    [BossControl("Player/Move Speed")]
    public float MoveSpeed = 5f;

    // 2. Action Button
    [BossControl("Cheats/Kill All")]
    private void KillAllEnemies() { ... }

    // 3. Economy Tab
    [BossControl("Economy/Gold Drop", true)]
    public static int GoldDrop = 100;
}
🔹 Access
Trigger: Triple-tap the semi-transparent "DEV" icon in the top-right corner.

Clear: Press "CLEAR" in the Logs tab to reset logs and the alert icon.

