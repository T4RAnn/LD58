using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;

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
    public GameObject rewardPanel;
    public CanvasGroup fadePanel; // назначить в инспекторе

    [Header("UI Текст победы")]
    public GameObject victoryTextUI; // текст "YOU WON!"

private void Start()
{
    if (victoryTextUI != null)
        victoryTextUI.SetActive(false); // точно скрываем при старте

    if (fadePanel != null)
        StartCoroutine(FadeIn(1.2f));

    StartPlayerTurn();
    StartWave(currentWaveIndex);

    if (pauseButton != null) pauseButton.onClick.AddListener(TogglePause);
    if (normalButton != null) normalButton.onClick.AddListener(SetNormalMode);
    if (fastButton != null) fastButton.onClick.AddListener(SetFastMode);
}


    // === Плавное проявление / исчезновение ===
    private IEnumerator FadeIn(float duration = 1f)
    {
        fadePanel.alpha = 1f;
        fadePanel.gameObject.SetActive(true);
        while (fadePanel.alpha > 0f)
        {
            fadePanel.alpha -= Time.deltaTime / duration;
            yield return null;
        }
        fadePanel.alpha = 0f;
        fadePanel.gameObject.SetActive(false);
    }

    private IEnumerator FadeOut(float duration = 1f)
    {
        fadePanel.gameObject.SetActive(true);
        fadePanel.alpha = 0f;
        while (fadePanel.alpha < 1f)
        {
            fadePanel.alpha += Time.deltaTime / duration;
            yield return null;
        }
        fadePanel.alpha = 1f;
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

        deckManager.EndTurn();
        battleManager.StartBattle();
    }

    // --- Волны врагов ---
    public void StartWave(int index)
    {
        ClearEnemies();

        if (index >= enemyWaves.Count)
        {
            Debug.Log("Все волны побеждены! 🎉");
            return;
        }

        EnemyWave wave = enemyWaves[index];

        foreach (CardData enemyCard in wave.enemies)
        {
            if (enemyCard == null || enemyCard.creaturePrefab == null) continue;

            GameObject enemyGO = Instantiate(enemyCard.creaturePrefab, enemySlot.transform);
            CreatureInstance creature = enemyGO.GetComponent<CreatureInstance>();
            creature.Initialize(enemyCard.attack, enemyCard.health, true, enemyCard);
        }

        Debug.Log($"Волна {index + 1} началась!");
    }

    private void ClearEnemies()
    {
        foreach (Transform child in enemySlot.transform)
        {
            Destroy(child.gameObject);
        }
    }

    // --- Победа ---
    public void OnBattleWon()
    {
        StartCoroutine(HandleBattleWonTransition());
    }

private IEnumerator HandleBattleWonTransition()
{


    Debug.Log("Победа! ✅");

    // затемняем экран
    yield return StartCoroutine(FadeOut(0.8f));

    // проверяем, последняя ли волна
    bool isLastWave = currentWaveIndex >= enemyWaves.Count - 1;

    if (victoryTextUI != null)
    {
        victoryTextUI.SetActive(true);
        CanvasGroup cg = victoryTextUI.GetComponent<CanvasGroup>();
        if (cg == null) cg = victoryTextUI.AddComponent<CanvasGroup>();
        cg.alpha = 0;
        Vector3 originalScale = victoryTextUI.transform.localScale;
        victoryTextUI.transform.localScale = originalScale * 0.5f;

        TMP_Text tmpText = victoryTextUI.GetComponent<TMP_Text>();
        if (tmpText != null)
        {
            tmpText.text = isLastWave ? "Thank you for playing!" : "YOU WON!";
        }

        float duration = 0.5f;
        for (float t = 0; t <= 1; t += Time.deltaTime / duration)
        {
            cg.alpha = t;
            victoryTextUI.transform.localScale = Vector3.Lerp(originalScale * 0.5f, originalScale, t);
            yield return null;
        }
        
        // 🔊 проигрываем звук победы
            AudioManager.Instance?.PlayVictory();
        
        // если не последняя волна — подождём и спрячем текст
            if (!isLastWave)
            {
                yield return new WaitForSeconds(1.5f);
                for (float t = 1; t >= 0; t -= Time.deltaTime / duration)
                {
                    cg.alpha = t;
                    victoryTextUI.transform.localScale = Vector3.Lerp(originalScale * 0.5f, originalScale, t);
                    yield return null;
                }
                victoryTextUI.SetActive(false);
                victoryTextUI.transform.localScale = originalScale;
            }
            else
            {
                // последняя волна — текст остаётся, экран черный
                cg.alpha = 1f;
                victoryTextUI.transform.localScale = originalScale;
            }
    }

    // очистка слотов и колоды
    ClearAllSlots();
    ResetDeck();

    // скрываем слоты перед показом награды
    playerSlot.gameObject.SetActive(false);
    enemySlot.gameObject.SetActive(false);

    // показываем панель наград
    rewardPanel.SetActive(true);

        // 🎯 Награды из текущей волны
        if (!isLastWave && currentWaveIndex < enemyWaves.Count)
        {
            rewardPanel.SetActive(true); // включаем только если реально будут награды
            List<CardData> currentEnemies = enemyWaves[currentWaveIndex].enemies;
            if (currentEnemies != null && currentEnemies.Count > 0)
            {
                rewardManager.ShowRewardsFromEnemies(currentEnemies);
            }
        }
        else
        {
            rewardPanel.SetActive(false); // если наград нет — панель выключена
        }

    // возвращаем плавное проявление фона только если не последняя волна
    if (!isLastWave)
        yield return StartCoroutine(FadeIn(0.8f));
}



    // --- Новый метод для перехода после награды ---
    public IEnumerator TransitionToNextWave()
    {
        yield return StartCoroutine(FadeOut(0.8f));

        playerSlot.gameObject.SetActive(true);
        enemySlot.gameObject.SetActive(true);

        StartWave(currentWaveIndex);
        StartPlayerTurn();

        yield return StartCoroutine(FadeIn(0.8f));
    }

    private void ClearAllSlots()
    {
        foreach (Transform child in playerSlot.slotPanel)
        {
            CreatureInstance creature = child.GetComponent<CreatureInstance>();
            if (creature != null && !creature.isEnemy && creature.cardData != null)
            {
                DeckManager.Instance.StartCoroutine(
                    DeckManager.Instance.AnimateToDiscardCard(creature.cardData, child.gameObject)
                );
            }
        }

        foreach (Transform child in enemySlot.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void ResetDeck()
    {
        DeckManager.Instance.EndTurn();
        DeckManager.Instance.deck.AddRange(DeckManager.Instance.discardPile);
        DeckManager.Instance.discardPile.Clear();

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

    public void OnBattleLost()
    {
        StartCoroutine(RestartWithFade());
    }

    private IEnumerator RestartWithFade()
    {
        yield return StartCoroutine(FadeOut(0.8f));
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

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
    public List<CardData> enemies;
    [Tooltip("Индекс заготовки награды после победы над этой волной")]
    public int rewardTemplateIndex = 0;
}
