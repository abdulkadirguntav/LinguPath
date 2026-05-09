using UnityEngine;

public static class StatisticsLogger
{
    private static StatisticsManager statisticsManager;

    static StatisticsLogger()
    {
        statisticsManager = StatisticsManager.instance;
    }

    public static void LogCorrectAnswer(string topic)
    {
        if (statisticsManager == null)
        {
            statisticsManager = StatisticsManager.instance;
            if (statisticsManager == null) { Debug.LogWarning("StatisticsManager not found!"); return; }
        }
        statisticsManager.LogCorrectAnswer(topic);
        Debug.Log($"Correct answer logged: {topic}");
    }

    public static void LogWrongAnswer(string topic, string mistakeType = "")
    {
        if (statisticsManager == null)
        {
            statisticsManager = StatisticsManager.instance;
            if (statisticsManager == null) { Debug.LogWarning("StatisticsManager not found!"); return; }
        }
        statisticsManager.LogWrongAnswer(topic, mistakeType);
        Debug.Log($"Wrong answer logged: {topic} - {mistakeType}");
    }

    public static void LogQuickAnswer(string topic, bool isCorrect)
    {
        if (isCorrect) LogCorrectAnswer(topic);
        else LogWrongAnswer(topic, "Quick Test Error");
    }

    public static void LogSwipeAnswer(bool isCorrect, string cardType = "Swipe Card")
    {
        if (isCorrect) LogCorrectAnswer($"Vocabulary - {cardType}");
        else LogWrongAnswer($"Vocabulary - {cardType}", "Wrong Selection");
    }

    public static void LogMarketAnswer(bool isCorrect, string itemName = "")
    {
        if (isCorrect) LogCorrectAnswer("Market Shopping");
        else LogWrongAnswer("Market Shopping", $"Wrong Item: {itemName}");
    }

    public static void LogTurnBaseBattle(bool isCorrect, string questionType = "")
    {
        if (isCorrect) LogCorrectAnswer($"Battle - {questionType}");
        else LogWrongAnswer($"Battle - {questionType}", "Wrong Answer");
    }

    public static void LogGrammarError(string errorMessage)
    {
        string topic = ExtractGrammarTopic(errorMessage);
        LogWrongAnswer(topic, errorMessage);
    }

    public static void LogSpeechFluency(bool isCorrect)
    {
        if (isCorrect) LogCorrectAnswer("Speaking Fluency");
        else LogWrongAnswer("Speaking Fluency", "Unclear Pronunciation");
    }

    public static void LogWordPronunciation(bool isCorrect, string word)
    {
        if (isCorrect) LogCorrectAnswer($"Pronunciation - {word}");
        else LogWrongAnswer($"Pronunciation - {word}", "Wrong Pronunciation");
    }

    private static string ExtractGrammarTopic(string errorMessage)
    {
        string lower = errorMessage.ToLower();
        if (lower.Contains("subject")) return "Subject Sentence";
        if (lower.Contains("verb") || lower.Contains("tense")) return "Verb Tense";
        if (lower.Contains("adjective")) return "Adjectives";
        if (lower.Contains("noun")) return "Noun Sentences";
        if (lower.Contains("pronoun")) return "Pronouns";
        if (lower.Contains("comma") || lower.Contains("punctuation")) return "Punctuation";
        if (lower.Contains("exclamation")) return "Exclamatory Sentences";
        if (lower.Contains("question")) return "Question Structure";
        if (lower.Contains("suffix")) return "Suffix Usage";
        return "Other Errors";
    }
}
