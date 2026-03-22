using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(SphereCollider))]
public class NPC : MonoBehaviour
{
    [SerializeField] private float SphereRadius = 4f;
    [SerializeField] private GameObject interactionUI; 
    [SerializeField] private Button talkButton;
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private Image dialogBackground;
    [SerializeField] private float dialogDisplayDuration = 3f;

    private SphereCollider sphereCollider;
    private bool isPlayerNear = false;
    private float dialogTimer = 0f;
    private bool isDialogActive = false;

    // NPC Diyalog Seçenekleri
    private string[] dialogues = new string[]
    {
        "Merhaba! Ben bir NPC'yim.",
        "Oyunda kaybolan nesneleri bulmamda bana yardım edebilir misin?",
        "Çok teşekkürler! Başarın için iyi şanslar!"
    };

    private int currentDialogIndex = 0;

    void Start()
    {
        sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.radius = SphereRadius;
        sphereCollider.isTrigger = true;
        
        interactionUI.SetActive(false);
        
        if (talkButton != null)
        {
            talkButton.onClick.AddListener(OnTalkButtonClick);
        }

        if (dialogText != null)
        {
            dialogText.gameObject.SetActive(false);
        }

        if (dialogBackground != null)
        {
            dialogBackground.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (isDialogActive && dialogTimer > 0)
        {
            dialogTimer -= Time.deltaTime;
            if (dialogTimer <= 0)
            {
                HideDialog();
            }
        }

        // Mobil Dokunma Desteği - Herhangi bir yerden tıklayarak dialog devam ettir
        #if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0 && isPlayerNear && isDialogActive)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                // Dialog otomatik devam edecek
            }
        }
        #endif
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Tilki NPC ile etkileşime girdi!");
            isPlayerNear = true;
            interactionUI.SetActive(true);
            currentDialogIndex = 0;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Tilki NPC'den ayrıldı!");
            isPlayerNear = false;
            interactionUI.SetActive(false);
            HideDialog();
        }
    }

    void OnTalkButtonClick()
    {
        if (isPlayerNear)
        {
            ShowDialog(dialogues[currentDialogIndex]);
            currentDialogIndex++;
            
            if (currentDialogIndex >= dialogues.Length)
            {
                currentDialogIndex = 0;
            }
        }
    }

    void ShowDialog(string text)
    {
        isDialogActive = true;
        dialogTimer = dialogDisplayDuration;

        if (dialogText != null)
        {
            dialogText.text = text;
            dialogText.gameObject.SetActive(true);
        }

        if (dialogBackground != null)
        {
            dialogBackground.gameObject.SetActive(true);
        }

        Debug.Log("Dialog: " + text);
    }

    void HideDialog()
    {
        isDialogActive = false;
        dialogTimer = 0f;

        if (dialogText != null)
        {
            dialogText.gameObject.SetActive(false);
        }

        if (dialogBackground != null)
        {
            dialogBackground.gameObject.SetActive(false);
        }
    }
}
