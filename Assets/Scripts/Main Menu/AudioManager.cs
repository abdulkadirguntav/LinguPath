using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("SFX (Kısa Ses Efektleri)")]
    public AudioSource sfxSource;
    public AudioClip buttonClickClip;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        bool sfxOn = PlayerPrefs.GetInt("SFXOn", 1) == 1;
        if (sfxSource != null) sfxSource.volume = sfxOn ? 1f : 0f;
    }

    public void PlayButtonClick()
    {
        if (buttonClickClip != null) PlaySFX(buttonClickClip);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
            sfxSource.PlayOneShot(clip);
    }

    public void SetSFXOn(bool isOn)
    {
        PlayerPrefs.SetInt("SFXOn", isOn ? 1 : 0);
        if (sfxSource != null) sfxSource.volume = isOn ? 1f : 0f;
    }

    public void SetMusicOn(bool isOn)
    {
        PlayerPrefs.SetInt("MusicOn", isOn ? 1 : 0);
        float vol = isOn ? 1f : 0f;

        if (SceneMusicController.instance != null)
        {
            SceneMusicController.instance.SetVolume(vol);
            return;
        }

        // Fallback: sahnedeki loop yapan tüm AudioSource'ları bul (bunlar müzik)
        AudioSource[] all = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource src in all)
        {
            if (src != sfxSource && src.loop)
                src.volume = vol;
        }
    }
}
