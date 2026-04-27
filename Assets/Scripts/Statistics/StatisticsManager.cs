using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class StatisticsManager : MonoBehaviour
{
    public static StatisticsManager instance;

    private StatisticsData statisticsData;
    private string csvPath;
    private string statsJsonPath;

    private Dictionary<string, string> grammarTopicMap = new Dictionary<string, string>()
    {
        { "konu", "Konu Cümlesi" },
        { "fiil", "Fiil Zamanı" },
        { "sıfat", "Sıfatlandırma" },
        { "isim", "İsim Cümleleri" },
        { "zamir", "Zamir Kullanımı" },
        { "virgül", "Noktalama İşaretleri" },
        { "ünlem", "İfade Cümleleri" },
        { "soru", "Soru Yapısı" },
    };

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        csvPath = Application.persistentDataPath + "/OyuncuVerileri.csv";
        statsJsonPath = Application.persistentDataPath + "/statistics.json";

        statisticsData = new StatisticsData();
        LoadStatistics();
    }

    void OnEnable()
    {
        // Event'leri dinle
        GameEventSystem.OnAnswerGiven += HandleAnswerGiven;
        GameEventSystem.OnGameEnded += HandleGameEnded;
    }

    void OnDisable()
    {
        // Event'leri çıkar
        GameEventSystem.OnAnswerGiven -= HandleAnswerGiven;
        GameEventSystem.OnGameEnded -= HandleGameEnded;
    }

    private void HandleAnswerGiven(string topic, bool isCorrect, string mistakeDetails)
    {
        if (isCorrect)
        {
            LogCorrectAnswer(topic);
        }
        else
        {
            LogWrongAnswer(topic, mistakeDetails);
        }
    }

    private void HandleGameEnded(string gameName, bool playerWon, int score)
    {
        string topic = $"{gameName} ({score} puan)";
        if (playerWon)
            LogCorrectAnswer(topic);
        else
            LogWrongAnswer(topic, "Oyun kaybedildi");
    }

    /// <summary>
    /// CSV dosyasını oku ve istatistikleri güncelle
    /// </summary>
    public void LoadStatistics()
    {
        if (!File.Exists(csvPath))
        {
            Debug.LogWarning("📊 CSV dosyası henüz oluşturulmadı: " + csvPath);
            return;
        }

        statisticsData = new StatisticsData();
        string[] lines = File.ReadAllLines(csvPath);

        // İlk satır header olduğu için atla
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] parts = ParseCSVLine(line);
            if (parts.Length < 3) continue;

            string category = parts[0].Trim();
            string eventType = parts[1].Trim();
            string detail = parts[2].Trim();

            // Verileri analiz et
            if (category == "Yapay_Zeka" && eventType == "Gramer_Hatasi")
            {
                // Gramer hatasını kayıt et
                string topic = ExtractTopicFromError(detail);
                statisticsData.AddToTopic(topic, false, detail);
            }
            else if (category == "Yapay_Zeka" && eventType == "Dogu_Cevap")
            {
                // Doğru cevabı kayıt et
                statisticsData.AddToTopic("Doğru Cevaplar", true);
            }
        }

        Debug.Log("✅ İstatistikler yüklendi!");
    }

    /// <summary>
    /// CSV satırını ayrıştır (tırnak içi virgülleri dikkate al)
    /// </summary>
    private string[] ParseCSVLine(string line)
    {
        List<string> result = new List<string>();
        string currentField = "";
        bool insideQuotes = false;

        foreach (char c in line)
        {
            if (c == '"')
            {
                insideQuotes = !insideQuotes;
            }
            else if (c == ',' && !insideQuotes)
            {
                result.Add(currentField.Trim('"'));
                currentField = "";
            }
            else
            {
                currentField += c;
            }
        }
        result.Add(currentField.Trim('"'));
        return result.ToArray();
    }

    /// <summary>
    /// Gramer hatasından konuyu çıkar
    /// </summary>
    private string ExtractTopicFromError(string errorMessage)
    {
        // Hata mesajında hangi konu geçiyorsa onu döndür
        foreach (var kvp in grammarTopicMap)
        {
            if (errorMessage.ToLower().Contains(kvp.Key))
            {
                return kvp.Value;
            }
        }
        return "Diğer Hatalar";
    }

    /// <summary>
    /// Konuya doğru cevap ekle
    /// </summary>
    public void LogCorrectAnswer(string topic)
    {
        statisticsData.AddToTopic(topic, true);
        SaveStatistics();
    }

    /// <summary>
    /// Konuya yanlış cevap ekle
    /// </summary>
    public void LogWrongAnswer(string topic, string mistakeType = "")
    {
        statisticsData.AddToTopic(topic, false, mistakeType);
        SaveStatistics();
    }

    /// <summary>
    /// Tüm konuların istatistiklerini getir
    /// </summary>
    public List<TopicStats> GetAllTopics()
    {
        return statisticsData.GetAllTopicsStats();
    }

    /// <summary>
    /// En zayıf konuları getir
    /// </summary>
    public List<TopicStats> GetWeakestTopics(int count = 5)
    {
        return statisticsData.GetWeakestTopics(count);
    }

    /// <summary>
    /// En güçlü konuları getir
    /// </summary>
    public List<TopicStats> GetBestTopics(int count = 5)
    {
        return statisticsData.GetBestTopics(count);
    }

    /// <summary>
    /// Belirli bir konunun istatistiklerini getir
    /// </summary>
    public TopicStats GetTopicStats(string topicName)
    {
        return statisticsData.GetTopicStats(topicName);
    }

    /// <summary>
    /// Genel başarı oranını getir
    /// </summary>
    public float GetOverallSuccessRate()
    {
        return statisticsData.GetOverallSuccessRate();
    }

    /// <summary>
    /// Toplam denemeler
    /// </summary>
    public int GetTotalAttempts()
    {
        return statisticsData.totalAttempts;
    }

    /// <summary>
    /// İstatistikleri JSON olarak kaydet
    /// </summary>
    public void SaveStatistics()
    {
        try
        {
            string json = JsonUtility.ToJson(statisticsData, true);
            File.WriteAllText(statsJsonPath, json);
            Debug.Log("💾 İstatistikler kaydedildi: " + statsJsonPath);
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ İstatistikler kaydedilirken hata: " + e.Message);
        }
    }

    /// <summary>
    /// Tüm verileri sıfırla
    /// </summary>
    public void ResetAllStatistics()
    {
        statisticsData = new StatisticsData();
        SaveStatistics();
        Debug.Log("🔄 Tüm istatistikler sıfırlandı!");
    }

    public StatisticsData GetStatisticsData()
    {
        return statisticsData;
    }
}
