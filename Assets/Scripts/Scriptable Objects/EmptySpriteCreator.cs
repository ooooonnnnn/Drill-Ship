using UnityEngine;

[CreateAssetMenu(fileName = "EmptySpriteCreator", menuName = "Scriptable Objects/EmptySpriteCreator")]
public class EmptySpriteCreator : ScriptableObject
{
    [SerializeField] private Sprite originalSprite;
    [SerializeField] private Sprite emptySprite;
    
    public void CreateEmptySprite()
    {
        //emptySprite = Sprite.Create(Instantiate(originalSprite.texture), originalSprite.rect, originalSprite.pivot); 
        Texture2D texture = emptySprite.texture;
        Vector2Int btmLeft = new Vector2Int((int)emptySprite.rect.x, (int)emptySprite.rect.y);
        Vector2Int topRight = new Vector2Int((int)emptySprite.rect.width, (int)emptySprite.rect.height) + btmLeft;
        for (int i = btmLeft.x; i < topRight.x; i++)
        {
            for (int j = btmLeft.y; j < topRight.y; j++)
            {
                texture.SetPixel(i, j, Color.clear);
            }
        }
        
        texture.Apply();
    }
}
