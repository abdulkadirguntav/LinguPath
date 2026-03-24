using UnityEngine;
using UnityEngine.EventSystems;

public class SwipeCard : MonoBehaviour , IDragHandler , IEndDragHandler
{
    public SwipeManager manager;
    public void OnDrag(PointerEventData eventData)
    {
        transform.localPosition += (Vector3)eventData.delta; 
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        float distanceX = transform.localPosition.x;

        if(distanceX > 150)
        {
            Debug.Log("Right Swipe");
            manager.CardSwiped(true);
        }
        else if(distanceX < -150)
        {
            Debug.Log("Left Swipe");
            manager.CardSwiped(false);
        }
        else
        {
            transform.localPosition = Vector3.zero;
        }
    }
}
