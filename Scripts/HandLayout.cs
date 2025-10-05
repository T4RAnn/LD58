using UnityEngine;
using System.Collections.Generic;

public class HandLayout : MonoBehaviour
{
    public static HandLayout Instance { get; private set; }
    public static CardInstance DraggingCard;

    [Header("Настройки раскладки")]
    public float spacing = 150f;
    public float curveStrength = 20f;
    public float rotationAngle = 10f;
    public float moveSpeed = 10f;

    [Header("Плейсхолдер")]
    public GameObject placeholderPrefab;
    [HideInInspector] public GameObject placeholder;

    private List<RectTransform> cards = new List<RectTransform>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        CollectCards();
        ArrangeCards();
    }

    private void CollectCards()
    {
        cards.Clear();
        foreach (Transform child in transform)
        {
            // игнорируем плейсхолдер
            if (placeholder != null && child.gameObject == placeholder)
                continue;

            RectTransform rt = child as RectTransform;
            if (rt != null)
                cards.Add(rt);
        }
    }

    private void ArrangeCards()
    {
        if (cards.Count == 0) return;

        float totalWidth = (cards.Count - 1) * spacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < cards.Count; i++)
        {
            RectTransform card = cards[i];

            // тащимая карта не перестраивается
            if (DraggingCard != null && card == DraggingCard.GetComponent<RectTransform>())
                continue;

            float targetX = startX + i * spacing;
            float normalized = (i - (cards.Count - 1) / 2f);
            float targetY = -Mathf.Pow(normalized, 2) * curveStrength;

            Vector3 targetPos = new Vector3(targetX, targetY, 0);

            float angle = (cards.Count > 1)
                ? Mathf.Lerp(-rotationAngle, rotationAngle, i / (float)(cards.Count - 1))
                : 0f;

            card.localPosition = Vector3.Lerp(card.localPosition, targetPos, Time.deltaTime * moveSpeed);
            card.localRotation = Quaternion.Lerp(
                card.localRotation,
                Quaternion.Euler(0, 0, angle),
                Time.deltaTime * moveSpeed
            );
        }
    }

    // === Работа с плейсхолдером ===
    public void CreatePlaceholder(CardInstance card)
    {
        if (placeholder == null && placeholderPrefab != null)
        {
            placeholder = Instantiate(placeholderPrefab, transform);
            placeholder.transform.SetSiblingIndex(card.transform.GetSiblingIndex());
        }
    }

    public void DestroyPlaceholder()
    {
        if (placeholder != null)
        {
            Destroy(placeholder);
            placeholder = null;
        }
    }
}
