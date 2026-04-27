# 📊 İSTATİSTİK SİSTEMİ KURULUM REHBERİ

## 🎯 Sistem Özeti
Bu sistem oyuncunun **konulara göre** yanlışlarını ve doğrularını analiz eder. Her yanlıştan **ne tür hata yaptığını** kaydeder (gramer hatası, soru yapısı, vs).

---

## 📁 Oluşturulan Dosyalar

1. **TopicStatistics.cs** - Veri yapıları (TopicStats, StatisticsData)
2. **StatisticsManager.cs** - Tüm veriyi yönetir ve analiz eder
3. **StatisticsLogger.cs** - Kolay kayıt fonksiyonları
4. **StatisticsUI.cs** - Detaylı UI (prefab gerekli)
5. **SimpleStatisticsPanel.cs** - Otomatik UI (prefab SİZE gerekli)
6. **GeminiManager.cs** - Güncellenmiş (istatistik kaydını ekledim)

---

## 🚀 HIZLI BAŞLAMA (En Kolay Yol)

### Adım 1: StatisticsManager'ı Sahnede Oluştur
```
1. Scene'de boş bir GameObject oluştur
2. Adını "StatisticsManager" yap
3. StatisticsManager.cs scriptini buna ekle
```

### Adım 2: SimpleStatisticsPanel'i Ekle
```
1. Canvas'ınızda boş bir GameObject oluştur (adı: "StatisticsUI")
2. SimpleStatisticsPanel.cs scriptini buna ekle
3. "Create UI On Start" = TRUE yap (default'ta öyle)
```

### Adım 3: Açı Kapatı Butonu (İsteğe bağlı)
```
Canvas'da bir button oluştur (adı: "StatsButton")
Inspector'da SimpleStatisticsPanel'in "Toggle Button" alanına bunu sürükle
```

**Artık istatistikler otomatik olarak toplanacak!** ✅

---

## 📝 KODDAN İSTATİSTİK KAYDETME

### Örnek 1: GeminiManager'da (Zaten Yapılmış)
```csharp
// Doğru cevap
StatisticsLogger.LogCorrectAnswer("Konuşma Akıcılığı");

// Yanlış cevap
StatisticsLogger.LogWrongAnswer("Fiil Zamanı", "Wrong tense used");
```

### Örnek 2: SwipeManager'da
```csharp
void CheckAnswer(bool isCorrect)
{
    if(isCorrect)
        StatisticsLogger.LogSwipeAnswer(true, cardType.name);
    else
        StatisticsLogger.LogSwipeAnswer(false, cardType.name);
}
```

### Örnek 3: MarketManager'da
```csharp
void OnItemSelected(string itemName, bool isCorrect)
{
    if(isCorrect)
        StatisticsLogger.LogMarketAnswer(true, itemName);
    else
        StatisticsLogger.LogMarketAnswer(false, itemName);
}
```

### Örnek 4: TurnBaseManager'da
```csharp
void OnQuestionAnswered(bool correct, string questionType)
{
    StatisticsLogger.LogTurnBaseBattle(correct, questionType);
}
```

---

## 🎨 KENDİ UI'NI OLUŞTURMA

StatisticsUI.cs kullanarak daha detaylı UI yapmak istersen:

### Adım 1: Prefab Oluştur
```
Canvas > Image (adı: "TopicStatItem") oluştur
- Child'ler: Title (TextMeshPro), Stats (TextMeshPro), ProgressBar (Image)
```

### Adım 2: StatisticsUI Scriptini Ekle
```
Canvas'da StatisticsUI.cs ekle
Tüm referansları (containers, buttons, prefab) doldur
```

---

## 📊 MAVİ KONULAR (Topics)

Sistem bu konuları otomatik olarak çıkarır:

- 🎯 Fiil Zamanı (Verb Tenses)
- 🎯 Konu Cümlesi (Subject Sentences)
- 🎯 Sıfatlandırma (Adjectives)
- 🎯 İsim Cümleleri (Nouns)
- 🎯 Zamir Kullanımı (Pronouns)
- 🎯 Noktalama İşaretleri (Punctuation)
- 🎯 Soru Yapısı (Questions)
- 🎯 Ek Kullanımı (Suffixes)
- 🎯 Diğer Hatalar

---

## 🔧 AYARLAMALAR

### StatisticsManager'da Konu Haritası Değiştir
```csharp
private Dictionary<string, string> grammarTopicMap = new Dictionary<string, string>()
{
    { "konu", "Konu Cümlesi" },
    { "fiil", "Fiil Zamanı" },
    // Kendi konularını ekle!
};
```

### Hata Mesajından Konu Çıkartma
```csharp
private string ExtractTopicFromError(string errorMessage)
{
    if (errorMessage.ToLower().Contains("sarı_kelime"))
        return "Senin_Konun";
    // ...
}
```

---

## 💾 VERİ DOSYALARI

- **OyuncuVerileri.csv** - Ham log verileri
- **statistics.json** - İşlenmiş istatistikler

Lokasyon: `Application.persistentDataPath`

---

## 🎮 İSTATİSTİKLERE ERIŞME

### Koddan
```csharp
// Tüm konuları al
List<TopicStats> allTopics = StatisticsManager.instance.GetAllTopics();

// En zayıfını al
TopicStats weakest = StatisticsManager.instance.GetWeakestTopics(1)[0];

// Belirli konuyu al
TopicStats fiilStats = StatisticsManager.instance.GetTopicStats("Fiil Zamanı");

// Genel başarı
float successRate = StatisticsManager.instance.GetOverallSuccessRate();
```

---

## 🧹 TEMİZLEME

```csharp
// Tüm istatistikleri sıfırla
StatisticsManager.instance.ResetAllStatistics();

// Veya UI'den sıfırla butonu ekle
public void ResetStatistics()
{
    StatisticsManager.instance.ResetAllStatistics();
}
```

---

## 📋 KONTROL LİSTESİ

- [ ] StatisticsManager GameObject'ine ekledim
- [ ] SimpleStatisticsPanel'i Canvas'a ekledim
- [ ] GeminiManager'da statisticsManager referansını ekledim
- [ ] Diğer oyun modlarında StatisticsLogger çağrılarını ekledim
- [ ] Test ettim ve veri kaydediliyor mu diye kontrol ettim

---

## 🐛 SORUN GIDERME

**Veri toplanmıyor mu?**
- Console'da hata var mı kontrol et
- StatisticsManager boş GameObject'e mi ekli?
- GeminiManager'da statisticsManager referansı dolu mu?

**UI açılmıyor mu?**
- SimpleStatisticsPanel'in "Create UI On Start" = TRUE?
- Canvas var mı?

**Yanlış konular gösteriliyor mu?**
- GeminiManager.ExtractTopicFromError() methodu kontrol et
- Gemini'nin dönderdiği hata mesajını console'da bak

---

## 🔄 VERİ AKIŞI

```
Oyuncu → Cevap Verir
         ↓
    GeminiManager → Gemini API
         ↓
    Gramer Kontrol (Doğru/Yanlış)
         ↓
    GeminiManager.LogCorrectAnswer() veya LogWrongAnswer()
         ↓
    StatisticsManager (TopicStats güncelle)
         ↓
    DataLogger & statistics.json (kaydet)
         ↓
    SimpleStatisticsPanel (UI'de göster)
```

---

## 📞 DESTEK

Eğer eklemek istediğin başka bir oyun modu varsa:

```csharp
// Template
void OnGameEnd(bool playerWon)
{
    if(playerWon)
        StatisticsLogger.LogCorrectAnswer("Oyun_Adi");
    else
        StatisticsLogger.LogWrongAnswer("Oyun_Adi", "detay");
}
```

Bitti! 🎉
