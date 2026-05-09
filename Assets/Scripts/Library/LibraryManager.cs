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

    private List<WordDataSO> activeSessionDeck = new List<WordDataSO>();

    void Start()
    {
        StartSession(5);
    }

    public void StartSession(int wordCount)
    {
        activeSessionDeck.Clear();

        List<WordDataSO> availableWords = new List<WordDataSO>();
        foreach (WordDataSO w in wordList)
        {
            if (DataManager.instance.GetWordProgress(w.wordID).masteryLevel < 3)
                availableWords.Add(w);
        }

        for (int i = 0; i < availableWords.Count; i++)
        {
            WordDataSO temp = availableWords[i];
            int randomIndex = Random.Range(i, availableWords.Count);
            availableWords[i] = availableWords[randomIndex];
            availableWords[randomIndex] = temp;
        }

        int limit = Mathf.Min(wordCount, availableWords.Count);
        for (int i = 0; i < limit; i++)
            activeSessionDeck.Add(availableWords[i]);

        if (activeSessionDeck.Count == 0)
        {
            englishText.text = "CONGRATULATIONS!";
            exampleText.text = "You have mastered all words in the library.";
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
            englishText.text = "SESSION COMPLETE";
            exampleText.text = "You completed today's goal. Go ahead and take a break!";
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
