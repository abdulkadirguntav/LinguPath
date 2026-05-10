using UnityEngine;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(SphereCollider))]
public class NPC : MonoBehaviour
{
    public enum NPCState { BeforeMission, MissionFailed, MissionCompleted }
    public NPCState currentState = NPCState.BeforeMission;

    public static NPC ActiveNPC;

    [Header("Mission Setup")]
    public GameObject missionPanelToOpen;
    public GameObject mainGameUI;
    public bool isAIGame = false;

    [Header("Mission Confirmation PopUp")]
    public GameObject missionPopupPanel;
    public Button acceptMissionButton;
    public Button declineMissionButton;

    [Header("Data Connection")]
    public NPCDialogueSO dialogueData;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    private static readonly int WaveHash = Animator.StringToHash("Wave");

    [Header("UI Connection")]
    [SerializeField] private float SphereRadius = 4f;
    [SerializeField] private GameObject interactionButton;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private TextMeshProUGUI nameText;

    private SphereCollider sphereCollider;
    private bool isPlayerNear = false;

    private int currentLineIndex = 0;
    private bool isDialogActive = false;
    private string[] currentDialogueLines;

    void Start()
    {
        sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.radius = SphereRadius;
        sphereCollider.isTrigger = true;

        if (interactionButton != null) interactionButton.SetActive(false);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            ActiveNPC = this;

            if (interactionButton != null)
            {
                Button btn = interactionButton.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(OnTalkOrNextClicked);
                }
            }

            if (dialoguePanel != null)
            {
                Button panelBtn = dialoguePanel.GetComponent<Button>();
                if (panelBtn != null)
                {
                    panelBtn.onClick.RemoveAllListeners();
                    panelBtn.onClick.AddListener(OnTalkOrNextClicked);
                }
            }

            if (!isDialogActive && interactionButton != null) interactionButton.SetActive(true);

            if (animator != null) animator.SetTrigger(WaveHash);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            if (interactionButton != null) interactionButton.SetActive(false);
            if (ActiveNPC == this) ActiveNPC = null;
            EndDialogue();
        }
    }

    public void OnTalkOrNextClicked()
    {
        if (!isPlayerNear) return;
        if (dialogueData == null) { Debug.LogError("ERROR: NPC dialogue data not assigned!"); return; }

        if (!isDialogActive)
        {
            if (currentState == NPCState.BeforeMission || currentState == NPCState.MissionFailed)
                StartDialogue(dialogueData.preGameDialogues);
            else if (currentState == NPCState.MissionCompleted)
                StartDialogue(dialogueData.winDialogues);
        }
        else
        {
            DisplayNextLine();
        }
    }

    private void StartDialogue(string[] linesToPlay)
    {
        if (linesToPlay == null || linesToPlay.Length == 0) return;

        isDialogActive = true;
        currentDialogueLines = linesToPlay;
        currentLineIndex = 0;

        if (interactionButton != null) interactionButton.SetActive(false);
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (nameText != null) nameText.text = dialogueData.npcName;

        DisplayNextLine();
    }

    private void DisplayNextLine()
    {
        if (currentLineIndex >= currentDialogueLines.Length)
        {
            EndDialogue();
            if (currentState == NPCState.BeforeMission || currentState == NPCState.MissionFailed)
                TriggerMiniGame();
            return;
        }

        dialogText.text = currentDialogueLines[currentLineIndex];
        currentLineIndex++;
    }

    private void EndDialogue()
    {
        isDialogActive = false;
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (isPlayerNear && interactionButton != null) interactionButton.SetActive(true);
    }

    private void TriggerMiniGame()
    {
        if (missionPopupPanel != null && acceptMissionButton != null && declineMissionButton != null)
        {
            missionPopupPanel.SetActive(true);

            acceptMissionButton.onClick.RemoveAllListeners();
            acceptMissionButton.onClick.AddListener(AcceptMission);

            declineMissionButton.onClick.RemoveAllListeners();
            declineMissionButton.onClick.AddListener(DeclineMission);
        }
        else
        {
            AcceptMission();
        }
    }

    private void AcceptMission()
    {
        if (missionPopupPanel != null) missionPopupPanel.SetActive(false);
        if (mainGameUI != null) mainGameUI.SetActive(false);
        if (missionPanelToOpen != null) missionPanelToOpen.SetActive(true);
    }

    private void DeclineMission()
    {
        if (missionPopupPanel != null) missionPopupPanel.SetActive(false);
    }

    public void FinishMission(bool isWin)
    {
        if (isWin)
        {
            currentState = NPCState.MissionCompleted;
            StartDialogue(dialogueData.winDialogues);

            if (GameCompletionManager.instance != null)
                GameCompletionManager.instance.CheckCompletion();
        }
        else
        {
            currentState = NPCState.MissionFailed;
            StartDialogue(dialogueData.loseDialogues);
        }
    }
}
