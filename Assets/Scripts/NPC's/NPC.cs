using UnityEngine;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(SphereCollider))]
public class NPC : MonoBehaviour
{
    [Header("Mission Setup")]
    public GameObject missionPanelToOpen;
    public GameObject mainGameUI;
    [Header("Data Connection")]
    public NPCDialogueSO dialogueData;

    [Header("UI Connection")]
    [SerializeField] private float SphereRadius = 4f;
    [SerializeField] private GameObject interactionButton;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private TextMeshProUGUI nameText;

    private SphereCollider sphereCollider;
    private bool isPlayerNear = false;

    // Dialogue Flow System
    private int currentLineIndex = 0;
    private bool isDialogActive = false;
    private string[] currentDialogueLines;
    
    void Start()
    {
        sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.radius = SphereRadius;
        sphereCollider.isTrigger = true;
        
        // UI cleanup at game startup
        if(interactionButton != null) interactionButton.SetActive(false);
        if(dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    private void OnTriggerEnter(Collider other) 
    {
        if(other.CompareTag("Player"))
        {
            isPlayerNear = true;
            
            // --- YENİ EKLENEN SİHİR: Dinamik Buton Bağlama ---
            // Oyuncu hangi NPC'nin yanına geldiyse, butonları kodla o NPC'ye bağlıyoruz!
            if (interactionButton != null)
            {
                Button btn = interactionButton.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners(); // Önceki NPC'yi hafızadan sil
                    btn.onClick.AddListener(OnTalkOrNextClicked); // Beni dinle!
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
            // --------------------------------------------------

            // Eğer diyalog halihazırda açık değilse butonu göster
            if(!isDialogActive && interactionButton != null) interactionButton.SetActive(true);
        }    
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            isPlayerNear = false;
            if(interactionButton != null) interactionButton.SetActive(false);
            EndDialogue();
        }
    }

    public void OnTalkOrNextClicked()
    {
        if(!isPlayerNear) return;

        if (dialogueData == null) { Debug.LogError("HATA: NPC içine Dialogue Data (SO) atılmamış!"); return; }
        if (dialogueData.preGameDialogues == null || dialogueData.preGameDialogues.Length == 0) { Debug.LogError("HATA: SO dosyasının içi BOŞ! Yazı yazılmamış."); return; }
        if (dialogText == null) { Debug.LogError("HATA: Dialog Text objesi sürüklenmemiş!"); return; }

        // DÜZELTİLEN KISIM: Oyuncu yakın mı değil mi diye değil, diyalog başladı mı başlamadı mı diye bakıyoruz!
        if(!isDialogActive)
        {
            StartDialogue(dialogueData.preGameDialogues);
        }
        else
        {
            DisplayNextLine();
        }
    }

    private void StartDialogue(string[] linesToPlay)
    {
        isDialogActive = true;
        currentDialogueLines = linesToPlay;
        currentLineIndex = 0;

        if(interactionButton != null) interactionButton.SetActive(false);
        if(dialoguePanel != null) dialoguePanel.SetActive(true);

        if(nameText != null) nameText.text = dialogueData.npcName;

        DisplayNextLine();
    }

    private void DisplayNextLine()
    {
        if(currentLineIndex >= currentDialogueLines.Length)
        {
            EndDialogue();
            TriggerMiniGame();
            return;
        }

        dialogText.text = currentDialogueLines[currentLineIndex];
        currentLineIndex++;
    }

    private void EndDialogue()
    {
        isDialogActive = false;
        if(dialoguePanel != null) dialoguePanel.SetActive(false);
        
        // Uzaklaşmadıysa butonu geri getir
        if(isPlayerNear && interactionButton != null) interactionButton.SetActive(true);
    }

    private void TriggerMiniGame()
    {
        Debug.Log($"şimdi {dialogueData.npcName} adlı NPC'nin görevi ( mini oyun ) tetiklendi");

        if(mainGameUI != null) mainGameUI.SetActive(false);

        if(missionPanelToOpen != null) missionPanelToOpen.SetActive(true);
    }
}