using UnityEngine;
using TMPro;

public class CreatureInstance : MonoBehaviour
{
    public int currentHP;
    public int attack;

    [Header("UI (опционально)")]
    public TMP_Text hpText;
    public TMP_Text atkText;

    // Инициализация при создании
    public void Initialize(int atk, int hp)
    {
        attack = atk;
        currentHP = hp;
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
        Destroy(gameObject);
    }

    private void UpdateUI()
    {
        if (hpText != null) hpText.text = currentHP.ToString();
        if (atkText != null) atkText.text = attack.ToString();
    }
}
