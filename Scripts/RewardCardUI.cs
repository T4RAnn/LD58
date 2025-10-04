using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class RewardCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TMP_Text nameText;
    public TMP_Text statsText;
    public Button selectButton;

    private CardData cardData;
    private RewardManager rewardManager;

    public void Setup(CardData data, RewardManager manager, int index)
    {
        cardData = data;
        rewardManager = manager;

        nameText.text = data.cardName;
        statsText.text = $"ATK: {data.attack} | HP: {data.health}";

        selectButton.onClick.AddListener(() => rewardManager.OnCardSelected(cardData));
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (cardData == null || TooltipUI.Instance == null) return;

        string desc = AbilityDescriptions.GetDescription(cardData.ability);
        TooltipUI.Instance.ShowTooltip(desc, transform);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipUI.Instance != null)
            TooltipUI.Instance.HideTooltip();
    }
}
