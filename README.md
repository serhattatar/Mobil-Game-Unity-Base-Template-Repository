# 🎮 M.S.T. Unity Base Template

<p align="center">
  <img src="https://img.shields.io/badge/Unity-6000.0.x-black?style=for-the-badge&logo=unity" />
  <img src="https://img.shields.io/badge/Architecture-Service--Based-blue?style=for-the-badge" />
  <img src="https://img.shields.io/badge/License-Proprietary-red?style=for-the-badge" />
</p>

---

<p align="center">
  <img src="https://dummyimage.com/900x200/222/fff&text=M.S.T.+Unity+Base+Template" style="border-radius:12px;" />
</p>

---

# 🧱 Overview

A **production‑ready Unity 6 template** built for  
**Hyper‑Casual, Idle, Puzzle, and Lightweight Action games.**

**Clean Architecture + Bootstrap Pattern + Zero Singletons.**

Author: **Muhammet Serhat Tatar (M.S.T.)**

---

# 🏗️ Architecture Diagram

```
[ Bootstrap Scene ]
        │
        ▼
┌─────────────────────────────┐
│         AppStartup          │
└─────────────────────────────┘
        │ Instantiates
        ▼
 ┌──────────────┬───────────────┬──────────────┐
 │ PoolManager  │ SaveManager    │ AudioManager │
 └──────────────┴───────────────┴──────────────┘
        │
        ▼
   [ Game Scene ]
```

---

# 🔄 Lifecycle

1. **Bootstrap Scene (Index 0)** loads first  
2. `AppStartup` initializes:
   - `PoolManager`
   - `SaveManager`
   - `AudioManager`
3. Auto‑loads **GameScene**

⚠️ Managers **must NOT** exist inside gameplay scenes.

---

# ✨ Key Systems

---

## 🎱 Zero‑Allocation Object Pooling

Wrapper over `UnityEngine.Pool`.

### Benefits
- Zero‑GC
- Instant spawn/despawn
- Prewarm support

### Configure
```
_Project/Prefabs/Managers/PoolManager_Prefab
```

### Spawn Example
```csharp
[SerializeField] private GameObject _bulletPrefab;

public void Fire()
{
    _bulletPrefab.Spawn(transform.position, transform.rotation);
}
```

### Return Example
```csharp
private void OnEnable() => gameObject.ReturnToPool(3f);
private void OnCollisionEnter(Collision col) => gameObject.ReturnToPool();
```

---

## 💾 Secure Save System

- JSON (XOR encrypted)
- Auto‑save on pause & quit
- Main entry: `SaveManager.Data`

```csharp
int coins = SaveManager.Data.Coins;
SaveManager.Data.Coins += 100;
SaveManager.Save();
SaveManager.DeleteSave();
```

---

## 🔊 Audio System

```csharp
[SerializeField] private AudioClip _shootSfx;
[SerializeField] private AudioClip _bgMusic;

void Start() => AudioManager.PlayMusic(_bgMusic);

void Attack()
{
    AudioManager.PlaySFX(_shootSfx);
    AudioManager.PlaySFX(_shootSfx, volume: 1f, randomPitch: false);
}
```

---

## 🛠️ Debug Console (Reflection‑Based)

- Mobile → **3‑finger tap**
- Editor → **F1**
- Auto‑UI for `[DebugCommand]`
- Removed in Release builds

```csharp
[DebugCommand("Add 1000 Gold", "Economy")]
public static void Cheat_AddGold()
{
    SaveManager.Data.Coins += 1000;
    Debug.Log("Cheat Applied!");
}
```

---

# 📦 Installation

```bash
git clone https://github.com/YourUsername/MST-Unity-Template.git MyNewGame
```

1. Open with **Unity 6 (6000.0.x)**  
2. Load `_Project/Scenes/Bootstrap`  
3. Validate `AppStartup` references  
4. Enable **Development Build** for Debug Console  

---

# 🗂️ Folder Rules

```
_Project/       → All custom assets & scripts  
ThirdParty/     → External libraries  
Resources/      → Avoid unless required  
```

---

# 🧩 Extra Utilities

## Object Spawn (Fast)
```csharp
_bulletPrefab.Spawn(transform.position, transform.rotation);
```

## Return to Pool
```csharp
gameObject.ReturnToPool(2f);
```

---

# 🧩 License
© 2025 Muhammet Serhat Tatar (M.S.T.). All rights reserved.
