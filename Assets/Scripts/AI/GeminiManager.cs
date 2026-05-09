using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using TMPro;

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

    [Header("UI Settings")]
    public TextMeshProUGUI chatText;
    public TMP_InputField chatInput;

    [Header("Audio Engine")]
    public RunJets audioEngine;

    [Header("Statistics")]
    public StatisticsManager statisticsManager;

    [Header("API Settings")]
    public string apiKey = "AIzaSyB041H4Kc1JRGJHrk6FS5cxnuvwzssPNeg";

    [Header("Fox Characters")]
    public List<FoxCharacter> characters = new();
    public int activeCharacterIndex = 0;

    private string currentScenario = "";
    private string activeCharacterName = "Fox";

    private string endpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

    private void Start()
    {
        if (characters.Count > 0)
            SelectCharacter(activeCharacterIndex);
    }

    public void SelectCharacter(int index)
    {
        if (index < 0 || index >= characters.Count) return;
        activeCharacterIndex = index;
        currentScenario = characters[index].scenario;
        activeCharacterName = characters[index].characterName;
        if (chatText != null) chatText.text = "";
    }

    public void AskGemini(string playerMessage)
    {
        StartCoroutine(SendRequest(playerMessage, currentScenario));
    }

    private IEnumerator SendRequest(string playerMessage, string scenario)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("API Key is empty! Get a new key from Google AI Studio and paste it in the Inspector.");
            yield break;
        }

        string url = endpoint + "?key=" + apiKey;

        string systemPrompt = $"You are an English language mentor. Your current role: {scenario}. " +
            "The player will say something in English. Reply in English without breaking character. " +
            "Return ONLY this JSON format, no markdown or explanation: " +
            "{\"reply\": \"your in-character English reply\", \"grammar_feedback\": \"grammar error with explanation if any, otherwise write 'Perfect!'\", \"emotion\": \"choose one: happy, sad or neutral\"}";

        JObject payload = new JObject
        {
            ["systemInstruction"] = new JObject
            {
                ["parts"] = new JArray { new JObject { ["text"] = systemPrompt } }
            },
            ["contents"] = new JArray
            {
                new JObject
                {
                    ["parts"] = new JArray { new JObject { ["text"] = playerMessage } }
                }
            },
            ["generationConfig"] = new JObject
            {
                ["responseMimeType"] = "application/json"
            }
        };

        string jsonData = payload.ToString();
        Debug.Log("Fox is thinking... (Request sent to API)");

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 30;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("API Error: " + request.error);
                Debug.LogError("HTTP Code: " + request.responseCode);
                Debug.LogError("Response: " + request.downloadHandler.text);
            }
            else
            {
                ParseGeminiResponse(request.downloadHandler.text);
            }
        }
    }

    private void ParseGeminiResponse(string jsonResponse)
    {
        string reply = "";
        string grammar = "";
        string emotion = "neutral";

        try
        {
            JObject data = JObject.Parse(jsonResponse);
            string textResult = data["candidates"][0]["content"]["parts"][0]["text"].ToString();
            JObject finalData = JObject.Parse(textResult);

            reply = finalData["reply"]?.ToString() ?? "No response received";
            grammar = finalData["grammar_feedback"]?.ToString() ?? "No grammar note received";
            emotion = finalData["emotion"]?.ToString() ?? "neutral";

            Debug.Log("FOX REPLY: " + reply);
            Debug.Log("GRAMMAR NOTE: " + grammar);
            Debug.Log("FOX EMOTION: " + emotion);

            if (audioEngine != null)
                audioEngine.SpeakFromAI(reply);

            if (emotionEngine != null)
                emotionEngine.SetEmotion(emotion);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Parse error: " + e.Message + " | Raw data: " + jsonResponse);
            reply = "An error occurred, please try again.";
            grammar = "";
        }

        if (chatText != null && !string.IsNullOrEmpty(reply))
        {
            chatText.text += $"\n<color=#FFA500><b>{activeCharacterName}:</b></color> {reply}";

            if (!string.IsNullOrEmpty(grammar) && !grammar.ToLower().Contains("perfect"))
            {
                chatText.text += $"\n<color=#FFFF00><i>(Note: {grammar})</i></color>";
                DataLogger.LogData("AI", "Grammar_Error", grammar);

                if (statisticsManager != null)
                {
                    string topic = ExtractTopicFromError(grammar);
                    statisticsManager.LogWrongAnswer(topic, grammar);
                }
            }
            else
            {
                DataLogger.LogData("AI", "Correct_Answer", "Grammar perfect");

                if (statisticsManager != null)
                    statisticsManager.LogCorrectAnswer("Speaking Fluency");
            }
        }
    }

    private string ExtractTopicFromError(string errorMessage)
    {
        string lowerError = errorMessage.ToLower();

        if (lowerError.Contains("subject")) return "Subject Sentence";
        if (lowerError.Contains("verb") || lowerError.Contains("tense")) return "Verb Tense";
        if (lowerError.Contains("adjective")) return "Adjectives";
        if (lowerError.Contains("noun")) return "Noun Sentences";
        if (lowerError.Contains("pronoun")) return "Pronouns";
        if (lowerError.Contains("comma") || lowerError.Contains("punctuation")) return "Punctuation";
        if (lowerError.Contains("exclamation")) return "Exclamatory Sentences";
        if (lowerError.Contains("question")) return "Question Structure";
        if (lowerError.Contains("suffix")) return "Suffix Usage";

        return "Other Errors";
    }

    public void SendTextMessage()
    {
        if (chatInput == null || string.IsNullOrWhiteSpace(chatInput.text)) return;

        string playerMessage = chatInput.text;

        if (chatText != null)
            chatText.text += $"\n\n<color=#00FF00><b>You:</b></color> {playerMessage}";

        AskGemini(playerMessage);
        chatInput.text = "";
    }
}
