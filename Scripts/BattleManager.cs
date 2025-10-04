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
        GameManager gameManager = FindObjectOfType<GameManager>();

        // Формируем порядок
        List<CreatureInstance> playerOrder = playerSlot.GetCreatures()
            .FindAll(c => c != null && !c.isDead);
        playerOrder.Sort((a, b) => b.transform.GetSiblingIndex().CompareTo(a.transform.GetSiblingIndex())); 
        // справа → налево

        List<CreatureInstance> enemyOrder = enemySlot.GetCreatures()
            .FindAll(c => c != null && !c.isDead);
        enemyOrder.Sort((a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex())); 
        // слева → направо

        int pIndex = 0;
        int eIndex = 0;

        while (pIndex < playerOrder.Count || eIndex < enemyOrder.Count)
        {
            // --- ход игрока ---
            if (pIndex < playerOrder.Count)
            {
                var attacker = GetNextAlive(playerOrder, ref pIndex);
                if (attacker != null)
                {
                    var enemies = enemySlot.GetCreatures();
                    if (enemies.Count > 0)
                    {
                        var target = enemies[0]; // левый живой враг
                        yield return StartCoroutine(Attack(attacker, target, false));
                    }
                }
                yield return new WaitForSeconds(battleDelay);
            }

            // --- ход врага ---
            if (eIndex < enemyOrder.Count)
            {
                var attacker = GetNextAlive(enemyOrder, ref eIndex);
                if (attacker != null)
                {
                    var players = playerSlot.GetCreatures();
                    if (players.Count > 0)
                    {
                        var target = players[players.Count - 1]; // правый живой игрок
                        yield return StartCoroutine(Attack(attacker, target, true));
                    }
                    else
                    {
                        // атака напрямую по игроку
                        yield return StartCoroutine(attacker.DoAttackAnimation(true, battleDelay));
                        playerHealth.TakeDamage(attacker.attack);
                        if (playerHealth.currentHealth <= 0)
                        {
                            gameManager.OnBattleLost();
                            yield break;
                        }
                    }
                }
                yield return new WaitForSeconds(battleDelay);
            }
        }

        // --- завершение боя ---
        if (enemySlot.GetCreatures().Count <= 0)
        {
            gameManager.OnBattleWon();
            yield break;
        }
        if (playerHealth.currentHealth <= 0)
        {
            gameManager.OnBattleLost();
            yield break;
        }

        // если никто не умер → передаём ход игроку
        gameManager.StartPlayerTurn();
    }

    // --- атака ---
    private IEnumerator Attack(CreatureInstance attacker, CreatureInstance target, bool isEnemyAttack)
    {
        if (attacker == null || target == null || attacker.isDead || target.isDead) yield break;

        // сперва способности
        yield return StartCoroutine(TriggerAbilities(attacker));

        // потом атака
        Debug.Log($"{attacker.name} атакует {target.name}");
        yield return StartCoroutine(attacker.DoAttackAnimation(isEnemyAttack, battleDelay));

        target.TakeDamage(attacker.attack);
        if (target.isDead)
            Destroy(target.gameObject);
    }

    // --- способности ---
    private IEnumerator TriggerAbilities(CreatureInstance unit)
    {
        if (unit == null || unit.ability == AbilityType.None) yield break;

        Debug.Log($"[{unit.name}] Активирует способность: {unit.ability}");

        // сам юзер трясётся первым
        yield return unit.StartCoroutine(unit.Shake(0.25f, 7f, battleDelay));

        ICreatureSlot slot = unit.isEnemy ? (ICreatureSlot)enemySlot : playerSlot;
        var allies = slot.GetCreatures();
        int index = allies.IndexOf(unit);
        if (index == -1) yield break;

        // список тех, кто получит эффект
        List<CreatureInstance> affected = new List<CreatureInstance>();

        switch (unit.ability)
        {
            case AbilityType.BuffFrontHP4:
                var front = GetFrontAlly(allies, index, unit.isEnemy);
                if (front != null)
                {
                    front.ApplyBuff(0, 4);
                    affected.Add(front);
                }
                break;

            case AbilityType.BuffBackHP5:
                var back = GetBackAlly(allies, index, unit.isEnemy);
                if (back != null)
                {
                    back.ApplyBuff(0, 5);
                    affected.Add(back);
                }
                break;

            case AbilityType.BuffBackATK3:
                var backAtk = GetBackAlly(allies, index, unit.isEnemy);
                if (backAtk != null)
                {
                    backAtk.ApplyBuff(3, 0);
                    affected.Add(backAtk);
                }
                break;

            case AbilityType.BuffBackAllATK1:
                if (unit.isEnemy)
                {
                    for (int i = index + 1; i < allies.Count; i++)
                        if (allies[i] != null)
                        {
                            allies[i].ApplyBuff(1, 0);
                            affected.Add(allies[i]);
                        }
                }
                else
                {
                    for (int i = index - 1; i >= 0; i--)
                        if (allies[i] != null)
                        {
                            allies[i].ApplyBuff(1, 0);
                            affected.Add(allies[i]);
                        }
                }
                break;

            case AbilityType.BuffBackAllHP1:
                if (unit.isEnemy)
                {
                    for (int i = index + 1; i < allies.Count; i++)
                        if (allies[i] != null)
                        {
                            allies[i].ApplyBuff(0, 1);
                            affected.Add(allies[i]);
                        }
                }
                else
                {
                    for (int i = index - 1; i >= 0; i--)
                        if (allies[i] != null)
                        {
                            allies[i].ApplyBuff(0, 1);
                            affected.Add(allies[i]);
                        }
                }
                break;
        }

        // теперь трясём всех получивших эффект
        List<Coroutine> running = new List<Coroutine>();
        foreach (var ally in affected)
        {
            if (ally != null)
            {
                running.Add(ally.StartCoroutine(ally.Shake(0.2f, 5f, battleDelay)));
            }
        }

        foreach (var c in running)
            yield return c;

        // задержка после применения абилки
        yield return new WaitForSeconds(battleDelay);
    }

    // получить "переднего" соседа
    private CreatureInstance GetFrontAlly(List<CreatureInstance> allies, int index, bool isEnemy)
    {
        if (isEnemy)
            return (index > 0) ? allies[index - 1] : null;
        else
            return (index < allies.Count - 1) ? allies[index + 1] : null;
    }

    // получить "заднего" соседа
    private CreatureInstance GetBackAlly(List<CreatureInstance> allies, int index, bool isEnemy)
    {
        if (isEnemy)
            return (index < allies.Count - 1) ? allies[index + 1] : null;
        else
            return (index > 0) ? allies[index - 1] : null;
    }

    // выбрать живого
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

public interface ICreatureSlot
{
    List<CreatureInstance> GetCreatures();
}
