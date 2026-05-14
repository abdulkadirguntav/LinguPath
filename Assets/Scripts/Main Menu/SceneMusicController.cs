using UnityEngine;

// Her sahnedeki müzik objesine bu scripti ekle.
// Oyuncu müziği kapattıysa bu sahne başladığında da kapalı gelir.
[RequireComponent(typeof(AudioSource))]
public class SceneMusicController : MonoBehaviour
{
    void Start()
    {
        bool musicOn = PlayerPrefs.GetInt("MusicOn", 1) == 1;
        GetComponent<AudioSource>().volume = musicOn ? 1f : 0f;
    }
}
