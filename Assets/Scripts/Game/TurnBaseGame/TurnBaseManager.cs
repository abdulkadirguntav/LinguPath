using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

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
    [SerializeField] private GameState currentState;

    [Header("Score UI")]
    public TMP_Text playerGoalText;
    public TMP_Text enemyGoalText;

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

    [Header("Goal Settings")]
    [SerializeField] private int playerGoals;
    [SerializeField] private int enemyGoals;
    [SerializeField] private int maxGoals = 5;

    [Header("Time Fields")]
    [SerializeField] private float turnTimer;
    [SerializeField] private float maxTime = 15f;
    [SerializeField] private float defenseMaxTime = 5f;

    [Header("UI References")]
    public Transform answerContainer;
    public GameObject answerWordPrefab;
    public List<Button> wordButtons = new List<Button>();
    public Slider timerSlider;
    public TMP_Text turkishHintText;

    private struct AnswerChip
    {
        public GameObject chip;
        public Button sourceBtn;
        public string word;
    }
    private List<AnswerChip> answerChips = new List<AnswerChip>();

    [Header("Panel References")]
    public GameObject offensePanel;
    public GameObject defensePanel;

    [Header("Defence References")]
    public List<Button> stoneButtons = new List<Button>();

    void Start()
    {
        LoadSentencesData();
        LoadDefenseData();
        playerGoals = 0;
        enemyGoals = 0;
        UpdateGoalUI();
        currentState = GameState.PlayerTurn;
        SetupPlayerTurn();
    }

    private void LoadSentencesData()
    {
        TextAsset csvData = Resources.Load<TextAsset>("Sentences");
        if (csvData == null) { Debug.LogError("Sentences.csv not found!"); return; }

        string[] dataLines = csvData.text.Split('\n');
        for (int i = 1; i < dataLines.Length; i++)
        {
            string line = dataLines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] columns = line.Split(';');
            if (columns.Length >= 4)
            {
                BattleSentence newSentence = new BattleSentence();
                newSentence.id = int.Parse(columns[0]);
                newSentence.correctWords = new List<string>(columns[1].Trim().Split(' '));
                newSentence.trapWords = new List<string>(columns[2].Trim().Split(','));
                newSentence.turkishHint = columns[3].Trim();
                allSentences.Add(newSentence);
            }
        }
        Debug.Log("Sentence database loaded: " + allSentences.Count);
    }

    private void LoadDefenseData()
    {
        TextAsset csvData = Resources.Load<TextAsset>("Defense");
        if (csvData == null) { Debug.LogError("Defense.csv not found!"); return; }

        string[] dataLines = csvData.text.Split('\n');
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
                newDef.normalWords = new List<string>(columns[2].Trim().Split(','));
                allDefenses.Add(newDef);
            }
        }
        Debug.Log("Defense database loaded: " + allDefenses.Count);
    }

    void Update()
    {
        if (currentState == GameState.PlayerTurn)
        {
            turnTimer -= Time.deltaTime;
            timerSlider.value = turnTimer / maxTime;

            if (turnTimer <= 0)
            {
                Debug.Log("Süre doldu! Şut kaçtı (OUT).");
                TransitionToEnemyTurn();
            }
        }
        else if (currentState == GameState.EnemyTurn)
        {
            turnTimer -= Time.deltaTime;
            timerSlider.value = turnTimer / defenseMaxTime;

            if (turnTimer <= 0)
            {
                enemyGoals++;
                UpdateGoalUI();
                Debug.Log("Savunma süresi doldu! Gol yenildi.");

                if (CheckWinLoseCondition()) return;
                TransitionToPlayerTurn();
            }
        }
    }

    void SetupPlayerTurn()
    {
        offensePanel.SetActive(true);
        defensePanel.SetActive(false);
        turnTimer = maxTime;
        currentInput.Clear();
        ClearAnswerChips();

        int randIndex = Random.Range(0, allSentences.Count);
        currentSentence = allSentences[randIndex];
        if (turkishHintText != null) turkishHintText.text = currentSentence.turkishHint;

        List<string> mixedWords = new List<string>(currentSentence.correctWords);
        mixedWords.AddRange(currentSentence.trapWords);
        Shuffle(mixedWords);

        for (int i = 0; i < wordButtons.Count; i++)
        {
            wordButtons[i].onClick.RemoveAllListeners();

            if (i < mixedWords.Count)
            {
                wordButtons[i].gameObject.SetActive(true);
                wordButtons[i].interactable = true;
                wordButtons[i].GetComponentInChildren<TMP_Text>().text = mixedWords[i];

                string word = mixedWords[i];
                Button btn = wordButtons[i];
                btn.onClick.AddListener(() => OnWordButtonClicked(btn, word));
            }
            else
            {
                wordButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void SetupEnemyTurn()
    {
        offensePanel.SetActive(false);
        defensePanel.SetActive(true);
        turnTimer = defenseMaxTime;

        int randIndex = Random.Range(0, allDefenses.Count);
        DefenseData currentDef = allDefenses[randIndex];

        List<string> defenseWords = new List<string> { currentDef.oddWord };
        for (int i = 0; i < stoneButtons.Count - 1 && i < currentDef.normalWords.Count; i++)
            defenseWords.Add(currentDef.normalWords[i]);

        Shuffle(defenseWords);

        for (int i = 0; i < stoneButtons.Count; i++)
        {
            stoneButtons[i].onClick.RemoveAllListeners();

            if (i < defenseWords.Count)
            {
                stoneButtons[i].gameObject.SetActive(true);
                stoneButtons[i].interactable = true;
                stoneButtons[i].GetComponentInChildren<TMP_Text>().text = defenseWords[i];

                string word = defenseWords[i];
                string odd = currentDef.oddWord;
                stoneButtons[i].onClick.AddListener(() => OnStoneClicked(word, odd));
            }
            else
            {
                stoneButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void OnWordButtonClicked(Button clickedBtn, string word)
    {
        currentInput.Add(word);
        clickedBtn.interactable = false;

        GameObject chip = Instantiate(answerWordPrefab, answerContainer);
        chip.GetComponentInChildren<TMP_Text>().text = word;

        AnswerChip entry = new AnswerChip { chip = chip, sourceBtn = clickedBtn, word = word };
        answerChips.Add(entry);

        Button chipBtn = chip.GetComponent<Button>();
        if (chipBtn != null)
            chipBtn.onClick.AddListener(() => RemoveAnswerChip(entry));

        if (currentInput.Count == currentSentence.correctWords.Count)
            CheckSentences();
    }

    private void RemoveAnswerChip(AnswerChip entry)
    {
        int index = answerChips.IndexOf(entry);
        if (index < 0) return;

        answerChips.RemoveAt(index);
        currentInput.RemoveAt(index);
        entry.sourceBtn.interactable = true;
        Destroy(entry.chip);
    }

    private void ClearAnswerChips()
    {
        foreach (AnswerChip entry in answerChips)
            if (entry.chip != null) Destroy(entry.chip);
        answerChips.Clear();
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
            playerGoals++;
            UpdateGoalUI();
            Debug.Log("GOL! Doğru cümle kuruldu.");

            if (CheckWinLoseCondition()) return;
        }
        else
        {
            Debug.Log("OUT! Yanlış cümle, şut kaçtı.");
        }

        TransitionToEnemyTurn();
    }

    public void OnStoneClicked(string clickedWord, string oddWord)
    {
        foreach (Button btn in stoneButtons)
            btn.interactable = false;

        if (clickedWord == oddWord)
        {
            Debug.Log("KURTARMA! Doğru taş seçildi.");
        }
        else
        {
            enemyGoals++;
            UpdateGoalUI();
            Debug.Log("GOL YEDİN! Yanlış taş seçildi.");

            if (CheckWinLoseCondition()) return;
        }

        TransitionToPlayerTurn();
    }

    public void OnClearButtonClicked()
    {
        currentInput.Clear();
        ClearAnswerChips();

        foreach (Button btn in wordButtons)
            if (btn.gameObject.activeSelf) btn.interactable = true;
    }

    private void TransitionToPlayerTurn()
    {
        currentState = GameState.PlayerTurn;
        SetupPlayerTurn();
    }

    private void TransitionToEnemyTurn()
    {
        currentState = GameState.EnemyTurn;
        SetupEnemyTurn();
    }

    private void UpdateGoalUI()
    {
        if (playerGoalText != null) playerGoalText.text = playerGoals.ToString();
        if (enemyGoalText != null) enemyGoalText.text = enemyGoals.ToString();
    }

    // Returns true if the game ended
    private bool CheckWinLoseCondition()
    {
        if (playerGoals >= maxGoals)
        {
            EndBattle(true);
            return true;
        }
        if (enemyGoals >= maxGoals)
        {
            EndBattle(false);
            return true;
        }
        return false;
    }

    private void EndBattle(bool isWin)
    {
        currentState = GameState.GameOver;
        Debug.Log(isWin ? "MAÇ KAZANILDI!" : "MAÇ KAYBEDİLDİ!");

        int finalScore = playerGoals * 20;
        Debug.Log($"Maç bitti. Skor: {finalScore}");

        if (battlePanel != null) battlePanel.SetActive(false);
        if (mainGameUI != null) mainGameUI.SetActive(true);

        playerGoals = 0;
        enemyGoals = 0;
        UpdateGoalUI();

        if (NPC.ActiveNPC != null)
            NPC.ActiveNPC.FinishMission(isWin);
    }

    public void FleeBattle()
    {
        Debug.Log("Maçtan çıkıldı.");
        playerGoals = 0;
        enemyGoals = 0;
        UpdateGoalUI();

        if (battlePanel != null) battlePanel.SetActive(false);
        if (mainGameUI != null) mainGameUI.SetActive(true);
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(0, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }
}
