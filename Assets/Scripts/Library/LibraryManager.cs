using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;

public class LibraryManager : MonoBehaviour
{
    [Header("DataBase")]
    public List<WordDataSO> wordList = new List<WordDataSO>();
    private WordDataSO word;

    [Header("UI")]
    [SerializeField] private TMP_Text englishText;
    [SerializeField] private TMP_Text turkishText;
    [SerializeField] private TMP_Text exampleText;
    [SerializeField] private GameObject Library;

    [Header("Reveal Mechanics")]
    [SerializeField] private GameObject revealButtonObj;

    private const int DailyWordLimit = 5;
    private List<WordDataSO> activeSessionDeck = new List<WordDataSO>();

    void Start()
    {
        StartSession(DailyWordLimit);
    }

    private int GetWordsStudiedToday()
    {
        string today = DateTime.Now.ToString("yyyy-MM-dd");
        if (PlayerPrefs.GetString("LibraryLastDate", "") != today)
        {
            PlayerPrefs.SetString("LibraryLastDate", today);
            PlayerPrefs.SetInt("LibraryWordsToday", 0);
            PlayerPrefs.Save();
            return 0;
        }
        return PlayerPrefs.GetInt("LibraryWordsToday", 0);
    }

    private void IncrementWordsStudiedToday()
    {
        PlayerPrefs.SetInt("LibraryWordsToday", GetWordsStudiedToday() + 1);
        PlayerPrefs.Save();
    }

    public void StartSession(int wordCount)
    {
        activeSessionDeck.Clear();

        int wordsStudiedToday = GetWordsStudiedToday();
        int remaining = Mathf.Max(0, wordCount - wordsStudiedToday);

        if (remaining == 0)
        {
            englishText.text = "Bugünlük bu kadar!";
            exampleText.text = "Harika! Bugün " + wordCount + " kelimeyi tamamladın. Yarın tekrar görüşürüz!";
            turkishText.text = "";
            StartCoroutine(Wait());
            return;
        }

        List<WordDataSO> availableWords = new List<WordDataSO>();
        foreach (WordDataSO w in wordList)
        {
            if (DataManager.instance.GetWordProgress(w.wordID).masteryLevel < 3)
                availableWords.Add(w);
        }

        for (int i = 0; i < availableWords.Count; i++)
        {
            WordDataSO temp = availableWords[i];
            int randomIndex = UnityEngine.Random.Range(i, availableWords.Count);
            availableWords[i] = availableWords[randomIndex];
            availableWords[randomIndex] = temp;
        }

        int limit = Mathf.Min(remaining, availableWords.Count);
        for (int i = 0; i < limit; i++)
            activeSessionDeck.Add(availableWords[i]);

        if (activeSessionDeck.Count == 0)
        {
            englishText.text = "TEBRIKLER!";
            exampleText.text = "Kütüphanedeki tüm kelimeleri öğrendin!";
            turkishText.text = "";
            StartCoroutine(Wait());
            return;
        }

        LoadNextWord();
    }

    void LoadNextWord()
    {
        if (activeSessionDeck.Count == 0)
        {
            englishText.text = "Günlük hedef tamamlandı!";
            exampleText.text = "Bugün " + DailyWordLimit + " kelimeyi bitirdin. Biraz mola ver, yarın devam ederiz!";
            turkishText.text = "";
            StartCoroutine(Wait());
            return;
        }

        word = activeSessionDeck[0];
        englishText.text = word.englishWord;
        exampleText.text = word.exampleSentences;
        turkishText.text = word.turkishMeaning;

        turkishText.gameObject.SetActive(false);
        if (revealButtonObj != null) revealButtonObj.SetActive(true);
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(5f);
        Library.SetActive(false);
    }

    public void KnownButton()
    {
        if (activeSessionDeck.Count == 0) return;

        WordProgress currentProgress = DataManager.instance.GetWordProgress(word.wordID);
        if (currentProgress.masteryLevel < 3)
            currentProgress.masteryLevel++;
        DataManager.instance.SaveData();

        IncrementWordsStudiedToday();

        activeSessionDeck.RemoveAt(0);
        LoadNextWord();
    }

    public void StudyButton()
    {
        if (activeSessionDeck.Count == 0) return;

        WordProgress currentProgress = DataManager.instance.GetWordProgress(word.wordID);
        currentProgress.masteryLevel = 0;
        DataManager.instance.SaveData();

        WordDataSO currentWord = activeSessionDeck[0];
        activeSessionDeck.RemoveAt(0);
        activeSessionDeck.Add(currentWord);

        LoadNextWord();
    }

    public void RevealTranslation()
    {
        turkishText.gameObject.SetActive(true);
        if (revealButtonObj != null) revealButtonObj.SetActive(false);
    }
}
