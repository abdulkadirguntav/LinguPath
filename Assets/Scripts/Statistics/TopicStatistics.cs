using System;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class TopicStats
{
    public string topicName;
    public int correctAnswers;
    public int wrongAnswers;
    public float successRate; // Yüzde
    public List<string> mistakeTopics = new List<string>(); // Sık yapılan hatalar
    public DateTime lastPracticed;

    public TopicStats(string name)
    {
        topicName = name;
        correctAnswers = 0;
        wrongAnswers = 0;
        successRate = 0f;
        lastPracticed = DateTime.Now;
    }

    public void RecalculateSuccessRate()
    {
        int total = correctAnswers + wrongAnswers;
        successRate = total > 0 ? (correctAnswers * 100f) / total : 0f;
    }

    public string GetPerformanceLevel()
    {
        if (successRate >= 90) return "🌟 Mükemmel";
        if (successRate >= 75) return "✅ İyi";
        if (successRate >= 50) return "⚠️ Orta";
        return "❌ Geliştirilmeli";
    }
}

public class StatisticsData
{
    public Dictionary<string, TopicStats> topicStatsDict = new Dictionary<string, TopicStats>();
    
    // Genel istatistikler
    public int totalCorrectAnswers;
    public int totalWrongAnswers;
    public int totalAttempts;
    public DateTime gameStartDate;

    public StatisticsData()
    {
        totalCorrectAnswers = 0;
        totalWrongAnswers = 0;
        totalAttempts = 0;
        gameStartDate = DateTime.Now;
    }

    public float GetOverallSuccessRate()
    {
        if (totalAttempts == 0) return 0f;
        return (totalCorrectAnswers * 100f) / totalAttempts;
    }

    public void AddToTopic(string topicName, bool isCorrect, string mistakeType = "")
    {
        if (!topicStatsDict.ContainsKey(topicName))
        {
            topicStatsDict[topicName] = new TopicStats(topicName);
        }

        TopicStats stats = topicStatsDict[topicName];

        if (isCorrect)
        {
            stats.correctAnswers++;
            totalCorrectAnswers++;
        }
        else
        {
            stats.wrongAnswers++;
            totalWrongAnswers++;
            
            if (!string.IsNullOrEmpty(mistakeType))
            {
                stats.mistakeTopics.Add(mistakeType);
            }
        }

        totalAttempts++;
        stats.lastPracticed = DateTime.Now;
        stats.RecalculateSuccessRate();
    }

    public TopicStats GetTopicStats(string topicName)
    {
        return topicStatsDict.ContainsKey(topicName) ? topicStatsDict[topicName] : null;
    }

    public List<TopicStats> GetAllTopicsStats()
    {
        return topicStatsDict.Values.OrderByDescending(x => x.successRate).ToList();
    }

    public List<TopicStats> GetWeakestTopics(int count = 5)
    {
        return topicStatsDict.Values
            .OrderBy(x => x.successRate)
            .Take(count)
            .ToList();
    }

    public List<TopicStats> GetBestTopics(int count = 5)
    {
        return topicStatsDict.Values
            .OrderByDescending(x => x.successRate)
            .Take(count)
            .ToList();
    }
}
