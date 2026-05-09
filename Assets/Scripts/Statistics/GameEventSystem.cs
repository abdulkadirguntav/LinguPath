using UnityEngine;
using System;

public static class GameEventSystem
{
    public static Action<string, bool, string> OnAnswerGiven;
    public static Action<string> OnTopicPracticed;
    public static Action<string, bool, int> OnGameEnded;
    public static Action<int> OnStreakChanged;

    public static void LogAnswer(string topic, bool isCorrect, string mistakeDetails = "")
    {
        OnAnswerGiven?.Invoke(topic, isCorrect, mistakeDetails);
        Debug.Log($"Answer Logged: {topic} | {(isCorrect ? "Correct" : "Wrong")}");
    }

    public static void LogGameEnd(string gameName, bool playerWon, int score)
    {
        OnGameEnded?.Invoke(gameName, playerWon, score);
        Debug.Log($"Game Ended: {gameName} | {(playerWon ? "Won" : "Lost")} | Score: {score}");
    }

    public static void LogTopicPractice(string topic)
    {
        OnTopicPracticed?.Invoke(topic);
    }

    public static void LogStreakUpdate(int currentStreak)
    {
        OnStreakChanged?.Invoke(currentStreak);
    }
}
