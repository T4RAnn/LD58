using UnityEngine;
using UnityEngine.EventSystems;

public class CardSlot : MonoBehaviour, IDropHandler
{
    [Header("Creature Spawn Settings")]
    public Transform slotPanel;   // панель, куда будут вставляться существа (RectTransform)
    public int maxCreatures = 5;

    private int currentCount = 0;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        CardInstance card = eventData.pointerDrag.GetComponent<CardInstance>();
        if (card == null) return;

        if (currentCount >= maxCreatures)
        {
            Debug.Log("Слот переполнен!");
            return;
        }

        // === СПАВНИМ СУЩЕСТВО ВНУТРИ ПАНЕЛИ ===
        GameObject go = Instantiate(card.data.creaturePrefab, slotPanel);

        // Сбрасываем позицию и масштаб
        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchoredPosition = Vector2.zero;  // в центре панели
            rt.localScale = Vector3.one;
        }

        // Если у существа есть логика
        CreatureInstance creature = go.GetComponent<CreatureInstance>();
        if (creature != null)
        {
            creature.Initialize(card.data.attack, card.data.health);
        }

        currentCount++;

        // Удаляем банку (из руки)
        Destroy(card.gameObject);
    }
}
