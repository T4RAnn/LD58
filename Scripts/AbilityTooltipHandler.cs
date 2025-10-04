using UnityEngine;
using UnityEngine.EventSystems;

public class AbilityTooltipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public CreatureInstance creature; // назначить в инспекторе

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (creature != null && creature.ability != AbilityType.None)
        {
            string desc = AbilityDescriptions.Descriptions[creature.ability];
            TooltipUI.Instance.ShowTooltip(desc);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipUI.Instance.HideTooltip();
    }
}
