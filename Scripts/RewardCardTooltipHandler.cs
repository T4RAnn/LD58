using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class RewardCardTooltipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private RewardCardUI cardUI;
    [SerializeField] private float showDelay = 0.08f;

    private Coroutine showRoutine;

    private void Reset()
    {
        if (cardUI == null) cardUI = GetComponent<RewardCardUI>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (showRoutine != null) StopCoroutine(showRoutine);
        showRoutine = StartCoroutine(ShowTooltipDelayed());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (showRoutine != null) { StopCoroutine(showRoutine); showRoutine = null; }
        if (TooltipUI.Instance != null) TooltipUI.Instance.HideTooltip();
    }

    private IEnumerator ShowTooltipDelayed()
    {
        yield return new WaitForSeconds(showDelay);

        if (cardUI == null || cardUI.cardData == null) yield break;

        AbilityType ability = cardUI.cardData.ability;
        string desc = null;

        if (AbilityDescriptions.Descriptions.TryGetValue(ability, out desc))
        {
            if (TooltipUI.Instance != null)
                TooltipUI.Instance.ShowTooltip(desc, cardUI.transform);
        }
    }
}
