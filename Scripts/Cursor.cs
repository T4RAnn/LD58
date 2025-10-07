using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public Texture2D cursorTexture; // ваша текстура курсора
    public Vector2 hotspot = Vector2.zero; // точка "клика" курсора
    public CursorMode cursorMode = CursorMode.Auto;

    void Start()
    {
        Cursor.SetCursor(cursorTexture, hotspot, cursorMode);
    }
}
