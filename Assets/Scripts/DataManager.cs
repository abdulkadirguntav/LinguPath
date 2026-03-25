using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager instance;

    [Header("Player Memory")]

    public List<WordProgress> studentProgressList = new List<WordProgress>();

    private string saveFilePath;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        saveFilePath = Application.persistentDataPath + "/student_progress.json";

        LoadData();
    }

    [System.Serializable]

    private class ProgressWrapper
    {
        public List<WordProgress> wrappedList;
    }

    public void SaveData()
    {
        ProgressWrapper wrapper = new ProgressWrapper { wrappedList = studentProgressList };

        string jsonText = JsonUtility.ToJson(wrapper,true);

        File.WriteAllText(saveFilePath, jsonText);

        Debug.Log(" Save Data " + saveFilePath);
    }

    public void LoadData()
    {
        if(File.Exists(saveFilePath))
        {
            string jsonText = File.ReadAllText(saveFilePath);
            ProgressWrapper wrapper = JsonUtility.FromJson<ProgressWrapper>(jsonText);
            studentProgressList = wrapper.wrappedList;
            Debug.Log(" Save Data Loaded ");
        }
        else
        {
            Debug.Log(" The save file could not be found. A new player profile is being created. ");
            studentProgressList = new List<WordProgress>();
        }
    }

    public WordProgress GetWordProgress(string targetID)
    {
        WordProgress foundProgress = studentProgressList.Find(x => x.wordID == targetID);
    
        if(foundProgress == null)
        {
            foundProgress = new WordProgress(targetID);
            studentProgressList.Add(foundProgress);
        }

        return foundProgress;
    }
}
