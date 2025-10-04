using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Cards/CardData")]
public class CardData : ScriptableObject
{
    public string cardName;

    [Header("Sprites")]
    public Sprite jarSprite;       // банка
    public Sprite creatureInside;  // существо внутри банки

    [Header("Creature Settings")]
    public GameObject creaturePrefab; // префаб существа
    public int attack;
    public int health;
}
