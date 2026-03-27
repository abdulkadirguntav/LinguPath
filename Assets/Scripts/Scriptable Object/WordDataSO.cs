using UnityEngine;

[CreateAssetMenu(fileName = "NewWord", menuName = "LanguageGame/Word Data")]
public class WordDataSO : ScriptableObject
{
    [Header("Identity")]

    public string wordID;

    [Header("Word Detail")]

    public string englishWord;
    public string turkishMeaning;   

    [Header("Context")]

    [TextArea(2,4)]

    public string exampleSentences;
}
