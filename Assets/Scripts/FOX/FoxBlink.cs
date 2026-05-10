using UnityEngine;
using System.Collections;

public class FoxBlink : MonoBehaviour
{
    public SkinnedMeshRenderer tilkiYuzu;
    public int blinkBlendShapeIndex;

    [Tooltip("Assign FoxEmotionController so blink doesn't get overridden by emotion lerping")]
    public FoxEmotionController emotionController;

    void Start()
    {
        StartCoroutine(BlinkRoutine());
    }

    IEnumerator BlinkRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(2f, 5f));

            SetBlink(100f);
            yield return new WaitForSeconds(0.12f);
            SetBlink(0f);
        }
    }

    void SetBlink(float weight)
    {
        if (emotionController != null)
            // Route through EmotionController so the lerp system owns this blend shape
            emotionController.SetBlendShapeTarget(blinkBlendShapeIndex, weight);
        else
            tilkiYuzu.SetBlendShapeWeight(blinkBlendShapeIndex, weight);
    }
}
