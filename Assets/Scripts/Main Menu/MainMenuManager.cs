using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Paneller")]
    public GameObject mainMenuPanel;
    public GameObject characterCreatorPanel;

    public void PlayGame()
    {
        // Kayıtlı karakter yoksa önce oluşturma ekranını aç
        if (CharacterSaveManager.instance == null || !CharacterSaveManager.instance.HasSavedCharacter())
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (characterCreatorPanel != null) characterCreatorPanel.SetActive(true);
            return;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
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