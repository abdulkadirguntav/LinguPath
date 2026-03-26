using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LibraryManager : MonoBehaviour
{
    [Header("DataBase")]
    public List<WordDataSO> wordList = new List<WordDataSO>();
    private WordDataSO word; // O an ekranda olan kelime

    [Header(" UI ")]
    [SerializeField] private TMP_Text englishText;
    [SerializeField] private TMP_Text turkishText;
    [SerializeField] private TMP_Text exampleText;

    [Header("Reveal Mechanics")]
    [SerializeField] private GameObject revealButtonObj;

    // YENİ: Oyuncunun o anki oturumda çalıştığı geçici deste
    private List<WordDataSO> activeSessionDeck = new List<WordDataSO>();

    void Start()
    {
        // Şimdilik test için oyun başlar başlamaz 5 kelimelik bir deste oluştursun.
        // İleride arayüze 3-5-7-10 butonları koyduğunda bu satırı silip butonlara bağlayacağız.
        StartSession(5); 
    }

    // Arayüzdeki 3-5-7-10 butonlarına basıldığında çalışacak ana fonksiyon
    public void StartSession(int wordCount)
    {
        activeSessionDeck.Clear(); // Önceki desteyi temizle

        // 1. Havuzdan seviyesi 3'ten küçük olan (öğrenilmemiş) tüm kelimeleri geçici bir listeye al
        List<WordDataSO> availableWords = new List<WordDataSO>();
        foreach (WordDataSO w in wordList)
        {
            if (DataManager.instance.GetWordProgress(w.wordID).masteryLevel < 3)
            {
                availableWords.Add(w);
            }
        }

        // 2. Listeyi KARIŞTIR (Fisher-Yates Shuffle). 
        // Böylece oyuncu hep A, B, C kelimelerini sırayla görmez, her gün farklı kelimelerle karşılaşır.
        for (int i = 0; i < availableWords.Count; i++)
        {
            WordDataSO temp = availableWords[i];
            int randomIndex = Random.Range(i, availableWords.Count);
            availableWords[i] = availableWords[randomIndex];
            availableWords[randomIndex] = temp;
        }

        // 3. Oyuncunun istediği sayı kadar kelimeyi (Eğer o kadar kelime kaldıysa) Aktif Desteye at
        int limit = Mathf.Min(wordCount, availableWords.Count);
        for (int i = 0; i < limit; i++)
        {
            activeSessionDeck.Add(availableWords[i]);
        }

        // Eğer öğrenilecek kelime hiç kalmadıysa
        if (activeSessionDeck.Count == 0)
        {
            englishText.text = "TEBRİKLER!";
            exampleText.text = "Kütüphanedeki tüm kelimelerde ustalaştın.";
            turkishText.text = "";
            return;
        }

        // Destemiz hazır olduğuna göre ilk kelimeyi ekrana çek
        LoadNextWord();
    }

    void LoadNextWord()
    {
        // Eğer deste bittiyse (Oyuncu hepsine Biliyorum dediyse) oturumu bitir
        if (activeSessionDeck.Count == 0)
        {
            englishText.text = "OTURUM BİTTİ";
            exampleText.text = "Bugünkü hedefini tamamladın. Gidip biraz dinlenebilirsin!";
            turkishText.text = "";
            return;
        }

        // Destenin her zaman EN ÜSTÜNDEKİ (0. index) kelimeyi çek ve ekrana bas
        word = activeSessionDeck[0];

        englishText.text = word.englishWord;
        exampleText.text = word.exampleSentences;
        turkishText.text = word.turkishMeaning;

        turkishText.gameObject.SetActive(false);
        if(revealButtonObj != null) revealButtonObj.SetActive(true);
    }

    // --- BUTON FONKSİYONLARI ---

    public void KnownButton()
    {
        if (activeSessionDeck.Count == 0) return; // Deste boşsa butona basmayı engelle

        // 1. Seviyeyi artır ve kaydet
        WordProgress currentProgress = DataManager.instance.GetWordProgress(word.wordID);
        if (currentProgress.masteryLevel < 3)
        {
            currentProgress.masteryLevel++;
        }
        DataManager.instance.SaveData();

        // 2. BİLDİĞİ İÇİN DESTEDEN TAMAMEN AT (RemoveAt)
        activeSessionDeck.RemoveAt(0);

        // 3. Sıradaki kelimeye geç
        LoadNextWord();
    }

    public void StudyButton()
    {
        if (activeSessionDeck.Count == 0) return;

        // 1. Bilemediği için cezayı kes (Seviye 0) ve kaydet
        WordProgress currentProgress = DataManager.instance.GetWordProgress(word.wordID);
        currentProgress.masteryLevel = 0;
        DataManager.instance.SaveData();

        // 2. BİLEMEDİĞİ İÇİN DESTENİN EN ALTINA (SONUNA) AT
        // Önce kelimeyi aklımızda tutuyoruz, sonra en üstten silip, listenin en sonuna ekliyoruz.
        WordDataSO currentWord = activeSessionDeck[0];
        activeSessionDeck.RemoveAt(0);
        activeSessionDeck.Add(currentWord);

        // 3. Sıradaki kelimeye geç
        LoadNextWord();
    }

    // ÇEVİRİYİ GÖR BUTONU
    public void RevealTranslation()
    {
        turkishText.gameObject.SetActive(true); // Türkçe metni açığa çıkar
        if(revealButtonObj != null) revealButtonObj.SetActive(false); // Göz butonunu yok et
    }
}