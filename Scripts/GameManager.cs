using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public DeckManager deckManager;
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

    private Coroutine battleRoutine;
    private float battleDelay = 1.0f; // задержка по умолчанию (обычный режим)
    private bool isPaused = false;

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

        if (battleRoutine != null) StopCoroutine(battleRoutine);
        battleRoutine = StartCoroutine(AutoBattle());
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

    private IEnumerator AutoBattle()
    {
        Debug.Log("=== Автобой начался ===");

        int round = 0;

        while (playerSlot.GetCreatures().Count > 0 && enemySlot.GetCreatures().Count > 0)
        {
            // ждём пока пауза не снимется
            while (isPaused)
                yield return null;

            List<CreatureInstance> playerCreatures = playerSlot.GetCreatures();
            List<CreatureInstance> enemyCreatures = enemySlot.GetCreatures();

            // сортировка
            playerCreatures.Sort((a, b) => b.transform.GetSiblingIndex().CompareTo(a.transform.GetSiblingIndex()));
            enemyCreatures.Sort((a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));

            if (round % 2 == 0) // игрок
            {
                if (playerCreatures.Count > 0 && enemyCreatures.Count > 0)
                {
                    Attack(playerCreatures[0], enemyCreatures[0]);
                }
            }
            else // враг
            {
                if (enemyCreatures.Count > 0 && playerCreatures.Count > 0)
                {
                    Attack(enemyCreatures[0], playerCreatures[0]);
                }
            }

            round++;
            yield return new WaitForSeconds(battleDelay);
        }

        // результат
        if (playerSlot.GetCreatures().Count > 0)
            Debug.Log("Победа игрока!");
        else if (enemySlot.GetCreatures().Count > 0)
            Debug.Log("Победа врага!");
        else
            Debug.Log("Ничья!");

        Debug.Log("=== Автобой завершён ===");

        yield return new WaitForSeconds(1f);
        StartPlayerTurn();
    }

private void Attack(CreatureInstance attacker, CreatureInstance target)
{
    if (attacker == null || target == null || attacker.isDead || target.isDead) return;

    Debug.Log($"{attacker.name} атакует {target.name}");

    StartCoroutine(AttackRoutine(attacker, target));
}

private IEnumerator AttackRoutine(CreatureInstance attacker, CreatureInstance target)
{
    // анимация движения
    yield return StartCoroutine(attacker.DoAttackAnimation(target.transform));

    // урон
    target.TakeDamage(attacker.attack);

    if (target.isDead)
    {
        Destroy(target.gameObject);
    }
}


    // --- Кнопки управления ---
    private void TogglePause()
    {
        isPaused = !isPaused;
        Debug.Log(isPaused ? "Бой на паузе" : "Бой продолжается");
    }

    private void SetNormalMode()
    {
        battleDelay = 1.0f; // медленно, чтобы видно было
        isPaused = false;
        Debug.Log("Обычный режим скорости");
    }

    private void SetFastMode()
    {
        battleDelay = 0.25f; // ускоренный
        isPaused = false;
        Debug.Log("Ускоренный режим скорости");
    }
}
