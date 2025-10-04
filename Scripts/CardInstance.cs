using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class CardInstance : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private CardSlot currentSlot;

    [Header("Данные карты")]
    public CardData data;

    [Header("UI текста")]
    public TMP_Text atkText;
    public TMP_Text hpText;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        UpdateUI(); // сразу показываем атк/хп
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

    public void ReturnToHand()
    {
        // просто возвращаем позицию в панели руки
        transform.SetParent(DeckManager.Instance.handPanel);
        transform.localPosition = Vector3.zero;
    }

    /// <summary>
    /// Обновление UI (атк/хп) на карте
    /// </summary>
    public void UpdateUI()
    {
        if (data == null) return;

        if (atkText != null)
            atkText.text = data.attack.ToString();

        if (hpText != null)
            hpText.text = data.health.ToString();
    }
}
