// CardData.cs
using UnityEngine;

public enum AbilityType
{
    None,
    BuffFrontHP4,
    BuffBackHP5,
    BuffBackATK3,
    BuffBackAllATK1,
    BuffBackAllHP1,
    BuffAllHP2,
    BuffAllATK1,

    SelfBuff1HP1ATK,       // Увеличивает себя на 1 хп и 1 атк
    Block1Damage,          // Блокирует 1 урон
    DoubleAttack,          // Атакует дважды
    SummonInFront          // Призывает существо перед собой
}

[CreateAssetMenu(fileName = "NewCard", menuName = "Cards/CardData")]
public class CardData : ScriptableObject
{
    public string cardName;

    [Header("Sprites")]
    public Sprite jarSprite;
    public Sprite creatureInside;

    [Header("Creature Settings")]
    public GameObject creaturePrefab;
    public GameObject summonPrefab; // ← добавляем сюда

    public int attack;
    public int health;

    [Header("Ability")]
    public AbilityType ability = AbilityType.None; // ← укажи способность в инспекторе
}
