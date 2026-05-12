using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Paneller")]
    public GameObject mainMenuPanel;
    public GameObject characterCreatorPanel;
    public GameObject slotSelectionPanel;

    public void PlayGame()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (slotSelectionPanel != null) slotSelectionPanel.SetActive(true);
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