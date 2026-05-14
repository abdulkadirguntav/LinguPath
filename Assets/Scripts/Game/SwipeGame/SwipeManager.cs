using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class SwipeCardData
{
    public int id;
    public string imageName;
    public string correctSentence;
    public string wrongSentence;
    public Sprite cardSprite;
}

public class SwipeManager : MonoBehaviour
{
    [Header("Database (Auto-filled)")]
    public List<SwipeCardData> allCards = new List<SwipeCardData>();

    [Header("UI References")]
    public TMP_Text targetSentencesText;
    public Image activeCardImage;
    public List<GameObject> heartIcons = new List<GameObject>();

    [Header("Card Sprites (CSV sırasıyla)")]
    public List<Sprite> cardSprites = new List<Sprite>();

    [Header("Core Loop Connections")]
    public GameObject swipePanel;
    public GameObject mainGameUI;

    [Header("Physics Memory")]
    Vector3 initialPosition;
    Quaternion initialRotation;

    [Header("Check & Player Data")]
    public int maxCards = 7;
    bool isCurrentSentenceTrue;
    int currentHealth = 3;
    int currentCardIndex = 0;

    void Start()
    {
        ResetGame();
        LoadSwipeData();
        ShuffleCards();
        LoadNextCard();
    }

    public void ResetGame()
    {
        initialPosition = activeCardImage.transform.localPosition;
        initialRotation = activeCardImage.transform.localRotation;
        currentHealth = heartIcons.Count;
        currentCardIndex = 0;

        foreach (GameObject heart in heartIcons)
            if (heart != null) heart.SetActive(true);
    }

    private void LoadSwipeData()
    {
        TextAsset csvData = Resources.Load<TextAsset>("SwipeCards");
        if (csvData == null) { Debug.LogError("SwipeCards.csv not found!"); return; }

        string[] dataLines = csvData.text.Split(new char[] { '\n' });
        for (int i = 1; i < dataLines.Length; i++)
        {
            string line = dataLines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] columns = line.Split(';');
            if (columns.Length >= 4)
            {
                SwipeCardData newCard = new SwipeCardData();
                newCard.id = int.Parse(columns[0]);
                newCard.imageName = columns[1].Trim();
                newCard.correctSentence = columns[2].Trim();
                newCard.wrongSentence = columns[3].Trim();
                allCards.Add(newCard);
            }
        }
        for (int i = 0; i < allCards.Count; i++)
        {
            if (i < cardSprites.Count)
                allCards[i].cardSprite = cardSprites[i];
        }

        Debug.Log("Swipe database loaded! Total cards: " + allCards.Count);
    }

    private void ShuffleCards()
    {
        for (int i = 0; i < allCards.Count; i++)
        {
            int randomIndex = Random.Range(0, allCards.Count);
            SwipeCardData temp = allCards[i];
            allCards[i] = allCards[randomIndex];
            allCards[randomIndex] = temp;
        }
    }

    void LoadNextCard()
    {
        if (currentCardIndex >= maxCards || currentCardIndex >= allCards.Count)
        {
            Debug.Log("All cards done, you win!");
            EndGame(true);
            return;
        }

        SwipeCardData currentCard = allCards[currentCardIndex];

        if (currentCard.cardSprite != null)
            activeCardImage.sprite = currentCard.cardSprite;
        else
            Debug.LogWarning(currentCard.imageName + " image not found!");

        int randomChoice = Random.Range(0, 2);
        if (randomChoice == 0)
        {
            targetSentencesText.text = currentCard.correctSentence;
            isCurrentSentenceTrue = true;
        }
        else
        {
            targetSentencesText.text = currentCard.wrongSentence;
            isCurrentSentenceTrue = false;
        }

        activeCardImage.transform.localPosition = initialPosition;
        activeCardImage.transform.localRotation = initialRotation;
    }

    public void CardSwiped(bool isSwipedRight)
    {
        if (isSwipedRight == isCurrentSentenceTrue)
        {
            Debug.Log("Correct!");
        }
        else
        {
            Debug.Log("Wrong!");
            currentHealth--;

            if (currentHealth >= 0 && currentHealth < heartIcons.Count)
                heartIcons[currentHealth].SetActive(false);

            if (currentHealth <= 0)
            {
                Debug.Log("Game Over! No lives remaining.");
                EndGame(false);
                return;
            }
        }

        currentCardIndex++;
        LoadNextCard();
    }

    private void EndGame(bool isWin)
    {
        if (swipePanel != null) swipePanel.SetActive(false);
        if (mainGameUI != null) mainGameUI.SetActive(true);

        if (NPC.ActiveNPC != null)
            NPC.ActiveNPC.FinishMission(isWin);

        currentCardIndex = 0;
        currentHealth = heartIcons.Count;
        foreach (GameObject heart in heartIcons)
            if (heart != null) heart.SetActive(true);
        ShuffleCards();
    }

    public void ExitSwipeGame()
    {
        Debug.Log("Exited swipe game, returning to town.");

        currentHealth = heartIcons.Count;
        foreach (GameObject heart in heartIcons)
            if (heart != null) heart.SetActive(true);

        if (swipePanel != null) swipePanel.SetActive(false);
        if (mainGameUI != null) mainGameUI.SetActive(true);
    }
}
