using UnityEngine;
using System.Collections.Generic;

public class DeckManager : MonoBehaviour
{
    public List<CardData> deck = new List<CardData>();
    public Transform handParent;
    public GameObject cardPrefab;

    public void DrawCard()
    {
        if (deck.Count > 0)
        {
            int index = Random.Range(0, deck.Count);
            CardData drawnCard = deck[index];

            GameObject cardGO = Instantiate(cardPrefab, handParent);
            CardInstance instance = cardGO.GetComponent<CardInstance>();
            instance.data = drawnCard;

            deck.RemoveAt(index);
        }
    }
}
