using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Основные менеджеры")]
    public DeckManager deckManager;
    public BattleManager battleManager;

    [Header("Слоты")]
    public CardSlot playerSlot;
    public EnemySlot enemySlot;

    [Header("Вражеские карты для боя")]
    public List<CardData> enemyCards;

    public int cardsPerTurn = 3;
    private bool isPlayerTurn = true;

    [Header("UI кнопки управления боем")]
    public Button pauseButton;
    public Button normalButton;
    public Button fastButton;

    private void Start()
    {
        StartPlayerTurn();
        SpawnEnemies();

        // подписка на кнопки
        if (pauseButton != null) pauseButton.onClick.AddListener(TogglePause);
        if (normalButton != null) normalButton.onClick.AddListener(SetNormalMode);
        if (fastButton != null) fastButton.onClick.AddListener(SetFastMode);
    }

    public void StartPlayerTurn()
    {
        isPlayerTurn = true;
        deckManager.DrawCards(cardsPerTurn);
    }

    public void EndTurn()
    {
        if (!isPlayerTurn) return;
        isPlayerTurn = false;

        // Сбросить все карты из руки в discard
        deckManager.EndTurn();

        // запускаем автобой
        battleManager.StartBattle();
    }

    private void SpawnEnemies()
    {
        foreach (CardData enemyCard in enemyCards)
        {
            if (enemyCard == null || enemyCard.creaturePrefab == null) continue;

            GameObject enemyGO = Instantiate(enemyCard.creaturePrefab, enemySlot.transform);
            CreatureInstance creature = enemyGO.GetComponent<CreatureInstance>();
            creature.Initialize(enemyCard.attack, enemyCard.health, true);
        }
    }

    // --- Кнопки управления ---
    private void TogglePause()
    {
        battleManager.isPaused = !battleManager.isPaused;
        Debug.Log(battleManager.isPaused ? "Бой на паузе" : "Бой продолжается");
    }

    private void SetNormalMode()
    {
        battleManager.battleDelay = 1.0f;
        battleManager.isPaused = false;
        Debug.Log("Обычный режим скорости");
    }

    private void SetFastMode()
    {
        battleManager.battleDelay = 0.25f;
        battleManager.isPaused = false;
        Debug.Log("Ускоренный режим скорости");
    }
}
