using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using TMPro;

public class GeminiManager : MonoBehaviour
{
    [Header("Bağlantılar")]
    public FoxEmotionController duyguMotoru;

    [Header("UI Ayarları")]
    public TextMeshProUGUI chatText;
    public TMP_InputField chatInput;

    [Header("Ses Motoru Bağlantısı")]
    public RunJets sesMotoru;

    [Header("İstatistikler Bağlantısı")]
    public StatisticsManager statisticsManager;

    [Header("API Ayarları")]
    // DİKKAT: Eski API anahtarını sildim, Google AI Studio'dan YENİ BİR TANE alıp buraya (ve Unity Inspector'a) yapıştır!
    public string apiKey = "AIzaSyB041H4Kc1JRGJHrk6FS5cxnuvwzssPNeg"; 
    
    [Header("Senaryo Ayarı")]
    public string gecerliSenaryo = "Kafe Garsonu";

    // 🚀 ÇÖZÜM 1: Gemini 1.5 Flash (Stabil ve Güvenilir)
    private string endpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

    public void AskGemini(string playerMessage)
    {
        StartCoroutine(SendRequest(playerMessage, gecerliSenaryo));
    }

    private IEnumerator SendRequest(string playerMessage, string scenario)
    {
        // API Key kontrol
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("❌ API Key boş! Google AI Studio'dan yeni bir API Key alıp Inspector'a yapıştır!");
            yield break;
        }

        string url = endpoint + "?key=" + apiKey;

        string systemPrompt = $"Sen bir İngilizce dil mentorusun. Şu anki rolün: {scenario}. " +
            "Oyuncu sana İngilizce bir şey söyleyecek. Rolden çıkmadan ona İngilizce cevap ver. " +
            "Bana SADECE şu formatta bir JSON döndür, hiçbir markdown veya açıklama yazma: " +
            "{\"reply\": \"rolüne uygun İngilizce cevabın\", \"grammar_feedback\": \"varsa gramer hatası ve Türkçe açıklaması, yoksa 'Kusursuz!' yaz\", \"emotion\": \"verdiğin cevaba göre şu duygulardan birini seç: happy, sad veya neutral\"}";

        // 🚀 ÇÖZÜM 2: Google'ın resmi isimlendirme standartları (CamelCase) ile güncellendi
        JObject payload = new JObject
        {
            ["systemInstruction"] = new JObject
            {
                ["parts"] = new JArray
                {
                    new JObject { ["text"] = systemPrompt }
                }
            },
            ["contents"] = new JArray
            {
                new JObject
                {
                    ["parts"] = new JArray
                    {
                        new JObject { ["text"] = playerMessage }
                    }
                }
            },
            ["generationConfig"] = new JObject
            {
                ["responseMimeType"] = "application/json" 
            }
        };

        string jsonData = payload.ToString();

        Debug.Log("🦊 Tilki Düşünüyor... (API'ye İstek Gitti)");
        Debug.Log("📤 URL: " + url);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 30; // 30 saniye timeout

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("❌ API Hatası: " + request.error);
                Debug.LogError("📍 HTTP Code: " + request.responseCode);
                Debug.LogError("📄 Response: " + request.downloadHandler.text);
                
                if (request.responseCode == 503)
                {
                    Debug.LogError("🚨 503 Service Unavailable - Olası nedenleri:");
                    Debug.LogError("   1. API Key geçersiz veya revoke edilmiş");
                    Debug.LogError("   2. Endpoint/Model adı hatalı");
                    Debug.LogError("   3. Google API servisi geçici olarak kapalı");
                }
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
        string emotion = "neutral"; // Yeni: Duygu değişkenimiz (varsayılanı nötr yaptık)
        
        try
        {
            JObject data = JObject.Parse(jsonResponse);
            string textResult = data["candidates"][0]["content"]["parts"][0]["text"].ToString();

            JObject finalData = JObject.Parse(textResult);

            reply = finalData["reply"]?.ToString() ?? "Cevap alınamadı";
            grammar = finalData["grammar_feedback"]?.ToString() ?? "Gramer notu alınamadı";
            
            // Yeni: Duyguyu da senin yazdığın gibi güvenli (null check ile) çekiyoruz
            emotion = finalData["emotion"]?.ToString() ?? "neutral";

            Debug.Log("💬 TİLKİ CEVABI: " + reply);
            Debug.Log("📝 GRAMER NOTU: " + grammar);
            Debug.Log("🎭 TİLKİ DUYGUSU: " + emotion); // Konsoldan hangi duygunun geldiğini görebilirsin

            // Metni sese çevirmesi için RunJets'e komut yolla
            if (sesMotoru != null)
            {
                sesMotoru.SpeakFromAI(reply);
            }
            
            // Yeni: Gelen duyguyu yüz kaslarını kontrol eden motora yolla!
            if (duyguMotoru != null)
            {
                duyguMotoru.DuyguAyarla(emotion);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Veri Ayıklama Hatası: " + e.Message + " | Gelen Ham Veri: " + jsonResponse);
            reply = "Hata oluştu, lütfen tekrar deneyin";
            grammar = "";
        }

        // Ekrana Tilkinin cevabını ve gramer notunu yazdırıyoruz (Turuncu ve Sarı renklerde)
        if (chatText != null && !string.IsNullOrEmpty(reply))
        {
            chatText.text += $"\n<color=#FFA500><b>Tilki:</b></color> {reply}";
    
            // Eğer gramer hatası yoksa ("Kusursuz!" döndüyse) ekranda kalabalık yapmasın, varsa yazsın
            if(!string.IsNullOrEmpty(grammar) && !grammar.ToLower().Contains("kusursuz"))
            {
                chatText.text += $"\n<color=#FFFF00><i>(Not: {grammar})</i></color>";
                // Arka planda oyuncunun gramer hatasını logla (Ekranda görünmez)
                DataLogger.VeriKaydet("Yapay_Zeka", "Gramer_Hatasi", grammar);
                
                // İstatistiklere yanlış cevabı ekle
                if (statisticsManager != null)
                {
                    string topic = ExtractTopicFromError(grammar);
                    statisticsManager.LogWrongAnswer(topic, grammar);
                }
            }
            else
            {
                // Doğru cevap kayıt et
                DataLogger.VeriKaydet("Yapay_Zeka", "Dogu_Cevap", "Gramer kusursuz");
                
                // İstatistiklere doğru cevabı ekle
                if (statisticsManager != null)
                {
                    statisticsManager.LogCorrectAnswer("Konuşma Akıcılığı");
                }
            }
        }
    }

    /// <summary>
    /// Gramer hatasından konuyu çıkar
    /// </summary>
    private string ExtractTopicFromError(string errorMessage)
    {
        string lowerError = errorMessage.ToLower();
        
        // Hata mesajında hangi konu geçiyorsa onu döndür
        if (lowerError.Contains("konu") || lowerError.Contains("subject")) return "Konu Cümlesi";
        if (lowerError.Contains("fiil") || lowerError.Contains("verb")) return "Fiil Zamanı";
        if (lowerError.Contains("sıfat") || lowerError.Contains("adjective")) return "Sıfatlandırma";
        if (lowerError.Contains("isim") || lowerError.Contains("noun")) return "İsim Cümleleri";
        if (lowerError.Contains("zamir") || lowerError.Contains("pronoun")) return "Zamir Kullanımı";
        if (lowerError.Contains("virgül") || lowerError.Contains("comma")) return "Noktalama İşaretleri";
        if (lowerError.Contains("ünlem") || lowerError.Contains("exclamation")) return "İfade Cümleleri";
        if (lowerError.Contains("soru") || lowerError.Contains("question")) return "Soru Yapısı";
        if (lowerError.Contains("zaman") || lowerError.Contains("tense")) return "Fiil Zamanı";
        if (lowerError.Contains("ek") || lowerError.Contains("suffix")) return "Ek Kullanımı";
        
        return "Diğer Hatalar";
    }

    // Butona basıldığında veya Enter'a basıldığında çalışacak fonksiyon
    public void SendTextMessage()
    {
        // Eğer input boşsa veya sadece boşluk basılmışsa hiçbir şey yapma
        if (chatInput == null || string.IsNullOrWhiteSpace(chatInput.text))
            return;

        string playerMessage = chatInput.text;

        // 1. Oyuncunun yazdığını hemen ekrana (Chat'e) yazdır
        if (chatText != null)
        {
            chatText.text += $"\n\n<color=#00FF00><b>Sen:</b></color> {playerMessage}";
        }

        // 2. Mesajı Gemini beynine yolla! (İşte o sihirli bağlantı)
        AskGemini(playerMessage);

        // 3. Mesaj gittikten sonra oyuncu yeni bir şey yazabilsin diye kutunun içini temizle
        chatInput.text = "";
    }
}