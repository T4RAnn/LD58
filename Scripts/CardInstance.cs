using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class CardInstance : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private CardSlot currentSlot;

    private Vector2 dragOffset;
    private bool isHovered = false;
    private Coroutine shakeCoroutine;

    // Сохраняем оригинальные трансформы для корректного возврата
    private Vector3 jarOriginalRotation;
    private Vector3 creatureOriginalPosition;

    [Header("Данные карты")]
    public CardData data;

    [Header("UI текста")]
    public TMP_Text atkText;
    public TMP_Text hpText;

    [Header("UI изображения")]
    public Image jarImage;
    public Image creatureImage;

    [Header("Настройки тряски банки")]
    public float shakeIntensity = 2f;
    public float shakeSpeed = 15f;

    [Header("Настройки тряски существа")]
    public float creatureShakeIntensity = 3f;
    public float creatureShakeSpeed = 20f;

    [Header("Дополнительные настройки дрожи")]
    public float sharpMovementChance = 0.1f;
    public float maxSharpMovement = 0.5f;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        SaveOriginalTransforms();
        UpdateUI();
    }

    // Сохраняем оригинальные трансформы при старте
    private void SaveOriginalTransforms()
    {
        if (jarImage != null)
        {
            RectTransform jarRect = jarImage.rectTransform;
            jarOriginalRotation = jarRect.localEulerAngles;
        }

        if (creatureImage != null)
        {
            RectTransform creatureRect = creatureImage.rectTransform;
            creatureOriginalPosition = creatureRect.localPosition;
        }
    }

public void OnPointerEnter(PointerEventData eventData)
{
    if (HandLayout.DraggingCard != null) return;

    isHovered = true;

    // Принудительно сбросить трансформы перед новой тряской
    ResetTransforms();

    if (shakeCoroutine != null)
        StopCoroutine(shakeCoroutine);

    shakeCoroutine = StartCoroutine(ShakeAnimation());
}


public void OnPointerExit(PointerEventData eventData)
{
    isHovered = false;

    if (shakeCoroutine != null)
    {
        StopCoroutine(shakeCoroutine);
        shakeCoroutine = null;
    }

    // Принудительно сбрасываем трансформы
    ResetTransforms();
}

private void ResetTransforms()
{
    if (jarImage != null)
        jarImage.rectTransform.localEulerAngles = jarOriginalRotation;

    if (creatureImage != null)
        creatureImage.rectTransform.localPosition = creatureOriginalPosition;
}


    private IEnumerator ShakeAnimation()
    {
        float elapsed = 0f;
        RectTransform jarRect = jarImage != null ? jarImage.rectTransform : null;
        RectTransform creatureRect = creatureImage != null ? creatureImage.rectTransform : null;

        // Добавляем случайные фазы для более хаотичного движения
        float jarRandomPhase = Random.Range(0f, Mathf.PI * 2f);
        float creatureRandomPhaseX = Random.Range(0f, Mathf.PI * 2f);
        float creatureRandomPhaseY = Random.Range(0f, Mathf.PI * 2f);

        while (isHovered)
        {
            elapsed += Time.deltaTime;
            
            // Банка - более резкое и хаотичное вращение
            if (jarRect != null)
            {
                // Используем Perlin noise для более естественной дрожи
                float jarShakeX = Mathf.PerlinNoise(elapsed * shakeSpeed * 0.7f, jarRandomPhase) * 2f - 1f;
                float jarShakeY = Mathf.PerlinNoise(jarRandomPhase, elapsed * shakeSpeed * 0.9f) * 2f - 1f;
                float jarShakeZ = Mathf.PerlinNoise(elapsed * shakeSpeed, jarRandomPhase * 0.5f) * 2f - 1f;
                
                jarRect.localEulerAngles = new Vector3(
                    jarOriginalRotation.x + jarShakeX * shakeIntensity * 0.3f,
                    jarOriginalRotation.y + jarShakeY * shakeIntensity * 0.3f,
                    jarOriginalRotation.z + jarShakeZ * shakeIntensity
                );
            }

            // Существо - более резкие и случайные движения
            if (creatureRect != null)
            {
                // Разные частоты для X и Y чтобы избежать синхронности
                float creatureShakeX = Mathf.PerlinNoise(elapsed * creatureShakeSpeed * 1.3f, creatureRandomPhaseX) * 2f - 1f;
                float creatureShakeY = Mathf.PerlinNoise(creatureRandomPhaseY, elapsed * creatureShakeSpeed * 1.7f) * 2f - 1f;
                
                // Случайные резкие движения
                float sharpMovement = 0f;
                if (Random.value < sharpMovementChance)
                {
                    sharpMovement = Random.Range(-maxSharpMovement, maxSharpMovement) * creatureShakeIntensity;
                }
                
                creatureRect.localPosition = new Vector3(
                    creatureOriginalPosition.x + creatureShakeX * creatureShakeIntensity + sharpMovement,
                    creatureOriginalPosition.y + creatureShakeY * creatureShakeIntensity * 0.6f,
                    creatureOriginalPosition.z
                );
            }

            yield return null;
        }
    }

    private IEnumerator StopShakeAnimation()
    {
        RectTransform jarRect = jarImage != null ? jarImage.rectTransform : null;
        RectTransform creatureRect = creatureImage != null ? creatureImage.rectTransform : null;
        
        Vector3 jarCurrentRotation = jarRect != null ? jarRect.localEulerAngles : Vector3.zero;
        Vector3 creatureCurrentPosition = creatureRect != null ? creatureRect.localPosition : Vector3.zero;

        float t = 0f;
        float duration = 0.2f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            
            if (jarRect != null)
            {
                jarRect.localEulerAngles = Vector3.Lerp(jarCurrentRotation, jarOriginalRotation, t);
            }
            
            if (creatureRect != null)
            {
                creatureRect.localPosition = Vector3.Lerp(creatureCurrentPosition, creatureOriginalPosition, t);
            }

            yield return null;
        }

        // Финальное выравнивание
        if (jarRect != null)
            jarRect.localEulerAngles = jarOriginalRotation;
        
        if (creatureRect != null)
            creatureRect.localPosition = creatureOriginalPosition;
    }

    // Останавливаем тряску при начале drag
    public void OnBeginDrag(PointerEventData eventData)
    {
        isHovered = false;
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }

        // Немедленно сбрасываем rotation банки и позицию существа к оригинальным значениям
        if (jarImage != null)
        {
            RectTransform jarRect = jarImage.rectTransform;
            jarRect.localEulerAngles = jarOriginalRotation;
        }

        if (creatureImage != null)
        {
            RectTransform creatureRect = creatureImage.rectTransform;
            creatureRect.localPosition = creatureOriginalPosition;
        }

        // проигрываем звук взятия карты
        AudioManager.Instance?.PlayCardPick();

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

        if (creatureImage != null && data.creatureInside != null)
        {
            creatureImage.sprite = data.creatureInside;

            var scaler = creatureImage.GetComponent<CreatureImageScaler>();
            if (scaler != null) scaler.ApplyScale();
        }

        if (jarImage != null && data.jarSprite != null)
        {
            jarImage.sprite = data.jarSprite;

            var scaler = jarImage.GetComponent<CreatureImageScaler>();
            if (scaler != null) scaler.ApplyScale();
        }

        // Пересохраняем оригинальные трансформы после обновления UI
        SaveOriginalTransforms();
    }
}