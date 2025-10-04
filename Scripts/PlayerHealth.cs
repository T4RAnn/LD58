using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Настройки здоровья")]
    public int maxHealth = 20;
    public int currentHealth;

    [Header("UI")]
    public Slider healthSlider;

    private void Start()
    {
        currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    public void TakeDamage(int dmg)
    {
        currentHealth -= dmg;
        if (currentHealth < 0) currentHealth = 0;

        if (healthSlider != null)
            healthSlider.value = currentHealth;

        if (currentHealth <= 0)
        {
            Debug.Log("Игрок проиграл!");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // перезапуск сцены
        }
    }
}
