using UnityEngine;
using UnityEngine.EventSystems;

public class CardInstance : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private CardSlot currentSlot;
    public CardData data;
    
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        canvasGroup.blocksRaycasts = false; // чтобы слот видел дроп
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;

        // Проверяем, есть ли слот под мышкой
        if (eventData.pointerEnter != null)
        {
            CardSlot slot = eventData.pointerEnter.GetComponentInParent<CardSlot>();
            if (slot != null)
            {
                slot.UpdatePlaceholder(eventData); // говорим слоту, где показать placeholder
                currentSlot = slot;
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        if (currentSlot != null)
        {
            currentSlot.PlaceCard(this, eventData);
            currentSlot = null;
        }
        else
        {
            // вернём карту назад
            transform.SetParent(originalParent);
            transform.localPosition = Vector3.zero;
        }
    }
}
