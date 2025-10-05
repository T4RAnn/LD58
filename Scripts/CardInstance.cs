using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class CardInstance : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private CardSlot currentSlot;

    private Vector2 dragOffset; // смещение между курсором и картой

    [Header("Данные карты")]
    public CardData data;

    [Header("UI текста")]
    public TMP_Text atkText;
    public TMP_Text hpText;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        UpdateUI();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        canvasGroup.blocksRaycasts = false;
        HandLayout.DraggingCard = this;
        HandLayout.Instance.CreatePlaceholder(this);

        // считаем смещение между курсором и картой
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out dragOffset
        );
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPos
        );

        transform.localPosition = localPos - dragOffset;

        // проверяем слот под мышкой
        if (eventData.pointerEnter != null)
        {
            CardSlot slot = eventData.pointerEnter.GetComponentInParent<CardSlot>();
            if (slot != null)
            {
                slot.UpdatePlaceholder(eventData);
                currentSlot = slot;
            }
            else
            {
                currentSlot = null;
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
            ReturnToHand();
        }

        HandLayout.DraggingCard = null;
        HandLayout.Instance.DestroyPlaceholder();
    }

    public void ReturnToHand()
    {
        Transform handPanel = DeckManager.Instance.handPanel;

        // индекс и позиция плейсхолдера
        int targetIndex = handPanel.childCount;
        Vector3 targetPos = handPanel.position;

        if (HandLayout.Instance != null && HandLayout.Instance.placeholder != null)
        {
            targetIndex = HandLayout.Instance.placeholder.transform.GetSiblingIndex();
            targetPos = HandLayout.Instance.placeholder.transform.position;
        }

        // временно оставляем карту под тем же Canvas
        transform.SetParent(handPanel.parent);

        StopAllCoroutines();
        StartCoroutine(SmoothReturnToHand(handPanel, targetPos, targetIndex));
    }

    private IEnumerator SmoothReturnToHand(Transform handPanel, Vector3 targetPos, int targetIndex)
    {
        float t = 0f;
        Vector3 start = transform.position;

        while (t < 1f)
        {
            t += Time.deltaTime * 6f; // скорость анимации
            transform.position = Vector3.Lerp(start, targetPos, t);
            yield return null;
        }

        // в конце закрепляем карту в руке
        transform.SetParent(handPanel);
        transform.SetSiblingIndex(targetIndex);
        transform.position = targetPos;
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
