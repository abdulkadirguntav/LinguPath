using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SceneMusicController : MonoBehaviour
{
    public static SceneMusicController instance;

    private AudioSource audioSource;

    void Awake()
    {
        instance = this;
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        bool musicOn = PlayerPrefs.GetInt("MusicOn", 1) == 1;
        audioSource.volume = musicOn ? 1f : 0f;
    }

    public void SetVolume(float volume)
    {
        if (audioSource != null) audioSource.volume = volume;
    }
}
