using UnityEngine;
using TMPro;

public class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance;

    [SerializeField] private RectTransform tooltipPanel; // сам RectTransform панели тултипа
    [SerializeField] private TMP_Text tooltipText;       // текст тултипа
    [SerializeField] private Vector2 screenOffset = new Vector2(0, 60); // смещение в пикселях над целью

    private Canvas canvas;
    private RectTransform canvasRect;
    private Camera uiCamera;
    private bool isVisible = false;
    private Transform target; // цель, над которой показываем тултип

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        canvas = GetComponentInParent<Canvas>();
        if (canvas == null) Debug.LogError("TooltipUI: Canvas not found in parents!");
        canvasRect = canvas.GetComponent<RectTransform>();

        // камера для конвертаций (null для ScreenSpaceOverlay)
        uiCamera = (canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : canvas.worldCamera;

        if (tooltipPanel != null)
            tooltipPanel.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (isVisible && target != null)
        {
            UpdatePosition();
        }
    }

    public void ShowTooltip(string text, Transform targetTransform)
    {
        if (tooltipPanel == null || tooltipText == null) return;

        tooltipText.text = text;
        target = targetTransform;
        tooltipPanel.gameObject.SetActive(true);
        isVisible = true;

        // Немедленно обновить позицию (без задержки)
        UpdatePosition();
    }

    public void HideTooltip()
    {
        if (tooltipPanel == null) return;
        tooltipPanel.gameObject.SetActive(false);
        isVisible = false;
        target = null;
    }

    private void UpdatePosition()
    {
        if (target == null) return;

        // 1) экранные координаты цели (в пикселях)
        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(uiCamera, target.position);

        // применяем пиксельный оффсет (смещение вверх)
        screenPos += new Vector3(screenOffset.x, screenOffset.y, 0f);

        // 2) конвертируем в локальные координаты Canvas
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, screenPos, uiCamera, out Vector2 localPoint);

        // 3) учтём размер тултипа и ограничим, чтобы тултип не вышел за экран
        Vector2 anchoredPos = localPoint;
        Vector2 tooltipSize = tooltipPanel.rect.size;
        Rect canvasR = canvasRect.rect;

        float minX = canvasR.xMin + tooltipSize.x * tooltipPanel.pivot.x;
        float maxX = canvasR.xMax - tooltipSize.x * (1f - tooltipPanel.pivot.x);
        float minY = canvasR.yMin + tooltipSize.y * tooltipPanel.pivot.y;
        float maxY = canvasR.yMax - tooltipSize.y * (1f - tooltipPanel.pivot.y);

        anchoredPos.x = Mathf.Clamp(anchoredPos.x, minX, maxX);
        anchoredPos.y = Mathf.Clamp(anchoredPos.y, minY, maxY);

        tooltipPanel.localPosition = anchoredPos;
    }
}
