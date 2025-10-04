using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;

public class CardSlot : MonoBehaviour
{
    public Transform slotPanel;
    public GameObject placeholderPrefab;
    public int maxCreatures = 5;

    private GameObject placeholder;
    private CreatureInstance hoveredCreature; // цель под курсором

    // Обновляем позицию placeholder во время Drag
    public void UpdatePlaceholder(PointerEventData eventData)
    {
        List<CreatureInstance> creatures = GetCreatures();

        // если слоты заполнены → placeholder не показываем, только подсветка
        if (creatures.Count >= maxCreatures)
        {
            DestroyPlaceholder();
            HighlightCreatureUnderCursor(eventData);
            return;
        }

        // проверяем, наведён ли курсор на слот
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
        }

        int index = GetInsertIndexClosest(eventData);
        placeholder.transform.SetSiblingIndex(index);

        ClearHighlight(); // если placeholder есть, то подсветку снимаем
    }

    public void PlaceCard(CardInstance card, PointerEventData eventData)
    {
        if (card == null || card.data == null) return;

        List<CreatureInstance> creatures = GetCreatures();

        // === Если слот переполнен ===
        if (creatures.Count >= maxCreatures)
        {
            if (hoveredCreature != null && !hoveredCreature.isDead)
            {
                Debug.Log($"Заменяем {hoveredCreature.name} на {card.data.cardName}");

                // в сброс старую карту
                if (hoveredCreature.cardData != null)
                    DeckManager.Instance.DiscardCard(hoveredCreature.cardData);

                int replaceIndex = hoveredCreature.transform.GetSiblingIndex();
                Destroy(hoveredCreature.gameObject);

                // создаём нового
                GameObject go = Instantiate(card.data.creaturePrefab, slotPanel);
                go.transform.SetSiblingIndex(replaceIndex);

                CreatureInstance newCreature = go.GetComponent<CreatureInstance>();
                if (newCreature != null)
                {
                    newCreature.Initialize(card.data.attack, card.data.health, false, card.data);
                }

                Destroy(card.gameObject);
                DestroyPlaceholder();
                ClearHighlight();
                return;
            }

            Debug.Log("Слот переполнен! Нет цели для замены.");
            DestroyPlaceholder();
            card.ReturnToHand();
            ClearHighlight();
            return;
        }

        // === Обычное добавление ===
        if (placeholder == null)
        {
            card.ReturnToHand();
            return;
        }

        int insertIndex = placeholder.transform.GetSiblingIndex();

        GameObject newGO = Instantiate(card.data.creaturePrefab, slotPanel);
        newGO.transform.SetSiblingIndex(insertIndex);

        CreatureInstance creature = newGO.GetComponent<CreatureInstance>();
        if (creature != null)
        {
            creature.Initialize(card.data.attack, card.data.health, false, card.data);
        }

        Destroy(card.gameObject);
        DestroyPlaceholder();
    }

    // === Подсветка ===
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
                        hoveredCreature.skullIcon.SetActive(true); // показать черепок
                }
                return;
            }
        }

        ClearHighlight();
    }

    private void ClearHighlight()
    {
        if (hoveredCreature != null && hoveredCreature.skullIcon != null)
        {
            hoveredCreature.skullIcon.SetActive(false); // убрать черепок
        }
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
            {
                creatures.Add(c);
            }
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
