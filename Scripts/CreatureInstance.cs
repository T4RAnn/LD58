using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CreatureInstance : MonoBehaviour
{
    [Header("Статы")]
    public int currentHP;
    public int attack;
    public int blockValue = 0;       // временный блок (тратится)
    public int passiveBlock = 0;     // постоянный блок (не тратится)

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

        summonPrefab = summon ?? cardData?.summonPrefab;
        ability = (cardData != null) ? cardData.ability : AbilityType.None;

        // Если у карты пассивный блок — активируем
        if (ability == AbilityType.Block1Damage)
            passiveBlock = 1;

        // Спрайт существа
        if (creatureImage != null && cardData?.creatureInside != null)
        {
            creatureImage.sprite = cardData.creatureInside;
            creatureImage.GetComponent<CreatureImageScaler>()?.ApplyScale();
        }

        // Спрайт банки
        if (jarObject != null && cardData?.jarSprite != null)
        {
            Image jarImg = jarObject.GetComponent<Image>();
            if (jarImg != null)
            {
                jarImg.sprite = cardData.jarSprite;
                jarImg.GetComponent<CreatureImageScaler>()?.ApplyScale();
            }
        }

        skullIcon?.SetActive(false);
        UpdateUI();
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

        if (atkDelta != 0 || hpDelta != 0)
            AudioManager.Instance?.PlayStatUp();
    }

    // === Урон ===
    public void TakeDamage(int dmg)
    {
        if (isDead) return;

        // общий блок = пассивный + временный
        int totalBlock = passiveBlock + blockValue;

        int blocked = Mathf.Min(totalBlock, dmg);
        dmg -= blocked;

        // уменьшаем только временный блок
        blockValue -= Mathf.Min(blockValue, blocked);

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
        AudioManager.Instance?.PlayAttack();

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
        AudioManager.Instance?.PlayShake();

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

    private IEnumerator DeathAnimationEnemy()
    {
        AudioManager.Instance?.PlayDeath();

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

    private IEnumerator DeathAnimationAlly()
    {
        AudioManager.Instance?.PlayDeath();

        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
        cg.interactable = false;
        cg.blocksRaycasts = false;

        RectTransform rect = GetComponent<RectTransform>();
        RectTransform discardRect = DeckManager.Instance.discardPileTransform?.GetComponent<RectTransform>();

        if (discardRect == null)
        {
            Destroy(gameObject);
            yield break;
        }

        Vector2 startPos = rect.anchoredPosition;
        Vector2 targetPos = discardRect.anchoredPosition;
        Vector3 originalScale = rect.localScale;
        Vector3 targetScale = originalScale * 0.5f;

        float duration = 0.8f;
        float elapsed = 0f;

        float zigzagMagnitude = 40f;
        int zigzagCount = 3;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            Vector2 basePos = Vector2.Lerp(startPos, targetPos, Mathf.SmoothStep(0, 1, t));
            float zigzagOffset = Mathf.Sin(t * Mathf.PI * zigzagCount) * zigzagMagnitude * (1 - t);
            rect.anchoredPosition = basePos + new Vector2(zigzagOffset, 0);

            float rotationZ = Mathf.Lerp(0f, 180f, t);
            rect.localRotation = Quaternion.Euler(0, 0, rotationZ);
            rect.localScale = Vector3.Lerp(originalScale, targetScale, t);
            cg.alpha = Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        if (cardData != null)
            DeckManager.Instance.DiscardCard(cardData);

        Destroy(gameObject);
    }

    public void ShowSkullIcon() { if (skullIcon != null) skullIcon.SetActive(true); }
    public void HideSkullIcon() { if (skullIcon != null) skullIcon.SetActive(false); }
}
