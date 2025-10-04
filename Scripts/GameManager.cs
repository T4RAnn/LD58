using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    [Header("Основные менеджеры")]
    public DeckManager deckManager;
    public BattleManager battleManager;
    public RewardManager rewardManager;

    [Header("Слоты")]
    public CardSlot playerSlot;
    public EnemySlot enemySlot;

    [Header("Вражеские волны")]
    public List<EnemyWave> enemyWaves;
    public int currentWaveIndex = 0;

    public int cardsPerTurn = 3;
    private bool isPlayerTurn = true;

    [Header("UI кнопки управления боем")]
    public Button pauseButton;
    public Button normalButton;
    public Button fastButton;
    public GameObject rewardPanel; // UI панель награды

    private void Start()
    {
        StartPlayerTurn();
        StartWave(currentWaveIndex);

        // подписка на кнопки
        if (pauseButton != null) pauseButton.onClick.AddListener(TogglePause);
        if (normalButton != null) normalButton.onClick.AddListener(SetNormalMode);
        if (fastButton != null) fastButton.onClick.AddListener(SetFastMode);
    }

    // --- Управление ходами ---
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

    // --- Запуск волны врагов ---
    public void StartWave(int index)
    {
        ClearEnemies();

        if (index >= enemyWaves.Count)
        {
            Debug.Log("Все волны побеждены! 🎉");
            return;
        }

        // берём нужную волну
        EnemyWave wave = enemyWaves[index];

        // перебираем врагов внутри волны
        foreach (CardData enemyCard in wave.enemies)
        {
            if (enemyCard == null || enemyCard.creaturePrefab == null) continue;

            GameObject enemyGO = Instantiate(enemyCard.creaturePrefab, enemySlot.transform);
            CreatureInstance creature = enemyGO.GetComponent<CreatureInstance>();
            creature.Initialize(enemyCard.attack, enemyCard.health, true, enemyCard);
        }

        Debug.Log($"Волна {index + 1} началась!");
    }

    // --- Очистка врагов между боями ---
    private void ClearEnemies()
    {
        foreach (Transform child in enemySlot.transform)
        {
            Destroy(child.gameObject);
        }
    }

    // --- Вызов после победы ---

public void OnBattleWon()
{
    Debug.Log("Победа! ✅");

    ClearAllSlots();
    ResetDeck();

    // скрыть слоты на время награды
    playerSlot.gameObject.SetActive(false);
    enemySlot.gameObject.SetActive(false);

    // показать награду
    rewardPanel.SetActive(true);
    rewardManager.ShowRewards();
}

// --- Полная очистка всех слотов ---
private void ClearAllSlots()
{
    // --- игрок ---
    foreach (Transform child in playerSlot.slotPanel)
    {
        CreatureInstance creature = child.GetComponent<CreatureInstance>();
        if (creature != null && creature.cardData != null && !creature.isEnemy)
        {
            // вернуть карту в сброс
            DeckManager.Instance.DiscardCard(creature.cardData);
        }

        Destroy(child.gameObject);
    }

    // --- враги ---
    foreach (Transform child in enemySlot.transform)
    {
        Destroy(child.gameObject); // враги не возвращаются в колоду
    }
}


// --- Сброс и возврат всех карт в колоду ---
private void ResetDeck()
{
    // Сбросить всё из руки в сброс
    DeckManager.Instance.EndTurn();

    // Вернуть все карты из сброса в колоду
    DeckManager.Instance.deck.AddRange(DeckManager.Instance.discardPile);
    DeckManager.Instance.discardPile.Clear();

    // Перемешать
    var deck = DeckManager.Instance.deck;
    for (int i = 0; i < deck.Count; i++)
    {
        var temp = deck[i];
        int randomIndex = Random.Range(i, deck.Count);
        deck[i] = deck[randomIndex];
        deck[randomIndex] = temp;
    }

    Debug.Log("Все карты возвращены в колоду и перемешаны");
}


    // --- Вызов после поражения ---
    public void OnBattleLost()
    {
        Debug.Log("Игрок проиграл ❌");
        // рестарт сцены
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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

[System.Serializable]
public class EnemyWave
{
    public List<CardData> enemies; // список врагов для этой волны
}