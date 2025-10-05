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

    List<CreatureInstance> playerOrder = playerSlot.GetCreatures()
        .FindAll(c => c != null && !c.isDead);
    playerOrder.Sort((a, b) => b.transform.GetSiblingIndex().CompareTo(a.transform.GetSiblingIndex()));

    List<CreatureInstance> enemyOrder = enemySlot.GetCreatures()
        .FindAll(c => c != null && !c.isDead);
    enemyOrder.Sort((a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));

    int pIndex = 0;
    int eIndex = 0;

    while (pIndex < playerOrder.Count || eIndex < enemyOrder.Count)
    {
        // 🟡 Ждём, пока не закончится пауза
        while (isPaused)
            yield return null;

        // --- ход игрока ---
        if (pIndex < playerOrder.Count)
        {
            var attacker = GetNextAlive(playerOrder, ref pIndex);
            if (attacker != null)
            {
                var enemies = enemySlot.GetCreatures();
                if (enemies.Count > 0)
                {
                    var target = enemies[0];
                    yield return StartCoroutine(Attack(attacker, target, false));
                }
            }
            yield return new WaitForSeconds(battleDelay);
        }

        // 🟡 Проверяем паузу между ходами
        while (isPaused)
            yield return null;

        // --- ход врага ---
        if (eIndex < enemyOrder.Count)
        {
            var attacker = GetNextAlive(enemyOrder, ref eIndex);
            if (attacker != null)
            {
                var players = playerSlot.GetCreatures();
                if (players.Count > 0)
                {
                    var target = players[players.Count - 1];
                    yield return StartCoroutine(Attack(attacker, target, true));
                }
                else
                {
                    yield return StartCoroutine(attacker.DoAttackAnimation(true));
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

    gameManager.StartPlayerTurn();
}


    // --- атака ---
    private IEnumerator Attack(CreatureInstance attacker, CreatureInstance target, bool isEnemyAttack)
    {
        if (attacker == null || attacker.isDead) yield break;

        if (target == null || target.isDead)
        {
            target = GetNextTarget(isEnemyAttack);
            if (target == null)
            {
                Debug.Log("❌ Нет живых целей для атаки.");
                yield break;
            }
        }

        // сперва способности
        yield return StartCoroutine(TriggerAbilities(attacker));

        // ⚠️ Если это DoubleAttack — обычную атаку пропускаем
        if (attacker.ability == AbilityType.DoubleAttack)
            yield break;

        // обычная атака
        Debug.Log($"{attacker.name} атакует {target.name}");
        yield return StartCoroutine(attacker.DoAttackAnimation(isEnemyAttack));

        target.TakeDamage(attacker.attack);
        yield return new WaitForSeconds(0.2f);
    }

    private CreatureInstance GetNextTarget(bool isEnemyAttack)
    {
        ICreatureSlot targetSlot = isEnemyAttack ? (ICreatureSlot)playerSlot : enemySlot;
        var list = targetSlot.GetCreatures();

        if (list == null || list.Count == 0)
            return null;

        // для врагов — бьём правого игрока, для игрока — левого врага
        if (isEnemyAttack)
        {
            for (int i = list.Count - 1; i >= 0; i--)
                if (list[i] != null && !list[i].isDead)
                    return list[i];
        }
        else
        {
            for (int i = 0; i < list.Count; i++)
                if (list[i] != null && !list[i].isDead)
                    return list[i];
        }

        return null;
    }

    // --- способности ---
    private IEnumerator TriggerAbilities(CreatureInstance unit)
    {
        if (unit == null || unit.ability == AbilityType.None) yield break;

        Debug.Log($"[{unit.name}] Активирует способность: {unit.ability}");

        // сам юзер трясётся первым
        yield return unit.StartCoroutine(unit.Shake(0.25f, 7f));

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

            // 🔹 Увеличивает себя 1 хп и 1 атк
            case AbilityType.SelfBuff1HP1ATK:
                unit.ApplyBuff(1, 1);
                affected.Add(unit);
                break;

            // 🔹 Блокирует 1 урон — запоминаем эффект
            case AbilityType.Block1Damage:
                unit.StartCoroutine(ApplyTemporaryBlock(unit, 1));
                break;

            // 🔹 Атакует дважды — просто делаем вторую атаку позже
            case AbilityType.DoubleAttack:
                unit.StartCoroutine(DoubleAttack(unit, unit.isEnemy));
                break;

            // 🔹 Призывает существо перед собой
            case AbilityType.SummonInFront:
                unit.StartCoroutine(SummonAllyInFront(unit));
                break;
        }

        // теперь трясём всех получивших эффект
        List<Coroutine> running = new List<Coroutine>();
        foreach (var ally in affected)
        {
            if (ally != null)
            {
                running.Add(ally.StartCoroutine(ally.Shake(0.2f, 5f)));
            }
        }

        foreach (var c in running)
            yield return c;

        // задержка после применения абилки
        yield return new WaitForSeconds(battleDelay);
    }

    // --- Временный блок урона ---
    private IEnumerator ApplyTemporaryBlock(CreatureInstance unit, int amount)
    {
        unit.blockValue += amount;
        Debug.Log($"{unit.name} получает блок {amount} на каждую атаку!");
        yield return null;
    }

    // --- Двойная атака ---
    private IEnumerator DoubleAttack(CreatureInstance unit, bool isEnemy)
    {
        Debug.Log($"{unit.name} выполняет двойную атаку!");

        ICreatureSlot targetSlot = isEnemy ? (ICreatureSlot)playerSlot : enemySlot;
        var targets = targetSlot.GetCreatures();

        if (targets == null || targets.Count == 0)
            yield break;

        var target = isEnemy ? GetRightmostAlive(targets) : GetLeftmostAlive(targets);
        if (target == null)
            yield break;

        yield return StartCoroutine(AttackOnce(unit, target, isEnemy));
        yield return new WaitForSeconds(0.4f);

        if (target == null || target.isDead)
        {
            targets = targetSlot.GetCreatures();
            target = isEnemy ? GetRightmostAlive(targets) : GetLeftmostAlive(targets);
            if (target == null)
            {
                Debug.Log($"{unit.name} никого не нашёл для второго удара.");
                yield break;
            }
        }

        Debug.Log($"{unit.name} наносит второй удар по {target.name}");
        yield return StartCoroutine(AttackOnce(unit, target, isEnemy));

        yield return new WaitForSeconds(0.2f);
    }

    private IEnumerator AttackOnce(CreatureInstance attacker, CreatureInstance target, bool isEnemyAttack)
    {
        if (attacker == null || attacker.isDead || target == null || target.isDead)
            yield break;

        Debug.Log($"{attacker.name} атакует {target.name} (одиночная атака)");
        yield return StartCoroutine(attacker.DoAttackAnimation(isEnemyAttack));

        target.TakeDamage(attacker.attack);

        yield return new WaitForSeconds(0.2f);
    }

    // --- Призыв существа перед собой ---
    private IEnumerator SummonAllyInFront(CreatureInstance summoner)
    {
        if (summoner == null) yield break;

        Debug.Log($"{summoner.name} призывает существо перед собой!");

        ICreatureSlot slot = summoner.isEnemy ? (ICreatureSlot)enemySlot : playerSlot;
        var allies = slot.GetCreatures();
        int index = allies.IndexOf(summoner);
        if (index == -1) yield break;

        // Выбираем префаб для призыва
        GameObject prefabToSummon = summoner.summonPrefab != null
            ? summoner.summonPrefab
            : summoner.cardData?.creaturePrefab;

        if (prefabToSummon == null) yield break;

        // Создаём новый экземпляр
        GameObject newCreature = Instantiate(prefabToSummon, summoner.transform.parent);

        // Рассчитываем безопасный индекс
        int insertIndex = summoner.isEnemy ? Mathf.Max(0, index - 1) : Mathf.Min(allies.Count, index + 1);

        // Ставим объект на нужную позицию в иерархии
        newCreature.transform.SetSiblingIndex(insertIndex);

    // Инициализируем CreatureInstance
    var instance = newCreature.GetComponent<CreatureInstance>();
        if (instance != null)
        {
            // Всегда 2/2 для призываемого монстра
            int atk = 2;
            int hp = 2;

            instance.Initialize(atk, hp, summoner.isEnemy);
            allies.Insert(insertIndex, instance);
        }

        yield return new WaitForSeconds(battleDelay);
    }

    public static CreatureInstance GetExtremeAlly(ICreatureSlot slot, bool leftmost = true)
    {
        if (slot == null) return null;

        var list = slot.GetCreatures();
        if (list == null || list.Count == 0)
            return null;

        return leftmost ? list[0] : list[list.Count - 1];
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

    private CreatureInstance GetLeftmostAlive(List<CreatureInstance> list)
    {
        foreach (var c in list)
            if (c != null && !c.isDead)
                return c;
        return null;
    }

    private CreatureInstance GetRightmostAlive(List<CreatureInstance> list)
    {
        for (int i = list.Count - 1; i >= 0; i--)
            if (list[i] != null && !list[i].isDead)
                return list[i];
        return null;
    }
}

public interface ICreatureSlot
{
    List<CreatureInstance> GetCreatures();
}
