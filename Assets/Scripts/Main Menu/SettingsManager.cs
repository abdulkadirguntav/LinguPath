using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance;

    [Header("Paneller")]
    public GameObject settingsPanel;

    [Header("Müzik")]
    public Toggle musicToggle;

    [Header("Ses Efektleri")]
    public Toggle sfxToggle;

    private const string KEY_MUSIC_ON = "MusicOn";
    private const string KEY_SFX_ON   = "SFXOn";

    void Awake()
    {
        if (instance == null) instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        if (musicToggle != null)
        {
            musicToggle.isOn = PlayerPrefs.GetInt(KEY_MUSIC_ON, 1) == 1;
            musicToggle.onValueChanged.AddListener(OnMusicToggled);
        }
        if (sfxToggle != null)
        {
            sfxToggle.isOn = PlayerPrefs.GetInt(KEY_SFX_ON, 1) == 1;
            sfxToggle.onValueChanged.AddListener(OnSFXToggled);
        }
    }

    public void OpenSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        PlayerPrefs.Save();
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    private void OnMusicToggled(bool isOn)
    {
        PlayerPrefs.SetInt(KEY_MUSIC_ON, isOn ? 1 : 0);
        // Sahnedeki müzik AudioSource'unu bul ve uygula
        AudioSource music = FindAnyObjectByType<AudioSource>();
        if (music != null) music.volume = isOn ? 1f : 0f;
    }

    private void OnSFXToggled(bool isOn)
    {
        PlayerPrefs.SetInt(KEY_SFX_ON, isOn ? 1 : 0);
        if (AudioManager.instance != null) AudioManager.instance.SetSFXOn(isOn);
    }
}
