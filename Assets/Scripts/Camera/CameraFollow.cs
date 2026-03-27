using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // Takip edilecek karakter (Tilki)
    public float smoothSpeed = 0.125f; // Kameranın yumuşak gelme hızı (0 ile 1 arası)
    public Vector3 offset; // Kamera ile karakter arasındaki mesafe farkı

    void LateUpdate() // Karakter hareketini bitirdikten sonra çalışır (Titremeyi önler)
    {
        if (target == null) return; // Eğer karakter yoksa hata verme

        // 1. Kameranın gitmesi gereken hedef pozisyonu hesapla (Karakterin yeri + baştaki mesafe)
        Vector3 desiredPosition = target.position + offset;

        // 2. Oraya yumuşakça kay (Lerp fonksiyonu)
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // 3. Kamerayı yeni pozisyona taşı
        transform.position = smoothedPosition;
    }
}