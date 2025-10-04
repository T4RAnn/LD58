using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class CardInstance : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public CardData data;
    public Image jarImage;
    public Image creatureImage;

    private Transform parentToReturn;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>(); // убедись, что карта внутри Canvas
    }

    private void Start()
    {
        if (data != null)
        {
            if (jarImage) jarImage.sprite = data.jarSprite;
            if (creatureImage) creatureImage.sprite = data.creatureInside;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        parentToReturn = transform.parent;
        // Переносим в корневой canvas, чтобы быть над другими элементами
        transform.SetParent(canvas.transform, true);

        // ВАЖНО: чтобы карта не блокировала raycast на слотах
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.95f;

        Debug.Log("Card BeginDrag: " + name);
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Включаем raycasts обратно
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        // Если не был обработан OnDrop (карта все ещё на корневом canvas) — вернуть в руку
        if (transform.parent == canvas.transform)
        {
            transform.SetParent(parentToReturn);
            rectTransform.localPosition = Vector3.zero;
        }

        Debug.Log("Card EndDrag: " + name);
    }
}
