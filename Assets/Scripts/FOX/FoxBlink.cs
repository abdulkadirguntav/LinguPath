using UnityEngine;
using System.Collections;

public class FoxBlink : MonoBehaviour
{
    public SkinnedMeshRenderer tilkiYuzu;
    public int blinkBlendShapeIndex; // Tilkinin listesindeki "Göz Kırpma / Blink" sırası
    
    void Start()
    {
        StartCoroutine(BlinkRoutine());
    }

    IEnumerator BlinkRoutine()
    {
        while (true)
        {
            // 2 ile 5 saniye arası rastgele bekle
            yield return new WaitForSeconds(Random.Range(2f, 5f)); 

            // Gözü kapat
            tilkiYuzu.SetBlendShapeWeight(blinkBlendShapeIndex, 100f);
            yield return new WaitForSeconds(0.15f); // 0.15 saniye kapalı kalsın (hızlı kırpma)
            
            // Gözü aç
            tilkiYuzu.SetBlendShapeWeight(blinkBlendShapeIndex, 0f);
        }
    }
}