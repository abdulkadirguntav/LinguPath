using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

[System.Serializable] 
public class SentencesData
{
    public List<string> correctWords = new List<string>();
    public List<string> trapWords = new List<string>();
}
public class TurnBaseManager : MonoBehaviour
{    
    enum GameState
    {
        PlayerTurn,
        EnemyTurn,
        GameOver
    }

    [Header("Game Stats")]
    [SerializeField] private GameState currnetState;

    
    [Header("Sentences Data")]
    public SentencesData currentSentencesData;
    private List<string> currentInput = new List<string>();

    [Header("Health And Damage Fields")]
    [SerializeField] private float playerHP;
    [SerializeField] private float enemyHP;
    [SerializeField] private float baseDamage = 10f;
    [SerializeField] private float enemyBaseDamage = 15f;

    [Header("Time Fields")]
    [SerializeField] private float turnTimer;
    [SerializeField] private float maxTime = 15f;

    [Header("UI References")]
    public List<TMP_Text> sentencesSlots = new List<TMP_Text>();
    public List<Button> wordButtons = new List<Button>();
    public Slider timerSlider;

    [Header("Panel References")]
    public GameObject offensePanel;
    public GameObject defensePanel;

    [Header("Defence References")]

    public float defenseMaxTime = 5f;
    public List<Button> stoneButtons = new List<Button>();
    
    void Start()
    {
        playerHP = enemyHP = 100f;
        currnetState = GameState.PlayerTurn;
        SetupPlayerTurn();
    }

    void SetupPlayerTurn()
    {
        offensePanel.SetActive(true);
        defensePanel.SetActive(false);
        turnTimer = maxTime;
        currentInput.Clear();

        foreach(TMP_Text slotText in sentencesSlots)
        {
            slotText.text = "_";
        }

        List<string> mixedWords = new List<string>();

        mixedWords.AddRange(currentSentencesData.correctWords);
        mixedWords.AddRange(currentSentencesData.trapWords);

        for(int j = 0; j < mixedWords.Count; j++)
        {
            int randomIndex = UnityEngine.Random.Range(0,mixedWords.Count);

            string temp = mixedWords[j];
            mixedWords[j] = mixedWords[randomIndex];
            mixedWords[randomIndex] = temp;
        }

        for(int i= 0; i < mixedWords.Count; i++)
        {
            wordButtons[i].onClick.RemoveAllListeners();

            if(i < mixedWords.Count)
            {
                wordButtons[i].gameObject.SetActive(true);
                wordButtons[i].interactable = true;

                wordButtons[i].GetComponentInChildren<TMP_Text>().text = mixedWords[i];

                string currentWord = mixedWords[i];
                Button currentBtn = wordButtons[i];

                currentBtn.onClick.AddListener(() => OnWordButtonClicked(currentBtn, currentWord));
            }
            else
            {
                wordButtons[i].gameObject.SetActive(false);
            }

        }
        
    }
   void Update()
    {
        // BİZİM TURUMUZDA SÜRE AKIŞI (Zaten yazmıştın)
        if (currnetState == GameState.PlayerTurn)
        {
            turnTimer -= Time.deltaTime;
            timerSlider.value = turnTimer / maxTime;

            if (turnTimer <= 0)
            {
                playerHP -= enemyBaseDamage;
                currnetState = GameState.EnemyTurn;
                Debug.Log("Süre Bitti! Cümleyi kuramadın, sıra düşmanda.");
                SetupEnemyTurn(); // Düşman turunu başlat!
            }
        }
        // YENİ EKLENECEK KISIM: DÜŞMAN TURUNDA SÜRE AKIŞI
        else if (currnetState == GameState.EnemyTurn)
        {
            turnTimer -= Time.deltaTime;
            timerSlider.value = turnTimer / defenseMaxTime; // Dikkat: Burada maxTime değil, defenseMaxTime kullanıyoruz

            if (turnTimer <= 0)
            {
                // Süre bitti, taşı seçemedi! Hasar alır ve tur bize geçer.
                playerHP -= enemyBaseDamage;
                Debug.Log("Savunma süresi bitti! Taş kafana çarptı.");
                
                currnetState = GameState.PlayerTurn;
                offensePanel.SetActive(true);
                defensePanel.SetActive(false);
                SetupPlayerTurn();
            }
        }
    }
    public void OnWordButtonClicked(Button clickedBtn, string word)
    {
        currentInput.Add(word);
        sentencesSlots[currentInput.Count - 1].text = word;
        clickedBtn.interactable = false;

        if(currentInput.Count == currentSentencesData.correctWords.Count)
        {
            CheckSentences();
        }
    }

    void CheckSentences()
    {
        Debug.Log("Sentences Full, is Being Checked");
        bool isCorrect = true;

        for(int i = 0; i < currentInput.Count; i++)
        {
            if(currentInput[i] != currentSentencesData.correctWords[i])
            {
                isCorrect = false;
                break;
            }
        }

        if(isCorrect)
        {
            float finalDamage = baseDamage * (1 + (turnTimer / maxTime));
            enemyHP -= finalDamage;
            Debug.Log($"Correct! Enemy Took {finalDamage} Damage. Remaining HP: {enemyHP}");
        }
        else
        {
            Debug.Log("Wrong sentence! You missed your attack.");
        }

        currnetState = GameState.EnemyTurn;
        SetupEnemyTurn();
        Debug.Log("Enemy's Turn");
    }

    public void OnClearButtonClicked()
    {
        currentInput.Clear();

        foreach(TMP_Text slot in sentencesSlots)
        {
            slot.text = "-";
        }

        foreach(Button btn in wordButtons)
        {
            btn.interactable = true;
        }

        Debug.Log("Clear");
    }

    public void SetupEnemyTurn()
    {
        offensePanel.SetActive(false);
        defensePanel.SetActive(true);
        turnTimer = defenseMaxTime;

        // 2. Kelimeleri belirle (3 taşımız var, 3 kelime yazdık)
        List<string> defenseWords = new List<string> { "Apple", "Banana","Peace","Car" };
        string oddWord = "Car"; // Farklı olan kelime bu. Bunu hafızada tutuyoruz.

        // 3. Karıştırma (Shuffle) Algoritması (Bizim turumuzda yazdığımızın aynısı)
        for (int j = 0; j < defenseWords.Count; j++)
        {
            int randomIndex = UnityEngine.Random.Range(0, defenseWords.Count);
            string temp = defenseWords[j];
            defenseWords[j] = defenseWords[randomIndex];
            defenseWords[randomIndex] = temp;
        }

        // 4. Kelimeleri Ekrandaki Taşlara (Butonlara) Dağıt
        for (int i = 0; i < stoneButtons.Count; i++)
        {
            stoneButtons[i].interactable = true; // Taşı tıklanabilir yap
            
            // Taşın içindeki yazıya (Text) kelimeyi yazdır
            stoneButtons[i].GetComponentInChildren<TMP_Text>().text = defenseWords[i];

            // 5. TAŞLARA GÖREV VERME (Zurnanın zırt dediği yer)
            stoneButtons[i].onClick.RemoveAllListeners(); // Önceki turdan kalan tıklama hafızasını sil
            
            string clickedWord = defenseWords[i]; // O anki taşın kelimesini kopyala
            
            // "Ey buton, sana basıldığında OnStoneClicked fonksiyonuna git ve yanında bu iki kelimeyi götür" diyoruz:
            stoneButtons[i].onClick.AddListener(() => OnStoneClicked(clickedWord, oddWord));
        }
    }

    public void OnStoneClicked(string clickedWord, string oddWord)
    {
        // 1. Oyuncu doğru (farklı olan) taşa mı tıkladı?
        if (clickedWord == oddWord)
        {
            // Başarılı savunma! Hasar almıyoruz.
            Debug.Log("Kusursuz Blok! Farklı olanı buldun, hasar almadın.");
        }
        else
        {
            // Yanlış taşa tıklandı, cezayı (hasarı) kesiyoruz.
            playerHP -= enemyBaseDamage;
            Debug.Log($"Yanlış taş! Düşman {enemyBaseDamage} hasar vurdu. Kalan Canın: {playerHP}");
        }

        // 2. Taşların hepsini tekrar tıklanamaz yap (ki arka arkaya iki kere basamasın)
        foreach (Button btn in stoneButtons)
        {
            btn.interactable = false;
        }

        // 3. Sırayı tekrar bize (Player) geçir
        currnetState = GameState.PlayerTurn;

        // 4. Panelleri eski haline getir (Saldırı paneli açılsın, savunma kapansın)
        offensePanel.SetActive(true);
        defensePanel.SetActive(false);

        // 5. Bizim turumuzu sıfırdan hazırlayan o devasa fonksiyonu tekrar çağır
        SetupPlayerTurn();
    }


}
