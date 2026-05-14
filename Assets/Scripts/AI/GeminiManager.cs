using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class FoxCharacter
{
    public string characterName;
    [TextArea(3, 6)]
    public string scenario;
}

public class GeminiManager : MonoBehaviour
{
    [Header("References")]
    public FoxEmotionController emotionEngine;
    public FoxVoiceEngine audioEngine;

    [Header("UI")]
    public TextMeshProUGUI chatText;
    public TMP_InputField chatInput;
    public GameObject thinkingIndicator;
    public ScrollRect chatScrollRect;

    [Header("Panel Bağlantıları")]
    public GameObject chatPanel;
    public GameObject mainGameUI;

    [Header("API Settings")]
    public string apiKey = "";

    [Header("Fox Characters")]
    public List<FoxCharacter> characters = new();
    public int activeCharacterIndex = 0;

    private string currentScenario = "";
    private string activeCharacterName = "Fox";
    private bool isProcessing = false;

    private readonly List<JObject> conversationHistory = new();

    private const string Endpoint =
        "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

    private void Start()
    {
        if (characters.Count > 0)
            SelectCharacter(activeCharacterIndex);

        SetThinking(false);
    }

    public void SelectCharacter(int index)
    {
        if (index < 0 || index >= characters.Count) return;
        activeCharacterIndex = index;
        currentScenario = characters[index].scenario;
        activeCharacterName = characters[index].characterName;
        conversationHistory.Clear();

        if (chatText != null) chatText.text = "";
    }

    // Called from UI Send button (text input)
    public void SendTextMessage()
    {
        if (chatInput == null || string.IsNullOrWhiteSpace(chatInput.text)) return;
        string msg = chatInput.text.Trim();
        chatInput.text = "";
        SendMessage(msg);
    }

    // Called from MicrophoneInputManager after speech-to-text
    public void SendVoiceMessage(string transcribedText)
    {
        if (string.IsNullOrWhiteSpace(transcribedText)) return;
        SendMessage(transcribedText);
    }

    private void SendMessage(string playerMessage)
    {
        if (isProcessing) return;

        AppendToChat($"\n\n<color=#00FF00><b>You:</b></color> {playerMessage}");
        AskGemini(playerMessage);
    }

    public void AskGemini(string playerMessage)
    {
        StartCoroutine(SendRequest(playerMessage));
    }

    private IEnumerator SendRequest(string playerMessage)
    {
        isProcessing = true;
        SetThinking(true);

        string url = Endpoint + "?key=" + apiKey;

        string systemPrompt =
            $"You are a friendly English language mentor fox named {activeCharacterName}. " +
            $"Your current role: {currentScenario}. " +
            "The player is learning English. Speak naturally in English, stay in character. " +
            "Return ONLY this JSON format with no markdown or extra text: " +
            "{\"reply\": \"your in-character English reply (1-3 sentences)\", " +
            "\"grammar_feedback\": \"describe the grammar mistake if any, otherwise write exactly: Perfect!\", " +
            "\"emotion\": \"happy, sad, or neutral\"}";

        // Build contents array from conversation history + new message
        JArray contents = new JArray();
        foreach (JObject turn in conversationHistory)
            contents.Add(turn);

        contents.Add(new JObject
        {
            ["role"] = "user",
            ["parts"] = new JArray { new JObject { ["text"] = playerMessage } }
        });

        JObject payload = new JObject
        {
            ["systemInstruction"] = new JObject
            {
                ["parts"] = new JArray { new JObject { ["text"] = systemPrompt } }
            },
            ["contents"] = contents,
            ["generationConfig"] = new JObject
            {
                ["responseMimeType"] = "application/json"
            }
        };

        byte[] bodyRaw = Encoding.UTF8.GetBytes(payload.ToString());

        using UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.timeout = 30;

        yield return request.SendWebRequest();

        SetThinking(false);
        isProcessing = false;

        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"API Error {request.responseCode}: {request.error}");
            AppendToChat($"\n<color=#FF4444><i>(Connection error. Check your API key and internet.)</i></color>");
            yield break;
        }

        ParseGeminiResponse(request.downloadHandler.text, playerMessage);
    }

    private void ParseGeminiResponse(string jsonResponse, string playerMessage)
    {
        string reply = "";
        string grammar = "";
        string emotion = "neutral";

        try
        {
            JObject data = JObject.Parse(jsonResponse);
            string textResult = data["candidates"][0]["content"]["parts"][0]["text"].ToString();
            JObject parsed = JObject.Parse(textResult);

            reply = parsed["reply"]?.ToString() ?? "I didn't understand that.";
            grammar = parsed["grammar_feedback"]?.ToString() ?? "";
            emotion = parsed["emotion"]?.ToString() ?? "neutral";
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Parse error: {e.Message}\nRaw: {jsonResponse}");
            reply = "Something went wrong. Please try again.";
        }

        // Save turns to conversation history
        conversationHistory.Add(new JObject
        {
            ["role"] = "user",
            ["parts"] = new JArray { new JObject { ["text"] = playerMessage } }
        });
        conversationHistory.Add(new JObject
        {
            ["role"] = "model",
            ["parts"] = new JArray { new JObject { ["text"] = reply } }
        });

        // Keep history from growing unbounded (last 10 exchanges = 20 entries)
        while (conversationHistory.Count > 20)
            conversationHistory.RemoveAt(0);

        // Display fox reply
        AppendToChat($"\n<color=#FFA500><b>{activeCharacterName}:</b></color> {reply}");

        // Grammar feedback
        if (!string.IsNullOrEmpty(grammar) && !grammar.ToLower().StartsWith("perfect"))
            AppendToChat($"\n<color=#FFFF00><size=80%><i>Note: {grammar}</i></size></color>");

        emotionEngine?.SetEmotion(emotion);
        audioEngine?.SpeakFromAI(reply);
    }

    private void AppendToChat(string text)
    {
        if (chatText == null) return;
        chatText.text += text;

        if (chatScrollRect != null)
            StartCoroutine(ScrollToBottom());
    }

    private IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)chatScrollRect.content);
        chatScrollRect.verticalNormalizedPosition = 0f;
    }

    public void ExitChat()
    {
        if (chatPanel != null)   chatPanel.SetActive(false);
        if (mainGameUI != null)  mainGameUI.SetActive(true);
    }

    private void SetThinking(bool thinking)
    {
        if (thinkingIndicator != null)
            thinkingIndicator.SetActive(thinking);
    }
}
