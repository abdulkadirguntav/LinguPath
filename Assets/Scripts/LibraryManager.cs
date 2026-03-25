using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LibraryManager : MonoBehaviour
{
    public List<WordDataSO> wordList = new List<WordDataSO>();
    public WordDataSO word;
    [SerializeField] private TMP_Text englishText;
    [SerializeField] private TMP_Text turkishText;
    [SerializeField] private TMP_Text exampleText;

    void Start()
    {
        LoadNextWord();
    }

    void LoadNextWord()
    {
        foreach(WordDataSO item in wordList)
        {
            WordProgress currentProgress = DataManager.instance.GetWordProgress(item.wordID);

            if(currentProgress.masteryLevel < 3)
            {
                word = item;

                englishText.text = word.englishWord;
                exampleText.text = word.exampleSentences;
                turkishText.text = word.turkishMeaning;

                return;
                
            }
        }

        Debug.Log(" Congratulations! You’ve mastered all the words in the library! ");

    }
}
