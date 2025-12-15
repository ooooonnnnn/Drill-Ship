using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class DestructibleTerrainTest : MonoBehaviour
{
    [SerializeField] private Sprite spritePrerfab;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Camera mainCam;
    private Sprite sprite => spriteRenderer.sprite;
    private Texture2D texture;

    private void OnValidate()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        texture = Instantiate(spritePrerfab.texture);
        spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    private void Awake()
    {
        texture.Apply();
        InputManager.MouseDown += DeleteSquare;
    }

    private void DeleteSquare()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        mousePos = mainCam.ScreenToWorldPoint(mousePos);
        
        Vector2 texturePos = WorldToPixel(mousePos);
        
        int width = 50;
        Vector2 start = new Vector2(texturePos.x - width * 0.5f, texturePos.y - width * 0.5f);
        Vector2 end = new Vector2(texturePos.x + width * 0.5f, texturePos.y + width * 0.5f);
        
        print($"Deleting square from {start} to {end}");
        
        for (int x = (int)start.x; x < (int)end.x; x++)
        {
            if (x < 0 || x >= texture.width) continue;
            for (int y = (int)start.y; y < (int)end.y; y++)
            {
                if (y < 0 || y >= texture.height) continue;
                texture.SetPixel(x, y, Color.clear);
            }
        }
        texture.Apply();
    }
    
    private Vector2 WorldToPixel(Vector2 worldPos)
    {
        Sprite sprite = spriteRenderer.sprite;
        Texture2D tex = sprite.texture;

        // 1. World → local space
        Vector2 localPos = transform.InverseTransformPoint(worldPos);
        // Vector2 localPos = worldPos;

        // 2. Local units → pixels
        float ppu = sprite.pixelsPerUnit;
        Vector2 pixelPos = localPos * ppu;

        // 3. Offset by pivot
        Vector2 pivot = sprite.pivot;
        pixelPos += pivot;

        return pixelPos;
    }

}
