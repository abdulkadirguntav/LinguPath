using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

[System.Serializable] 
public class BattleSentence
{
    public int id;
    public string turkishHint;
    public List<string> correctWords = new List<string>();
    public List<string> trapWords = new List<string>();
}

[System.Serializable]
public class DefenseData
{
    public int id;
    public string oddWord;                                 // Farklı olan kelime (Doğru cevap)
    public List<string> normalWords = new List<string>();  // Çeldirici kelimeler
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

    [Header("Health UI")]
    public Slider playerHealthSlider;
    public Slider enemyHealthSlider;
    public TMP_Text playerHealthText;
    public TMP_Text enemyHealthText;

    [Header("Core Loop Connections")]
    public GameObject battlePanel;
    public GameObject mainGameUI;

    [Header("Defense Database")]
    public List<DefenseData> allDefenses = new List<DefenseData>();

    [Header("Database")]
    public List<BattleSentence> allSentences = new List<BattleSentence>();
    
    [Header("Sentences Data")]
    public BattleSentence currentSentence;
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
    public TMP_Text turkishHintText;

    [Header("Panel References")]
    public GameObject offensePanel;
    public GameObject defensePanel;

    [Header("Defence References")]

    public float defenseMaxTime = 5f;
    public List<Button> stoneButtons = new List<Button>();
    
    void Start()
    {
        LoadSentencesData();
        LoadDefenseData();
        playerHP = enemyHP = 100f;
        UpdateHealthUI();
        currnetState = GameState.PlayerTurn;
        SetupPlayerTurn();
    }

    private void LoadSentencesData()
    {
        TextAsset csvData = Resources.Load<TextAsset>("Sentences");
        if (csvData == null) { Debug.LogError("Sentences.csv bulunamadı!"); return; }

        string[] dataLines = csvData.text.Split(new char[] { '\n' });
        for (int i = 1; i < dataLines.Length; i++)
        {
            string line = dataLines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] columns = line.Split(';'); // Noktalı virgüle göre böl

            if (columns.Length >= 4)
            {
                BattleSentence newSentence = new BattleSentence();
                newSentence.id = int.Parse(columns[0]);
                
                // 1. Cümleyi boşluklardan bölüp liste yap (Örn: "I want an apple" -> "I", "want", "an", "apple")
                string[] words = columns[1].Trim().Split(' ');
                newSentence.correctWords = new List<string>(words);

                // 2. Tuzak kelimeleri virgülden bölüp liste yap
                string[] traps = columns[2].Trim().Split(',');
                newSentence.trapWords = new List<string>(traps);

                newSentence.turkishHint = columns[3].Trim();

                allSentences.Add(newSentence);
            }
        }
        Debug.Log("Savaş Veritabanı Yüklendi! Toplam Cümle: " + allSentences.Count);
    }

    void SetupPlayerTurn()
    {
        offensePanel.SetActive(true);
        defensePanel.SetActive(false);
        turnTimer = maxTime;
        currentInput.Clear();

        // 1. Veritabanından rastgele cümle seç ve ipucunu yaz
        int randIndex = UnityEngine.Random.Range(0, allSentences.Count);
        currentSentence = allSentences[randIndex];
        if (turkishHintText != null) turkishHintText.text = currentSentence.turkishHint;

        // 2. Doğru ve tuzak kelimeleri aynı sepete at
        List<string> mixedWords = new List<string>();
        mixedWords.AddRange(currentSentence.correctWords);
        mixedWords.AddRange(currentSentence.trapWords);

        // 3. Çorba gibi karıştır (Shuffle)
        for (int j = 0; j < mixedWords.Count; j++)
        {
            int randomIndex = UnityEngine.Random.Range(0, mixedWords.Count);
            string temp = mixedWords[j];
            mixedWords[j] = mixedWords[randomIndex];
            mixedWords[randomIndex] = temp;
        }

        // 4. SİHİRLİ KISIM (Slotları Cümle Uzunluğuna Göre Ayarla)
        for (int i = 0; i < sentencesSlots.Count; i++)
        {
            if (i < currentSentence.correctWords.Count)
            {
                sentencesSlots[i].gameObject.SetActive(true); // Gerekli slotu aç
                sentencesSlots[i].text = "_";
            }
            else
            {
                sentencesSlots[i].gameObject.SetActive(false); // Fazlalık slotu gizle
            }
        }

        // 5. GÜVENLİ DÖNGÜ (Butonları Patlatmadan Ayarla)
        for (int i = 0; i < wordButtons.Count; i++)
        {
            wordButtons[i].onClick.RemoveAllListeners();

            if (i < mixedWords.Count)
            {
                // Eğer kelime varsa butonu aç ve içini doldur
                wordButtons[i].gameObject.SetActive(true);
                wordButtons[i].interactable = true;
                wordButtons[i].GetComponentInChildren<TMP_Text>().text = mixedWords[i];

                string currentWord = mixedWords[i];
                Button currentBtn = wordButtons[i];
                currentBtn.onClick.AddListener(() => OnWordButtonClicked(currentBtn, currentWord));
            }
            else
            {
                // Kelime bittiyse (örneğin 6 kelimelik cümlede 7. buton) onu gizle!
                wordButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void LoadDefenseData()
    {
        TextAsset csvData = Resources.Load<TextAsset>("Defense");
        if (csvData == null) { Debug.LogError("Defense.csv bulunamadı!"); return; }

        string[] dataLines = csvData.text.Split(new char[] { '\n' });
        for (int i = 1; i < dataLines.Length; i++)
        {
            string line = dataLines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] columns = line.Split(';');
            if (columns.Length >= 3)
            {
                DefenseData newDef = new DefenseData();
                newDef.id = int.Parse(columns[0]);
                newDef.oddWord = columns[1].Trim();
                
                string[] normals = columns[2].Trim().Split(',');
                newDef.normalWords = new List<string>(normals);
                
                allDefenses.Add(newDef);
            }
        }
        Debug.Log("Savunma Veritabanı Yüklendi! Toplam Bulmaca: " + allDefenses.Count);
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
                UpdateHealthUI();
                CheckWinLoseCondition();
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

        if(currentInput.Count == currentSentence.correctWords.Count)
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
            if(currentInput[i] != currentSentence.correctWords[i])
            {
                isCorrect = false;
                break;
            }
        }

        if(isCorrect)
        {
            float finalDamage = baseDamage * (1 + (turnTimer / maxTime));
            enemyHP -= finalDamage;
            UpdateHealthUI(); // EKLENDİ
            CheckWinLoseCondition(); // EKLENDİ
            Debug.Log($"Correct! Enemy Took {finalDamage} Damage.");
            GameEventSystem.LogAnswer("Savaş - Cümle Kurma", true);
            
            if(enemyHP <= 0) return; // Eğer düşman öldüyse düşman turuna (SetupEnemyTurn) geçmesin!
        }
        else
        {
            Debug.Log("Wrong sentence! You missed your attack.");
            GameEventSystem.LogAnswer("Savaş - Cümle Kurma", false, "Yanlış cümle sırası");
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

        // 1. Veritabanından rastgele bir savunma bulmacası seç
        int randIndex = UnityEngine.Random.Range(0, allDefenses.Count);
        DefenseData currentDef = allDefenses[randIndex];

        // 2. Taşlara koyulacak kelimeleri bir sepette topla
        List<string> defenseWords = new List<string>();
        string oddWord = currentDef.oddWord; // Uyumsuz olan (Aradığımız cevap)
        defenseWords.Add(oddWord);

        // Normal kelimelerden, (Ekranda kaç taş varsa o kadar - 1) adet seçip ekle
        for (int i = 0; i < stoneButtons.Count - 1; i++)
        {
            if (i < currentDef.normalWords.Count)
            {
                defenseWords.Add(currentDef.normalWords[i]);
            }
        }

        // 3. Kelimeleri çorba gibi karıştır (Odd word hep başta çıkmasın)
        for (int j = 0; j < defenseWords.Count; j++)
        {
            int randomIndex = UnityEngine.Random.Range(0, defenseWords.Count);
            string temp = defenseWords[j];
            defenseWords[j] = defenseWords[randomIndex];
            defenseWords[randomIndex] = temp;
        }

        // 4. Güvenli Döngü ile taşlara (butonlara) ata
        for (int i = 0; i < stoneButtons.Count; i++)
        {
            stoneButtons[i].onClick.RemoveAllListeners();

            if (i < defenseWords.Count)
            {
                stoneButtons[i].gameObject.SetActive(true);
                stoneButtons[i].interactable = true;
                stoneButtons[i].GetComponentInChildren<TMP_Text>().text = defenseWords[i];

                string clickedWord = defenseWords[i];
                stoneButtons[i].onClick.AddListener(() => OnStoneClicked(clickedWord, oddWord));
            }
            else
            {
                // Fazladan taş butonu varsa gizle
                stoneButtons[i].gameObject.SetActive(false);
            }
        }
    }
    public void OnStoneClicked(string clickedWord, string oddWord)
    {
        // 1. Oyuncu doğru (farklı olan) taşa mı tıkladı?
        if (clickedWord == oddWord)
        {
            // Başarılı savunma! Hasar almıyoruz.
            Debug.Log("Kusursuz Blok! Farklı olanı buldun, hasar almadın.");
            GameEventSystem.LogAnswer("Savaş - Farklı Kelime Bul", true);
        }
        else
        {
            playerHP -= enemyBaseDamage;
            UpdateHealthUI(); // EKLENDİ
            CheckWinLoseCondition(); // EKLENDİ
            Debug.Log($"Yanlış taş! Düşman {enemyBaseDamage} hasar vurdu.");
            GameEventSystem.LogAnswer("Savaş - Farklı Kelime Bul", false, "Yanlış kelime seçildi");
        
            if(playerHP <= 0) return; // Öldüysek tur bize geçmesin
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

    private void UpdateHealthUI()
    {
        if(playerHealthSlider != null) playerHealthSlider.value = playerHP / 100;
        if(enemyHealthSlider != null) enemyHealthSlider.value = enemyHP / 100;

        if(playerHealthText != null) playerHealthText.text = playerHP.ToString("F0");
        if(enemyHealthText != null) enemyHealthText.text = enemyHP.ToString("F0");
    }

    private void CheckWinLoseCondition()
    {
        if (enemyHP <= 0)
        {
            enemyHP = 0;
            UpdateHealthUI();
            Debug.Log("Zafer! Düşman yenildi.");
            EndBattle(true); // Kazandık!
        }
        else if (playerHP <= 0)
        {
            playerHP = 0;
            UpdateHealthUI();
            Debug.Log("Maalesef! Öldün.");
            EndBattle(false); // Kaybettik!
        }
    }

    private void EndBattle(bool isWin)
    {
        // 🎮 Savaş sonucunu istatistiklere kaydet
        int finalScore = (int)((100 - enemyHP) * 1.5f);
        GameEventSystem.LogGameEnd("Savaş", isWin, finalScore);
        
        // 1. Savaş ekranını kapat, kasabayı aç
        if (battlePanel != null) battlePanel.SetActive(false);
        if (mainGameUI != null) mainGameUI.SetActive(true);

        // 2. Kaybedildiyse savaşı sıfırla (tekrar denenebilsin diye)
        if (!isWin)
        {
            playerHP = 100f;
            enemyHP = 100f;
            UpdateHealthUI();
            currnetState = GameState.PlayerTurn;
            SetupPlayerTurn();
        }

        // 3. SİHİRLİ KOD: Teyzeye (veya aktif NPC'ye) görevin bittiğini haber ver!
        if (NPC.ActiveNPC != null)
        {
            NPC.ActiveNPC.FinishMission(isWin);
        }
    }

    // Savaştan Çıkış (Kaçma) Butonuna Bağlanacak
    public void FleeBattle()
    {
        Debug.Log("Savaştan kaçıldı! Kasabaya dönülüyor...");
        
        // Canları tekrar 100'e çekelim ki, oyuncu savaşa tekrar girdiğinde yaralı başlamasın
        playerHP = 100f;
        enemyHP = 100f;
        UpdateHealthUI();

        // Savaş panelini kapat, kasaba arayüzünü (Joystick) geri aç
        if (battlePanel != null) battlePanel.SetActive(false);
        if (mainGameUI != null) mainGameUI.SetActive(true);

        // Not: Görev tamamlanmadığı için NPC.FinishMission() ÇAĞIRMIYORUZ. 
        // Oyuncu kasabada serbest kalır, isterse Teyze'ye tekrar tıklayıp savaşı baştan başlatabilir.
    }
}
