using UnityEngine;
using UnityEngine.EventSystems;

public class SwipeCard : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public SwipeManager manager;
    
    private RectTransform rectTransform;
    private Vector2 startPosition;

    void Start()
    {
        // UI elemanı olduğu için RectTransform kullanıyoruz
        rectTransform = GetComponent<RectTransform>();
        
        // Oyun başladığında kartın tam merkez noktasını hafızaya al
        startPosition = rectTransform.anchoredPosition; 
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Parmağı takip et
        rectTransform.anchoredPosition += eventData.delta; 
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Kart merkezden ne kadar uzaklaştı?
        float distanceX = rectTransform.anchoredPosition.x - startPosition.x;

        if(distanceX > 150)
        {
            Debug.Log("Right Swipe");
            if(manager != null) manager.CardSwiped(true);
            ResetPosition(); // Yönlendir ve hemen merkeze dön
        }
        else if(distanceX < -150)
        {
            Debug.Log("Left Swipe");
            if(manager != null) manager.CardSwiped(false);
            ResetPosition(); // Yönlendir ve hemen merkeze dön
        }
        else
        {
            // Yeterince uzağa çekilmediyse direkt merkeze dön
            ResetPosition(); 
        }
    }

    // Kartı lastik gibi başladığı yere (ortaya) fırlatan fonksiyon
    public void ResetPosition()
    {
        if(rectTransform != null)
        {
            rectTransform.anchoredPosition = startPosition;
        }
    }
}