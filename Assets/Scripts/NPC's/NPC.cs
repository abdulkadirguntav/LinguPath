using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

[RequireComponent(typeof(SphereCollider))]
public class NPC : MonoBehaviour
{
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
            // Button is active
            if(!isPlayerNear) interactionButton.SetActive(true);
        }    
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            isPlayerNear = false;
            if(!isPlayerNear) interactionButton.SetActive(false);
            EndDialogue();
        }
    }

    public void OnTalkOrNextClicked()
    {
        if(!isPlayerNear) return;

        if(!isPlayerNear)
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

        interactionButton.SetActive(false);
        dialoguePanel.SetActive(true);

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
        dialoguePanel.SetActive(false);
        // If it hasn't disappeared, bring back the “Talk” button
        if(isPlayerNear) interactionButton.SetActive(true);
    }

    private void TriggerMiniGame()
    {
        Debug.Log($"şimdi {dialogueData.npcName} adlı NPC'nin görevi ( mini oyun ) tetiklendi");
    }
}
    