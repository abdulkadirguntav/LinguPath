using UnityEngine;

public class CharacterApplier : MonoBehaviour
{
    [Header("Part Parents — Player Hierarchy")]
    [Tooltip("Parent of head variations")]
    public Transform headParent;
    [Tooltip("Parent of hair variations")]
    public Transform hairParent;
    [Tooltip("Parent of top clothing variations")]
    public Transform topParent;
    [Tooltip("Parent of bottom clothing variations")]
    public Transform bottomParent;
    [Tooltip("Parent of beard variations (optional)")]
    public Transform beardParent;
    [Tooltip("Parent of glasses variations (optional)")]
    public Transform glassesParent;

    void Start()
    {
        if (CharacterSaveManager.instance == null || !CharacterSaveManager.instance.HasSavedCharacter())
            return;

        CharacterSaveData d = CharacterSaveManager.instance.Data;
        ApplyPart(headParent,    d.headIndex);
        ApplyPart(hairParent,    d.hairIndex);
        ApplyPart(topParent,     d.topIndex);
        ApplyPart(bottomParent,  d.bottomIndex);
        ApplyPart(beardParent,   d.beardIndex);
        ApplyPart(glassesParent, d.glassesIndex);
    }

    private void ApplyPart(Transform parent, int index)
    {
        if (parent == null) return;
        for (int i = 0; i < parent.childCount; i++)
            parent.GetChild(i).gameObject.SetActive(i == index);
    }
}
