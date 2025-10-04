using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RewardManager : MonoBehaviour
{
    [Header("UI контейнеры")]
    public Transform rewardContainer;        // куда будут появляться карты-награды
    public GameObject rewardCardPrefab;      // префаб кнопки/карты для награды

    [Header("UI Панель награды")]
    public GameObject rewardPanelUI;         // сама панель наград (объект в Canvas, который можно включать/выключать)

    [Header("Пул наград")]
    public List<CardData> rewardPool = new List<CardData>(); // список возможных карт-наград (задаётся в инспекторе или кодом)

    private List<CardData> rewardChoices = new List<CardData>();

    /// <summary>
    /// Показать награды игроку
    /// </summary>
    public void ShowRewards(List<CardData> customPool = null)
    {
        ClearRewards();

        // если передали кастомный пул → используем его, иначе используем rewardPool
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

        // показать панель наград
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

    /// <summary>
    /// Когда игрок выбрал карту
    /// </summary>
    public void OnCardSelected(CardData chosenCard)
    {
        if (DeckManager.Instance == null)
        {
            Debug.LogError("DeckManager.Instance = null! Убедись, что DeckManager есть на сцене.");
            return;
        }

        if (DeckManager.Instance.deck == null)
            DeckManager.Instance.deck = new List<CardData>();

        Debug.Log($"Игрок выбрал награду: {chosenCard.cardName}");
        DeckManager.Instance.deck.Add(chosenCard);

        // скрыть только UI панель, но не сам RewardManager
        if (rewardPanelUI != null)
            rewardPanelUI.SetActive(false);

        // показать слоты обратно
        var gm = FindObjectOfType<GameManager>();
        gm.playerSlot.gameObject.SetActive(true);
        gm.enemySlot.gameObject.SetActive(true);

        // запустить следующую волну
        gm.currentWaveIndex++;
        gm.StartWave(gm.currentWaveIndex);
        gm.StartPlayerTurn();
    }
}
