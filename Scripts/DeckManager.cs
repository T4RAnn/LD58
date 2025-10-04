using UnityEngine;
using System.Collections.Generic;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance { get; private set; }

    [Header("Начальная колода")]
    public List<CardData> startingDeck = new List<CardData>();

    [Header("Колода (текущая стопка)")]
    public List<CardData> deck = new List<CardData>();

    [Header("Сброс")]
    public List<CardData> discardPile = new List<CardData>();

    [Header("Ссылка на родительский объект руки")]
    public Transform handPanel;

    [Header("Префаб карты")]
    public GameObject cardPrefab;

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

    // перемешивание
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

    // взять карты из колоды
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
            // тут можно сразу обновить UI карты, если есть метод
            // card.SetupUI();
        }
    }

    // отправить карту в сброс (когда существо умирает)
    public void DiscardCard(CardData card)
    {
        discardPile.Add(card);
        Debug.Log($"Карта {card.cardName} ушла в сброс");
    }

    // сброс → обратно в колоду
    private void ReshuffleDiscardIntoDeck()
    {
        if (discardPile.Count == 0) return;

        Debug.Log("Перемешиваем сброс обратно в колоду...");
        deck.AddRange(discardPile);
        discardPile.Clear();
        Shuffle(deck);
    }
}
