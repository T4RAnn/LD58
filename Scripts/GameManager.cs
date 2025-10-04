using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public DeckManager deckManager;
    public CardSlot playerSlot;
    public EnemySlot enemySlot;

    [Header("Вражеские карты для боя")]
    public List<CardData> enemyCards; // сюда в инспекторе закидываешь врагов

    public int cardsPerTurn = 3;
    private bool isPlayerTurn = true;

    private void Start()
    {
        // старт игры
        StartPlayerTurn();
        SpawnEnemies(); // сразу добавляем врагов
    }

    public void StartPlayerTurn()
    {
        isPlayerTurn = true;
        deckManager.DrawCards(cardsPerTurn); // выдаём карты игроку
    }

    public void EndTurn()
    {
        if (!isPlayerTurn) return;
        isPlayerTurn = false;
        StartCoroutine(AutoBattle());
    }

    private void SpawnEnemies()
    {
        foreach (CardData enemyCard in enemyCards)
        {
            if (enemyCard == null || enemyCard.creaturePrefab == null) continue;

            GameObject enemyGO = Instantiate(enemyCard.creaturePrefab, enemySlot.transform);
            CreatureInstance creature = enemyGO.GetComponent<CreatureInstance>();
            creature.Initialize(enemyCard.attack, enemyCard.health, true); // true = враг
        }
    }

    private IEnumerator AutoBattle()
    {
        Debug.Log("=== Автобой начался ===");

        List<CreatureInstance> playerCreatures = playerSlot.GetCreatures();
        List<CreatureInstance> enemyCreatures = enemySlot.GetCreatures();

        int maxRounds = Mathf.Max(playerCreatures.Count, enemyCreatures.Count);

        for (int i = 0; i < maxRounds; i++)
        {
            yield return new WaitForSeconds(0.5f);

            // атаки (оставляем твою логику)
            if (enemyCreatures.Count > i && playerCreatures.Count > 0)
            {
                CreatureInstance attacker = enemyCreatures[enemyCreatures.Count - 1 - i];
                if (attacker != null && !attacker.isDead)
                {
                    CreatureInstance target = playerCreatures[0];
                    Attack(attacker, target, playerCreatures);
                }
            }

            yield return new WaitForSeconds(0.5f);

            if (enemyCreatures.Count > i && playerCreatures.Count > 0)
            {
                CreatureInstance attacker = enemyCreatures[i];
                if (attacker != null && !attacker.isDead)
                {
                    CreatureInstance target = playerCreatures[playerCreatures.Count - 1];
                    Attack(attacker, target, playerCreatures);
                }
            }

            yield return new WaitForSeconds(0.5f);

            if (playerCreatures.Count > i && enemyCreatures.Count > 0)
            {
                CreatureInstance attacker = playerCreatures[i];
                if (attacker != null && !attacker.isDead)
                {
                    CreatureInstance target = enemyCreatures[enemyCreatures.Count - 1];
                    Attack(attacker, target, enemyCreatures);
                }
            }

            yield return new WaitForSeconds(0.5f);

            if (enemyCreatures.Count > i && playerCreatures.Count > 0)
            {
                CreatureInstance attacker = enemyCreatures[i];
                if (attacker != null && !attacker.isDead)
                {
                    CreatureInstance target = playerCreatures[playerCreatures.Count - 1];
                    Attack(attacker, target, playerCreatures);
                }
            }
        }

        Debug.Log("=== Автобой завершён ===");

        yield return new WaitForSeconds(1f);
        StartPlayerTurn();
    }

    private void Attack(CreatureInstance attacker, CreatureInstance target, List<CreatureInstance> list)
    {
        if (attacker == null || target == null) return;

        Debug.Log($"{attacker.name} атакует {target.name}");

        target.TakeDamage(attacker.attack);

        if (target.isDead)
        {
            list.Remove(target);
            Destroy(target.gameObject);
        }
    }
}
