using UnityEngine;

public static class TextureUtils
{
    /// <summary>
    /// Get a snapshot of the texture with solid (high alpha) pixels marked as true.
    /// </summary>
    /// <param name="sprite">The sprite you want to snapshot</param>
    /// <param name="snapshotOut">The values are written here</param>
    /// <param name="solidAlphaThreshold"></param>
    /// <returns></returns>
    public static void ReadTextureToBoolArr(Sprite sprite, bool[,] snapshotOut, float solidAlphaThreshold)
    {
        Rect rect = sprite.rect;
        Texture2D texture = sprite.texture;
        var pixels = texture.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
        for (int i = 0; i < pixels.Length; i++)
        {
            int j = i / (int)rect.width;
            snapshotOut[i % (int)rect.width, j] = pixels[i].a >= solidAlphaThreshold;
        }
    }

    public static void WriteBoolArrToTexture(Sprite sprite, bool[,] boolArr)
    {
        Rect rect = sprite.rect;
        Texture2D texture = sprite.texture;
        
        Color[] pixels = new Color[(int)rect.width * (int)rect.height];
        for (int i = 0; i < (int)rect.width; i++)
        {
            for (int j = 0; j < (int)rect.height; j++)
            {
                pixels[i + j * (int)rect.width] = boolArr[i, j] ? Color.white : Color.clear;
            }
        }
        texture.SetPixels(pixels);
        texture.Apply();
    }
}
