using System;

[Serializable]
public class WordProgress
{
    public string wordID;
    public int masteryLevel;
    public string lastReviewedData;

    public WordProgress(string id)
    {
        wordID = id;
        masteryLevel = 0;
        lastReviewedData = DateTime.Now.ToString("yyyy-MM-dd");
    }
}
