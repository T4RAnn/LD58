using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class CardInstance : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private CardSlot currentSlot;

    private Vector2 dragOffset;

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

        // Проверка слота под мышкой
        if (eventData.pointerEnter != null)
        {
            CardSlot slot = eventData.pointerEnter.GetComponentInParent<CardSlot>();

            if (slot != null)
            {
                RectTransform slotRect = slot.slotPanel as RectTransform;

                if (slotRect != null &&
                    RectTransformUtility.RectangleContainsScreenPoint(slotRect, eventData.position, eventData.pressEventCamera))
                {
                    slot.UpdatePlaceholder(eventData);
                    currentSlot = slot;
                }
                else currentSlot = null;
            }
            else currentSlot = null;
        }
        else currentSlot = null;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // Проверяем, осталась ли мышь над слотом при отпускании
        if (currentSlot != null)
        {
            RectTransform slotRect = currentSlot.slotPanel as RectTransform;
            if (slotRect == null ||
                !RectTransformUtility.RectangleContainsScreenPoint(slotRect, eventData.position, eventData.pressEventCamera))
            {
                currentSlot = null;
            }
        }

        if (currentSlot != null)
        {
            // Плавное перемещение в слот
            StopAllCoroutines();
            StartCoroutine(SmoothMoveToSlot(currentSlot));
            currentSlot = null;
        }
        else
        {
            ReturnToHand();
        }

        HandLayout.DraggingCard = null;
        HandLayout.Instance.DestroyPlaceholder();
    }

    private IEnumerator SmoothMoveToSlot(CardSlot slot)
    {
        RectTransform cardRect = transform as RectTransform;
        RectTransform slotRect = slot.slotPanel as RectTransform;

        // Форсируем обновление LayoutGroup слота, чтобы placeholder занял место
        LayoutRebuilder.ForceRebuildLayoutImmediate(slotRect);

        // Временно ставим карту в тот же родитель, что и плейсхолдер, чтобы локальные координаты совпадали
        Transform originalParent = cardRect.parent;
        cardRect.SetParent(slotRect, true); // true — сохраняем мировые координаты

        Vector3 startLocalPos = cardRect.localPosition;
        Vector3 targetLocalPos = Vector3.zero; // placeholder занимает (0,0) в родителе

        float t = 0f;
        float duration = 0.2f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            cardRect.localPosition = Vector3.Lerp(startLocalPos, targetLocalPos, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        // Завершаем привязку карты к слоту
        cardRect.localPosition = targetLocalPos;
        slot.PlaceCard(this, null);
    }


    public void ReturnToHand()
    {
        Transform handPanel = DeckManager.Instance.handPanel;

        int targetIndex = handPanel.childCount;
        Vector3 targetPos = handPanel.position;

        if (HandLayout.Instance != null && HandLayout.Instance.placeholder != null)
        {
            targetIndex = HandLayout.Instance.placeholder.transform.GetSiblingIndex();
            targetPos = HandLayout.Instance.placeholder.transform.position;
        }

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
            t += Time.deltaTime * 6f;
            transform.position = Vector3.Lerp(start, targetPos, t);
            yield return null;
        }

        transform.SetParent(handPanel);
        transform.SetSiblingIndex(targetIndex);
        transform.position = targetPos;
    }

    public void UpdateUI()
    {
        if (data == null) return;

        if (atkText != null)
            atkText.text = data.attack.ToString();

        if (hpText != null)
            hpText.text = data.health.ToString();
    }
}
