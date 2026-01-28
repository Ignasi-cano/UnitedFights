using UnityEngine;

public class OptionsMenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private UnityEngine.UI.Toggle fullScreenToggle;
    [Header("Audio Controls")]
    [SerializeField] private UnityEngine.UI.Slider volumeSlider;
    [SerializeField] private UnityEngine.UI.Toggle musicMuteToggle;

    void Start()
    {
        // 1. Fullscreen Setup
        if (fullScreenToggle != null)
        {
            fullScreenToggle.isOn = Screen.fullScreen;
            fullScreenToggle.onValueChanged.AddListener(SetFullscreen);
        }

        // 2. Music Setup
        if (MusicManager.HasInstance)
        {
            // Initialize UI with saved values
            if (volumeSlider != null)
            {
                volumeSlider.value = MusicManager.Instance.Volume;
                volumeSlider.onValueChanged.AddListener(MusicManager.Instance.SetVolume);
            }
            else Debug.LogWarning("[OptionsMenu] Volume Slider is NOT assigned in the Inspector!");

            if (musicMuteToggle != null)
            {
                // UI says "Music" (implying enable/disable) or "Mute"? 
                // Usually a toggle named "Music" means "Is On". 
                // If user checks it, Music is ON (IsMuted = false).
                // Let's assume Checkbox = Music ON.
                musicMuteToggle.isOn = !MusicManager.Instance.IsMuted;
                musicMuteToggle.onValueChanged.AddListener((isOn) => MusicManager.Instance.ToggleMute(!isOn));
            }
        }
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        Debug.Log($"[OptionsMenu] Fullscreen set to: {isFullscreen}");
    }
}
