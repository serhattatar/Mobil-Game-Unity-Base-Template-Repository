\# 🎮 M.S.T. Unity Base Template



!\[Unity Version](https://img.shields.io/badge/Unity-6000.0.x-black?style=flat\\\&logo=unity)

!\[Architecture](https://img.shields.io/badge/Architecture-Service--Based-blue)

!\[License](https://img.shields.io/badge/License-Proprietary-red)



> \*\*A production‑ready, high‑performance Unity 6 template designed for Hyper‑Casual, Idle, and Puzzle games.\*\*

> \*\*Author:\*\* Muhammet Serhat Tatar (M.S.T.)



---



\## 📖 Architecture Overview



This template enforces a strict \*\*Bootstrap Pattern\*\* while following \*\*Clean Architecture\*\* principles. No public singletons are used—everything runs through \*\*Service‑Based Wrappers\*\*.



\### 🔄 Lifecycle



1\. \*\*Bootstrap Scene (Index 0)\*\* — Must always load first. Contains `AppStartup`.

2\. \*\*Service Initialization\*\* — `AppStartup` instantiates:



&nbsp;  \* `PoolManager`

&nbsp;  \* `SaveManager`

&nbsp;  \* `AudioManager`

3\. \*\*Game Load\*\* — After initialization, the system autoloads the \*\*GameScene\*\*.



> ⚠️ \*\*IMPORTANT:\*\* Manager scripts must never be placed inside gameplay scenes. They are injected automatically.



---



\## ✨ Key Systems



\### 🎱 Zero‑Allocation Object Pooling



A lightweight wrapper around `UnityEngine.Pool`.



\*\*Benefits:\*\*



\* Prevents GC spikes

\* Instant spawn/despawn

\* Prewarming support



\*\*Configure:\*\*



```

\_Project/Prefabs/Managers → PoolManager\_Prefab

```



\*\*Spawn Example:\*\*



```csharp

\[SerializeField] private GameObject \_bulletPrefab;



public void Fire()

{

&nbsp;   \_bulletPrefab.Spawn(transform.position, transform.rotation);

}

```



\*\*Return Example:\*\*



```csharp

private void OnEnable()

{

&nbsp;   gameObject.ReturnToPool(3f);

}



private void OnCollisionEnter(Collision col)

{

&nbsp;   gameObject.ReturnToPool();

}

```



---



\### 💾 Secure Save System



\* \*\*Format:\*\* JSON serialization (XOR encrypted)

\* \*\*Auto‑Save:\*\* On pause \& quit

\* \*\*API:\*\* `SaveManager.Data`



\*\*Usage:\*\*



```csharp

int coins = SaveManager.Data.Coins;

SaveManager.Data.Coins += 100;

SaveManager.Save();

SaveManager.DeleteSave();

```



---



\### 🔊 Audio System



\* Simple static API

\* Optional pitch randomness for game feel



```csharp

\[SerializeField] private AudioClip \_shootSfx;

\[SerializeField] private AudioClip \_bgMusic;



void Start()

{

&nbsp;   AudioManager.PlayMusic(\_bgMusic);

}



void Attack()

{

&nbsp;   AudioManager.PlaySFX(\_shootSfx);

&nbsp;   AudioManager.PlaySFX(\_shootSfx, volume: 1f, randomPitch: false);

}

```



---



\### 🛠️ Reflection‑Based Debug Console



\* \*\*Mobile:\*\* Tap with 3 fingers

\* \*\*Editor:\*\* Press `F1`

\* Auto‑generates UI for methods marked with `\[DebugCommand]`

\* Fully stripped from Release builds



```csharp

\[DebugCommand("Add 1000 Gold", "Economy")]

public static void Cheat\_AddGold()

{

&nbsp;   SaveManager.Data.Coins += 1000;

&nbsp;   Debug.Log("Cheat Applied!");

}

```



---



\## 📦 Installation \& Setup



1\. \*\*Clone the repository:\*\*



```bash

git clone https://github.com/YourUsername/MST-Unity-Template.git MyNewGame

```



2\. \*\*Open in Unity 6 (6000.0.x)\*\*

3\. Open scene: `\_Project/Scenes/Bootstrap`

4\. Select `AppStartup` and verify all manager references

5\. Enable \*\*Development Build\*\* to use the Debug Console



---



\## 🗂️ Folder Structure Rules



\* \*\*\_Project/\*\* — All custom scripts \& assets

\* \*\*ThirdParty/\*\* — Imported dependencies (do not modify)

\* \*\*Resources/\*\* — Avoid unless absolutely necessary



---



\## 💻 Additional Code Examples



\### Spawning Objects



```csharp

\[SerializeField] private GameObject \_bulletPrefab;



public void Fire()

{

&nbsp;   \_bulletPrefab.Spawn(transform.position, transform.rotation);

}

```



\### Returning to Pool



```csharp

gameObject.ReturnToPool(2f);

```



---



\## 🧩 Notes



