using UnityEngine;

public class CharacterApplier : MonoBehaviour
{
    [System.Serializable]
    public class CharacterSlot
    {
        public string slotName;
        public GameObject[] variations;
    }

    [Header("Slots — Order: 0=Head 1=Hair 2=Top 3=Bottom 4=Beard 5=Glasses")]
    public CharacterSlot[] slots;

    void Start()
    {
        if (CharacterSaveManager.instance == null || !CharacterSaveManager.instance.HasSavedCharacter())
            return;

        CharacterSaveData d = CharacterSaveManager.instance.Data;
        int[] indices = { d.headIndex, d.hairIndex, d.topIndex, d.bottomIndex, d.beardIndex, d.glassesIndex };

        for (int s = 0; s < slots.Length && s < indices.Length; s++)
        {
            for (int i = 0; i < slots[s].variations.Length; i++)
            {
                if (slots[s].variations[i] != null)
                    slots[s].variations[i].SetActive(i == indices[s]);
            }
        }
    }
}
