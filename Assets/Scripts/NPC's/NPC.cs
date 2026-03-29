using UnityEngine;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(SphereCollider))]
public class NPC : MonoBehaviour
{
    // NPC'nin Hangi Aşamada Olduğunu Tutan Hafıza Sistemi
    public enum NPCState { BeforeMission, MissionCompleted }
    public NPCState currentState = NPCState.BeforeMission;

    // O an konuştuğumuz NPC'yi her yerden bulabilmek için pratik bir köprü
    public static NPC ActiveNPC; 

    [Header("Mission Setup")]
    public GameObject missionPanelToOpen;
    public GameObject mainGameUI;

    [Header("Mission Confirmation PopUp")]
    public GameObject missionPopupPanel;
    public Button acceptMissionButton;
    public Button declineMissionButton;
    
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
        
        if(interactionButton != null) interactionButton.SetActive(false);
        if(dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    private void OnTriggerEnter(Collider other) 
    {
        if(other.CompareTag("Player"))
        {
            isPlayerNear = true;
            ActiveNPC = this; // Oyuncu yanıma geldiğinde "Aktif NPC Benim!" diye bağır
            
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

            if(!isDialogActive && interactionButton != null) interactionButton.SetActive(true);
        }    
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            isPlayerNear = false;
            if(interactionButton != null) interactionButton.SetActive(false);
            
            // Eğer yanımdan uzaklaşırsa aktifliği bırak
            if(ActiveNPC == this) ActiveNPC = null; 
            
            EndDialogue();
        }
    }

    public void OnTalkOrNextClicked()
    {
        if(!isPlayerNear) return;

        if (dialogueData == null) { Debug.LogError("HATA: SO Atılmamış!"); return; }

        if(!isDialogActive)
        {
            // HAFIZA KONTROLÜ: Görev bittiyse winDialogues, bitmediyse preGameDialogues oku!
            if (currentState == NPCState.BeforeMission)
            {
                StartDialogue(dialogueData.preGameDialogues);
            }
            else if (currentState == NPCState.MissionCompleted)
            {
                StartDialogue(dialogueData.winDialogues);
            }
        }
        else
        {
            DisplayNextLine();
        }
    }

    private void StartDialogue(string[] linesToPlay)
    {
        // Eğer o diyaloğun içi boşsa hata vermesin, direkt kapatsın
        if(linesToPlay == null || linesToPlay.Length == 0) return;

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
            
            // GÜVENLİK KİLİDİ: Görev panelini sadece "Görevi Almadan Önceki" diyalog bitince aç!
            // Teşekkür diyaloğu bitince tekrar oyunu açmasını engelliyoruz.
            if(currentState == NPCState.BeforeMission)
            {
                TriggerMiniGame();
            }
            return;
        }

        dialogText.text = currentDialogueLines[currentLineIndex];
        currentLineIndex++;
    }

    private void EndDialogue()
    {
        isDialogActive = false;
        if(dialoguePanel != null) dialoguePanel.SetActive(false);
        if(isPlayerNear && interactionButton != null) interactionButton.SetActive(true);
    }

    private void TriggerMiniGame()
    {
        Debug.Log($"Şimdi {dialogueData.npcName} adlı NPC'nin görevi için Pop-up açılıyor.");

        // Pop-up arayüzü Inspector'dan bağlandıysa onu aç, bağlanmadıysa direkt oyuna gir
        if (missionPopupPanel != null && acceptMissionButton != null && declineMissionButton != null)
        {
            missionPopupPanel.SetActive(true);

            // Pop-up içindeki Evet/Hayır butonlarının eski hafızasını sil ve bu NPC'ye bağla
            acceptMissionButton.onClick.RemoveAllListeners();
            acceptMissionButton.onClick.AddListener(AcceptMission);

            declineMissionButton.onClick.RemoveAllListeners();
            declineMissionButton.onClick.AddListener(DeclineMission);
        }
        else
        {
            // Eğer arayüz bağlamayı unutursak oyun çökmesin, eski sistem direkt başlasın
            AcceptMission();
        }
    }

    // Oyuncu Pop-up'ta "Evet" derse burası çalışır
    private void AcceptMission()
    {
        if (missionPopupPanel != null) missionPopupPanel.SetActive(false); // Pop-up'ı kapat
        if (mainGameUI != null) mainGameUI.SetActive(false);               // Joystick'i gizle
        if (missionPanelToOpen != null) missionPanelToOpen.SetActive(true);// Mini oyunu (Market vs.) aç!
    }

    // Oyuncu Pop-up'ta "Hayır" derse burası çalışır
    private void DeclineMission()
    {
        if (missionPopupPanel != null) missionPopupPanel.SetActive(false); // Sadece Pop-up'ı kapat
        Debug.Log("Oyuncu görevi reddetti. Kasabada yürümeye devam edebilir.");
        // Başka hiçbir şeyi kapatmıyoruz, oyuncu serbest kalıyor. İsterse teyzeye tekrar tıklayabilir.
    }

    // Dışarıdan (Örneğin MarketManager'dan) çağrılacak Sihirli Fonksiyon
    public void FinishMission(bool isWin)
    {
        if(isWin)
        {
            currentState = NPCState.MissionCompleted; // Hafızayı "Tamamlandı" olarak değiştir
            StartDialogue(dialogueData.winDialogues); // Teşekkür diyaloğunu başlat
        }
        else
        {
            StartDialogue(dialogueData.loseDialogues); // (Şu an kullanmıyoruz ama ilerisi için hazır)
        }
    }
}