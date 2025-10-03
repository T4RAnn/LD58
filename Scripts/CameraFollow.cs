using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;      // Кого камера будет преследовать (игрок)
    public float smoothSpeed = 5f; // Скорость сглаживания
    public Vector3 offset;         // Смещение (чтобы не было прямо по центру)

    void LateUpdate()
    {
        if (target == null) return;

        // Желаемая позиция камеры
        Vector3 desiredPosition = target.position + offset;
        desiredPosition.z = transform.position.z; // фиксируем Z

        // Плавное движение
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}
