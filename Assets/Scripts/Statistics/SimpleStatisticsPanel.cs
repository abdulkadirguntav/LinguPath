using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class SimpleStatisticsPanel : MonoBehaviour
{
    [Header("Settings")]
    public bool createUIOnStart = true;
    public Button toggleButton;

    private GameObject mainPanel;
    private StatisticsManager statisticsManager;
    private bool isPanelOpen = false;

    void Start()
    {
        statisticsManager = StatisticsManager.instance;
        if (createUIOnStart) CreateSimpleUI();
    }

    public void CreateSimpleUI()
    {
        mainPanel = new GameObject("StatisticsPanel");
        mainPanel.transform.SetParent(transform);
        mainPanel.transform.localScale = Vector3.one;

        RectTransform panelRect = mainPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image panelImage = mainPanel.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

        GameObject scrollObj = new GameObject("ScrollView");
        scrollObj.transform.SetParent(mainPanel.transform);
        RectTransform scrollRect = scrollObj.AddComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0, 0);
        scrollRect.anchorMax = new Vector2(1, 1);
        scrollRect.offsetMin = new Vector2(20, 100);
        scrollRect.offsetMax = new Vector2(-20, -60);

        ScrollRect scroll = scrollObj.AddComponent<ScrollRect>();
        scroll.vertical = true;
        scroll.horizontal = false;

        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(scrollObj.transform);
        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = new Vector2(0, 0);

        VerticalLayoutGroup vlg = contentObj.AddComponent<VerticalLayoutGroup>();
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.spacing = 10;
        vlg.padding = new RectOffset(10, 10, 10, 10);

        ContentSizeFitter csf = contentObj.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.content = contentRect;

        AddTitle(contentObj.transform, "📊 STATISTICS");
        AddStatLine(contentObj.transform, "🎯 Success Rate: --", "OverallSuccess");
        AddStatLine(contentObj.transform, "📝 Total Attempts: --", "TotalAttempts");
        AddStatLine(contentObj.transform, "✅ Correct Answers: --", "CorrectAnswers");
        AddStatLine(contentObj.transform, "❌ Wrong Answers: --", "WrongAnswers");
        AddSpacer(contentObj.transform);
        AddTitle(contentObj.transform, "🌟 BEST TOPICS");
        AddTopicsList(contentObj.transform, true);
        AddSpacer(contentObj.transform);
        AddTitle(contentObj.transform, "⚠️ TOPICS TO IMPROVE");
        AddTopicsList(contentObj.transform, false);

        GameObject closeButtonObj = new GameObject("CloseButton");
        closeButtonObj.transform.SetParent(mainPanel.transform);
        RectTransform closeRect = closeButtonObj.AddComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1, 1);
        closeRect.anchorMax = new Vector2(1, 1);
        closeRect.pivot = new Vector2(1, 1);
        closeRect.anchoredPosition = new Vector2(-20, -20);
        closeRect.sizeDelta = new Vector2(60, 60);

        Image closeImage = closeButtonObj.AddComponent<Image>();
        closeImage.color = new Color(0.8f, 0.2f, 0.2f);

        Button closeBtn = closeButtonObj.AddComponent<Button>();
        closeBtn.onClick.AddListener(() => mainPanel.SetActive(false));

        TextMeshProUGUI closeText = closeButtonObj.AddComponent<TextMeshProUGUI>();
        closeText.text = "X";
        closeText.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        closeText.fontSize = 40;
        closeText.alignment = TextAlignmentOptions.Center;

        mainPanel.SetActive(false);

        if (toggleButton != null)
            toggleButton.onClick.AddListener(TogglePanel);

        Debug.Log("Statistics panel created!");
    }

    private void AddTitle(Transform parent, string text)
    {
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(parent);
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = text;
        titleText.fontSize = 36;
        titleText.alignment = TextAlignmentOptions.MidlineLeft;
        titleText.color = new Color(1, 1, 0.5f);
        LayoutElement le = titleObj.AddComponent<LayoutElement>();
        le.preferredHeight = 60;
    }

    private void AddStatLine(Transform parent, string text, string tagName)
    {
        GameObject statObj = new GameObject("Stat_" + tagName);
        statObj.transform.SetParent(parent);
        TextMeshProUGUI statText = statObj.AddComponent<TextMeshProUGUI>();
        statText.text = text;
        statText.fontSize = 28;
        statText.alignment = TextAlignmentOptions.MidlineLeft;
        LayoutElement le = statObj.AddComponent<LayoutElement>();
        le.preferredHeight = 50;
        statObj.name = tagName;
    }

    private void AddSpacer(Transform parent)
    {
        GameObject spacerObj = new GameObject("Spacer");
        spacerObj.transform.SetParent(parent);
        LayoutElement le = spacerObj.AddComponent<LayoutElement>();
        le.preferredHeight = 20;
    }

    private void AddTopicsList(Transform parent, bool isBest)
    {
        List<TopicStats> topics = isBest ?
            statisticsManager.GetBestTopics(5) :
            statisticsManager.GetWeakestTopics(5);

        if (topics.Count == 0)
        {
            TextMeshProUGUI noDataText = new GameObject("NoData").AddComponent<TextMeshProUGUI>();
            noDataText.transform.SetParent(parent);
            noDataText.text = "No data yet";
            noDataText.fontSize = 20;
            return;
        }

        foreach (TopicStats topic in topics)
        {
            GameObject topicObj = new GameObject("Topic_" + topic.topicName);
            topicObj.transform.SetParent(parent);
            Image topicImage = topicObj.AddComponent<Image>();
            topicImage.color = new Color(0.2f, 0.2f, 0.3f);
            LayoutElement le = topicObj.AddComponent<LayoutElement>();
            le.preferredHeight = 80;
            HorizontalLayoutGroup hlg = topicObj.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10;
            hlg.padding = new RectOffset(10, 10, 5, 5);

            GameObject infoObj = new GameObject("Info");
            infoObj.transform.SetParent(topicObj.transform);
            TextMeshProUGUI infoText = infoObj.AddComponent<TextMeshProUGUI>();
            infoText.text = $"<b>{topic.topicName}</b>\n" +
                           $"Success: {topic.successRate:F1}% | ✅ {topic.correctAnswers} ❌ {topic.wrongAnswers}\n" +
                           $"{topic.GetPerformanceLevel()}";
            infoText.fontSize = 20;
            LayoutElement infoLe = infoObj.AddComponent<LayoutElement>();
            infoLe.preferredWidth = 400;
            infoLe.flexibleWidth = 1;

            GameObject barObj = new GameObject("ProgressBar");
            barObj.transform.SetParent(topicObj.transform);
            Image barBg = barObj.AddComponent<Image>();
            barBg.color = new Color(0.1f, 0.1f, 0.1f);
            LayoutElement barLe = barObj.AddComponent<LayoutElement>();
            barLe.preferredWidth = 150;
            barLe.preferredHeight = 30;

            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(barObj.transform);
            RectTransform fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0, 0);
            fillRect.anchorMax = new Vector2(0, 1);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            fillRect.sizeDelta = new Vector2(topic.successRate * 1.5f, 0);
            Image fillImage = fillObj.AddComponent<Image>();
            if (topic.successRate >= 90) fillImage.color = Color.green;
            else if (topic.successRate >= 75) fillImage.color = new Color(0, 1, 1);
            else if (topic.successRate >= 50) fillImage.color = Color.yellow;
            else fillImage.color = Color.red;
        }
    }

    public void TogglePanel()
    {
        if (mainPanel != null)
        {
            mainPanel.SetActive(!mainPanel.activeSelf);
            isPanelOpen = mainPanel.activeSelf;
            if (isPanelOpen) RefreshUI();
        }
    }

    private void RefreshUI()
    {
        if (mainPanel == null || statisticsManager == null) return;

        Transform content = mainPanel.transform.Find("ScrollView/Content");
        if (content == null) return;

        UpdateStatText(content, "OverallSuccess",
            $"🎯 Success Rate: <color=#FFD700>{statisticsManager.GetOverallSuccessRate():F1}%</color>");
        UpdateStatText(content, "TotalAttempts",
            $"📝 Total Attempts: <color=#00FF00>{statisticsManager.GetTotalAttempts()}</color>");

        StatisticsData stats = statisticsManager.GetStatisticsData();
        UpdateStatText(content, "CorrectAnswers",
            $"✅ Correct Answers: <color=#00FF00>{stats.totalCorrectAnswers}</color>");
        UpdateStatText(content, "WrongAnswers",
            $"❌ Wrong Answers: <color=#FF0000>{stats.totalWrongAnswers}</color>");
    }

    private void UpdateStatText(Transform parent, string tagName, string newText)
    {
        foreach (Transform child in parent)
        {
            if (child.name == tagName)
            {
                TextMeshProUGUI text = child.GetComponent<TextMeshProUGUI>();
                if (text != null) text.text = newText;
                break;
            }
        }
    }
}
