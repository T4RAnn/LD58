using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class RewardManager : MonoBehaviour
{
    [Header("UI контейнеры")]
    public Transform rewardContainer;
    public GameObject rewardCardPrefab;

    [Header("UI Панель награды")]
    public GameObject rewardPanelUI;

    [Header("Текст настроения")]
    public TMP_Text rewardIntroTextLine1;
    public TMP_Text rewardIntroTextLine2;

    private List<CardData> currentRewardChoices = new List<CardData>();

    // Показываем награду из конкретного списка врагов
    public void ShowRewardsFromEnemies(List<CardData> enemies)
    {
        StartCoroutine(ShowRewardsRoutine(enemies));
    }

    private IEnumerator ShowRewardIntro(string line1, string line2)
    {
        if (rewardIntroTextLine1 == null)
        {
            GameObject go1 = new GameObject("RewardIntroLine1");
            go1.transform.SetParent(rewardPanelUI.transform, false);
            rewardIntroTextLine1 = go1.AddComponent<TMP_Text>();
            rewardIntroTextLine1.fontSize = 36;
            rewardIntroTextLine1.alignment = TextAlignmentOptions.Center;
            rewardIntroTextLine1.color = Color.white;
            rewardIntroTextLine1.rectTransform.anchoredPosition = new Vector2(0, 50);
        }

        if (rewardIntroTextLine2 == null)
        {
            GameObject go2 = new GameObject("RewardIntroLine2");
            go2.transform.SetParent(rewardPanelUI.transform, false);
            rewardIntroTextLine2 = go2.AddComponent<TMP_Text>();
            rewardIntroTextLine2.fontSize = 28;
            rewardIntroTextLine2.alignment = TextAlignmentOptions.Center;
            rewardIntroTextLine2.color = Color.white;
            rewardIntroTextLine2.rectTransform.anchoredPosition = new Vector2(0, -10);
        }

        rewardIntroTextLine1.text = line1;
        rewardIntroTextLine2.text = line2;

        rewardIntroTextLine1.gameObject.SetActive(true);
        rewardIntroTextLine2.gameObject.SetActive(true);

        CanvasGroup cg1 = rewardIntroTextLine1.GetComponent<CanvasGroup>();
        if (cg1 == null) cg1 = rewardIntroTextLine1.gameObject.AddComponent<CanvasGroup>();
        CanvasGroup cg2 = rewardIntroTextLine2.GetComponent<CanvasGroup>();
        if (cg2 == null) cg2 = rewardIntroTextLine2.gameObject.AddComponent<CanvasGroup>();

        cg1.alpha = 0;
        cg2.alpha = 0;

        float duration = 0.5f;
        for (float t = 0; t <= 1; t += Time.deltaTime / duration)
        {
            cg1.alpha = t;
            yield return null;
        }

        yield return new WaitForSeconds(1f);

        for (float t = 1; t >= 0; t -= Time.deltaTime / duration)
        {
            cg1.alpha = t;
            yield return null;
        }
        rewardIntroTextLine1.gameObject.SetActive(false);
        cg2.alpha = 1f;
    }

    private IEnumerator ShowRewardsRoutine(List<CardData> enemies)
    {
        ClearRewards();

        rewardPanelUI.SetActive(true);

        yield return StartCoroutine(ShowRewardIntro(
            "The battle is done. The cowed monsters await your selection.",
            "Choose one to bind to your will"
        ));

        currentRewardChoices.Clear();
        int count = Mathf.Min(3, enemies.Count);

        List<CardData> availableEnemies = new List<CardData>(enemies);
        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, availableEnemies.Count);
            CardData enemy = availableEnemies[index];
            availableEnemies.RemoveAt(index);

            currentRewardChoices.Add(enemy);

            GameObject cardGO = Instantiate(rewardCardPrefab, rewardContainer);
            cardGO.SetActive(false);
            RewardCardUI rewardUI = cardGO.GetComponent<RewardCardUI>();
            rewardUI.Setup(enemy, this, i);

            StartCoroutine(FadeInCardAndEnable(cardGO));
            yield return new WaitForSeconds(0.2f);
        }
    }

    private IEnumerator FadeInCardAndEnable(GameObject cardGO)
    {
        cardGO.SetActive(true);
        CanvasGroup cg = cardGO.GetComponent<CanvasGroup>();
        if (cg == null) cg = cardGO.AddComponent<CanvasGroup>();

        cg.alpha = 0;
        float duration = 0.5f;
        for (float t = 0; t <= 1; t += Time.deltaTime / duration)
        {
            cg.alpha = t;
            yield return null;
        }
    }

    private void ClearRewards()
    {
        foreach (Transform child in rewardContainer)
            Destroy(child.gameObject);
    }

    public void OnCardSelected(RewardCardUI selectedCardUI)
    {
        selectedCardUI.GetComponent<Button>().interactable = false;
        Canvas rootCanvas = FindObjectOfType<Canvas>();
        selectedCardUI.transform.SetParent(rootCanvas.transform, true);
        StartCoroutine(AnimateRewardCard(selectedCardUI));
    }

    private IEnumerator AnimateRewardCard(RewardCardUI cardUI)
    {
        yield return cardUI.StartCoroutine(cardUI.FlyToDeck(DeckManager.Instance.deckIconTransform));

        if (rewardPanelUI != null)
            rewardPanelUI.SetActive(false);

        DeckManager.Instance.deck.Add(cardUI.cardData);

        Destroy(cardUI.gameObject);

        var gm = FindObjectOfType<GameManager>();
        gm.currentWaveIndex++;
        yield return gm.StartCoroutine(gm.TransitionToNextWave());

        if (rewardIntroTextLine2 != null)
            rewardIntroTextLine2.gameObject.SetActive(false);
    }
}
