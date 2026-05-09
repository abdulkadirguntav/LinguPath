using UnityEngine;
using System.IO;

public static class DataLogger
{
    private static string filePath = Application.persistentDataPath + "/PlayerData.csv";

    public static void LogData(string category, string eventType, string detail)
    {
        if (!File.Exists(filePath))
        {
            string header = "Category,Event,Detail\n";
            File.WriteAllText(filePath, header);
        }

        string newRow = $"{category},{eventType},\"{detail}\"\n";
        File.AppendAllText(filePath, newRow);

        Debug.Log("Background data logged: " + newRow);
    }
}
