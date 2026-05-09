using UnityEngine;

public class GameCompletionManager : MonoBehaviour
{
    public static GameCompletionManager instance;

    [SerializeField] private GameObject congratsPanel;

    void Awake()
    {
        instance = this;
    }

    public void CheckCompletion()
    {
        NPC[] allNPCs = FindObjectsOfType<NPC>();

        foreach (NPC npc in allNPCs)
        {
            if (!npc.isAIGame && npc.currentState != NPC.NPCState.MissionCompleted)
                return;
        }

        if (congratsPanel != null)
            congratsPanel.SetActive(true);
    }
}
