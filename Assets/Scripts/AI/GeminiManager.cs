using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using TMPro;

public class GeminiManager : MonoBehaviour
{
    [Header("UI Ayarları")]
    public TextMeshProUGUI chatText;
    public TMP_InputField chatInput;

    [Header("Geliştirici Ayarları")]
    public bool testModu = true; // API bozukken bunu tikle, çalışırken tiki kaldır!

    [Header("API Ayarları")]
    // DİKKAT: Eski API anahtarını sildim, Google AI Studio'dan YENİ BİR TANE alıp buraya (ve Unity Inspector'a) yapıştır!
    public string apiKey = "AIzaSyB041H4Kc1JRGJHrk6FS5cxnuvwzssPNeg"; 
    
    [Header("Senaryo Ayarı")]
    public string gecerliSenaryo = "Kafe Garsonu";

    // 🚀 ÇÖZÜM 1: 2026 Yılı İçin Geçerli Olan Aktif Model (Gemini 2.5 Flash)
    private string endpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

    public void AskGemini(string playerMessage)
    {
        if (testModu)
        {
            Debug.Log("🛠️ TEST MODU AKTİF: API bypass ediliyor, sahte cevap üretiliyor...");
            StartCoroutine(SimulateFakeResponse()); // Sahte cevaba git
        }
        else
        {
            StartCoroutine(SendRequest(playerMessage, gecerliSenaryo)); // Gerçek API'ye git
        }
    }

    private IEnumerator SendRequest(string playerMessage, string scenario)
    {
        string url = endpoint + "?key=" + apiKey;

        string systemPrompt = $"Sen bir İngilizce dil mentorusun. Şu anki rolün: {scenario}. " +
            "Oyuncu sana İngilizce bir şey söyleyecek. Rolden çıkmadan ona İngilizce cevap ver. " +
            "Ayrıca oyuncunun cümlesindeki gramer hatalarını kontrol et. " +
            "Bana SADECE şu formatta bir JSON döndür, başka hiçbir açıklama, selamlama veya markdown yazma: " +
            "{\"reply\": \"rolüne uygun İngilizce cevabın\", \"grammar_feedback\": \"varsa gramer hatası ve Türkçe açıklaması, yoksa 'Kusursuz!' yaz\"}";

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

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            Debug.Log("🦊 Tilki Düşünüyor... (API'ye İstek Gitti)");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("API Hatası: " + request.error);
                Debug.LogError(request.downloadHandler.text); 
            }
            else
            {
                ParseGeminiResponse(request.downloadHandler.text);
            }
        }
    }

    // Google sunucuları çöktüğünde projeyi geliştirmeye devam etmemizi sağlayan fonksiyon
    private IEnumerator SimulateFakeResponse()
    {
        // İnternet gecikmesini taklit etmek için 1.5 saniye bekle
        yield return new WaitForSeconds(1.5f); 

        // Sanki Gemini'den gelmiş gibi kusursuz bir JSON paketi hazırlıyoruz
        string fakeJson = "{\n" +
            "  \"candidates\": [\n" +
            "    {\n" +
            "      \"content\": {\n" +
            "        \"parts\": [\n" +
            "          {\n" +
            "            \"text\": \"{\\\"reply\\\": \\\"Hello! Yes, I can certainly help you with that. Would you prefer a hot or cold drink?\\\", \\\"grammar_feedback\\\": \\\"Kusursuz!\\\"}\"\n" +
            "          }\n" +
            "        ]\n" +
            "      }\n" +
            "    }\n" +
            "  ]\n" +
            "}";

        // Kendi yazdığımız ayıklayıcıya (Parser) bu sahte veriyi yolluyoruz
        ParseGeminiResponse(fakeJson);
    }

    private void ParseGeminiResponse(string jsonResponse)
    {
        try
        {
            // Önce Gemini'nin karmaşık cevabından bizim asıl metnimizi çekiyoruz
            JObject data = JObject.Parse(jsonResponse);
            string textResult = data["candidates"][0]["content"]["parts"][0]["text"].ToString();

            // Kendi zorladığımız o temiz JSON formatını okuyoruz
            JObject finalData = JObject.Parse(textResult);

            string reply = finalData["reply"].ToString();
            string grammar = finalData["grammar_feedback"].ToString();

            Debug.Log("💬 TİLKİ CEVABI: " + reply);
            Debug.Log("📝 GRAMER NOTU: " + grammar);

            // UI EKRANINA YAZDIRMA KISMI BURADA OLMALI
            if (chatText != null)
            {
                chatText.text += $"\n<color=#FFA500><b>Tilki:</b></color> {reply}";
                
                // Eğer gramer hatası yoksa ekranda kalabalık yapmasın, varsa yazsın
                if(grammar.ToLower() != "kusursuz!" && !grammar.Contains("kusursuz"))
                {
                     chatText.text += $"\n<color=#FFFF00><i>(Not: {grammar})</i></color>";
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Veri Ayıklama Hatası: " + e.Message + " | Gelen Ham Veri: " + jsonResponse);
        }
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