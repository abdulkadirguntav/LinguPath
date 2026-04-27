using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class StatisticsUI : MonoBehaviour
{
    [Header("İstatistik Panelleri")]
    public GameObject statisticsPanel;
    public Button openStatisticsButton;
    public Button closeStatisticsButton;

    [Header("Genel İstatistikler")]
    public TextMeshProUGUI overallSuccessText;
    public TextMeshProUGUI totalAttemptsText;
    public TextMeshProUGUI totalCorrectText;
    public TextMeshProUGUI totalWrongText;

    [Header("En İyi/Kötü Konular")]
    public Transform bestTopicsContainer;
    public Transform weakestTopicsContainer;
    public GameObject topicStatPrefab; // Prefab: TopicStatItem

    [Header("Detaylı Konular Listesi")]
    public Transform allTopicsContainer;

    private StatisticsManager statisticsManager;
    private List<GameObject> instantiatedItems = new List<GameObject>();

    void Start()
    {
        statisticsManager = StatisticsManager.instance;

        if (openStatisticsButton != null)
            openStatisticsButton.onClick.AddListener(OpenStatistics);

        if (closeStatisticsButton != null)
            closeStatisticsButton.onClick.AddListener(CloseStatistics);

        // İlk açılışta paneli gizle
        if (statisticsPanel != null)
            statisticsPanel.SetActive(false);
    }

    public void OpenStatistics()
    {
        if (statisticsManager == null)
            statisticsManager = StatisticsManager.instance;

        if (statisticsManager == null)
        {
            Debug.LogError("❌ StatisticsManager bulunamadı!");
            return;
        }

        if (statisticsPanel != null)
            statisticsPanel.SetActive(true);

        RefreshAllStatistics();
    }

    public void CloseStatistics()
    {
        if (statisticsPanel != null)
            statisticsPanel.SetActive(false);
    }

    private void RefreshAllStatistics()
    {
        // Genel istatistikleri güncelle
        UpdateGeneralStats();

        // Önceki öğeleri temizle
        ClearContainers();

        // En iyi konuları göster
        ShowBestTopics();

        // En zayıf konuları göster
        ShowWeakestTopics();

        // Tüm konuları göster
        ShowAllTopics();
    }

    private void UpdateGeneralStats()
    {
        float successRate = statisticsManager.GetOverallSuccessRate();
        int totalAttempts = statisticsManager.GetTotalAttempts();
        StatisticsData stats = statisticsManager.GetStatisticsData();

        if (overallSuccessText != null)
        {
            overallSuccessText.text = $"🎯 Genel Başarı Oranı: <color=#FFD700>{successRate:F1}%</color>";
        }

        if (totalAttemptsText != null)
        {
            totalAttemptsText.text = $"📊 Toplam Denemeler: <color=#00FF00>{totalAttempts}</color>";
        }

        if (totalCorrectText != null)
        {
            totalCorrectText.text = $"✅ Doğru Cevaplar: <color=#00FF00>{stats.totalCorrectAnswers}</color>";
        }

        if (totalWrongText != null)
        {
            totalWrongText.text = $"❌ Yanlış Cevaplar: <color=#FF0000>{stats.totalWrongAnswers}</color>";
        }
    }

    private void ShowBestTopics()
    {
        if (bestTopicsContainer == null) return;

        List<TopicStats> bestTopics = statisticsManager.GetBestTopics(5);

        if (bestTopics.Count == 0)
        {
            TextMeshProUGUI noDataText = CreateTextElement(bestTopicsContainer, "Henüz veri yok");
            return;
        }

        foreach (TopicStats topic in bestTopics)
        {
            CreateTopicStatItem(bestTopicsContainer, topic, true);
        }
    }

    private void ShowWeakestTopics()
    {
        if (weakestTopicsContainer == null) return;

        List<TopicStats> weakestTopics = statisticsManager.GetWeakestTopics(5);

        if (weakestTopics.Count == 0)
        {
            TextMeshProUGUI noDataText = CreateTextElement(weakestTopicsContainer, "Henüz veri yok");
            return;
        }

        foreach (TopicStats topic in weakestTopics)
        {
            CreateTopicStatItem(weakestTopicsContainer, topic, false);
        }
    }

    private void ShowAllTopics()
    {
        if (allTopicsContainer == null) return;

        List<TopicStats> allTopics = statisticsManager.GetAllTopics();

        if (allTopics.Count == 0)
        {
            TextMeshProUGUI noDataText = CreateTextElement(allTopicsContainer, "Henüz veri yok");
            return;
        }

        foreach (TopicStats topic in allTopics)
        {
            CreateDetailedTopicItem(allTopicsContainer, topic);
        }
    }

    private void CreateTopicStatItem(Transform container, TopicStats topic, bool isBest)
    {
        GameObject item = Instantiate(topicStatPrefab, container);
        instantiatedItems.Add(item);

        TextMeshProUGUI titleText = item.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI statsText = item.transform.Find("Stats")?.GetComponent<TextMeshProUGUI>();
        Image progressBar = item.transform.Find("ProgressBar/Fill")?.GetComponent<Image>();

        if (titleText != null)
        {
            titleText.text = topic.topicName;
        }

        if (statsText != null)
        {
            statsText.text = $"{topic.successRate:F1}% | ✅ {topic.correctAnswers} ❌ {topic.wrongAnswers}\n{topic.GetPerformanceLevel()}";
        }

        if (progressBar != null)
        {
            progressBar.fillAmount = topic.successRate / 100f;
            
            // Renk değiş
            if (topic.successRate >= 90)
                progressBar.color = Color.green;
            else if (topic.successRate >= 75)
                progressBar.color = new Color(0, 1, 1); // Cyan
            else if (topic.successRate >= 50)
                progressBar.color = Color.yellow;
            else
                progressBar.color = Color.red;
        }
    }

    private void CreateDetailedTopicItem(Transform container, TopicStats topic)
    {
        // Detaylı gösterim için basit bir text oluştur
        TextMeshProUGUI detailText = CreateTextElement(container, "");
        instantiatedItems.Add(detailText.gameObject);

        string mistakes = "";
        if (topic.mistakeTopics.Count > 0)
        {
            mistakes = "\n   Sık Yapılan Hatalar:\n";
            foreach (var mistake in topic.mistakeTopics.Take(3)) // Son 3 hatayı göster
            {
                mistakes += $"   • {mistake}\n";
            }
        }

        detailText.text = $"<b>{topic.topicName}</b>\n" +
                          $"Başarı: {topic.successRate:F1}% | ✅ {topic.correctAnswers} | ❌ {topic.wrongAnswers}\n" +
                          $"Son Pratik: {topic.lastPracticed:dd/MM/yyyy HH:mm}" +
                          mistakes;
    }

    private TextMeshProUGUI CreateTextElement(Transform container, string text)
    {
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(container);
        textObj.transform.localScale = Vector3.one;

        TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = 24;

        RectTransform rectTransform = textObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(400, 100);

        return textComponent;
    }

    private void ClearContainers()
    {
        foreach (GameObject item in instantiatedItems)
        {
            Destroy(item);
        }
        instantiatedItems.Clear();

        // Prefab'dan oluşturulanları temizle
        if (bestTopicsContainer != null)
        {
            foreach (Transform child in bestTopicsContainer)
            {
                if (child.name.Contains(topicStatPrefab?.name ?? ""))
                    Destroy(child.gameObject);
            }
        }

        if (weakestTopicsContainer != null)
        {
            foreach (Transform child in weakestTopicsContainer)
            {
                if (child.name.Contains(topicStatPrefab?.name ?? ""))
                    Destroy(child.gameObject);
            }
        }

        if (allTopicsContainer != null)
        {
            foreach (Transform child in allTopicsContainer)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private List<T> Take<T>(List<T> list, int count)
    {
        return list.GetRange(0, Mathf.Min(count, list.Count));
    }

    public void ResetStatistics()
    {
        if (statisticsManager != null)
        {
            if (ConfirmReset())
            {
                statisticsManager.ResetAllStatistics();
                RefreshAllStatistics();
                Debug.Log("🔄 İstatistikler sıfırlandı!");
            }
        }
    }

    private bool ConfirmReset()
    {
        // Basit onay - gerekirse PopUp ile yapabilirsin
        return true;
    }
}
