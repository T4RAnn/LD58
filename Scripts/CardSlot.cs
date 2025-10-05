using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CardSlot : MonoBehaviour, ICreatureSlot
{
    public Transform slotPanel;
    public GameObject placeholderPrefab;
    public int maxCreatures = 5;

    private GameObject placeholder;
    private CreatureInstance hoveredCreature;

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
            placeholder = Instantiate(placeholderPrefab, slotPanel);

        int index = GetInsertIndexClosest(eventData);
        placeholder.transform.SetSiblingIndex(index);

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
            if (c != null && !c.isDead)
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
        }
    }

    private int GetInsertIndexClosest(PointerEventData eventData)
    {
        if (slotPanel.childCount == 0) return 0;

        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            slotPanel as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPos
        );

        float minDistance = float.MaxValue;
        int closestIndex = 0;

        for (int i = 0; i < slotPanel.childCount; i++)
        {
            RectTransform child = slotPanel.GetChild(i) as RectTransform;
            if (child == null || child == placeholder) continue;

            float childCenter = child.localPosition.x;
            float dist = Mathf.Abs(localPos.x - childCenter);

            if (dist < minDistance)
            {
                minDistance = dist;
                closestIndex = i;
                if (localPos.x > childCenter)
                    closestIndex = i + 1;
            }
        }

        return Mathf.Clamp(closestIndex, 0, slotPanel.childCount);
    }
}
