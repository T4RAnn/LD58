using UnityEngine;
using System.Collections.Generic;

public class EnemySlot : MonoBehaviour, ICreatureSlot
{
    public float cardSpacing = 120f; // Расстояние между картами
    public float positionSmoothTime = 0.1f; // Чем меньше, тем быстрее движение

    private Dictionary<RectTransform, Vector2> velocityMap = new Dictionary<RectTransform, Vector2>();

    void Update()
    {
        UpdateCardPositions();
    }

    private void UpdateCardPositions()
    {
        List<CreatureInstance> creatures = GetCreatures();
        // Сортировка по иерархии (по необходимости)
        creatures.Sort((a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));

        float totalWidth = creatures.Count * cardSpacing;
        float startX = -totalWidth * 0.5f + cardSpacing * 0.5f;

        for (int i = 0; i < creatures.Count; i++)
        {
            RectTransform rect = creatures[i].GetComponent<RectTransform>();
            if (rect != null)
            {
                Vector2 targetPos = new Vector2(startX + i * cardSpacing, 0f);

                if (!velocityMap.ContainsKey(rect))
                    velocityMap[rect] = Vector2.zero;

                Vector2 velocity = velocityMap[rect];

                rect.anchoredPosition = Vector2.SmoothDamp(
                    rect.anchoredPosition,
                    targetPos,
                    ref velocity,
                    positionSmoothTime
                );

                velocityMap[rect] = velocity;
            }
        }
    }

    public List<CreatureInstance> GetCreatures()
    {
        List<CreatureInstance> list = new List<CreatureInstance>();
        foreach (Transform child in transform)
        {
            CreatureInstance c = child.GetComponent<CreatureInstance>();
            if (c != null && !c.isDead) list.Add(c);
        }
        return list;
    }
}
