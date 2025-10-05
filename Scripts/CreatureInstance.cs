using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CreatureInstance : MonoBehaviour
{
    [Header("Статы")]
    public int currentHP;
    public int attack;
    public int blockValue = 0; // блокируемое значение урона

    [Header("Данные карты")]
    public CardData cardData;
    public AbilityType ability = AbilityType.None;

    [Header("UI")]
    public TMP_Text hpText;
    public TMP_Text atkText;
    public Image creatureImage;

    [Header("Иконка смерти")]
    public GameObject skullIcon;

    [Header("Банка (внутри префаба)")]
    public GameObject jarObject; 

    [Header("Призывной префаб (опционально)")]
    public GameObject summonPrefab;

    [Header("Принадлежность")]
    public bool isEnemy;

    public bool isDead => currentHP <= 0;

    private Color originalColor;

    private void Awake()
    {
        if (creatureImage != null)
            originalColor = creatureImage.color;

        if (skullIcon != null)
            skullIcon.SetActive(false);

        if (jarObject != null)
            jarObject.SetActive(false); 
    }

    // === Инициализация ===
public void Initialize(int atk, int hp, bool enemy = false, CardData data = null, GameObject summon = null)
{
    attack = atk;
    currentHP = hp;
    isEnemy = enemy;
    cardData = data;
    summonPrefab = summon;

    ability = (cardData != null) ? cardData.ability : AbilityType.None;

if (creatureImage != null && cardData != null && cardData.creatureInside != null)
{
    creatureImage.sprite = cardData.creatureInside;

    // Подгоняем размер под maxSize
    var scaler = creatureImage.GetComponent<CreatureImageScaler>();
    if (scaler != null) scaler.ApplyScale();
}

if (jarObject != null && cardData != null && cardData.jarSprite != null)
{
    Image jarImg = jarObject.GetComponent<Image>();
    if (jarImg != null)
    {
        jarImg.sprite = cardData.jarSprite;

        var scaler = jarImg.GetComponent<CreatureImageScaler>();
        if (scaler != null) scaler.ApplyScale();
    }
}

    UpdateUI();

    if (skullIcon != null)
        skullIcon.SetActive(false);
}

// === Новый метод для масштабирования Image ===
private void SetImageWithMaxSize(Image image, Sprite sprite, Vector2 maxSize)
{
    image.sprite = sprite;

    float spriteWidth = sprite.rect.width;
    float spriteHeight = sprite.rect.height;

    float scaleX = maxSize.x / spriteWidth;
    float scaleY = maxSize.y / spriteHeight;

    float finalScale = Mathf.Min(scaleX, scaleY);

    RectTransform rt = image.rectTransform;
    rt.sizeDelta = new Vector2(spriteWidth * finalScale, spriteHeight * finalScale);
}


    // === Баф ===
    public void ApplyBuff(int atkDelta, int hpDelta)
    {
        attack += atkDelta;
        currentHP += hpDelta;

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

        int blocked = Mathf.Min(blockValue, dmg);
        dmg -= blocked;

        currentHP -= dmg;
        if (currentHP < 0) currentHP = 0;

        UpdateUI();

        if (currentHP == 0)
        {
            if (isEnemy) StartCoroutine(DeathAnimationEnemy());
            else StartCoroutine(DeathAnimationAlly());
        }
        else
        {
            StartCoroutine(Shake(0.2f, 5f));
            StartCoroutine(HitFlash(0.15f));
        }
    }

    // === UI ===
    public void UpdateUI()
    {
        if (hpText != null) hpText.text = currentHP.ToString();
        if (atkText != null) atkText.text = attack.ToString();
    }

    // === Анимации ===
    public IEnumerator DoAttackAnimation(bool isEnemyAttack)
    {
        Vector3 startPos = transform.localPosition;
        Vector3 targetPos = startPos + (isEnemyAttack ? Vector3.left : Vector3.right) * 50f;

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 5f;
            transform.localPosition = Vector3.Lerp(startPos, targetPos, Mathf.Sin(t * Mathf.PI));
            yield return null;
        }

        transform.localPosition = startPos;
    }

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

    private IEnumerator HitFlash(float duration)
    {
        if (creatureImage != null)
        {
            creatureImage.color = Color.red;
            yield return new WaitForSeconds(duration);
            creatureImage.color = originalColor;
        }
    }

    private IEnumerator BuffFlash(TMP_Text textElement, Color flashColor, float duration)
    {
        Color original = textElement.color;
        textElement.color = flashColor;
        yield return new WaitForSeconds(duration);
        textElement.color = original;
    }

    public IEnumerator SpawnAnimationFlyOff(float delay = 0.2f, float flyDistance = 800f, float flyDuration = 0.6f)
    {
        if (jarObject == null) yield break;

        jarObject.SetActive(true);
        RectTransform jarRect = jarObject.GetComponent<RectTransform>();
        Vector3 startPos = jarRect.localPosition;

        yield return new WaitForSeconds(delay);

        float randomX = Random.Range(-150f, 150f);
        Vector3 targetPos = startPos + new Vector3(randomX, -flyDistance, 0);
        float randomRotation = Random.Range(-40f, 40f);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / flyDuration;
            float eased = Mathf.Pow(t, 1.7f);
            jarRect.localPosition = Vector3.Lerp(startPos, targetPos, eased);
            jarRect.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(0, randomRotation, eased));
            yield return null;
        }

        jarObject.SetActive(false);
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

    Transform discardPile = DeckManager.Instance.discardPileTransform;
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

    float zigzagMagnitude = 30f;
    int zigzagCount = 3;

    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float t = elapsed / duration;

        Vector3 basePos = Vector3.Lerp(startPos, targetPos, Mathf.SmoothStep(0, 1, t));
        float zigzagOffset = Mathf.Sin(t * Mathf.PI * zigzagCount) * zigzagMagnitude * (1 - t);
        transform.position = basePos + new Vector3(zigzagOffset, 0, 0);

        float rotationZ = Mathf.Lerp(0f, 180f, t);
        transform.rotation = Quaternion.Euler(0, 0, rotationZ);

        transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
        cg.alpha = Mathf.Lerp(1f, 0f, t);

        yield return null;
    }

    if (cardData != null)
        DeckManager.Instance.DiscardCard(cardData);

    Destroy(gameObject);
}

    // === Черепок ===
    public void ShowSkullIcon() { if (skullIcon != null) skullIcon.SetActive(true); }
    public void HideSkullIcon() { if (skullIcon != null) skullIcon.SetActive(false); }
}
