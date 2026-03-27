using UnityEngine;

[CreateAssetMenu(fileName = "New NPC Dialogue", menuName = "Game/NPC Dialogue Data")]
public class NPCDialogueSO : ScriptableObject
{
    [Header("NPC ID")]
    public string npcName;

    [Header("Dialogue Set")]
    [TextArea(3,5)]
    public string[] preGameDialogues;

    [TextArea(3,5)]
    public string[] winDialogues;

    [TextArea(3,5)]
    public string[] loseDialogues;
}
