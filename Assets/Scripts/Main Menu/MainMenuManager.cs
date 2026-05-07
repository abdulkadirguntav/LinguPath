using UnityEngine;
using UnityEngine.SceneManagement; // Sahneler arası geçiş için bu kütüphane şart

public class MainMenuManager : MonoBehaviour
{
    // PLAY butonu tetikleyecek
    public void PlayGame()
    {
        // Mevcut sahneden bir sonraki sahneye (oyun sahnesine) geçer
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        
        // Veya direkt ismen de çağırabilirsin:
        // SceneManager.LoadScene("OyunLevel_1");
    }

    // SETTINGS butonu tetikleyecek
    public void OpenSettings()
    {
        // İleride buraya ayarlar panelini (GameObject) aktif etme kodu gelecek
        Debug.Log("Ayarlar menüsü açıldı!");
    }

    // QUIT butonu tetikleyecek
    public void QuitGame()
    {
        Debug.Log("Oyundan çıkılıyor..."); 
        Application.Quit(); // Not: Bu kod Unity Editor'de çalışmaz, oyunu build aldığında çalışır.
    }
}