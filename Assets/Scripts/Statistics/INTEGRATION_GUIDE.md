/*
 * ===============================================
 * TÜM OYUN MODLARINA ENTEGRASYON REHBERI
 * ===============================================
 * 
 * Sistem artık EVENT tabanlı çalışıyor!
 * Her oyun modunda cevapları kaydetmek için
 * GameEventSystem.LogAnswer() çağırmanız yeterli.
 */

// ═══════════════════════════════════════════════════════════════════
// 1️⃣ SWIPEMANAGER - KELIME EŞLEŞTİRME OYUNU
// ═══════════════════════════════════════════════════════════════════

/*
SwipeManager'ın CheckAnswer() veya benzeri yerinde ekle:

void CheckCardAnswer(bool isCorrect)
{
    if(isCorrect)
    {
        // ✅ Doğru cevap
        GameEventSystem.LogAnswer("Kelime Eşleştirme", true);
        // Puan, ses, animasyon vs...
    }
    else
    {
        // ❌ Yanlış cevap
        GameEventSystem.LogAnswer("Kelime Eşleştirme", false, "Yanlış kart seçildi");
        // Hayat kaybı, animasyon vs...
    }
}

// Oyun bittiğinde:
void OnGameOver()
{
    int finalScore = currentCardIndex * 10;
    bool didPlayerWin = currentHealth > 0; // Canı kaldı mı?
    
    GameEventSystem.LogGameEnd("Swipe Kartları", didPlayerWin, finalScore);
}
*/

// ═══════════════════════════════════════════════════════════════════
// 2️⃣ MARKETMANAGER - ALIŞVERIŞ LİSTESİ OYUNU
// ═══════════════════════════════════════════════════════════════════

/*
MarketManager'ın Checked() metodunda düzenle:

void Checked()
{
    if(currentItems.Count != npcList.Count)
    {
        Debug.Log("Wrong Order!");
        // ❌ Yanlış sayıda ürün
        GameEventSystem.LogAnswer("Market Alışverişi", false, "Ürün sayısı eşleşmiyor");
        currentItems.Clear();
        return;
    }

    // Kontrol kodları... (aynı kalabilir)

    if(isCorrect)
    {
        Debug.Log("Mission Complete!");
        // ✅ Doğru ürünler seçildi
        GameEventSystem.LogAnswer("Market Alışverişi", true);
        
        int score = CalculateScore(); // Hızını hesapla
        GameEventSystem.LogGameEnd("Market", true, score);
        
        currentItems.Clear();
        ExitMiniGame();
        // ...
    }
    else
    {
        Debug.Log("Mission Failed!");
        // ❌ Yanlış ürünler seçildi
        GameEventSystem.LogAnswer("Market Alışverişi", false, "Yanlış ürün seçildi");
        GameEventSystem.LogGameEnd("Market", false, 0);
        
        currentItems.Clear();
        // ...
    }
}
*/

// ═══════════════════════════════════════════════════════════════════
// 3️⃣ TURNBASEMANAGER - SAVAŞ OYUNU
// ═══════════════════════════════════════════════════════════════════

/*
TurnBaseManager'da:

// Oyuncu cümle yapıyı doğru yaşladığında:
void OnPlayerAttack(bool answeredCorrectly)
{
    if(answeredCorrectly)
    {
        // ✅ Cümle doğru oluşturuldu
        GameEventSystem.LogAnswer("Savaş - Cümle Kurma", true);
        
        enemyHP -= baseDamage;
        // Hasar animasyonu vs...
    }
    else
    {
        // ❌ Cümle yanlış oluşturuldu
        GameEventSystem.LogAnswer("Savaş - Cümle Kurma", false, "Yanlış cümle sırası");
        
        // Saldırı başarısız
        Debug.Log("Saldırı başarısız!");
    }

    // Tur bittiğinde
    currnetState = GameState.EnemyTurn;
}

// Oyun bittiğinde:
void OnGameOver(bool playerWon)
{
    int finalScore = (int)(100 - enemyHP); // Hasar puanı
    
    GameEventSystem.LogGameEnd("Savaş", playerWon, finalScore);
    
    if(playerWon)
    {
        Debug.Log("🏆 Oyun Kazandı!");
    }
    else
    {
        Debug.Log("💀 Oyun Kaybetti!");
    }
}
*/

// ═══════════════════════════════════════════════════════════════════
// 4️⃣ GEMINMANAGER - KONUŞMATESİ (Zaten Yapılmış!)
// ═══════════════════════════════════════════════════════════════════

/*
GeminiManager'da ParseGeminiResponse() metodunda (zaten ekli!):

if(!grammar.ToLower().Contains("kusursuz"))
{
    // ❌ Gramer hatası
    GameEventSystem.LogAnswer("Konuşma - Gramer", false, grammar);
}
else
{
    // ✅ Kusursuz cevap
    GameEventSystem.LogAnswer("Konuşma - Akıcılık", true);
}
*/

// ═══════════════════════════════════════════════════════════════════
// 5️⃣ KENDİ OYUNUNUZ - TEMPLATE
// ═══════════════════════════════════════════════════════════════════

/*
YourGameManager'da:

void CheckAnswer(string playerAnswer, string correctAnswer)
{
    bool isCorrect = playerAnswer.ToLower() == correctAnswer.ToLower();
    
    if(isCorrect)
    {
        // ✅ Doğru
        GameEventSystem.LogAnswer("Oyun Adı", true);
    }
    else
    {
        // ❌ Yanlış
        GameEventSystem.LogAnswer("Oyun Adı", false, $"Beklenen: {correctAnswer}, Verilen: {playerAnswer}");
    }
}

void OnGameComplete(bool won, int score)
{
    GameEventSystem.LogGameEnd("Oyun Adı", won, score);
}
*/

// ═══════════════════════════════════════════════════════════════════
// 📊 VERİ AKIŞI
// ═══════════════════════════════════════════════════════════════════

/*
SwipeManager/MarketManager/TurnBaseManager/GeminiManager
        ↓
GameEventSystem.LogAnswer("Konu", true/false, "Detaylar")
        ↓
StatisticsManager.HandleAnswerGiven()
        ↓
StatisticsManager.LogCorrectAnswer/LogWrongAnswer()
        ↓
StatisticsData güncellenir
        ↓
JSON/CSV'ye kaydedilir
        ↓
SimpleStatisticsPanel'de görüntülenir
*/

// ═══════════════════════════════════════════════════════════════════
// ⚡ QUICK COPY-PASTE KODU
// ═══════════════════════════════════════════════════════════════════

/*
Hızlıca oyununuza eklemek için:

// Doğru cevap:
GameEventSystem.LogAnswer("KONU_ADI", true);

// Yanlış cevap:
GameEventSystem.LogAnswer("KONU_ADI", false, "Hata detayları");

// Oyun bitişi:
GameEventSystem.LogGameEnd("Oyun Adı", playerWon, score);
*/

// ═══════════════════════════════════════════════════════════════════
// ✅ KONTROL LİSTESİ
// ═══════════════════════════════════════════════════════════════════

/*
Tüm oyun modları için yapılacak:

[ ] SwipeManager -> Cevap kontrolünde GameEventSystem.LogAnswer() ekle
[ ] MarketManager -> Checked() metodunda GameEventSystem.LogAnswer() ekle
[ ] TurnBaseManager -> Saldırı metodunda GameEventSystem.LogAnswer() ekle
[ ] GeminiManager -> Zaten yapılmış! ✅
[ ] Diğer oyunlar -> Yukarıdaki templateleri takip et
[ ] Test et -> Play mode'da konsolda "Cevap Kaydedildi" mesajı görün
[ ] SimpleStatisticsPanel ile istatistikleri gör
*/
