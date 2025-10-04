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

    // текущие карты в руке (объекты)
    private readonly List<CardInstance> hand = new List<CardInstance>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // скопируем стартовую колоду в рабочую
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

            GameObject cardGO = Instantiate(cardPrefab, handPanel);
            CardInstance card = cardGO.GetComponent<CardInstance>();
            card.data = cardData;

            hand.Add(card);
        }
    }

    // универсальный метод сброса (с анимацией если есть объект)
    public void DiscardCard(CardInstance card)
    {
        if (card != null && card.data != null)
        {
            discardPile.Add(card.data);
            StartCoroutine(AnimateToDiscard(card));
            hand.Remove(card);
        }
    }

    // перегрузка: если есть только CardData (например, Creature умерло)
    public void DiscardCard(CardData cardData)
    {
        if (cardData != null)
        {
            discardPile.Add(cardData);
            Debug.Log($"Карта {cardData.cardName} ушла в сброс (без анимации, объекта нет)");
        }
    }

    // конец хода – сбрасываем всю руку
    public void EndTurn()
    {
        Debug.Log("Конец хода. Все карты из руки уходят в сброс.");

        foreach (var card in new List<CardInstance>(hand))
        {
            DiscardCard(card); // теперь всё через общий метод
        }

        hand.Clear();
    }

    // анимация полёта карты в сброс
    private IEnumerator AnimateToDiscard(CardInstance card)
    {
        if (discardPileTransform == null)
        {
            Destroy(card.gameObject);
            yield break;
        }

        RectTransform rect = card.GetComponent<RectTransform>();
        RectTransform discardRect = discardPileTransform.GetComponent<RectTransform>();

        Vector2 startPos = rect.anchoredPosition;
        Vector2 endPos = discardRect.anchoredPosition;

        float t = 0f;
        float duration = 0.5f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            rect.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);
            yield return null;
        }

        Destroy(card.gameObject);
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
