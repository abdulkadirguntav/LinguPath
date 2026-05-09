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
    public string oddWord;
    public List<string> normalWords = new List<string>();
}

public class TurnBaseManager : MonoBehaviour
{
    enum GameState { PlayerTurn, EnemyTurn, GameOver }

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
        if (csvData == null) { Debug.LogError("Sentences.csv not found!"); return; }

        string[] dataLines = csvData.text.Split(new char[] { '\n' });
        for (int i = 1; i < dataLines.Length; i++)
        {
            string line = dataLines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] columns = line.Split(';');
            if (columns.Length >= 4)
            {
                BattleSentence newSentence = new BattleSentence();
                newSentence.id = int.Parse(columns[0]);

                string[] words = columns[1].Trim().Split(' ');
                newSentence.correctWords = new List<string>(words);

                string[] traps = columns[2].Trim().Split(',');
                newSentence.trapWords = new List<string>(traps);

                newSentence.turkishHint = columns[3].Trim();
                allSentences.Add(newSentence);
            }
        }
        Debug.Log("Battle database loaded! Total sentences: " + allSentences.Count);
    }

    void SetupPlayerTurn()
    {
        offensePanel.SetActive(true);
        defensePanel.SetActive(false);
        turnTimer = maxTime;
        currentInput.Clear();

        int randIndex = UnityEngine.Random.Range(0, allSentences.Count);
        currentSentence = allSentences[randIndex];
        if (turkishHintText != null) turkishHintText.text = currentSentence.turkishHint;

        List<string> mixedWords = new List<string>();
        mixedWords.AddRange(currentSentence.correctWords);
        mixedWords.AddRange(currentSentence.trapWords);

        for (int j = 0; j < mixedWords.Count; j++)
        {
            int randomIndex = UnityEngine.Random.Range(0, mixedWords.Count);
            string temp = mixedWords[j];
            mixedWords[j] = mixedWords[randomIndex];
            mixedWords[randomIndex] = temp;
        }

        for (int i = 0; i < sentencesSlots.Count; i++)
        {
            if (i < currentSentence.correctWords.Count)
            {
                sentencesSlots[i].gameObject.SetActive(true);
                sentencesSlots[i].text = "_";
            }
            else
            {
                sentencesSlots[i].gameObject.SetActive(false);
            }
        }

        for (int i = 0; i < wordButtons.Count; i++)
        {
            wordButtons[i].onClick.RemoveAllListeners();

            if (i < mixedWords.Count)
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

    private void LoadDefenseData()
    {
        TextAsset csvData = Resources.Load<TextAsset>("Defense");
        if (csvData == null) { Debug.LogError("Defense.csv not found!"); return; }

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
        Debug.Log("Defense database loaded! Total puzzles: " + allDefenses.Count);
    }

    void Update()
    {
        if (currnetState == GameState.PlayerTurn)
        {
            turnTimer -= Time.deltaTime;
            timerSlider.value = turnTimer / maxTime;

            if (turnTimer <= 0)
            {
                playerHP -= enemyBaseDamage;
                currnetState = GameState.EnemyTurn;
                Debug.Log("Time's up! Failed to build sentence, enemy's turn.");
                SetupEnemyTurn();
            }
        }
        else if (currnetState == GameState.EnemyTurn)
        {
            turnTimer -= Time.deltaTime;
            timerSlider.value = turnTimer / defenseMaxTime;

            if (turnTimer <= 0)
            {
                playerHP -= enemyBaseDamage;
                UpdateHealthUI();
                CheckWinLoseCondition();
                Debug.Log("Defense time expired! Took damage.");

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

        if (currentInput.Count == currentSentence.correctWords.Count)
            CheckSentences();
    }

    void CheckSentences()
    {
        bool isCorrect = true;

        for (int i = 0; i < currentInput.Count; i++)
        {
            if (currentInput[i] != currentSentence.correctWords[i])
            {
                isCorrect = false;
                break;
            }
        }

        if (isCorrect)
        {
            float finalDamage = baseDamage * (1 + (turnTimer / maxTime));
            enemyHP -= finalDamage;
            UpdateHealthUI();
            CheckWinLoseCondition();
            Debug.Log($"Correct! Enemy took {finalDamage} damage.");
            GameEventSystem.LogAnswer("Battle - Sentence Building", true);

            if (enemyHP <= 0) return;
        }
        else
        {
            Debug.Log("Wrong sentence! Missed attack.");
            GameEventSystem.LogAnswer("Battle - Sentence Building", false, "Wrong word order");
        }

        currnetState = GameState.EnemyTurn;
        SetupEnemyTurn();
    }

    public void OnClearButtonClicked()
    {
        currentInput.Clear();

        foreach (TMP_Text slot in sentencesSlots)
            slot.text = "-";

        foreach (Button btn in wordButtons)
            btn.interactable = true;
    }

    public void SetupEnemyTurn()
    {
        offensePanel.SetActive(false);
        defensePanel.SetActive(true);
        turnTimer = defenseMaxTime;

        int randIndex = UnityEngine.Random.Range(0, allDefenses.Count);
        DefenseData currentDef = allDefenses[randIndex];

        List<string> defenseWords = new List<string>();
        string oddWord = currentDef.oddWord;
        defenseWords.Add(oddWord);

        for (int i = 0; i < stoneButtons.Count - 1; i++)
        {
            if (i < currentDef.normalWords.Count)
                defenseWords.Add(currentDef.normalWords[i]);
        }

        for (int j = 0; j < defenseWords.Count; j++)
        {
            int randomIndex = UnityEngine.Random.Range(0, defenseWords.Count);
            string temp = defenseWords[j];
            defenseWords[j] = defenseWords[randomIndex];
            defenseWords[randomIndex] = temp;
        }

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
                stoneButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void OnStoneClicked(string clickedWord, string oddWord)
    {
        if (clickedWord == oddWord)
        {
            Debug.Log("Perfect block! Found the odd one, no damage taken.");
            GameEventSystem.LogAnswer("Battle - Find Odd Word", true);
        }
        else
        {
            playerHP -= enemyBaseDamage;
            UpdateHealthUI();
            CheckWinLoseCondition();
            Debug.Log($"Wrong stone! Enemy dealt {enemyBaseDamage} damage.");
            GameEventSystem.LogAnswer("Battle - Find Odd Word", false, "Wrong word selected");

            if (playerHP <= 0) return;
        }

        foreach (Button btn in stoneButtons)
            btn.interactable = false;

        currnetState = GameState.PlayerTurn;
        offensePanel.SetActive(true);
        defensePanel.SetActive(false);
        SetupPlayerTurn();
    }

    private void UpdateHealthUI()
    {
        if (playerHealthSlider != null) playerHealthSlider.value = playerHP / 100;
        if (enemyHealthSlider != null) enemyHealthSlider.value = enemyHP / 100;
        if (playerHealthText != null) playerHealthText.text = playerHP.ToString("F0");
        if (enemyHealthText != null) enemyHealthText.text = enemyHP.ToString("F0");
    }

    private void CheckWinLoseCondition()
    {
        if (enemyHP <= 0)
        {
            enemyHP = 0;
            UpdateHealthUI();
            Debug.Log("Victory! Enemy defeated.");
            EndBattle(true);
        }
        else if (playerHP <= 0)
        {
            playerHP = 0;
            UpdateHealthUI();
            Debug.Log("Defeated! Player died.");
            EndBattle(false);
        }
    }

    private void EndBattle(bool isWin)
    {
        int finalScore = (int)((100 - enemyHP) * 1.5f);
        GameEventSystem.LogGameEnd("Battle", isWin, finalScore);

        if (battlePanel != null) battlePanel.SetActive(false);
        if (mainGameUI != null) mainGameUI.SetActive(true);

        if (!isWin)
        {
            playerHP = 100f;
            enemyHP = 100f;
            UpdateHealthUI();
            currnetState = GameState.PlayerTurn;
            SetupPlayerTurn();
        }

        if (NPC.ActiveNPC != null)
            NPC.ActiveNPC.FinishMission(isWin);
    }

    public void FleeBattle()
    {
        Debug.Log("Fled from battle! Returning to town.");

        playerHP = 100f;
        enemyHP = 100f;
        UpdateHealthUI();

        if (battlePanel != null) battlePanel.SetActive(false);
        if (mainGameUI != null) mainGameUI.SetActive(true);
    }
}
