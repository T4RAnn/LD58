using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance { get; private set; }

    [Header("Начальная колода")]
    public List<CardData> startingDeck = new List<CardData>();

    [Header("Колода (текущая стопка)")]
    public List<CardData> deck = new List<CardData>();

    [Header("Сброс (данные карт)")]
    public List<CardData> discardPile = new List<CardData>();

    [Header("Ссылка на родительский объект руки")]
    public Transform handPanel;

    [Header("Префаб карты")]
    public GameObject cardPrefab;

    [Header("Зона сброса (куда летят карты)")]
    public Transform discardPileTransform;

    [Header("Иконка колоды (откуда вылетают карты)")]
    public Transform deckIconTransform;

    private readonly List<CardInstance> hand = new List<CardInstance>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        deck = new List<CardData>(startingDeck);
        Shuffle(deck);
    }

    private void Shuffle(List<CardData> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            CardData temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    // взять карты в руку
    public void DrawCards(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (deck.Count == 0)
            {
                ReshuffleDiscardIntoDeck();
                if (deck.Count == 0)
                {
                    Debug.Log("Колода пуста окончательно!");
                    return;
                }
            }

            CardData cardData = deck[0];
            deck.RemoveAt(0);

            // создаём карту у иконки колоды
            GameObject cardGO = Instantiate(cardPrefab, handPanel);
            RectTransform rect = cardGO.GetComponent<RectTransform>();

            // Переводим мировую позицию иконки колоды в локальную позицию руки
            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(handPanel as RectTransform, deckIconTransform.position, null, out localPos);
            rect.anchoredPosition = localPos;


            CardInstance card = cardGO.GetComponent<CardInstance>();
            card.data = cardData;
            card.UpdateUI();

            hand.Add(card);

            // анимация перелёта из колоды в руку
            StartCoroutine(AnimateToHand(rect));
        }
    }

    private IEnumerator AnimateToHand(RectTransform rect)
    {
        Vector2 startPos = rect.anchoredPosition;
        Vector2 endPos = Vector2.zero; // в руке карты раскладываются автоматически LayoutGroup'ом
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one;

        rect.localScale = startScale;

        float t = 0f;
        float duration = 0.5f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float eased = Mathf.SmoothStep(0, 1, t);

            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, eased);
            rect.localScale = Vector3.Lerp(startScale, endScale, eased);

            yield return null;
        }

        rect.anchoredPosition = endPos;
        rect.localScale = endScale;
    }

    public IEnumerator AnimateToDiscardCard(CardData data, GameObject cardGO)
    {
        if (discardPileTransform == null)
        {
            Destroy(cardGO);
            yield break;
        }

        RectTransform rect = cardGO.GetComponent<RectTransform>();
        RectTransform discardRect = discardPileTransform.GetComponent<RectTransform>();

        Vector2 startPos = rect.anchoredPosition;
        Vector2 endPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect.parent as RectTransform, discardRect.position, null, out endPos);

        float t = 0f;
        float duration = 0.5f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float eased = Mathf.SmoothStep(0, 1, t);

            // полёт по дуге
            Vector2 pos = Vector2.Lerp(startPos, endPos, eased);
            pos.y += Mathf.Sin(eased * Mathf.PI) * 100f; // дуга
            rect.anchoredPosition = pos;

            rect.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, eased);
            yield return null;
        }

        discardPile.Add(data);
        Destroy(cardGO);
    }

    // сброс карты
    // сброс карты
    public void DiscardCard(CardInstance card)
    {
        if (card != null && card.data != null)
        {
            hand.Remove(card); // убираем из руки сразу
            StartCoroutine(AnimateToDiscardCard(card.data, card.gameObject)); // передаём и данные, и объект
        }
    }

    public void DiscardCard(CardData cardData)
    {
        if (cardData != null)
        {
            discardPile.Add(cardData);
            Debug.Log($"Карта {cardData.cardName} ушла в сброс (без анимации, объекта нет)");
        }
    }

    public void EndTurn()
    {
        Debug.Log("Конец хода. Все карты из руки уходят в сброс.");

        foreach (var card in new List<CardInstance>(hand))
        {
            DiscardCard(card);
        }

        hand.Clear();
    }

    private void ReshuffleDiscardIntoDeck()
    {
        if (discardPile.Count == 0) return;

        Debug.Log("Перемешиваем сброс обратно в колоду...");
        deck.AddRange(discardPile);
        discardPile.Clear();
        Shuffle(deck);
    }
}
