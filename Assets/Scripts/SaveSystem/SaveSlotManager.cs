using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class NpcSaveEntry
{
    public string npcId;
    public int state; // 0=BeforeMission 1=MissionFailed 2=MissionCompleted
}

[System.Serializable]
public class SlotSaveData
{
    public bool isEmpty = true;
    public string playerName = "";
    public CharacterSaveData characterData = new CharacterSaveData();
    public List<WordProgress> wordProgress = new List<WordProgress>();
    public List<NpcSaveEntry> npcStates = new List<NpcSaveEntry>();
}

public class SaveSlotManager : MonoBehaviour
{
    public static SaveSlotManager instance;
    public static int ActiveSlot = -1;

    private SlotSaveData[] slots = new SlotSaveData[3];

    void Awake()
    {
        if (instance != null) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
        LoadAllSlots();
    }

    private string SlotPath(int index) =>
        Path.Combine(Application.persistentDataPath, $"slot_{index}.json");

    private void LoadAllSlots()
    {
        for (int i = 0; i < 3; i++)
        {
            string path = SlotPath(i);
            slots[i] = File.Exists(path)
                ? JsonUtility.FromJson<SlotSaveData>(File.ReadAllText(path))
                : new SlotSaveData();
        }
    }

    public SlotSaveData GetSlot(int index) => slots[index];

    // Ana menüden slot seçildiğinde — veriyi yöneticilere enjekte eder
    public void SelectSlot(int index)
    {
        ActiveSlot = index;
        SlotSaveData slot = slots[index];
        CharacterSaveManager.instance?.LoadFromSlot(slot.characterData);
        DataManager.instance?.LoadFromSlot(slot.wordProgress);
    }

    // Boş slot için yeni kayıt oluşturur
    public void CreateSlot(int index, string playerName)
    {
        slots[index] = new SlotSaveData { isEmpty = false, playerName = playerName };
        ActiveSlot = index;
        WriteSlot(index);

        CharacterSaveManager.instance?.LoadFromSlot(slots[index].characterData);
        DataManager.instance?.LoadFromSlot(slots[index].wordProgress);
    }

    // Tüm aktif veriyi diske yazar (CharacterSaveManager ve DataManager'dan çeker)
    public void SaveActiveSlot()
    {
        if (ActiveSlot < 0) return;
        SlotSaveData slot = slots[ActiveSlot];

        if (CharacterSaveManager.instance != null)
            slot.characterData = CharacterSaveManager.instance.Data;

        if (DataManager.instance != null)
            slot.wordProgress = DataManager.instance.studentProgressList;

        WriteSlot(ActiveSlot);
    }

    // NPC durumu değiştiğinde anında yazar
    public void SaveNpcState(string npcId, int state)
    {
        if (ActiveSlot < 0) return;
        SlotSaveData slot = slots[ActiveSlot];

        NpcSaveEntry entry = slot.npcStates.Find(e => e.npcId == npcId);
        if (entry != null)
            entry.state = state;
        else
            slot.npcStates.Add(new NpcSaveEntry { npcId = npcId, state = state });

        WriteSlot(ActiveSlot);
    }

    public int GetNpcState(string npcId)
    {
        if (ActiveSlot < 0) return 0;
        NpcSaveEntry entry = slots[ActiveSlot].npcStates.Find(e => e.npcId == npcId);
        return entry?.state ?? 0;
    }

    public int GetCompletedMissionCount(int slotIndex)
    {
        return slots[slotIndex].npcStates.FindAll(e => e.state == 2).Count;
    }

    public void DeleteSlot(int index)
    {
        slots[index] = new SlotSaveData();
        string path = SlotPath(index);
        if (File.Exists(path)) File.Delete(path);
    }

    private void WriteSlot(int index)
    {
        File.WriteAllText(SlotPath(index), JsonUtility.ToJson(slots[index], true));
    }
}
