using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LibraryManager : MonoBehaviour
{
    [Header("DataBase")]
    public List<WordDataSO> wordList = new List<WordDataSO>();
    private WordDataSO currentWord;

    [Header("UI")]
    [SerializeField] private TMP_Text englishText;
    [SerializeField] private TMP_Text turkishText;
    [SerializeField] private TMP_Text exampleText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] public GameObject Library;
    [SerializeField] private GameObject mainGameUI;

    [Header("Reveal Mechanics")]
    [SerializeField] private GameObject revealButtonObj;

    private const int SessionWordCount = 10;
    private const string PageKey = "LibraryPageIndex";

    private List<WordDataSO> sessionDeck = new List<WordDataSO>();
    private int wordsCompletedThisSession = 0;

    void Start()
    {
        if (wordList.Count == 0)
            wordList.AddRange(Resources.LoadAll<WordDataSO>("Words"));

        if (wordList.Count == 0)
        {
            englishText.text = "Kelime bulunamadı!";
            exampleText.text = "Araçlarım → Kelimeleri Otomatik Üret çalıştır.";
            turkishText.text = "";
            return;
        }

        if (DataManager.instance == null)
        {
            Debug.LogError("DataManager yok!");
            return;
        }

        BuildDeck();
        LoadNextWord();
    }

    private void BuildDeck()
    {
        sessionDeck.Clear();
        wordsCompletedThisSession = 0;

        int pageIndex = PlayerPrefs.GetInt(PageKey, 0);

        // wordList sınırını aşmışsa başa dön
        if (pageIndex >= wordList.Count) pageIndex = 0;

        int count = Mathf.Min(SessionWordCount, wordList.Count);
        for (int i = 0; i < count; i++)
        {
            int idx = (pageIndex + i) % wordList.Count;
            if (wordList[idx] != null)
                sessionDeck.Add(wordList[idx]);
        }

        // Bir sonraki ziyaret için sayfayı ilerlet
        int nextPage = (pageIndex + count) % wordList.Count;
        PlayerPrefs.SetInt(PageKey, nextPage);
        PlayerPrefs.Save();
    }

    private void LoadNextWord()
    {
        if (sessionDeck.Count == 0)
        {
            CompleteSession();
            return;
        }

        currentWord = sessionDeck[0];
        englishText.text = currentWord.englishWord;
        exampleText.text = currentWord.exampleSentences;
        turkishText.text = currentWord.turkishMeaning;

        turkishText.gameObject.SetActive(false);
        if (revealButtonObj != null) revealButtonObj.SetActive(true);

        UpdateProgressText();
    }

    private void UpdateProgressText()
    {
        if (progressText != null)
            progressText.text = $"{wordsCompletedThisSession + 1} / {SessionWordCount}";
    }

    private void CompleteSession()
    {
        englishText.text = "Harika!";
        exampleText.text = "Bu turda " + SessionWordCount + " kelimeyi tamamladın!";
        turkishText.text = "";
        if (progressText != null) progressText.text = SessionWordCount + " / " + SessionWordCount;
        if (revealButtonObj != null) revealButtonObj.SetActive(false);

        if (NPC.ActiveNPC != null)
            NPC.ActiveNPC.FinishMission(true);
    }

    public void RevealTranslation()
    {
        turkishText.gameObject.SetActive(true);
        if (revealButtonObj != null) revealButtonObj.SetActive(false);
    }

    public void KnownButton()
    {
        if (currentWord == null) return;

        WordProgress progress = DataManager.instance.GetWordProgress(currentWord.wordID);
        if (progress.masteryLevel < 3) progress.masteryLevel++;
        DataManager.instance.SaveData();

        sessionDeck.RemoveAt(0);
        wordsCompletedThisSession++;
        LoadNextWord();
    }

    public void StudyButton()
    {
        if (currentWord == null) return;

        WordProgress progress = DataManager.instance.GetWordProgress(currentWord.wordID);
        progress.masteryLevel = 0;
        DataManager.instance.SaveData();

        // Kelimeyi desteye geri at
        WordDataSO word = sessionDeck[0];
        sessionDeck.RemoveAt(0);
        sessionDeck.Add(word);

        LoadNextWord();
    }

    public void ExitLibrary()
    {
        if (Library    != null) Library.SetActive(false);
        if (mainGameUI != null) mainGameUI.SetActive(true);
    }
}
