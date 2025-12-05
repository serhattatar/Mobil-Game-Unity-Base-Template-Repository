using UnityEngine;
using DG.Tweening; // DOTween is recommended for cross-fading music (Optional but better)

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// A centralized Audio Service.
/// Handles Background Music (BGM) and Sound Effects (SFX).
/// Features random pitch variation for repetitive sounds to avoid ear fatigue.
/// </summary>
[DefaultExecutionOrder(-95)] // Initialize after PoolManager but before gameplay
public class AudioManager : MonoBehaviour
{
    // --- CONFIGURATION ---
    [Header("Sources")]
    [Tooltip("Assign a child AudioSource for music.")]
    [SerializeField] private AudioSource _musicSource;
    [Tooltip("Assign a child AudioSource for sound effects.")]
    [SerializeField] private AudioSource _sfxSource;

    [Header("Settings")]
    [Range(0f, 0.5f)]
    [SerializeField] private float _pitchRandomness = 0.1f;

    // --- INTERNAL STATE ---
    private static AudioManager _instance;
    private float _masterVolume = 1f;

    // --- PUBLIC STATIC API ---

    public static void PlaySFX(AudioClip clip, float volume = 1f, bool randomPitch = true)
    {
        if (_instance != null) _instance.PlaySFXInternal(clip, volume, randomPitch);
    }

    public static void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (_instance != null) _instance.PlayMusicInternal(clip, loop);
    }

    public static void SetVolume(float volume)
    {
        if (_instance != null) _instance.SetVolumeInternal(volume);
    }

    public static void ToggleMute(bool isMuted)
    {
        if (_instance != null) _instance.SetMuteInternal(isMuted);
    }

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

        // Setup AudioSources if not assigned manually
        if (_musicSource == null) _musicSource = gameObject.AddComponent<AudioSource>();
        if (_sfxSource == null) _sfxSource = gameObject.AddComponent<AudioSource>();

        // Apply saved settings immediately
        // Note: Assuming SaveManager is initialized before this or data is ready.
        // If SaveManager is -90, this needs to check explicitly or rely on Start.
    }

    private void Start()
    {
        // Load initial volume from SaveManager (Integration Step)
        // Ensure SaveManager has loaded data.
        if (SaveManager.Data != null)
        {
            // Apply saved volume settings if you stored them
            // _musicSource.volume = SaveManager.Data.MusicVolume;
        }
    }

    // --- INTERNAL LOGIC ---

    private void PlaySFXInternal(AudioClip clip, float volume, bool randomPitch)
    {
        if (clip == null) return;

        // Randomize pitch for "game feel" (Juiciness)
        if (randomPitch)
        {
            _sfxSource.pitch = 1f + Random.Range(-_pitchRandomness, _pitchRandomness);
        }
        else
        {
            _sfxSource.pitch = 1f;
        }

        _sfxSource.PlayOneShot(clip, volume * _masterVolume);
    }

    private void PlayMusicInternal(AudioClip clip, bool loop)
    {
        if (clip == null || _musicSource.clip == clip) return;

        _musicSource.Stop();
        _musicSource.clip = clip;
        _musicSource.loop = loop;
        _musicSource.Play();

        // DOTween Fade In (Optional)
        // _musicSource.volume = 0;
        // _musicSource.DOFade(1f, 1f);
    }

    private void SetVolumeInternal(float volume)
    {
        _masterVolume = Mathf.Clamp01(volume);
        _musicSource.volume = _masterVolume;
        // SFX volume is applied during PlayOneShot
    }

    private void SetMuteInternal(bool isMuted)
    {
        _musicSource.mute = isMuted;
        _sfxSource.mute = isMuted;
    }
}

// --- CUSTOM INSPECTOR ---
#if UNITY_EDITOR
[CustomEditor(typeof(AudioManager))]
public class AudioManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        string helpMessage = "<b>AUDIO SERVICE:</b>\n\n" +
                             "1. <b>SFX:</b> AudioManager.PlaySFX(clip);\n" +
                             "2. <b>Music:</b> AudioManager.PlayMusic(clip);\n" +
                             "3. <b>Volume:</b> AudioManager.SetVolume(0.5f);\n\n" +
                             "<i>* Automatically adds AudioSources if missing.</i>";

        GUIStyle style = new GUIStyle(EditorStyles.helpBox);
        style.richText = true;

        EditorGUILayout.LabelField(helpMessage, style);
        EditorGUILayout.Space(5);
        DrawDefaultInspector();

        // Test Buttons
        if (Application.isPlaying)
        {
            EditorGUILayout.Space(10);
            if (GUILayout.Button("Test SFX (Random Pitch)"))
            {
                Debug.Log("Playing Test Sound (Needs AudioClip in script to work fully)");
                // In a real scenario, you'd need a reference clip here to test
            }
        }

        EditorGUILayout.Space(10);
        var signatureStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
        signatureStyle.fontStyle = FontStyle.Italic;
        EditorGUILayout.LabelField("System by M.S.T.", signatureStyle);
    }
}
#endif