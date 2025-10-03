using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlantGrowth : MonoBehaviour
{
    public Sprite[] growthStages;      // 0 = семечко, 1-3 = рост, 4 = готовое растение
    public float timePerStage = 5f;    // время (секунд) между стадиями

    private SpriteRenderer sr;
    private int currentStage = 0;
    private float timer;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (growthStages.Length > 0)
        {
            sr.sprite = growthStages[0]; // первая стадия при посадке
        }
    }

    void Update()
    {
        if (currentStage < growthStages.Length - 1)
        {
            timer += Time.deltaTime;
            if (timer >= timePerStage)
            {
                timer = 0f;
                currentStage++;
                sr.sprite = growthStages[currentStage];
            }
        }
    }

    public bool IsFullyGrown()
    {
        return currentStage == growthStages.Length - 1;
    }
}
