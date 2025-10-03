using UnityEngine;

public enum SeedType { Normal, Radius }

public class Seed : MonoBehaviour
{
    public SeedType seedType = SeedType.Normal;   // Тип семени
    public GameObject plantPrefab;                // Какое растение появляется
    public float radius = 2f;                     // Радиус (для Radius семени)
    public GameObject radiusIndicatorPrefab;      // Префаб кружка-подсказки
}
