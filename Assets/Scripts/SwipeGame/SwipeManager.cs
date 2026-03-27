using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SwipeManager : MonoBehaviour
{
    [Header("Card Deck ( Scriptable Object )")]
    public List<CardDataSO> deck = new List<CardDataSO>(); 
    
    [Header("UI Referances")]
    public TMP_Text targetSentencesText;
    public Image activeCardImage;
    public List<GameObject> heartIcons = new List<GameObject>();

    [Header("Physics Memory")]
    Vector3 initialPosition;
    Quaternion initialRotation;

    [Header("Check & Player Data")]
    bool isCurrentSentenceTrue;
    int currentHealth = 3;

    void Start()
    {
        initialPosition = activeCardImage.transform.localPosition;  
        initialRotation = activeCardImage.transform.localRotation;
        
        LoadNextCard();
    }
    void LoadNextCard()
    {
        if(deck.Count == 0)
        {
            Debug.Log("Card Finished");
            return;
        }
        else
        {
            CardDataSO currentCard = deck[0];
            activeCardImage.sprite = currentCard.cardSprite;
            int randomChoise = Random.Range(0, 2);
            if(randomChoise == 0)
            {
                targetSentencesText.text = currentCard.correctSentences;
                isCurrentSentenceTrue = true;
            }
            else
            {
                targetSentencesText.text = currentCard.wrongSentences;
                isCurrentSentenceTrue = false;
            }
        }

        activeCardImage.transform.localPosition = initialPosition;
        activeCardImage.transform.localRotation = initialRotation;
    }

    public void CardSwiped(bool isSwipedRight)
    {
        if(isSwipedRight == isCurrentSentenceTrue)
        {
            Debug.Log("Congratulations");
        }
        else
        {
            Debug.Log("Wrong!");
            currentHealth--;
            heartIcons[currentHealth].SetActive(false);
            if(currentHealth <= 0 )
            {
                Debug.Log("Game Over");
                return;
            }
        }

        deck.RemoveAt(0);
        LoadNextCard();
    }
}
