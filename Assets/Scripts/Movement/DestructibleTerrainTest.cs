using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Helper;
using UnityEngine;
using UnityEngine.InputSystem;

public class DestructibleTerrainTest : MonoBehaviour
{
    [SerializeField] private float solidAlphaThreshold;
    [SerializeField] private Sprite spritePrerfab;
    [HideInInspector] [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Camera mainCam;
    [HideInInspector] [SerializeField] private PolygonCollider2D polyCollider;
    private Sprite sprite => spriteRenderer.sprite;
    private Texture2D texture;
    // Flood fill maps
    private Dictionary<Vector2Int, bool> _solid;
    private Dictionary<Vector2Int, bool> _visited;

    private void OnValidate()
    {
        polyCollider = GetComponent<PolygonCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        texture = Instantiate(spritePrerfab.texture);
        spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    private void Awake()
    {
        texture.Apply();
        InputManager.MouseDown += UpdateTerrain;
    }

    // private void Start()
    // {
    //     //Test neighbors
    //     Vector2Int pixel = Vector2Int.zero;
    //     var neighbors = NeighborsOfPixel(pixel);
    //     MyDebug.DrawX(PixelToWorld(pixel), .01f, Color.red, 10f);
    //
    //     print($"pixel: {pixel} has {((List<Vector2Int>)neighbors).Count} neighbors");
    //     foreach (Vector2Int neighbor in neighbors)
    //     {
    //         print($"neighbor: {neighbor}, position {PixelToWorld(neighbor)}");
    //         MyDebug.DrawX(PixelToWorld(neighbor), .01f, Color.red, 10f);
    //     }
    // }

    private void UpdateTerrain()
    {
        DeleteSquare();
        UpdatePolyCol();
    }

    private void DeleteSquare()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        mousePos = mainCam.ScreenToWorldPoint(mousePos);
        
        Vector2 texturePos = WorldToPixel(mousePos);
        
        int width = 50;
        Vector2 start = new Vector2(texturePos.x - width * 0.5f, texturePos.y - width * 0.5f);
        Vector2 end = new Vector2(texturePos.x + width * 0.5f, texturePos.y + width * 0.5f);
        
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

    private void UpdatePolyCol()
    {
        print("Test paths");
        polyCollider.pathCount = 0;
        polyCollider.SetPath(0, new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) });
    }

    /// <summary>
    /// Find all connected regions
    /// </summary>
    private List<List<Vector2Int>> FindRegions()
    {
        //Construct boolean version of the textureKeep track of visited pixels
        _solid = new Dictionary<Vector2Int, bool>();
        _visited = new Dictionary<Vector2Int, bool>();
        for (int i = 0; i < sprite.bounds.size.x; i++)
        {
            for (int j = 0; j < sprite.bounds.size.y; j++)
            {
                _solid.Add(new Vector2Int(i, j), AlphaThreshold(texture.GetPixel(i, j).a));
                _visited.Add(new Vector2Int(i, j), false);
            }
        }
        
        //Get all regions
        List<List<Vector2Int>> regions = new List<List<Vector2Int>>();
        foreach (KeyValuePair<Vector2Int, bool> keyValue in _visited)
        {
            if (keyValue.Value) continue;   // Skip visited
            if (!_solid[keyValue.Key])      // Skip non-solid pixels
            {
                _visited[keyValue.Key] = true;
                continue;
            }
            
            regions.Add(FloodFill(keyValue.Key, out List<Vector2Int> visitedPixels));
            
            foreach (Vector2Int pixel in visitedPixels)
            {
                _visited[pixel] = true;
            }
        }
        
        return regions;
    }

    private bool AlphaThreshold(float alpha) => alpha > solidAlphaThreshold;

    private List<Vector2Int> FloodFill(Vector2Int start, out List<Vector2Int> visitedPixels)
    {
        if (!_solid[start]) Debug.LogError("Trying to flood fill from non-solid pixel");

        List<Vector2Int> region = new();
        Stack<Vector2Int> currentSearch = new();
        
        //The stack contains pixels that may have unvisited neighbors
        currentSearch.Push(start);
        _visited[start] = true;
        while (currentSearch.Count > 0)
        {
            foreach (var neighbor in NeighborsOfPixel(currentSearch.Peek()))
            {
                
            }
        }
        //TODO: temporary
        visitedPixels = region;
        return region;
    }

    private IEnumerable NeighborsOfPixel(Vector2Int peek)
    {
        List<Vector2Int> neighbors = new();
        if (peek.x > 0) neighbors.Add(peek + Vector2Int.left);
        if (peek.x < texture.width - 1) neighbors.Add(peek + Vector2Int.right);
        if (peek.y > 0) neighbors.Add(peek + Vector2Int.down);
        if (peek.y < texture.height - 1) neighbors.Add(peek + Vector2Int.up);
        
        return neighbors;
    }

    private Vector2 WorldToPixel(Vector2 worldPos)
    {
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

    private Vector2 PixelToWorld(Vector2 pixelPos)
    {
        pixelPos -= sprite.pivot;
        pixelPos /= sprite.pixelsPerUnit;
        return transform.TransformPoint(pixelPos);
    }

}
