using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public class GeminiManager : MonoBehaviour
{
    [Header("API Ayarları")]
    // DİKKAT: Eski API anahtarını sildim, Google AI Studio'dan YENİ BİR TANE alıp buraya (ve Unity Inspector'a) yapıştır!
    public string apiKey = "AIzaSyB041H4Kc1JRGJHrk6FS5cxnuvwzssPNeg"; 
    
    [Header("Senaryo Ayarı")]
    public string gecerliSenaryo = "Kafe Garsonu";

    // 🚀 ÇÖZÜM 1: 2026 Yılı İçin Geçerli Olan Aktif Model (Gemini 2.5 Flash)
    private string endpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

    public void AskGemini(string playerMessage)
    {
        StartCoroutine(SendRequest(playerMessage, gecerliSenaryo));
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

    private void ParseGeminiResponse(string jsonResponse)
    {
        try
        {
            JObject data = JObject.Parse(jsonResponse);
            string textResult = data["candidates"][0]["content"]["parts"][0]["text"].ToString();

            JObject finalData = JObject.Parse(textResult);

            string reply = finalData["reply"].ToString();
            string grammar = finalData["grammar_feedback"].ToString();

            Debug.Log("💬 TİLKİ CEVABI: " + reply);
            Debug.Log("📝 GRAMER NOTU: " + grammar);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Veri Ayıklama Hatası: " + e.Message + " | Gelen Ham Veri: " + jsonResponse);
        }
    }
}