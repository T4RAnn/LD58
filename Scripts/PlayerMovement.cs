using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;

    public Transform spriteRoot;   // визуальная часть персонажа
    private Quaternion baseRotation;

    private float animTimer;
    private float currentTiltPower = 0f; // текущая сила качания
    private float targetTiltPower = 0f;  // желаемая сила качания

    public float tiltAmount = 8f;   // максимальный угол наклона
    public float tiltSpeed = 6f;    // скорость качания
    public float buildupSpeed = 2f; // скорость "разгона" качания
    public float decaySpeed = 4f;   // скорость затухания качания

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (spriteRoot == null) spriteRoot = transform;
        baseRotation = spriteRoot.localRotation;
    }

    void Update()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        // Если двигаемся — хотим качаться
        if (moveInput.sqrMagnitude > 0.1f)
        {
            targetTiltPower = 1f;
        }
        else
        {
            targetTiltPower = 0f;
        }

        // Плавно приближаем силу качания к цели
        currentTiltPower = Mathf.MoveTowards(currentTiltPower, targetTiltPower,
                                             Time.deltaTime * (targetTiltPower > currentTiltPower ? buildupSpeed : decaySpeed));

        // Качание
        if (currentTiltPower > 0.01f)
        {
            animTimer += Time.deltaTime * tiltSpeed;
            float tilt = Mathf.Sin(animTimer) * tiltAmount * currentTiltPower;
            spriteRoot.localRotation = Quaternion.Euler(0, 0, tilt);
        }
        else
        {
            // возвращаемся в исходное положение
            spriteRoot.localRotation = Quaternion.Lerp(spriteRoot.localRotation, baseRotation, Time.deltaTime * decaySpeed);
            animTimer = 0;
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveInput.normalized * speed * Time.fixedDeltaTime);
    }

    // Приседание при взаимодействии
    public void DoSquash()
    {
        StopAllCoroutines();
        StartCoroutine(SquashCoroutine());
    }

    private System.Collections.IEnumerator SquashCoroutine()
    {
        Vector3 startScale = spriteRoot.localScale;
        spriteRoot.localScale = new Vector3(startScale.x * 1.2f, startScale.y * 0.7f, 1);
        yield return new WaitForSeconds(0.15f);
        spriteRoot.localScale = startScale;
    }
}
