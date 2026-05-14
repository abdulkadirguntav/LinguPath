using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NPC : MonoBehaviour
{
    public enum NPCState { BeforeMission, MissionFailed, MissionCompleted }
    public NPCState currentState = NPCState.BeforeMission;

    public static NPC ActiveNPC;

    [Header("Mission Setup")]
    public GameObject missionPanelToOpen;
    public GameObject mainGameUI;
    public bool isAIGame = false;

    [Header("NPC Portrait")]
    public Sprite portrait;

    [Header("Data Connection")]
    public NPCDialogueSO dialogueData;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    private static readonly int WaveHash = Animator.StringToHash("Wave");

    [Header("UI Connection")]
    [SerializeField] private float SphereRadius = 4f;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Button nextLineButton;
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image portraitImage;

    [Header("Player Reference (opsiyonel — boş bırakılırsa otomatik bulunur)")]
    [SerializeField] private Transform playerOverride;

    private bool isPlayerNear = false;
    private Transform playerTransform;

    private int currentLineIndex = 0;
    private bool isDialogActive = false;
    private string[] currentDialogueLines;

    void Start()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        playerTransform = playerOverride != null ? playerOverride : FindPlayer();
        if (playerTransform == null)
            Debug.LogWarning($"[NPC] '{gameObject.name}': Player bulunamadı! Inspector'dan 'Player Override' alanına player'ı sürükleyin.");

        // Kayıtlı durumu yükle
        if (SaveSlotManager.instance != null)
            currentState = (NPCState)SaveSlotManager.instance.GetNpcState(gameObject.name);
    }

    private Transform FindPlayer()
    {
        GameObject obj = GameObject.FindWithTag("Player");
        if (obj == null)
        {
            PlayerMovement pm = FindAnyObjectByType<PlayerMovement>();
            if (pm != null) obj = pm.gameObject;
        }
        return obj != null ? obj.transform : null;
    }

    void Update()
    {
        if (playerTransform == null)
        {
            playerTransform = FindPlayer();
            return;
        }

        float dist = Vector3.Distance(transform.position, playerTransform.position);
        bool near = dist <= SphereRadius;

        if (near && !isPlayerNear)
            OnPlayerEntered();
        else if (!near && isPlayerNear)
            OnPlayerExited();
    }

    private void OnPlayerEntered()
    {
        isPlayerNear = true;
        ActiveNPC = this;

        if (animator != null) animator.SetTrigger(WaveHash);

        if (dialogueData == null) return;

        if (currentState == NPCState.BeforeMission || currentState == NPCState.MissionFailed)
            StartDialogue(dialogueData.preGameDialogues);
        else if (currentState == NPCState.MissionCompleted)
            StartDialogue(dialogueData.winDialogues);
    }

    private void OnPlayerExited()
    {
        isPlayerNear = false;
        if (ActiveNPC == this) ActiveNPC = null;
        EndDialogue();
    }

    private void StartDialogue(string[] linesToPlay)
    {
        if (linesToPlay == null || linesToPlay.Length == 0) return;

        isDialogActive = true;
        currentDialogueLines = linesToPlay;
        currentLineIndex = 0;

        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (nameText != null) nameText.text = dialogueData.npcName;
        if (portraitImage != null) portraitImage.sprite = portrait;

        if (nextLineButton != null)
        {
            nextLineButton.onClick.RemoveAllListeners();
            nextLineButton.onClick.AddListener(DisplayNextLine);
        }

        DisplayNextLine();
    }

    public void DisplayNextLine()
    {
        if (!isDialogActive) return;

        if (currentLineIndex >= currentDialogueLines.Length)
        {
            EndDialogue();
            if (currentState == NPCState.BeforeMission || currentState == NPCState.MissionFailed)
                StartMission();
            return;
        }

        dialogText.text = currentDialogueLines[currentLineIndex];
        currentLineIndex++;
    }

    private void EndDialogue()
    {
        isDialogActive = false;
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    private void StartMission()
    {
        if (mainGameUI != null) mainGameUI.SetActive(false);
        if (missionPanelToOpen != null) missionPanelToOpen.SetActive(true);
    }

    public void FinishMission(bool isWin)
    {
        if (dialogueData == null) return;

        if (isWin)
        {
            currentState = NPCState.MissionCompleted;
            SaveSlotManager.instance?.SaveNpcState(gameObject.name, (int)currentState);
            StartDialogue(dialogueData.winDialogues);

            if (GameCompletionManager.instance != null)
                GameCompletionManager.instance.CheckCompletion();
        }
        else
        {
            currentState = NPCState.MissionFailed;
            SaveSlotManager.instance?.SaveNpcState(gameObject.name, (int)currentState);
            StartDialogue(dialogueData.loseDialogues);
        }
    }
}
