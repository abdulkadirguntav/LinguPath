
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

// --- GEMINI API JSON SINIFLARI ---
// Gemini'nin veri yapısı bu şekildedir, Unity'nin JsonUtility'si ile sorunsuz çalışır.
[System.Serializable]
public class GeminiPart { public string text; }

[System.Serializable]
public class GeminiContent { 
    public string role; // "user" veya "model" (Gemini'de AI'ın rolü model'dir)
    public List<GeminiPart> parts; 
}

[System.Serializable]
public class GeminiSystemInstruction { public List<GeminiPart> parts; }

[System.Serializable]
public class GeminiRequest { 
    public GeminiSystemInstruction system_instruction; // Mentör Tilki'nin kişiliğini buraya yazacağız
    public List<GeminiContent> contents; // Sohbet geçmişi
}

[System.Serializable]
public class GeminiResponse { public List<GeminiCandidate> candidates; }

[System.Serializable]
public class GeminiCandidate { public GeminiContent content; }

// --- ANA YÖNETİCİ SINIF ---
public class AIManager : MonoBehaviour
{
    [Header("UI Elemanları")]
    public TMP_InputField userInputField;
    public TextMeshProUGUI chatDisplay;
    public Button sendButton;
    public TMP_Dropdown toneDropdown;

    // API Ayarları
    private string apiKey = "AIzaSyA3ekX1IhVNh-8sOCptcCgekS-rNbdeDiU"; 
    private string apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key=";

    // Sohbet hafızası (Gemini konuşulanları unutmasın diye listeliyoruz)
    private List<GeminiContent> chatHistory = new List<GeminiContent>();

    void Start()
    {
        sendButton.onClick.AddListener(SendMessageToAI);
        chatHistory.Clear();
        chatDisplay.text = "Mentör Tilki (Gemini AI) Bağlandı. Ücretsiz ve hızlı!\nKonuşmaya başla!\n";
    }

    void SendMessageToAI()
    {
        string userText = userInputField.text;
        if (string.IsNullOrEmpty(userText)) return; // Boş mesajı engelle

        // 1. Ekrana yazdır ve input'u temizle
        chatDisplay.text += "\n\nSen: " + userText;
        userInputField.text = "";

        // 2. Mesajı hafızaya ekle (role: "user")
        GeminiPart newPart = new GeminiPart { text = userText };
        GeminiContent newContent = new GeminiContent { role = "user", parts = new List<GeminiPart> { newPart } };
        chatHistory.Add(newContent);

        // 3. API İsteğini başlat
        StartCoroutine(PostRequest());
    }

    IEnumerator PostRequest()
    {
        sendButton.interactable = false; // Spamlara karşı butonu kilitle
        chatDisplay.text += "\nTilki düşünüyor...";

        // Öğretmen tonunu Dropdown'dan al ve sistem komutunu (prompt) hazırla
        string tone = toneDropdown.options[toneDropdown.value].text;
        string systemText = $"Sen 'LinguPath' oyununda Mentör Tilkisin. Karşındaki kişi A1 seviyesinde İngilizce öğreniyor. Konuşma tarzın: {tone}. Lütfen sadece İngilizce, çok kısa, basit ve motive edici cevaplar ver.";
        
        GeminiSystemInstruction sysInst = new GeminiSystemInstruction {
            parts = new List<GeminiPart> { new GeminiPart { text = systemText } }
        };

        // Gönderilecek veriyi paketle
        GeminiRequest requestData = new GeminiRequest {
            system_instruction = sysInst,
            contents = chatHistory
        };

        string jsonData = JsonUtility.ToJson(requestData);
        string requestUrl = apiUrl + apiKey; // Gemini'de API Key direkt linkin sonuna eklenir

        // İnternet üzerinden veriyi yolla
        using (UnityWebRequest request = new UnityWebRequest(requestUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest(); // Cevap gelene kadar bekle

            if (request.result == UnityWebRequest.Result.Success)
            {
                // Gelen cevabı çöz
                GeminiResponse responseData = JsonUtility.FromJson<GeminiResponse>(request.downloadHandler.text);
                if(responseData.candidates != null && responseData.candidates.Count > 0)
                {
                    string aiText = responseData.candidates[0].content.parts[0].text;
                    
                    // Cevabı ekrana yazdır ("Tilki düşünüyor..." yazısının altına direkt eklenir)
                    chatDisplay.text += "\nTilki: " + aiText;
                    
                    // Hafızaya AI'ın cevabını ekle (role: "model") ki konuyu unutmasın
                    GeminiPart aiPart = new GeminiPart { text = aiText };
                    GeminiContent aiContent = new GeminiContent { role = "model", parts = new List<GeminiPart> { aiPart } };
                    chatHistory.Add(aiContent);
                }
            }
            else
            {
                Debug.LogError("API Hatası: " + request.error + " | " + request.downloadHandler.text);
                chatDisplay.text += "\n[Sistem]: Bağlantı hatası!";
                // Hata alırsak son mesajı hafızadan silelim ki sistem çökmesin
                chatHistory.RemoveAt(chatHistory.Count - 1); 
            }

            sendButton.interactable = true; // Butonu tekrar aç
        }
    }
}