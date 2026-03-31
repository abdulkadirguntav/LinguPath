using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 1. Veri Yapımız (CSV'den dolacak)
[System.Serializable]
public class SwipeCardData
{
    public int id;
    public string imageName;
    public string correctSentence;
    public string wrongSentence;
    public Sprite cardSprite; // Resim hafızada tutulacak
}

public class SwipeManager : MonoBehaviour
{
    [Header("Veritabanı (Otomatik Dolacak)")]
    public List<SwipeCardData> allCards = new List<SwipeCardData>(); 
    
    [Header("UI Referances")]
    public TMP_Text targetSentencesText;
    public Image activeCardImage;
    public List<GameObject> heartIcons = new List<GameObject>();

    [Header("Core Loop Connections")]
    public GameObject swipePanel; // Bu oyunun ana paneli
    public GameObject mainGameUI; // Joystick paneli

    [Header("Physics Memory")]
    Vector3 initialPosition;
    Quaternion initialRotation;

    [Header("Check & Player Data")]
    bool isCurrentSentenceTrue;
    int currentHealth = 3;
    int currentCardIndex = 0; // Destede kaçıncı karttayız

    void Start()
    {
        initialPosition = activeCardImage.transform.localPosition;  
        initialRotation = activeCardImage.transform.localRotation;
        currentHealth = heartIcons.Count; // Canı UI'daki kalp sayısına eşitle
        
        LoadSwipeData();
        ShuffleCards(); // Kartları her oyunda çorba et
        LoadNextCard();
    }

    private void LoadSwipeData()
    {
        TextAsset csvData = Resources.Load<TextAsset>("SwipeCards");
        if (csvData == null) { Debug.LogError("SwipeCards.csv bulunamadı!"); return; }

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
                
                // Resmi MarketIcons klasörün   den çek
                newCard.cardSprite = Resources.Load<Sprite>("Icons/" + newCard.imageName);

                allCards.Add(newCard);
            }
        }
        Debug.Log("Swipe Veritabanı Yüklendi! Toplam Kart: " + allCards.Count);
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
        // 1. Kartlar bittiyse oyunu kazanarak bitir
        if(currentCardIndex >= allCards.Count)
        {
            Debug.Log("Bütün kartlar bitti, Kazandın!");
            EndGame(true); 
            return;
        }

        // 2. Sıradaki kartı al
        SwipeCardData currentCard = allCards[currentCardIndex];
        
        if(currentCard.cardSprite != null)
            activeCardImage.sprite = currentCard.cardSprite;
        else
            Debug.LogWarning(currentCard.imageName + " resmi bulunamadı!");

        // 3. Yazıyı %50 ihtimalle doğru, %50 ihtimalle yanlış seç
        int randomChoice = Random.Range(0, 2);
        if(randomChoice == 0)
        {
            targetSentencesText.text = currentCard.correctSentence;
            isCurrentSentenceTrue = true; // Oyuncu bunu SAĞA atmalı
        }
        else
        {
            targetSentencesText.text = currentCard.wrongSentence;
            isCurrentSentenceTrue = false; // Oyuncu bunu SOLA atmalı
        }

        // 4. Kartı fiziksel olarak merkeze sıfırla
        activeCardImage.transform.localPosition = initialPosition;
        activeCardImage.transform.localRotation = initialRotation;
    }

    // SwipeCard.cs scripti (parmak çekildiğinde) bu fonksiyonu tetikler
    public void CardSwiped(bool isSwipedRight)
    {
        // Doğru mu bildi? (Doğru cümleyi sağa attıysa veya yanlış cümleyi sola attıysa kazanır)
        if(isSwipedRight == isCurrentSentenceTrue)
        {
            Debug.Log("Tebrikler, Doğru Bildin!");
        }
        else
        {
            Debug.Log("Yanlış!");
            currentHealth--;
            
            // UI'daki kalbi söndür
            if(currentHealth >= 0 && currentHealth < heartIcons.Count)
            {
                heartIcons[currentHealth].SetActive(false);
            }
            
            // Can bittiyse oyunu kaybederek bitir
            if(currentHealth <= 0 )
            {
                Debug.Log("Game Over! Canın bitti.");
                EndGame(false); 
                return;
            }
        }

        // Sonraki karta geç
        currentCardIndex++;
        LoadNextCard();
    }

    // Oyun Bitiş ve NPC'ye Dönüş Sistemi
    private void EndGame(bool isWin)
    {
        if (swipePanel != null) swipePanel.SetActive(false);
        if (mainGameUI != null) mainGameUI.SetActive(true);

        // Teyzeye/NPC'ye haber ver, diyaloğu patlatsın!
        if (NPC.ActiveNPC != null)
        {
            NPC.ActiveNPC.FinishMission(isWin);
        }
    }
}