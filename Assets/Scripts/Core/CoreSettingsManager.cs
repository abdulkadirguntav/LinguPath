using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CoreSettingsManager : MonoBehaviour
{
    [Header("Paneller")]
    public GameObject settingsPanel;
    public GameObject settingsButton;

    [Header("Müzik & SFX Butonları")]
    public Button musicButton;
    public Button sfxButton;

    [Header("Renkler")]
    public Color colorOn  = new Color(0.4f, 0.85f, 0.4f);
    public Color colorOff = new Color(0.6f, 0.6f, 0.6f);

    private const string KEY_MUSIC_ON = "MusicOn";
    private const string KEY_SFX_ON   = "SFXOn";

    private bool musicOn;
    private bool sfxOn;

    void Start()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        musicOn = PlayerPrefs.GetInt(KEY_MUSIC_ON, 1) == 1;
        sfxOn   = PlayerPrefs.GetInt(KEY_SFX_ON,   1) == 1;

        if (musicButton != null) musicButton.onClick.AddListener(ToggleMusic);
        if (sfxButton   != null) sfxButton.onClick.AddListener(ToggleSFX);

        RefreshButtons();
    }

    public void OpenSettings()
    {
        if (settingsPanel  != null) settingsPanel.SetActive(true);
        if (settingsButton != null) settingsButton.SetActive(false);
    }

    public void CloseSettings()
    {
        PlayerPrefs.Save();
        if (settingsPanel  != null) settingsPanel.SetActive(false);
        if (settingsButton != null) settingsButton.SetActive(true);
    }

    public void ReturnToMainMenu()
    {
        PlayerPrefs.Save();
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void ToggleMusic()
    {
        musicOn = !musicOn;
        if (AudioManager.instance != null) AudioManager.instance.SetMusicOn(musicOn);
        else PlayerPrefs.SetInt(KEY_MUSIC_ON, musicOn ? 1 : 0);
        RefreshButtons();
    }

    public void ToggleSFX()
    {
        sfxOn = !sfxOn;
        if (AudioManager.instance != null) AudioManager.instance.SetSFXOn(sfxOn);
        else PlayerPrefs.SetInt(KEY_SFX_ON, sfxOn ? 1 : 0);
        RefreshButtons();
    }

    private void RefreshButtons()
    {
        SetButtonColor(musicButton, musicOn);
        SetButtonColor(sfxButton,   sfxOn);
    }

    private void SetButtonColor(Button btn, bool isOn)
    {
        if (btn == null) return;
        Image img = btn.GetComponent<Image>();
        if (img != null) img.color = isOn ? colorOn : colorOff;
    }
}
