using UnityEngine;

public class MusicManager : PersistentSingleton<MusicManager>
{
    [Header("Audio Settings")]
    private AudioSource audioSource;
    private const string PREF_VOLUME = "MusicVolume";
    private const string PREF_MUTE = "MusicMute";

    public float Volume { get; private set; } = 1f;
    public bool IsMuted { get; private set; } = false;

    protected override void Awake()
    {
        base.Awake();
        
        // Ensure we have an AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // Force 2D sound so it doesn't matter where the camera is

        // Check for AudioListener
        if (Object.FindFirstObjectByType<AudioListener>() == null)
        {
            Debug.LogError("[MusicManager] NO AUDIOLISTENER FOUND IN SCENE! You won't hear anything.");
        }

        // Load saved settings
        LoadSettings();
    }

    private void LoadSettings()
    {
        Volume = PlayerPrefs.GetFloat(PREF_VOLUME, 0.5f); // Default 50%
        IsMuted = PlayerPrefs.GetInt(PREF_MUTE, 0) == 1;

        UpdateAudioSource();
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;

        // If playing the same song, don't restart
        if (audioSource.clip == clip && audioSource.isPlaying) return;

        audioSource.clip = clip;
        audioSource.Play();
        Debug.Log($"[MusicManager] Playing music: {clip.name} at Volume: {Volume}, Muted: {IsMuted}");
    }

    public void SetVolume(float volume)
    {
        Debug.Log($"[MusicManager] Volume changed to: {volume}");
        Volume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(PREF_VOLUME, Volume);
        PlayerPrefs.Save();
        
        UpdateAudioSource();
    }

    public void ToggleMute(bool isMuted)
    {
        IsMuted = isMuted;
        PlayerPrefs.SetInt(PREF_MUTE, IsMuted ? 1 : 0);
        PlayerPrefs.Save();

        UpdateAudioSource();
    }

    private void UpdateAudioSource()
    {
        if (audioSource == null) return;

        audioSource.volume = Volume;
        audioSource.mute = IsMuted;
    }
}
