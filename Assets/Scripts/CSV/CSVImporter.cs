#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class CSVImporter : EditorWindow
{
    // Unity'nin üst menüsüne kendi özel butonumuzu ekliyoruz
    [MenuItem("Araçlarım/Kelimeleri Otomatik Üret")]
    public static void KelimeleriUret()
    {
        // 1. CSV dosyamızın tam yolunu bul (Assets klasörünün direkt içinde olmalı)
        string path = Application.dataPath + "/kelimeler.csv";

        if (!File.Exists(path))
        {
            Debug.LogError("CSV dosyası bulunamadı! Lütfen Assets klasörünün ana dizinine 'kelimeler.csv' adında bir dosya koyun.");
            return;
        }

        // 2. Kelimeleri kaydedeceğimiz klasörü hazırla (Yoksa otomatik oluşturur)
        string saveFolder = "Assets/Resources/Words";
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Words"))
            AssetDatabase.CreateFolder("Assets/Resources", "Words");

        // 3. Encoding'i otomatik algıla (BOM varsa UTF-8, yoksa sistem default'u)
        string allText;
        using (var reader = new System.IO.StreamReader(path, System.Text.Encoding.Default, detectEncodingFromByteOrderMarks: true))
            allText = reader.ReadToEnd();
        string[] lines = allText.Split('\n');
        int sayac = 0;

        // 4. Her bir satırı dön
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            
            // Eğer satır boşsa atla
            if (string.IsNullOrWhiteSpace(line)) continue; 

            // Noktalı virgüle göre satırı 4 parçaya (ID, İng, Tür, Cümle) böl
            string[] data = line.Split(';'); 

            if (data.Length < 4)
            {
                Debug.LogWarning($"Satır {i+1} hatalı veya eksik veri içeriyor. Atlandı.");
                continue;
            }

            // 5. Havada yepyeni bir ScriptableObject oluştur
            WordDataSO yeniKelime = ScriptableObject.CreateInstance<WordDataSO>();

            // İçini bizim kelime paketindeki verilerle doldur (Trim() boşlukları temizler)
            yeniKelime.wordID = data[0].Trim();
            yeniKelime.englishWord = data[1].Trim();
            yeniKelime.turkishMeaning = data[2].Trim();
            yeniKelime.exampleSentences = data[3].Trim();

            // 6. Bu objeyi fiziksel bir '.asset' dosyası olarak klasöre kaydet
            string assetPath = saveFolder + "/" + yeniKelime.wordID + ".asset";
            AssetDatabase.CreateAsset(yeniKelime, assetPath);
            
            sayac++;
        }

        // Tüm işlemleri Unity hafızasına yaz ve klasörleri yenile
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"🎉 Otomasyon Tamamlandı! Toplam {sayac} adet kelime üretildi. Klasör: {saveFolder}");
    }
}
#endif