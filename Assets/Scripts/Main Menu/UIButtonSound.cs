using UnityEngine;
using UnityEngine.UI;

// Bu component'i hangi butona eklersen, tıklanınca otomatik ses çalar.
// Özel bir ses istersen specificClip alanını doldur, boş bırakırsan varsayılan buton sesi çalar.
[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour
{
    public AudioClip specificClip;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(PlaySound);
    }

    private void PlaySound()
    {
        if (AudioManager.instance == null) return;

        if (specificClip != null)
            AudioManager.instance.PlaySFX(specificClip);
        else
            AudioManager.instance.PlayButtonClick();
    }
}
