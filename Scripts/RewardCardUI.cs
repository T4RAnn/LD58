using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class RewardCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI элементы карты")]
    public Image creatureImage;      // <— добавлено поле для изображения
    public TMP_Text atkText;
    public TMP_Text hpText;
    public Button selectButton;

    [HideInInspector] public CardData cardData;
    private RewardManager rewardManager;
    private RectTransform rect;
    private Vector3 originalScale;

    private Coroutine hoverCoroutine;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        originalScale = rect.localScale;
    }

    public void Setup(CardData data, RewardManager manager, int index)
    {
        cardData = data;
        rewardManager = manager;

        // === обновление UI ===
        if (atkText) atkText.text = data.attack.ToString();
        if (hpText) hpText.text = data.health.ToString();

        if (creatureImage)
        {
            if (data.creatureInside != null)
            {
                creatureImage.sprite = data.creatureInside;

                // Автоматическая подгонка размера через CreatureImageScaler
                var scaler = creatureImage.GetComponent<CreatureImageScaler>();
                if (scaler != null)
                    scaler.ApplyScale();
            }
            else
            {
                creatureImage.sprite = null; // сброс если нет спрайта
            }
        }

        // Настройка кнопки выбора
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => rewardManager.OnCardSelected(this));
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverCoroutine != null) StopCoroutine(hoverCoroutine);
        hoverCoroutine = StartCoroutine(HoverAnimation());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverCoroutine != null) StopCoroutine(hoverCoroutine);
        StartCoroutine(ResetHover());
    }

    private IEnumerator HoverAnimation()
    {
        Vector3 targetScale = originalScale * 1.2f;

        while (true)
        {
            float shake = Mathf.Sin(Time.time * 20f) * 0.02f;
            rect.localScale = Vector3.Lerp(rect.localScale, targetScale + Vector3.one * shake, 0.2f);
            yield return null;
        }
    }

    private IEnumerator ResetHover()
    {
        while (Vector3.Distance(rect.localScale, originalScale) > 0.01f)
        {
            rect.localScale = Vector3.Lerp(rect.localScale, originalScale, Time.deltaTime * 10f);
            yield return null;
        }
        rect.localScale = originalScale;
    }

    // === Анимация полета награды по дуге в колоду ===
public IEnumerator FlyToDeck(Transform deckTransform)
{
    selectButton.interactable = false;

    // Получаем RectTransform и Canvas
    RectTransform rect = GetComponent<RectTransform>();
    Canvas canvas = GetComponentInParent<Canvas>();
    
    Vector3 startPos = rect.position;
    Vector3 endPos = deckTransform.position;
    Vector3 startScale = rect.localScale;
    Vector3 endScale = Vector3.zero;

    float duration = 0.8f;
    float elapsed = 0f;
    float arcHeight = 1.5f; // Уменьшаем высоту дуги для мировых координат

    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float t = elapsed / duration;
        t = Mathf.SmoothStep(0f, 1f, t);

        Vector3 midPoint = Vector3.Lerp(startPos, endPos, t);
        
        // Для Canvas с режимом Camera используем мировые координаты
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera || canvas.renderMode == RenderMode.WorldSpace)
        {
            midPoint.y += Mathf.Sin(t * Mathf.PI) * arcHeight;
        }
        else // Для Overlay
        {
            midPoint.y += Mathf.Sin(t * Mathf.PI) * arcHeight * 100f; // Большая высота для экранных координат
        }

        rect.position = midPoint;
        rect.localScale = Vector3.Lerp(startScale, endScale, t);
        rect.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(0, 360f, t));

        yield return null;
    }

    DeckManager.Instance.deck.Add(cardData);
    Destroy(gameObject);
}
}
