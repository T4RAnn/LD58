using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class RewardManager : MonoBehaviour
{
    [Header("UI контейнеры")]
    public Transform rewardContainer;
    public GameObject rewardCardPrefab;

    [Header("UI Панель награды")]
    public GameObject rewardPanelUI;

    [Header("Пул наград")]
    public List<CardData> rewardPool = new List<CardData>();

    private List<CardData> rewardChoices = new List<CardData>();

    public void ShowRewards(List<CardData> customPool = null)
    {
        ClearRewards();

        var pool = (customPool != null && customPool.Count > 0) ? customPool : rewardPool;

        rewardChoices.Clear();
        for (int i = 0; i < 3; i++)
        {
            CardData randomCard = GetRandomCard(pool);
            rewardChoices.Add(randomCard);

            GameObject cardGO = Instantiate(rewardCardPrefab, rewardContainer);
            RewardCardUI rewardUI = cardGO.GetComponent<RewardCardUI>();
            rewardUI.Setup(randomCard, this, i);
        }

        if (rewardPanelUI != null)
            rewardPanelUI.SetActive(true);
    }

    private CardData GetRandomCard(List<CardData> pool)
    {
        return pool[Random.Range(0, pool.Count)];
    }

    private void ClearRewards()
    {
        foreach (Transform child in rewardContainer)
            Destroy(child.gameObject);
    }

public void OnCardSelected(RewardCardUI selectedCardUI)
{
    if (DeckManager.Instance == null)
    {
        Debug.LogError("DeckManager.Instance = null!");
        return;
    }

    // отключаем возможность кликать на карту
    selectedCardUI.GetComponent<Button>().interactable = false;

    // переносим карту на уровень Canvas, чтобы она не зависела от rewardPanelUI
    Canvas rootCanvas = FindObjectOfType<Canvas>();
    selectedCardUI.transform.SetParent(rootCanvas.transform, true);

    // запускаем анимацию полёта карты + скрытие панели
    StartCoroutine(AnimateRewardCard(selectedCardUI));
}

    private IEnumerator AnimateRewardCard(RewardCardUI cardUI)
    {
        // анимация полёта карты (можно в колоду или центр экрана)
        yield return cardUI.StartCoroutine(cardUI.FlyToDeck(DeckManager.Instance.deckIconTransform));

        // скрываем панель награды после окончания анимации
        if (rewardPanelUI != null)
            rewardPanelUI.SetActive(false);

        // добавляем карту в колоду (если не сделано внутри FlyToDeck)
        DeckManager.Instance.deck.Add(cardUI.cardData);

        Destroy(cardUI.gameObject);

        // включаем слоты обратно
        var gm = FindObjectOfType<GameManager>();
        gm.playerSlot.gameObject.SetActive(true);
        gm.enemySlot.gameObject.SetActive(true);


        // запускаем следующую волну и ход
        gm.currentWaveIndex++;
        gm.StartWave(gm.currentWaveIndex);
        gm.StartPlayerTurn();
    }

}
