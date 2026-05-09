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
        { "subject", "Subject Sentence" },
        { "verb", "Verb Tense" },
        { "adjective", "Adjectives" },
        { "noun", "Noun Sentences" },
        { "pronoun", "Pronouns" },
        { "comma", "Punctuation" },
        { "exclamation", "Exclamatory Sentences" },
        { "question", "Question Structure" },
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

        csvPath = Application.persistentDataPath + "/PlayerData.csv";
        statsJsonPath = Application.persistentDataPath + "/statistics.json";

        statisticsData = new StatisticsData();
        LoadStatistics();
    }

    void OnEnable()
    {
        GameEventSystem.OnAnswerGiven += HandleAnswerGiven;
        GameEventSystem.OnGameEnded += HandleGameEnded;
    }

    void OnDisable()
    {
        GameEventSystem.OnAnswerGiven -= HandleAnswerGiven;
        GameEventSystem.OnGameEnded -= HandleGameEnded;
    }

    private void HandleAnswerGiven(string topic, bool isCorrect, string mistakeDetails)
    {
        if (isCorrect) LogCorrectAnswer(topic);
        else LogWrongAnswer(topic, mistakeDetails);
    }

    private void HandleGameEnded(string gameName, bool playerWon, int score)
    {
        string topic = $"{gameName} ({score} points)";
        if (playerWon) LogCorrectAnswer(topic);
        else LogWrongAnswer(topic, "Game lost");
    }

    public void LoadStatistics()
    {
        if (!File.Exists(csvPath))
        {
            Debug.LogWarning("CSV file not yet created: " + csvPath);
            return;
        }

        statisticsData = new StatisticsData();
        string[] lines = File.ReadAllLines(csvPath);

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] parts = ParseCSVLine(line);
            if (parts.Length < 3) continue;

            string category = parts[0].Trim();
            string eventType = parts[1].Trim();
            string detail = parts[2].Trim();

            if (category == "AI" && eventType == "Grammar_Error")
            {
                string topic = ExtractTopicFromError(detail);
                statisticsData.AddToTopic(topic, false, detail);
            }
            else if (category == "AI" && eventType == "Correct_Answer")
            {
                statisticsData.AddToTopic("Correct Answers", true);
            }
        }

        Debug.Log("Statistics loaded!");
    }

    private string[] ParseCSVLine(string line)
    {
        List<string> result = new List<string>();
        string currentField = "";
        bool insideQuotes = false;

        foreach (char c in line)
        {
            if (c == '"') insideQuotes = !insideQuotes;
            else if (c == ',' && !insideQuotes)
            {
                result.Add(currentField.Trim('"'));
                currentField = "";
            }
            else currentField += c;
        }
        result.Add(currentField.Trim('"'));
        return result.ToArray();
    }

    private string ExtractTopicFromError(string errorMessage)
    {
        foreach (var kvp in grammarTopicMap)
        {
            if (errorMessage.ToLower().Contains(kvp.Key))
                return kvp.Value;
        }
        return "Other Errors";
    }

    public void LogCorrectAnswer(string topic)
    {
        statisticsData.AddToTopic(topic, true);
        SaveStatistics();
    }

    public void LogWrongAnswer(string topic, string mistakeType = "")
    {
        statisticsData.AddToTopic(topic, false, mistakeType);
        SaveStatistics();
    }

    public List<TopicStats> GetAllTopics() => statisticsData.GetAllTopicsStats();
    public List<TopicStats> GetWeakestTopics(int count = 5) => statisticsData.GetWeakestTopics(count);
    public List<TopicStats> GetBestTopics(int count = 5) => statisticsData.GetBestTopics(count);
    public TopicStats GetTopicStats(string topicName) => statisticsData.GetTopicStats(topicName);
    public float GetOverallSuccessRate() => statisticsData.GetOverallSuccessRate();
    public int GetTotalAttempts() => statisticsData.totalAttempts;
    public StatisticsData GetStatisticsData() => statisticsData;

    public void SaveStatistics()
    {
        try
        {
            string json = JsonUtility.ToJson(statisticsData, true);
            File.WriteAllText(statsJsonPath, json);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error saving statistics: " + e.Message);
        }
    }

    public void ResetAllStatistics()
    {
        statisticsData = new StatisticsData();
        SaveStatistics();
        Debug.Log("All statistics reset.");
    }
}
