using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class CharacterCreatorManager : MonoBehaviour
{
    [System.Serializable]
    public class CharacterSlot
    {
        public string slotName;
        public GameObject[] variations;   // Drag each variation directly here
        public TextMeshProUGUI countLabel;
        public bool isOptional;
        [HideInInspector] public int currentIndex;
    }

    [Header("Slots — Order: 0=Head 1=Hair 2=Top 3=Bottom 4=Beard 5=Glasses")]
    public CharacterSlot[] slots;

    [Header("Panel References")]
    public GameObject creatorPanel;
    public GameObject mainMenuPanel;
    public GameObject slotSelectionPanel;

    void OnEnable()
    {
        InitSlots();
        RefreshAll();
    }

    private void InitSlots()
    {
        for (int i = 0; i < slots.Length; i++)
            slots[i].currentIndex = slots[i].isOptional ? -1 : 0;

        if (CharacterSaveManager.instance != null && CharacterSaveManager.instance.HasSavedCharacter())
            LoadFromSave();
    }

    private void LoadFromSave()
    {
        CharacterSaveData d = CharacterSaveManager.instance.Data;
        if (slots.Length > 0) slots[0].currentIndex = d.headIndex;
        if (slots.Length > 1) slots[1].currentIndex = d.hairIndex;
        if (slots.Length > 2) slots[2].currentIndex = d.topIndex;
        if (slots.Length > 3) slots[3].currentIndex = d.bottomIndex;
        if (slots.Length > 4) slots[4].currentIndex = d.beardIndex;
        if (slots.Length > 5) slots[5].currentIndex = d.glassesIndex;
    }

    public void Next(int slotIndex)
    {
        if (!IsValid(slotIndex)) return;
        var slot = slots[slotIndex];
        int min = slot.isOptional ? -1 : 0;

        slot.currentIndex++;
        if (slot.currentIndex >= slot.variations.Length) slot.currentIndex = min;

        ApplyAndUpdate(slot);
    }

    public void Prev(int slotIndex)
    {
        if (!IsValid(slotIndex)) return;
        var slot = slots[slotIndex];
        int min = slot.isOptional ? -1 : 0;

        slot.currentIndex--;
        if (slot.currentIndex < min) slot.currentIndex = slot.variations.Length - 1;

        ApplyAndUpdate(slot);
    }

    public void Remove(int slotIndex)
    {
        if (!IsValid(slotIndex)) return;
        var slot = slots[slotIndex];
        if (!slot.isOptional) return;

        slot.currentIndex = -1;
        ApplyAndUpdate(slot);
    }

    public void SaveAndPlay()
    {
        if (CharacterSaveManager.instance == null) return;

        CharacterSaveManager.instance.Save(
            head:    GetIndex(0),
            hair:    GetIndex(1),
            top:     GetIndex(2),
            bottom:  GetIndex(3),
            beard:   GetIndex(4),
            glasses: GetIndex(5)
        );

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void BackToMenu()
    {
        if (creatorPanel != null) creatorPanel.SetActive(false);
        GameObject target = slotSelectionPanel != null ? slotSelectionPanel : mainMenuPanel;
        if (target != null) target.SetActive(true);
    }

    private void ApplyAndUpdate(CharacterSlot slot)
    {
        for (int i = 0; i < slot.variations.Length; i++)
        {
            if (slot.variations[i] != null)
                slot.variations[i].SetActive(i == slot.currentIndex);
        }

        if (slot.countLabel != null)
        {
            if (slot.currentIndex == -1)
                slot.countLabel.text = "NONE";
            else
                slot.countLabel.text = $"{slot.currentIndex + 1}/{slot.variations.Length}";
        }
    }

    private void RefreshAll()
    {
        foreach (var slot in slots)
            ApplyAndUpdate(slot);
    }

    private bool IsValid(int index) =>
        index >= 0 && index < slots.Length && slots[index].variations != null && slots[index].variations.Length > 0;

    private int GetIndex(int slotIndex) =>
        slotIndex < slots.Length ? slots[slotIndex].currentIndex : (slotIndex >= 4 ? -1 : 0);
}
