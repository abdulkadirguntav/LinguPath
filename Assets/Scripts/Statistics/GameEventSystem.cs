using UnityEngine;
using System;

/// <summary>
/// Merkezi event sistemi - Tüm oyun modlarından cevapları topla
/// Kullanım: GameEventSystem.OnAnswerGiven?.Invoke("Konuşma", true, "");
/// </summary>
public static class GameEventSystem
{
    // ✅ Oyuncu bir cevap verdi (topic, isCorrect, mistakeDetails)
    public static Action<string, bool, string> OnAnswerGiven;

    // 🎯 Belirli bir konu çalışıldı
    public static Action<string> OnTopicPracticed;

    // ⭐ Oyun bitti (oyun adı, kazandı mı, skor)
    public static Action<string, bool, int> OnGameEnded;

    // 🏆 Streak başladı/kırıldı
    public static Action<int> OnStreakChanged;

    /// <summary>
    /// Cevap kaydı. Her yerden çağırabilirsin!
    /// Örnek: GameEventSystem.LogAnswer("Fiil Zamanı", true, "");
    /// </summary>
    public static void LogAnswer(string topic, bool isCorrect, string mistakeDetails = "")
    {
        OnAnswerGiven?.Invoke(topic, isCorrect, mistakeDetails);
        Debug.Log($"📊 Cevap Kaydedildi: {topic} | {(isCorrect ? "✅" : "❌")}");
    }

    /// <summary>
    /// Oyun bitiş kaydı
    /// Örnek: GameEventSystem.LogGameEnd("Savaş", true, 150);
    /// </summary>
    public static void LogGameEnd(string gameName, bool playerWon, int score)
    {
        OnGameEnded?.Invoke(gameName, playerWon, score);
        Debug.Log($"🎮 Oyun Bitti: {gameName} | {(playerWon ? "Kazandı" : "Kaybetti")} | Skor: {score}");
    }

    /// <summary>
    /// Konu çalışıldı
    /// </summary>
    public static void LogTopicPractice(string topic)
    {
        OnTopicPracticed?.Invoke(topic);
    }

    /// <summary>
    /// Streak güncelleme
    /// </summary>
    public static void LogStreakUpdate(int currentStreak)
    {
        OnStreakChanged?.Invoke(currentStreak);
    }
}
