using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
public class PlayerHealth : MonoBehaviour
{
    [Header("Настройки здоровья")]
    public int maxHealth = 20;
    public int currentHealth;

    [Header("UI")]
    public Scrollbar healthBar;       // шкала здоровья
    public TMP_Text healthText;           // текст "HP: X / Y"
    public RectTransform healthPanel; // сама панель (для тряски)

    [Header("Эффекты")]
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 10f;
    public float smoothFillSpeed = 5f;   // скорость плавного изменения HP

    private GameManager gameManager;
    private float targetFill; // куда должна изменяться полоска

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateUI(true);

        gameManager = FindObjectOfType<GameManager>();
    }

    private void Update()
    {
        // плавное обновление HP-бара
        if (healthBar != null)
            healthBar.size = Mathf.Lerp(healthBar.size, targetFill, Time.deltaTime * smoothFillSpeed);
    }

    public void TakeDamage(int dmg)
    {
        currentHealth -= dmg;
        if (currentHealth < 0) currentHealth = 0;

        UpdateUI();

        // Эффекты
        if (healthPanel != null)
            StartCoroutine(ShakeHealthPanel());

        if (currentHealth <= 0)
        {
            Debug.Log("Игрок проиграл!");
            if (gameManager != null)
                gameManager.OnBattleLost();
        }
    }

    private void UpdateUI(bool instant = false)
    {
        targetFill = (float)currentHealth / maxHealth;

        if (instant && healthBar != null)
            healthBar.size = targetFill;

        if (healthText != null)
            healthText.text = $"HP: {currentHealth} / {maxHealth}";
    }

    private IEnumerator ShakeHealthPanel()
    {
        Vector3 originalPos = healthPanel.anchoredPosition;

        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float offsetX = Random.Range(-1f, 1f) * shakeMagnitude;
            float offsetY = Random.Range(-1f, 1f) * shakeMagnitude;

            healthPanel.anchoredPosition = originalPos + new Vector3(offsetX, offsetY, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        healthPanel.anchoredPosition = originalPos;
    }
}
