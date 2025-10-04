using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BattleManager : MonoBehaviour
{
    [Header("Слоты")]
    public CardSlot playerSlot;
    public EnemySlot enemySlot;

    [Header("Параметры боя")]
    public float battleDelay = 1.0f;
    public bool isPaused = false;

    private Coroutine battleRoutine;

    public void StartBattle()
    {
        if (battleRoutine != null) StopCoroutine(battleRoutine);
        battleRoutine = StartCoroutine(AutoBattle());
    }

    private IEnumerator AutoBattle()
    {
        Debug.Log("=== Автобой начался ===");

        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();

        // Формируем порядок на раунд
        List<CreatureInstance> playerOrder = playerSlot.GetCreatures()
            .FindAll(c => c != null && !c.isDead);
        playerOrder.Sort((a, b) => b.transform.GetSiblingIndex().CompareTo(a.transform.GetSiblingIndex())); 
        // P4 → P3 → P2 → P1

        List<CreatureInstance> enemyOrder = enemySlot.GetCreatures()
            .FindAll(c => c != null && !c.isDead);
        enemyOrder.Sort((a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex())); 
        // E1 → E2 → E3 → E4

        int pIndex = 0;
        int eIndex = 0;

        // Пока есть живые в одной из сторон
        while (pIndex < playerOrder.Count || eIndex < enemyOrder.Count)
        {
            // --- ХОД ИГРОКА ---
            if (pIndex < playerOrder.Count)
            {
                var attacker = GetNextAlive(playerOrder, ref pIndex);
                if (attacker != null)
                {
                    var enemies = enemySlot.GetCreatures();
                    if (enemies.Count > 0)
                    {
                        var target = enemies[0]; // самый левый живой враг
                        yield return StartCoroutine(Attack(attacker, target, false));
                    }
                }
                yield return new WaitForSeconds(battleDelay);
            }

            // --- ХОД ВРАГА ---
            if (eIndex < enemyOrder.Count)
            {
                var attacker = GetNextAlive(enemyOrder, ref eIndex);
                if (attacker != null)
                {
                    var players = playerSlot.GetCreatures();
                    if (players.Count > 0)
                    {
                        var target = players[players.Count - 1]; // самый правый живой игрок
                        yield return StartCoroutine(Attack(attacker, target, true));
                    }
                    else
                    {
                        // атака по самому игроку напрямую
                        Debug.Log($"{attacker.name} атакует игрока напрямую!");
                        yield return StartCoroutine(attacker.DoAttackAnimation(true));
                        playerHealth.TakeDamage(attacker.attack);

                        if (playerHealth.currentHealth <= 0)
                        {
                            Debug.Log("Игрок проиграл!");
                            yield break;
                        }
                    }
                }
                yield return new WaitForSeconds(battleDelay);
            }
        }

        Debug.Log("=== Раунд завершён ===");

        // Проверка конца боя
        if (enemySlot.GetCreatures().Count <= 0)
        {
            Debug.Log("Победа игрока!");
            yield break;
        }
        if (playerHealth.currentHealth <= 0)
        {
            Debug.Log("Игрок проиграл!");
            yield break;
        }

        // Передаём ход обратно игроку
        FindObjectOfType<GameManager>().StartPlayerTurn();
    }

    // --- Атака с ожиданием ---
    private IEnumerator Attack(CreatureInstance attacker, CreatureInstance target, bool isEnemyAttack)
    {
        if (attacker == null || target == null || attacker.isDead || target.isDead) yield break;

        Debug.Log($"{attacker.name} атакует {target.name}");

        // ждём анимацию удара
        yield return StartCoroutine(attacker.DoAttackAnimation(isEnemyAttack));

        // наносим урон
        target.TakeDamage(attacker.attack);

        if (target.isDead)
            Destroy(target.gameObject);
    }

    // --- Вспомогательный метод: берём следующего живого ---
    private CreatureInstance GetNextAlive(List<CreatureInstance> order, ref int index)
    {
        while (index < order.Count)
        {
            var unit = order[index];
            index++;
            if (unit != null && !unit.isDead)
                return unit;
        }
        return null;
    }
}
