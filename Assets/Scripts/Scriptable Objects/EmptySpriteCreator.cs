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
        
        //Save
#if UNITY_EDITOR
        // Save the modified texture to disk to make changes permanent
        string texturePath = UnityEditor.AssetDatabase.GetAssetPath(texture);
        if (!string.IsNullOrEmpty(texturePath))
        {
            byte[] pngBytes = texture.EncodeToPNG();
            System.IO.File.WriteAllBytes(texturePath, pngBytes);
            UnityEditor.AssetDatabase.Refresh();
            Debug.Log("Texture saved permanently at: " + texturePath);
        }
        else
        {
            Debug.LogWarning("Could not find asset path for texture. Ensure it's an imported asset.");
        }
#endif
    }
}
