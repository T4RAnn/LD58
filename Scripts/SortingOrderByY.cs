using UnityEngine;
using UnityEngine.Rendering; // для SortingGroup

[ExecuteAlways]
public class YSortGroup : MonoBehaviour
{
    [Tooltip("Если задан — будет использована Y-координата этой точки (например 'Feet'). Иначе используется минимальная y среди всех SpriteRenderer.bounds.")]
    public Transform feetTransform;

    [Tooltip("Множитель для перевода Y в sortingOrder (целое). Больше — более точная сортировка.")]
    public int multiplier = 100;

    [Tooltip("Смещение порядка (полезно, чтобы персонажи всегда были над растениями и т.п.)")]
    public int baseOrderOffset = 0;

    private SortingGroup sortingGroup;
    private SpriteRenderer[] childRenderers;

    void Awake()
    {
        sortingGroup = GetComponent<SortingGroup>();
        RefreshRenderers();
    }

    void OnValidate()
    {
        // чтобы видеть эффект в редакторе сразу
        RefreshRenderers();
    }

    void RefreshRenderers()
    {
        childRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    void LateUpdate()
    {
        if (childRenderers == null || childRenderers.Length == 0)
            RefreshRenderers();

        if (childRenderers == null || childRenderers.Length == 0)
            return;

        float yForSorting;

        if (feetTransform != null)
        {
            // используем явно заданную точку (удобно если у спрайта pivot не в ногах)
            yForSorting = feetTransform.position.y;
        }
        else
        {
            // ищем самую нижнюю точку среди всех SpriteRenderer (world-space)
            float bottomY = float.MaxValue;
            foreach (var r in childRenderers)
            {
                if (r == null) continue;
                if (r.bounds.min.y < bottomY) bottomY = r.bounds.min.y;
            }
            yForSorting = bottomY;
        }

        int order = -Mathf.RoundToInt(yForSorting * multiplier) + baseOrderOffset;

        if (sortingGroup != null)
        {
            sortingGroup.sortingOrder = order;
        }
        else
        {
            // если нет SortingGroup — применим ко всем рендерам одинаково
            foreach (var r in childRenderers)
            {
                if (r == null) continue;
                r.sortingOrder = order;
            }
        }
    }
}
