using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public Transform handPoint;
    private GameObject seedInHand;
    private GameObject nearbySeed;

    private GameObject radiusIndicator; // активный кружок радиуса

    void Update()
    {
        // Подобрать или посадить (E)
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (seedInHand == null && nearbySeed != null)
            {
                // Подобрали
                seedInHand = nearbySeed;
                nearbySeed = null;

                seedInHand.GetComponent<Collider2D>().enabled = false;
                Rigidbody2D rb = seedInHand.GetComponent<Rigidbody2D>();
                if (rb != null) rb.simulated = false;

                seedInHand.transform.SetParent(handPoint);
                seedInHand.transform.localPosition = Vector3.zero;

                // Если семя с радиусом — показываем кружок
                Seed seedData = seedInHand.GetComponent<Seed>();
                if (seedData != null && seedData.seedType == SeedType.Radius && seedData.radiusIndicatorPrefab != null)
                {
                    radiusIndicator = Instantiate(seedData.radiusIndicatorPrefab);
                    radiusIndicator.transform.localScale = new Vector3(seedData.radius * 2, seedData.radius * 2, 1);
                }

                GetComponent<PlayerMovement>().DoSquash();
            }
            else if (seedInHand != null)
            {
                // Посадка
                Seed seedData = seedInHand.GetComponent<Seed>();
                if (seedData != null)
                {
                    Vector3 plantPos = transform.position - transform.up * 0.5f;
                    Instantiate(seedData.plantPrefab, plantPos, Quaternion.identity);
                }

                Destroy(seedInHand);
                seedInHand = null;

                if (radiusIndicator != null) Destroy(radiusIndicator);

                GetComponent<PlayerMovement>().DoSquash();
            }
        }

        // Выбросить (Q)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (seedInHand != null)
            {
                seedInHand.transform.SetParent(null);
                seedInHand.transform.position = transform.position - transform.up * 0.5f;
                seedInHand.GetComponent<Collider2D>().enabled = true;

                Rigidbody2D rb = seedInHand.GetComponent<Rigidbody2D>();
                if (rb != null) rb.simulated = true;

                seedInHand = null;

                if (radiusIndicator != null) Destroy(radiusIndicator);
            }
        }

        // Обновляем позицию радиуса (если держим)
        if (radiusIndicator != null)
        {
            radiusIndicator.transform.position = transform.position - transform.up * 0.5f;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Seed"))
        {
            nearbySeed = collision.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Seed") && collision.gameObject == nearbySeed)
        {
            nearbySeed = null;
        }
    }
}
