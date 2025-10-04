using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CardSlot : MonoBehaviour
{
    public Transform slotPanel;
    public GameObject placeholderPrefab;
    public int maxCreatures = 5;

    private GameObject placeholder;
    private int currentCount = 0;

    // Обновляем позицию placeholder во время Drag
public void UpdatePlaceholder(PointerEventData eventData)
{
    // проверяем, наведён ли курсор на слот
    if (!RectTransformUtility.RectangleContainsScreenPoint(
        slotPanel as RectTransform,
        eventData.position,
        eventData.pressEventCamera))
    {
        DestroyPlaceholder();
        return;
    }

    if (placeholder == null)
    {
        placeholder = Instantiate(placeholderPrefab, slotPanel);
    }

    int index = GetInsertIndexClosest(eventData);
    placeholder.transform.SetSiblingIndex(index);
}


    // Когда отпустили карту
public void PlaceCard(CardInstance card, PointerEventData eventData)
{
    if (card == null || card.data == null) return;

    // если не наведён на слот → вернуть карту в руку
    if (placeholder == null)
    {
        Debug.Log("Карта возвращена в руку");
        card.ReturnToHand(); // нужно реализовать в CardInstance
        return;
    }

        if (GetCreatures().Count >= maxCreatures)
        {
            Debug.Log("Слот переполнен!");
            DestroyPlaceholder();
            card.ReturnToHand(); // возвращаем карту в руку
            return;
        }

    int insertIndex = placeholder.transform.GetSiblingIndex();

    // создаём существо
    GameObject go = Instantiate(card.data.creaturePrefab, slotPanel);
    go.transform.SetSiblingIndex(insertIndex);

    CreatureInstance creature = go.GetComponent<CreatureInstance>();
    if (creature != null)
    {
        creature.Initialize(card.data.attack, card.data.health, false, card.data);
    }

    currentCount++;
    Destroy(card.gameObject);

    DestroyPlaceholder();
}


    // Получаем список всех живых существ в этом слоте
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
