using System.Collections.Generic;
using UnityEngine;
using TMPro; // Ekrana yazdıracağımız liste için gerekli

[System.Serializable]
public class MarketItem
{
    public int id;
    public string englishWord;
    public string category;
    public Sprite itemIcon;
}

public class CSV_Market : MonoBehaviour
{
    [Header("Veritabanı (Otomatik Dolacak)")]
    public List<MarketItem> allMarketItems = new List<MarketItem>();

    [Header("Bağlantılar (Inspector'dan Sürükle)")]
    public MarketManager cartManager;     // Senin yazdığın sepet kodunun olduğu obje
    public GameObject itemPrefab;         // Prefabs klasörüne attığımız kalıp buton
    public Transform contentParent;       // ScrollView içindeki Content objesi
    public TextMeshProUGUI questListText; // Sağ üstteki "Alınacaklar" yazısı

    void Start()
    {
        LoadMarketData();
        // Şimdilik test için oyun başlar başlamaz 3 görev, 12 raf ürünüyle başlatalım
        StartMarketGame(3, 36); 
    }

    private void LoadMarketData()
    {
        TextAsset csvData = Resources.Load<TextAsset>("urunler");
        if (csvData == null) { Debug.LogError("CSV bulunamadı!"); return; }

        string[] dataLines = csvData.text.Split(new char[] { '\n' });
        for (int i = 1; i < dataLines.Length; i++)
        {
            string line = dataLines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] columns = line.Split(new char[] { ',', ';' });
            if (columns.Length >= 3)
            {
                MarketItem newItem = new MarketItem();
                newItem.id = int.Parse(columns[0]);
                newItem.englishWord = columns[1].Trim();
                newItem.category = columns[2].Trim();
                newItem.itemIcon = Resources.Load<Sprite>("Icons/" + newItem.englishWord);
                
                allMarketItems.Add(newItem);
            }
        }
    }

    // İŞTE OYUNU KURAN ANA FONKSİYON
    public void StartMarketGame(int requestedItemCount, int totalSlots)
    {
        if(allMarketItems.Count < totalSlots) return;

        // 1. Tüm veritabanını çorba gibi karıştır (Shuffle)
        ShuffleList(allMarketItems);

        // 2. NPC'nin isteyeceği 3 ürünü seç
        List<MarketItem> questItems = new List<MarketItem>();
        cartManager.npcList.Clear(); // Senin kodundaki eski listeyi temizle
        string questString = "Shopping List:\n\n";

        for(int i = 0; i < requestedItemCount; i++)
        {
            questItems.Add(allMarketItems[i]);
            // Senin sepet koduna "NPC bunları istiyor" diye haber veriyoruz!
            cartManager.npcList.Add(allMarketItems[i].englishWord); 
            questString += "- " + allMarketItems[i].englishWord + "\n";
        }
        
        // Sağ üstteki UI yazısını güncelle
        if(questListText != null) questListText.text = questString;

        // 3. Raflara dizilecek toplam 12 ürünü ayarla (İlk 3'ü doğru, 9'u rastgele sahte)
        List<MarketItem> itemsToSpawn = new List<MarketItem>();
        for(int i = 0; i < totalSlots; i++)
        {
            itemsToSpawn.Add(allMarketItems[i]);
        }

        // Doğrular hep ilk 3 sırada (en başta) çıkmasın diye bu 12'lik rafı da kendi içinde karıştır
        ShuffleList(itemsToSpawn);

        // 4. Eski rafları temizle (Oyuncu tekrar oynarsa diye eski butonları siler)
        foreach(Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // 5. Prefabları Content içine bas ve içlerini resimlerle doldur!
        foreach(MarketItem item in itemsToSpawn)
        {
            GameObject newBtn = Instantiate(itemPrefab, contentParent);
            MarketItemButton btnScript = newBtn.GetComponent<MarketItemButton>();
            
            if(btnScript != null)
            {
                // Butona kimliğini ve sepet yöneticisini gönder
                btnScript.SetupItem(item.englishWord, item.itemIcon, cartManager);
            }
        }
    }

    // Listeleri rastgele karıştıran klasik mühendislik algoritması (Fisher-Yates)
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}