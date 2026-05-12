using System;
using System.IO;
using UnityEngine;

[Serializable]
public class CharacterSaveData
{
    public bool isCreated = false;
    public int headIndex = 0;
    public int hairIndex = 0;
    public int topIndex = 0;
    public int bottomIndex = 0;
    public int beardIndex = -1;   // -1 = yok
    public int glassesIndex = -1; // -1 = yok
}

public class CharacterSaveManager : MonoBehaviour
{
    public static CharacterSaveManager instance;

    private CharacterSaveData data;
    private string savePath;

    void Awake()
    {
        if (instance != null) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
        savePath = Application.persistentDataPath + "/character.json";
        Load();
    }

    public CharacterSaveData Data => data;

    public bool HasSavedCharacter() => data != null && data.isCreated;

    public void Save(int head, int hair, int top, int bottom, int beard, int glasses)
    {
        data = new CharacterSaveData
        {
            isCreated = true,
            headIndex = head,
            hairIndex = hair,
            topIndex = top,
            bottomIndex = bottom,
            beardIndex = beard,
            glassesIndex = glasses
        };
        SaveSlotManager.instance?.SaveActiveSlot();
    }

    // Slot yüklendiğinde SaveSlotManager tarafından çağrılır
    public void LoadFromSlot(CharacterSaveData slotData)
    {
        data = slotData ?? new CharacterSaveData();
    }

    public void DeleteSave()
    {
        data = new CharacterSaveData();
        if (File.Exists(savePath))
            File.Delete(savePath);
    }

    private void Load()
    {
        if (File.Exists(savePath))
            data = JsonUtility.FromJson<CharacterSaveData>(File.ReadAllText(savePath));
        else
            data = new CharacterSaveData();
    }
}
