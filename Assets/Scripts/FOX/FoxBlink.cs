using UnityEngine;
using System.Collections;

public class FoxBlink : MonoBehaviour
{
    public SkinnedMeshRenderer tilkiYuzu;
    public int blinkBlendShapeIndex;

    void Start() => StartCoroutine(BlinkRoutine());

    IEnumerator BlinkRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(2f, 5f));
            tilkiYuzu.SetBlendShapeWeight(blinkBlendShapeIndex, 100f);
            yield return new WaitForSeconds(0.12f);
            tilkiYuzu.SetBlendShapeWeight(blinkBlendShapeIndex, 0f);
        }
    }
}
