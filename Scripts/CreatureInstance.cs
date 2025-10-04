using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CreatureInstance : MonoBehaviour
{
    public int currentHP;
    public int attack;

    [Header("Данные карты")]
    public CardData cardData; // ссылка на карту, из которой создано существо

    [Header("UI (опционально)")]
    public TMP_Text hpText;
    public TMP_Text atkText;
    public Image creatureImage; // сюда перетащи Image врага/существа

    [Header("Принадлежность")]
    public bool isEnemy; // true = враг, false = игрок

    public bool isDead => currentHP <= 0;

    private Color originalColor;

    private void Awake()
    {
        if (creatureImage != null)
            originalColor = creatureImage.color;
    }

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
            StartCoroutine(Shake(0.2f, 5f));     // тряска (UI, локальные координаты)
            StartCoroutine(HitFlash(0.15f));     // красная вспышка
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

    // === Процедурная анимация атаки ===
    public IEnumerator DoAttackAnimation(Transform target)
    {
        Vector3 originalPos = transform.localPosition;
        Vector3 targetPos = transform.localPosition + (target.localPosition - transform.localPosition) * 0.3f;

        float t = 0f;
        // движение к цели
        while (t < 1f)
        {
            t += Time.deltaTime * 5f; // скорость
            transform.localPosition = Vector3.Lerp(originalPos, targetPos, t);
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);

        t = 0f;
        // возврат назад
        while (t < 1f)
        {
            t += Time.deltaTime * 5f;
            transform.localPosition = Vector3.Lerp(targetPos, originalPos, t);
            yield return null;
        }
    }

    // === Тряска при уроне ===
    private IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = originalPos + new Vector3(offsetX, offsetY, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }

    // === Вспышка при получении урона ===
    private IEnumerator HitFlash(float duration)
    {
        if (creatureImage != null)
        {
            creatureImage.color = Color.red;
            yield return new WaitForSeconds(duration);
            creatureImage.color = originalColor;
        }
    }
}
