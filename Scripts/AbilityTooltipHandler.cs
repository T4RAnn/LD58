using UnityEngine;
using UnityEngine.EventSystems;

public class AbilityTooltipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private CardInstance card;           // если это карта (UI)
    [SerializeField] private CreatureInstance creature;   // если это существо (UI/поле)
    [SerializeField] private float showDelay = 0.08f;     // короткая задержка перед показом

    private Coroutine showRoutine;

    private void Reset()
    {
        // автоподхват компонентов, если навесить на тот же префаб
        if (card == null) card = GetComponent<CardInstance>();
        if (creature == null) creature = GetComponent<CreatureInstance>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // стартуем рутину с небольшой задержкой (устраняет дергание)
        if (showRoutine != null) StopCoroutine(showRoutine);
        showRoutine = StartCoroutine(ShowTooltipDelayed());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (showRoutine != null) { StopCoroutine(showRoutine); showRoutine = null; }
        if (TooltipUI.Instance != null) TooltipUI.Instance.HideTooltip();
    }

    private System.Collections.IEnumerator ShowTooltipDelayed()
    {
        yield return new WaitForSeconds(showDelay);

        AbilityType ability = AbilityType.None;
        Transform t = null;
        string desc = null;

        if (card != null && card.data != null)
        {
            ability = card.data.ability;
            t = card.transform;
        }
        else if (creature != null)
        {
            ability = creature.ability;
            t = creature.transform;
        }

        if (ability != AbilityType.None && AbilityDescriptions.Descriptions.TryGetValue(ability, out desc))
        {
            if (TooltipUI.Instance != null)
                TooltipUI.Instance.ShowTooltip(desc, t);
        }
    }
}
