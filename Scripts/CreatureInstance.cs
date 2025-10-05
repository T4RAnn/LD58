using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CreatureInstance : MonoBehaviour
{
    public int currentHP;
    public int attack;

    [Header("Данные карты")]
    public CardData cardData;
    public AbilityType ability = AbilityType.None;

    [Header("UI (опционально)")]
    public TMP_Text hpText;
    public TMP_Text atkText;
    public Image creatureImage;

    [Header("Иконка замены")]
    public GameObject skullIcon;

    [Header("Принадлежность")]
    public bool isEnemy;

    [Header("Банка (внутри префаба)")]
    public GameObject jarObject;   // 👈 объект банки в самом префабе (скрыт в инспекторе)

    public bool isDead => currentHP <= 0;

    private Color originalColor;

    private void Awake()
    {
        if (creatureImage != null)
            originalColor = creatureImage.color;

        if (skullIcon != null)
            skullIcon.SetActive(false);

        if (jarObject != null)
            jarObject.SetActive(false); // банка скрыта при старте
    }

    // === Инициализация ===
    public void Initialize(int atk, int hp, bool enemy = false, CardData data = null)
    {
        attack = atk;
        currentHP = hp;
        isEnemy = enemy;

        cardData = data;
        ability = (cardData != null) ? cardData.ability : AbilityType.None;

        Debug.Log($"Init creature {name} | cardData={(cardData != null ? cardData.cardName : "null")} | ability={ability}");

        UpdateUI();

        if (skullIcon != null)
            skullIcon.SetActive(false);
    }

    // === Управление черепком ===
    public void ShowSkullIcon() { if (skullIcon != null) skullIcon.SetActive(true); }
    public void HideSkullIcon() { if (skullIcon != null) skullIcon.SetActive(false); }

    // === Баф ===
    public void ApplyBuff(int atkDelta, int hpDelta)
    {
        attack += atkDelta;
        currentHP += hpDelta;

        Debug.Log($"{name} получил баф: atk+{atkDelta}, hp+{hpDelta}");

        UpdateUI();

        if (atkDelta != 0 && atkText != null)
            StartCoroutine(BuffFlash(atkText, Color.green, 0.3f));

        if (hpDelta != 0 && hpText != null)
            StartCoroutine(BuffFlash(hpText, Color.green, 0.3f));
    }

    // === Урон ===
public void TakeDamage(int dmg)
{
    if (isDead) return;

    currentHP -= dmg;
    if (currentHP < 0) currentHP = 0;

    UpdateUI();

    if (currentHP == 0)
    {
        if (isEnemy)
            StartCoroutine(DeathAnimationEnemy());
        else
            StartCoroutine(DeathAnimationAlly());
    }
    else
    {
        StartCoroutine(Shake(0.2f, 5f));
        StartCoroutine(HitFlash(0.15f));
    }
}

// === Враги: сжатие + исчезновение ===
private IEnumerator DeathAnimationEnemy()
{
    CanvasGroup cg = GetComponent<CanvasGroup>();
    if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
    cg.interactable = false;
    cg.blocksRaycasts = false;

    float duration = 0.6f;
    float elapsed = 0f;
    Vector3 originalScale = transform.localScale;
    Vector3 targetScale = originalScale * 0.3f;

    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float t = elapsed / duration;
        transform.localScale = Vector3.Lerp(originalScale, targetScale, Mathf.SmoothStep(0, 1, t));
        cg.alpha = Mathf.Lerp(1f, 0f, t);
        yield return null;
    }

    // анимация банки, если есть
    if (jarObject != null)
        StartCoroutine(SpawnAnimationFlyOff());

    Destroy(gameObject);
}

// === Союзники: улетание в сброс ===
private IEnumerator DeathAnimationAlly()
{
    CanvasGroup cg = GetComponent<CanvasGroup>();
    if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
    cg.interactable = false;
    cg.blocksRaycasts = false;

    Transform discardPile = DeckManager.Instance.discardPileTransform; // ссылка на объект сброса
    if (discardPile == null)
    {
        Destroy(gameObject);
        yield break;
    }

    Vector3 startPos = transform.position;
    Vector3 targetPos = discardPile.position;
    Vector3 originalScale = transform.localScale;
    Vector3 targetScale = originalScale * 0.5f;

    float duration = 0.8f;
    float elapsed = 0f;

    // параметры зигзага
    float zigzagMagnitude = 30f;
    int zigzagCount = 3;

    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float t = elapsed / duration;

        // движение к сбросу
        Vector3 basePos = Vector3.Lerp(startPos, targetPos, Mathf.SmoothStep(0, 1, t));

        // добавляем зигзаг по X
        float zigzagOffset = Mathf.Sin(t * Mathf.PI * zigzagCount) * zigzagMagnitude * (1 - t); // уменьшается к концу
        transform.position = basePos + new Vector3(zigzagOffset, 0, 0);

        // вращение по Z
        float rotationZ = Mathf.Lerp(0f, 180f, t); // разворот на 180 градусов по пути
        transform.rotation = Quaternion.Euler(0, 0, rotationZ);

        // уменьшение и прозрачность
        transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
        cg.alpha = Mathf.Lerp(1f, 0f, t);

        yield return null;
    }

    // дискард карты и удаление объекта
    if (cardData != null)
        DeckManager.Instance.DiscardCard(cardData);

    Destroy(gameObject);
}


    public void UpdateUI()
    {
        if (hpText != null) hpText.text = currentHP.ToString();
        if (atkText != null) atkText.text = attack.ToString();
    }

    // === Атака ===
    // === Процедурная анимация атаки ===
    public IEnumerator DoAttackAnimation(bool isEnemyAttack)
    {
        Vector3 startPos = transform.localPosition;
        Vector3 targetPos = startPos + (isEnemyAttack ? Vector3.left : Vector3.right) * 50f; // рывок в сторону

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 5f;
            transform.localPosition = Vector3.Lerp(startPos, targetPos, Mathf.Sin(t * Mathf.PI));
            yield return null;
        }

        transform.localPosition = startPos; // возвращаем на место
    }

    // === Тряска при уроне ===
    public IEnumerator Shake(float duration, float magnitude)
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

    // === Вспышка при получении бафа ===
    public IEnumerator BuffFlash(float duration)
    {
        if (hpText != null) hpText.color = Color.green;
        if (atkText != null) atkText.color = Color.green;

        yield return new WaitForSeconds(duration);

        if (hpText != null) hpText.color = Color.white;
        if (atkText != null) atkText.color = Color.white;
    }

    // универсальный метод подсветки конкретного текста
    private IEnumerator BuffFlash(TMP_Text textElement, Color flashColor, float duration)
    {
        Color original = textElement.color;
        textElement.color = flashColor;
        yield return new WaitForSeconds(duration);
        textElement.color = original;
    }

    // === Анимация банки (встроенной в префаб) ===
public IEnumerator SpawnAnimationFlyOff(
    float delayBeforeFall = 0.2f, 
    float flyDistance = 800f, 
    float flyDuration = 0.6f)
{
    if (jarObject == null)
        yield break;

    jarObject.SetActive(true);

    RectTransform jarRect = jarObject.GetComponent<RectTransform>();
    Vector3 startPos = jarRect.localPosition;

    // Монстр сразу виден
    if (creatureImage != null)
        creatureImage.color = originalColor;
    if (atkText != null) atkText.alpha = 1f;
    if (hpText != null) hpText.alpha = 1f;

    // ⏳ Задержка перед падением
    yield return new WaitForSeconds(delayBeforeFall);

    // Случайное смещение по X
    float randomX = Random.Range(-150f, 150f);

    // Цель — далеко за экран вниз
    Vector3 targetPos = startPos + new Vector3(randomX, -flyDistance, 0);

    // Случайный наклон банки
    float randomRotation = Random.Range(-40f, 40f);

    float t = 0f;
    while (t < 1f)
    {
        t += Time.deltaTime / flyDuration;
        float eased = Mathf.Pow(t, 1.7f); // ускорение вниз
        jarRect.localPosition = Vector3.Lerp(startPos, targetPos, eased);
        jarRect.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(0, randomRotation, eased));
        yield return null;
    }

    // Банка исчезает
    jarObject.SetActive(false);
}


}
