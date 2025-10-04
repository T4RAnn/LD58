using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardCardUI : MonoBehaviour
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
}
