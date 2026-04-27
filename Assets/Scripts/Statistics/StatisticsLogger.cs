using UnityEngine;

/// <summary>
/// Tüm oyun modlarından StatisticsManager'a veri göndermeyi kolaylaştıran yardımcı sınıf
/// Kullanım: StatisticsLogger.LogCorrectAnswer("Konuşma Akıcılığı");
/// </summary>
public static class StatisticsLogger
{
    private static StatisticsManager statisticsManager;

    static StatisticsLogger()
    {
        statisticsManager = StatisticsManager.instance;
    }

    /// <summary>
    /// Doğru cevabı kayıt et
    /// Örnek: StatisticsLogger.LogCorrectAnswer("Fiil Zamanı");
    /// </summary>
    public static void LogCorrectAnswer(string topic)
    {
        if (statisticsManager == null)
        {
            statisticsManager = StatisticsManager.instance;
            if (statisticsManager == null)
            {
                Debug.LogWarning("⚠️ StatisticsManager bulunamadı!");
                return;
            }
        }

        statisticsManager.LogCorrectAnswer(topic);
        Debug.Log($"✅ Doğru cevap kaydedildi: {topic}");
    }

    /// <summary>
    /// Yanlış cevabı kayıt et
    /// Örnek: StatisticsLogger.LogWrongAnswer("Soru Yapısı", "Soru işareti unutulmuş");
    /// </summary>
    public static void LogWrongAnswer(string topic, string mistakeType = "")
    {
        if (statisticsManager == null)
        {
            statisticsManager = StatisticsManager.instance;
            if (statisticsManager == null)
            {
                Debug.LogWarning("⚠️ StatisticsManager bulunamadı!");
                return;
            }
        }

        statisticsManager.LogWrongAnswer(topic, mistakeType);
        Debug.Log($"❌ Yanlış cevap kaydedildi: {topic} - {mistakeType}");
    }

    /// <summary>
    /// Hızlı bir cevabı kayıt et (hızlı/doğru)
    /// </summary>
    public static void LogQuickAnswer(string topic, bool isCorrect)
    {
        if (isCorrect)
            LogCorrectAnswer(topic);
        else
            LogWrongAnswer(topic, "Hızlı Test Hatası");
    }

    /// <summary>
    /// SwipeGame için kayıt
    /// </summary>
    public static void LogSwipeAnswer(bool isCorrect, string cardType = "Swipe Kartı")
    {
        if (isCorrect)
            LogCorrectAnswer($"Kelime - {cardType}");
        else
            LogWrongAnswer($"Kelime - {cardType}", "Yanlış Seçim");
    }

    /// <summary>
    /// Market Game için kayıt
    /// </summary>
    public static void LogMarketAnswer(bool isCorrect, string itemName = "")
    {
        if (isCorrect)
            LogCorrectAnswer("Market Alışverişi");
        else
            LogWrongAnswer("Market Alışverişi", $"Yanlış Ürün: {itemName}");
    }

    /// <summary>
    /// TurnBase Game için kayıt
    /// </summary>
    public static void LogTurnBaseBattle(bool isCorrect, string questionType = "")
    {
        if (isCorrect)
            LogCorrectAnswer($"Savaş - {questionType}");
        else
            LogWrongAnswer($"Savaş - {questionType}", "Yanlış Cevap");
    }

    /// <summary>
    /// Gramer hatası kayıt et
    /// </summary>
    public static void LogGrammarError(string errorMessage)
    {
        // Hata mesajından konu çıkar
        string topic = ExtractGrammarTopic(errorMessage);
        LogWrongAnswer(topic, errorMessage);
    }

    /// <summary>
    /// Konuşma akıcılığını kayıt et
    /// </summary>
    public static void LogSpeechFluency(bool isCorrect)
    {
        if (isCorrect)
            LogCorrectAnswer("Konuşma Akıcılığı");
        else
            LogWrongAnswer("Konuşma Akıcılığı", "Anlaşılmayan Telaffuz");
    }

    private static string ExtractGrammarTopic(string errorMessage)
    {
        string lower = errorMessage.ToLower();

        if (lower.Contains("konu") || lower.Contains("subject")) return "Konu Cümlesi";
        if (lower.Contains("fiil") || lower.Contains("verb")) return "Fiil Zamanı";
        if (lower.Contains("sıfat") || lower.Contains("adjective")) return "Sıfatlandırma";
        if (lower.Contains("isim") || lower.Contains("noun")) return "İsim Cümleleri";
        if (lower.Contains("zamir") || lower.Contains("pronoun")) return "Zamir Kullanımı";
        if (lower.Contains("virgül") || lower.Contains("comma")) return "Noktalama İşaretleri";
        if (lower.Contains("ünlem") || lower.Contains("exclamation")) return "İfade Cümleleri";
        if (lower.Contains("soru") || lower.Contains("question")) return "Soru Yapısı";
        if (lower.Contains("zaman") || lower.Contains("tense")) return "Fiil Zamanı";
        if (lower.Contains("ek") || lower.Contains("suffix")) return "Ek Kullanımı";

        return "Diğer Hatalar";
    }

    /// <summary>
    /// Ses kayıt et (kelime telaffuzu)
    /// </summary>
    public static void LogWordPronunciation(bool isCorrect, string word)
    {
        if (isCorrect)
            LogCorrectAnswer($"Telaffuz - {word}");
        else
            LogWrongAnswer($"Telaffuz - {word}", "Yanlış Telaffuz");
    }
}
