using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EmotionShape
{
    [Tooltip("BlendShape index on the fox mesh")]
    public int shapeIndex;

    [Tooltip("How much this muscle moves (0-100)")]
    public float targetWeight = 100f;
}

[System.Serializable]
public class EmotionProfile
{
    [Tooltip("Name from Gemini (happy, sad, surprised, angry, neutral)")]
    public string emotionName;

    [Tooltip("Face muscles active for this emotion")]
    public List<EmotionShape> shapes = new List<EmotionShape>();
}

public class FoxEmotionController : MonoBehaviour
{
    [Header("References")]
    public SkinnedMeshRenderer foxFace;

    [Tooltip("Smoothing speed for emotion transitions")]
    public float transitionSpeed = 8f;

    [Header("Emotion Library")]
    public List<EmotionProfile> emotions = new List<EmotionProfile>();

    private Dictionary<int, float> targetValues = new Dictionary<int, float>();

    void Start()
    {
        if (foxFace != null && foxFace.sharedMesh != null)
        {
            for (int i = 0; i < foxFace.sharedMesh.blendShapeCount; i++)
                targetValues[i] = 0f;
        }
    }

    void Update()
    {
        if (foxFace == null) return;

        foreach (var kvp in targetValues)
        {
            float currentWeight = foxFace.GetBlendShapeWeight(kvp.Key);
            if (Mathf.Abs(currentWeight - kvp.Value) > 0.1f)
            {
                float newWeight = Mathf.Lerp(currentWeight, kvp.Value, Time.deltaTime * transitionSpeed);
                foxFace.SetBlendShapeWeight(kvp.Key, newWeight);
            }
        }
    }

    public void SetEmotion(string incomingEmotion)
    {
        if (string.IsNullOrEmpty(incomingEmotion)) return;

        incomingEmotion = incomingEmotion.ToLower().Trim();

        // Reset all except reserved indices (e.g. blink)
        List<int> keys = new List<int>(targetValues.Keys);
        foreach (int key in keys)
        {
            if (!reservedIndices.Contains(key))
                targetValues[key] = 0f;
        }

        EmotionProfile activeEmotion = emotions.Find(d => d.emotionName == incomingEmotion);

        if (activeEmotion != null)
        {
            foreach (var shape in activeEmotion.shapes)
                targetValues[shape.shapeIndex] = shape.targetWeight;
        }
    }

    // Used by FoxBlink to bypass emotion resets and drive the blink blend shape directly.
    public void SetBlendShapeTarget(int index, float value)
    {
        if (targetValues.ContainsKey(index))
        {
            targetValues[index] = value;
            if (value > 0f && !reservedIndices.Contains(index))
                reservedIndices.Add(index);
            else if (value <= 0f)
                reservedIndices.Remove(index);
        }
    }

    [HideInInspector]
    public List<int> reservedIndices = new List<int>();
}
