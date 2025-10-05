using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CreatureImageScaler : MonoBehaviour
{
    [Header("Максимальный размер изображения")]
    public Vector2 maxSize = new Vector2(150f, 150f);

    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
        if (image.sprite != null)
            ApplyScale();
    }

    public void ApplyScale()
    {
        if (image == null || image.sprite == null) return;

        float spriteWidth = image.sprite.rect.width;
        float spriteHeight = image.sprite.rect.height;

        float scaleX = maxSize.x / spriteWidth;
        float scaleY = maxSize.y / spriteHeight;

        float finalScale = Mathf.Min(scaleX, scaleY);

        RectTransform rt = image.rectTransform;
        rt.sizeDelta = new Vector2(spriteWidth * finalScale, spriteHeight * finalScale);
    }

    // Если спрайт меняется динамически
    public void SetSprite(Sprite newSprite)
    {
        image.sprite = newSprite;
        ApplyScale();
    }
}
