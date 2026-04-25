using System.Collections.Generic;
using UnityEngine;

// Inspector'da listelenebilmesi için bu etiketleri kullanıyoruz
[System.Serializable]
public class EmotionShape
{
    [Tooltip("Tilkinin BlendShape listesindeki sıra numarası")]
    public int shapeIndex; 
    
    [Tooltip("Bu kasın ne kadar hareket edeceği (0-100)")]
    public float targetWeight = 100f; 
}

[System.Serializable]
public class EmotionProfile
{
    [Tooltip("Gemini'den gelecek isim (happy, sad, surprised, angry, neutral)")]
    public string emotionName; 
    
    [Tooltip("Bu duygu için çalışacak yüz kasları")]
    public List<EmotionShape> shapes = new List<EmotionShape>();
}

public class FoxEmotionController : MonoBehaviour
{
    [Header("Bağlantılar")]
    public SkinnedMeshRenderer tilkiYuzu;
    
    [Tooltip("Duygular arası geçişin yumuşaklık hızı")]
    public float gecisHizi = 8f; 

    [Header("Duygu Kütüphanesi")]
    public List<EmotionProfile> duygular = new List<EmotionProfile>();

    // Arka planda hangi kasın hangi değere gideceğini tutan sözlük
    private Dictionary<int, float> hedefDegerler = new Dictionary<int, float>();

    void Start()
    {
        // Oyun başladığında modeldeki tüm Shape Key'leri sözlüğe 0 (kapalı) olarak ekle
        if (tilkiYuzu != null && tilkiYuzu.sharedMesh != null)
        {
            for (int i = 0; i < tilkiYuzu.sharedMesh.blendShapeCount; i++)
            {
                hedefDegerler[i] = 0f;
            }
        }
    }

    void Update()
    {
        if (tilkiYuzu == null) return;

        // Her karede, yüz kaslarını yavaşça ve yumuşakça hedef değerlerine (Lerp) kaydır
        foreach (var kvp in hedefDegerler)
        {
            float currentWeight = tilkiYuzu.GetBlendShapeWeight(kvp.Key);
            
            if (Mathf.Abs(currentWeight - kvp.Value) > 0.1f)
            {
                float newWeight = Mathf.Lerp(currentWeight, kvp.Value, Time.deltaTime * gecisHizi);
                tilkiYuzu.SetBlendShapeWeight(kvp.Key, newWeight);
            }
        }
    }

    // GeminiManager bu fonksiyonu çağırıp duyguyu ateşleyecek
    public void DuyguAyarla(string gelenDuygu)
    {
        if (string.IsNullOrEmpty(gelenDuygu)) return;
        
        // Gelen metni küçük harfe çevirip boşlukları temizle (Örn: " Happy " -> "happy")
        gelenDuygu = gelenDuygu.ToLower().Trim();

        // Yeni duygu geldiğinde, önce yüzü sıfırla (Eski duyguyu temizle)
        List<int> keys = new List<int>(hedefDegerler.Keys);
        foreach (int key in keys)
        {
            hedefDegerler[key] = 0f;
        }

        // Kütüphaneden gelen duyguya uygun profili bul
        EmotionProfile aktifDuygu = duygular.Find(d => d.emotionName == gelenDuygu);

        // Eğer duygu bulunduysa ("neutral" dışında), o kasların hedeflerini yükselt
        if (aktifDuygu != null)
        {
            foreach (var shape in aktifDuygu.shapes)
            {
                hedefDegerler[shape.shapeIndex] = shape.targetWeight;
            }
        }
    }
}