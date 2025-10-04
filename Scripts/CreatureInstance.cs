using UnityEngine;
using TMPro;

public class CreatureInstance : MonoBehaviour
{
    public int currentHP;
    public int attack;

    [Header("Данные карты")]
    public CardData cardData; // ссылка на карту, из которой создано существо

    [Header("UI (опционально)")]
    public TMP_Text hpText;
    public TMP_Text atkText;

    [Header("Принадлежность")]
    public bool isEnemy; // true = враг, false = игрок

    public bool isDead => currentHP <= 0;

    // Инициализация при создании
    public void Initialize(int atk, int hp, bool enemy = false, CardData data = null)
    {
        attack = atk;
        currentHP = hp;
        isEnemy = enemy;
        cardData = data; // сохраняем ссылку на карту
        UpdateUI();
    }

    // Получение урона
    public void TakeDamage(int dmg)
    {
        currentHP -= dmg;
        if (currentHP <= 0)
        {
            Die();
        }
        else
        {
            UpdateUI();
        }
    }

    private void Die()
    {
        Debug.Log($"{name} погиб ({(isEnemy ? "враг" : "игрок")})");

        if (!isEnemy && cardData != null)
        {
            // только игрок возвращает карту в сброс
            DeckManager.Instance.DiscardCard(cardData);
        }

        Destroy(gameObject);
    }

    private void UpdateUI()
    {
        if (hpText != null) hpText.text = currentHP.ToString();
        if (atkText != null) atkText.text = attack.ToString();
    }
}
