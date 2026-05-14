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

    [Header("Settings")]
    public SettingsManager settingsManager;

    public void OpenSettings()
    {
        if (settingsManager != null) settingsManager.OpenSettings();
    }

    public void CloseSettings()
    {
        if (settingsManager != null) settingsManager.CloseSettings();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}