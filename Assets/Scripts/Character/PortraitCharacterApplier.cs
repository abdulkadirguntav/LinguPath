using UnityEngine;

public class PortraitCharacterApplier : MonoBehaviour
{
    [System.Serializable]
    public class PortraitSlot
    {
        public string slotName;
        public GameObject[] variations;
    }

    [Header("Slotlar — 0=Head 1=Hair 2=Top 3=Bottom 4=Beard 5=Glasses")]
    public PortraitSlot[] slots;

    public void Apply(CharacterSaveData data)
    {
        if (data == null) return;
        SetVariation(0, data.headIndex);
        SetVariation(1, data.hairIndex);
        SetVariation(2, data.topIndex);
        SetVariation(3, data.bottomIndex);
        SetVariation(4, data.beardIndex);
        SetVariation(5, data.glassesIndex);
    }

    private void SetVariation(int slotIndex, int varIndex)
    {
        if (slotIndex >= slots.Length) return;
        GameObject[] variations = slots[slotIndex].variations;
        for (int i = 0; i < variations.Length; i++)
            if (variations[i] != null)
                variations[i].SetActive(i == varIndex);
    }
}
