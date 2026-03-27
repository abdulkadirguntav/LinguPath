using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "SwipeGame/Card Data")]
public class CardDataSO : ScriptableObject
{
    public Sprite cardSprite;
    [TextArea(2,4)]
    public string correctSentences;

    [TextArea(2,4)]
    public string wrongSentences;
}
