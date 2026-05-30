# LinguPath

**Yapay Zeka Destekli Mobil İngilizce Öğrenme Oyunu**

LinguPath, oyunlaştırma (gamification) ve büyük dil modeli (LLM) teknolojilerini bir araya getiren bir Ciddi Oyun (Serious Game) projesidir. Oyuncular, low-poly bir kasaba ortamında NPC karakterleriyle etkileşime girerek farklı mini oyunlar aracılığıyla İngilizce öğrenir.

> Nevşehir Hacı Bektaş Veli Üniversitesi — Bilgisayar Mühendisliği Lisans Bitirme Projesi (2026)
> **Geliştirici:** Abdülkadir Güntav

---

## Oyun Mekanikleri

Her NPC bir mini oyunu tetikler. Oyunu kazanırsan NPC'nin durumu "tamamlandı" olarak kalıcı kaydedilir.

### Swipe Game
Ekranda bir görsel ve cümle çifti belirir. Cümle görselle doğru eşleşiyorsa sağa, yanlışsa sola swipe et. Her turda 7 kart gösterilir, 3 hakkın var.  
Veri kaynağı: `Resources/SwipeCards.csv`

### Word Learning — Kütüphane
Her seansta 10 kelime gösterilir. Önce İngilizcesi ve örnek cümle görünür; "Göster" butonuyla Türkçe çevirisi açılır. `KNOW` ile masteriyi artır (0–3), `STUDY` ile kelimeyi desteye geri at. Her ziyarette 10'ar kelimelik sayfalar sırayla ilerler.  
Veri kaynağı: `Resources/Words/` (ScriptableObject)

### Market Game — Süpermarket
NPC sana İngilizce bir alışveriş listesi verir. Sahnedeki ürünlere tıklayarak sepete ekle, ardından "Checkout" ile kontrol et. Liste sıra bağımsız karşılaştırılır.

### Football Duel — Sıra Tabanlı Kelime Oyunu
Sıra tabanlı iki fazdan oluşur:
- **Hücum (15 sn):** Türkçe ipucuyla verilen cümleyi, karışık kelimelerden doğru sırada kurarak gol at.
- **Savunma (5 sn):** 4 Kelime arasından diğerlerinden farklı olan kelimeyi seç; yanlış seçersen gol yersin.

5 gol atan oyunu kazanır.  
Veri kaynağı: `Resources/Sentences.csv`, `Resources/Defense.csv`

### AI Fox Chat — Gemini Sohbet
Tilki maskotu Foxy ile serbest İngilizce konuşma pratiği yap. Her yanıtta gramer geri bildirimi verilir. Hem yazılı hem sesli (mikrofon) girişi desteklenir; ses kaydı WAV formatında Gemini'ye gönderilip metne dönüştürülür. Oculus LipSync SDK ile dudak senkronizasyonu sağlanır.  
Model: `gemini-2.5-flash` — API anahtarı `StreamingAssets/api_config.json` dosyasından okunur.

---

## Teknik Mimari

```
Assets/
├── Scripts/
│   ├── AI/                  # GeminiManager, MicrophoneInputManager
│   ├── Camera/              # CameraFollow
│   ├── Character/           # Karakter oluşturucu ve kayıt sistemi
│   ├── Core/                # CoreSettingsManager
│   ├── CSV/                 # CSV okuma (market, swipe)
│   ├── FOX/                 # FoxBlink animasyonu
│   ├── Game/
│   │   ├── SwipeGame/       # SwipeManager, SwipeCard
│   │   ├── MarketGame/      # MarketManager, CartItemUI, MarketItemButton
│   │   └── TurnBaseGame/    # TurnBaseManager
│   ├── Library/             # LibraryManager (kelime öğrenme)
│   ├── Main Menu/           # MainMenuManager, AudioManager, SettingsManager
│   ├── NPC's/               # NPC diyalog ve görev sistemi
│   ├── Player/              # PlayerMovement (joystick destekli)
│   ├── PlayerPref/          # DataManager, WordProgress
│   ├── SaveSystem/          # 3 slotlu JSON kayıt sistemi
│   └── Scriptable Object/   # CardDataSO, NPCDialogueSO, WordDataSO
├── Scene/
│   ├── MainMenu.unity
│   └── Core.unity           # Kasaba haritası + tüm mini oyunlar
└── Resources/
    ├── SwipeCards.csv
    ├── Sentences.csv
    ├── Defense.csv
    ├── Icons/               # Market ürün görselleri
    ├── SwipeImage/          # Swipe kartı görselleri
    └── Words/               # WordDataSO varlıkları
```

### Kayıt Sistemi
3 bağımsız oyun slotu `Application.persistentDataPath/slot_N.json` dosyasına JSON olarak yazılır. Her slotta karakter verisi, kelime ilerleme seviyeleri (0–3) ve NPC görev durumları saklanır.

### NPC Sistemi
Her NPC üç durumdan birinde bulunur: `BeforeMission` / `MissionFailed` / `MissionCompleted`. Oyuncu yaklaştığında `NPCDialogueSO` üzerinden diyalog başlar, diyalog bitince mini oyun açılır. Görev sonucu anında diske yazılır.

---

## Kullanılan Teknolojiler

| Teknoloji | Kullanım |
|---|---|
| Unity 6000.0.67f1 LTS | Oyun motoru |
| C# | Oyun mantığı, UI, NPC sistemi |
| Google Gemini 2.5 Flash API | AI sohbet + ses transkripsiyon |
| Oculus LipSync SDK | Foxy dudak senkronizasyonu |
| Newtonsoft.Json | API yanıt ayrıştırma |
| TextMeshPro | UI metinleri |
| SimplePoly City | Kasaba low-poly varlıkları |
| Joystick Pack | Mobil hareket kontrolü |

---

## Kurulum

### Gereksinimler
- Unity 6000.0.67f1 LTS + Android Build Support modülü
- Google Gemini API anahtarı (`gemini-2.5-flash` erişimi)

### Adımlar

1. Repoyu klonla ve Unity Hub'dan projeyi aç.

2. `Assets/StreamingAssets/` klasöründe `api_config.json` oluştur:
   ```json
   {
     "geminiApiKey": "BURAYA_API_ANAHTARINI_YAZ"
   }
   ```

3. `File > Build Settings` → Android → `Build`.

4. APK'yı Android 5.1+ (API 22+) cihaza yükle.

> Mikrofon özelliği için cihazda mikrofon izni gerekir; uygulama ilk kullanımda otomatik olarak ister.

---

## Veri Formatları

| Dosya | Format |
|---|---|
| `SwipeCards.csv` | `id;imageName;correctSentence;wrongSentence` |
| `Sentences.csv` | `id;word1 word2 word3;trap1,trap2;Türkçe İpucu` |
| `Defense.csv` | `id;oddWord;normal1,normal2,normal3` |
| `WordDataSO` | ScriptableObject: `wordID`, `englishWord`, `turkishMeaning`, `exampleSentences` |

---

## Gelecek Planlar

- [ ] Kelime kalıcılığı ölçümü ve istatistik ekranı
- [ ] Çoklu dil desteği (Almanca, Fransızca)
- [ ] Çevrimiçi çok oyunculu Football Duel modu
- [ ] iOS desteği

---

## Kaynaklar

- Prensky, M. (2001). *Digital Game-Based Learning*. McGraw-Hill.
- Deterding, S. et al. (2011). From game design elements to gamefulness: Defining gamification. *ACM MindTrek Conference*.
- [Unity Documentation](https://docs.unity3d.com)
- [Google Gemini API Docs](https://ai.google.dev/docs)
- [Oculus LipSync SDK](https://developer.oculus.com/documentation/unity/audio-ovrlipsync-unity)

---

## Lisans

Bu proje lisans bitirme ödevi kapsamında geliştirilmiştir.  
© 2026 Abdülkadir Güntav
