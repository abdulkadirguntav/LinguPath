using UnityEngine;
using System.IO;

public static class DataLogger
{
    // Dosyanın bilgisayarda gizlice kaydedileceği evrensel yol
    private static string dosyaYolu = Application.persistentDataPath + "/OyuncuVerileri.csv";

    // İstediğimiz yerden DataLogger.VeriKaydet() diyerek çağıracağımız fonksiyon
    public static void VeriKaydet(string kategori, string olay, string detay)
    {
        // Eğer oyun ilk defa açılıyorsa ve dosya yoksa, önce en üste başlıkları yaz (Tarih yok)
        if (!File.Exists(dosyaYolu))
        {
            string baslik = "Kategori,Olay,Detay\n";
            File.WriteAllText(dosyaYolu, baslik);
        }

        // CSV formatında aralara virgül koyarak satırı oluştur
        // (Detay kısmında oyuncu cümlesinde virgül geçerse kod bozulmasın diye onu tırnak içine alıyoruz)
        string yeniSatir = $"{kategori},{olay},\"{detay}\"\n";
        
        // Satırı dosyanın en altına gizlice ekle
        File.AppendAllText(dosyaYolu, yeniSatir);

        // Sadece senin Unity konsolunda görmen için ufak bir log
        Debug.Log("📊 ARKA PLAN VERİSİ KAYDEDİLDİ: " + yeniSatir);
    }
}