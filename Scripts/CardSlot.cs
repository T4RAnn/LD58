using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CardSlot : MonoBehaviour, ICreatureSlot
{
    public Transform slotPanel;
    public GameObject placeholderPrefab;
    public int maxCreatures = 5;
    public float cardSpacing = 120f; // Расстояние между картами
    public float cardWidth = 100f; // Ширина карты
public float positionSmoothTime = 0.1f; // Чем меньше, тем быстрее плавность

// Словарь для хранения текущей скорости смещения для каждой карты
private Dictionary<RectTransform, Vector2> velocityMap = new Dictionary<RectTransform, Vector2>();
    private GameObject placeholder;
    private CreatureInstance hoveredCreature;
    private List<CreatureInstance> sortedCreatures = new List<CreatureInstance>();

    void Update()
    {
        UpdateCardPositions();
    }

    // === Обновляем позиции всех карт ===
private void UpdateCardPositions()
{
    List<CreatureInstance> creatures = GetCreatures();
    creatures.Sort((a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));

    float totalWidth = creatures.Count * cardSpacing;
    float startX = -totalWidth * 0.5f + cardSpacing * 0.5f;

    for (int i = 0; i < creatures.Count; i++)
    {
        RectTransform rect = creatures[i].GetComponent<RectTransform>();
        if (rect != null)
        {
            Vector2 targetPos = new Vector2(startX + i * cardSpacing, 0f);

            // Получаем текущую скорость или создаём новую
            if (!velocityMap.ContainsKey(rect))
                velocityMap[rect] = Vector2.zero;

            Vector2 velocity = velocityMap[rect];

            // Плавное смещение
            rect.anchoredPosition = Vector2.SmoothDamp(
                rect.anchoredPosition,
                targetPos,
                ref velocity,
                positionSmoothTime
            );

            velocityMap[rect] = velocity;
        }
    }

    UpdatePlaceholderPosition();
}

private void UpdatePlaceholderPosition()
{
    if (placeholder != null)
    {
        RectTransform placeholderRect = placeholder.GetComponent<RectTransform>();
        int placeholderIndex = placeholder.transform.GetSiblingIndex();

        List<CreatureInstance> creatures = GetCreatures();
        float totalWidth = (creatures.Count + 1) * cardSpacing; // +1 для плейсхолдера
        float startX = -totalWidth * 0.5f + cardSpacing * 0.5f;

        Vector2 targetPos = new Vector2(startX + placeholderIndex * cardSpacing, 0f);

        if (!velocityMap.ContainsKey(placeholderRect))
            velocityMap[placeholderRect] = Vector2.zero;

        Vector2 velocity = velocityMap[placeholderRect];

        placeholderRect.anchoredPosition = Vector2.SmoothDamp(
            placeholderRect.anchoredPosition,
            targetPos,
            ref velocity,
            positionSmoothTime
        );

        velocityMap[placeholderRect] = velocity;
    }
}

private int GetInsertIndexClosest(PointerEventData eventData)
{
    List<CreatureInstance> creatures = GetCreatures();
    if (creatures.Count == 0) return 0;

    Vector2 localPos;
    RectTransformUtility.ScreenPointToLocalPointInRectangle(
        slotPanel as RectTransform,
        eventData.position,
        eventData.pressEventCamera,
        out localPos
    );

    float totalWidth = creatures.Count * cardSpacing;
    float startX = -totalWidth * 0.5f + cardSpacing * 0.5f;

    // Находим ближайшую позицию
    int closestIndex = 0;
    float minDistance = float.MaxValue;

    // Проверяем позиции между картами
    for (int i = 0; i <= creatures.Count; i++)
    {
        float slotPosX = startX + i * cardSpacing;
        float distance = Mathf.Abs(localPos.x - slotPosX);
        
        if (distance < minDistance)
        {
            minDistance = distance;
            closestIndex = i;
        }
    }

    return closestIndex;
}

// === Обновляем плейсхолдер во время Drag ===
public void UpdatePlaceholder(PointerEventData eventData)
{
    List<CreatureInstance> creatures = GetCreatures();

    // если слот переполнен → подсветка заменяемой карты
    if (creatures.Count >= maxCreatures)
    {
        DestroyPlaceholder();
        HighlightCreatureUnderCursor(eventData);
        return;
    }

    // если курсор вне зоны слота → убираем
    if (!RectTransformUtility.RectangleContainsScreenPoint(
        slotPanel as RectTransform,
        eventData.position,
        eventData.pressEventCamera))
    {
        DestroyPlaceholder();
        ClearHighlight();
        return;
    }

    if (placeholder == null)
    {
        placeholder = Instantiate(placeholderPrefab, slotPanel);
        // Убедимся, что у плейсхолдера правильный RectTransform
        RectTransform placeholderRect = placeholder.GetComponent<RectTransform>();
        if (placeholderRect != null)
        {
            placeholderRect.anchorMin = new Vector2(0.5f, 0.5f);
            placeholderRect.anchorMax = new Vector2(0.5f, 0.5f);
            placeholderRect.pivot = new Vector2(0.5f, 0.5f);
        }
    }

    int index = GetInsertIndexClosest(eventData);
    placeholder.transform.SetSiblingIndex(index);
    UpdatePlaceholderPosition(); // Обновляем позицию плейсхолдера

    ClearHighlight();
}

    // === Размещение карты ===
    public void PlaceCard(CardInstance card, PointerEventData eventData)
    {
        if (card == null || card.data == null) return;

        List<CreatureInstance> creatures = GetCreatures();

        // Слот переполнен → замена существующей карты
        if (creatures.Count >= maxCreatures)
        {
            if (hoveredCreature != null && !hoveredCreature.isDead)
            {
                int replaceIndex = hoveredCreature.transform.GetSiblingIndex();
                if (hoveredCreature.cardData != null)
                    DeckManager.Instance.DiscardCard(hoveredCreature.cardData);

                Destroy(hoveredCreature.gameObject);

                GameObject go = Instantiate(card.data.creaturePrefab, slotPanel);
                go.transform.SetSiblingIndex(replaceIndex);

                CreatureInstance newCreature = go.GetComponent<CreatureInstance>();
                newCreature?.Initialize(card.data.attack, card.data.health, false, card.data);
                StartCoroutine(newCreature.SpawnAnimationFlyOff());

                Destroy(card.gameObject);
                DestroyPlaceholder();
                ClearHighlight();
                UpdateCardPositions(); // Обновляем позиции после замены
                return;
            }

            // Нет цели для замены → возвращаем карту в руку
            card.ReturnToHand();
            DestroyPlaceholder();
            return;
        }

        // Обычное добавление
        int insertIndex = placeholder != null ? placeholder.transform.GetSiblingIndex() : creatures.Count;

        GameObject newGO = Instantiate(card.data.creaturePrefab, slotPanel);
        newGO.transform.SetSiblingIndex(insertIndex);

        CreatureInstance creature = newGO.GetComponent<CreatureInstance>();
        creature?.Initialize(card.data.attack, card.data.health, false, card.data);
        StartCoroutine(creature.SpawnAnimationFlyOff());

        Destroy(card.gameObject);

        // Убираем placeholder после завершения анимации
        DestroyPlaceholder();
        ClearHighlight();
        UpdateCardPositions(); // Обновляем позиции после добавления
    }

    // === Подсветка цели замены ===
    private void HighlightCreatureUnderCursor(PointerEventData eventData)
    {
        GameObject hovered = eventData.pointerEnter;
        if (hovered != null)
        {
            CreatureInstance target = hovered.GetComponentInParent<CreatureInstance>();
            if (target != null && !target.isDead)
            {
                if (hoveredCreature != target)
                {
                    ClearHighlight();
                    hoveredCreature = target;
                    if (hoveredCreature.skullIcon != null)
                        hoveredCreature.skullIcon.SetActive(true);
                }
                return;
            }
        }
        ClearHighlight();
    }

    private void ClearHighlight()
    {
        if (hoveredCreature != null && hoveredCreature.skullIcon != null)
            hoveredCreature.skullIcon.SetActive(false);

        hoveredCreature = null;
    }

    // === Вспомогательные ===
    public List<CreatureInstance> GetCreatures()
    {
        List<CreatureInstance> creatures = new List<CreatureInstance>();
        foreach (Transform child in slotPanel)
        {
            CreatureInstance c = child.GetComponent<CreatureInstance>();
            if (c != null && !c.isDead && c.gameObject != placeholder)
                creatures.Add(c);
        }
        return creatures;
    }

    private void DestroyPlaceholder()
    {
        if (placeholder != null)
        {
            Destroy(placeholder);
            placeholder = null;
            UpdateCardPositions(); // Обновляем позиции после удаления плейсхолдера
        }
    }

    // === Очистка при уничтожении ===
    private void OnDestroy()
    {
        DestroyPlaceholder();
    }
}