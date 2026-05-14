using UnityEngine;

public class UniversalExitButton : MonoBehaviour
{
    [Header("Buton Objesi")]
    public GameObject exitButtonObj;

    [Header("Oyun Yöneticileri")]
    public TurnBaseManager turnBaseManager;
    public SwipeManager swipeManager;
    public MarketManager marketManager;
    public GeminiManager geminiManager;
    public LibraryManager libraryManager;

    void Update()
    {
        if (exitButtonObj != null)
            exitButtonObj.SetActive(IsAnyGameActive());
    }

    private bool IsAnyGameActive()
    {
        if (turnBaseManager != null && turnBaseManager.battlePanel != null && turnBaseManager.battlePanel.activeSelf) return true;
        if (swipeManager    != null && swipeManager.swipePanel    != null && swipeManager.swipePanel.activeSelf)    return true;
        if (marketManager   != null && marketManager.marketPanel  != null && marketManager.marketPanel.activeSelf)  return true;
        if (geminiManager   != null && geminiManager.chatPanel    != null && geminiManager.chatPanel.activeSelf)    return true;
        if (libraryManager  != null && libraryManager.Library     != null && libraryManager.Library.activeSelf)     return true;
        return false;
    }

    public void ExitCurrentGame()
    {
        if (turnBaseManager != null && turnBaseManager.battlePanel != null && turnBaseManager.battlePanel.activeSelf)
            turnBaseManager.FleeBattle();
        else if (swipeManager != null && swipeManager.swipePanel != null && swipeManager.swipePanel.activeSelf)
            swipeManager.ExitSwipeGame();
        else if (marketManager != null && marketManager.marketPanel != null && marketManager.marketPanel.activeSelf)
            marketManager.ExitMiniGame();
        else if (geminiManager != null && geminiManager.chatPanel != null && geminiManager.chatPanel.activeSelf)
            geminiManager.ExitChat();
        else if (libraryManager != null && libraryManager.Library != null && libraryManager.Library.activeSelf)
            libraryManager.ExitLibrary();
    }
}
