using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Helper;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;

public class DestructibleTerrainTest : MonoBehaviour
{
    [SerializeField] private GameObject selfPrefab;
    [SerializeField] private float solidAlphaThreshold;
    [SerializeField] private Sprite solidSpritePrerfab;
    [SerializeField] private Sprite emptySpritePrerfab;
    [HideInInspector] [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Camera mainCam;
    [HideInInspector] [SerializeField] private PolygonCollider2D polyCollider;
    [SerializeField] [Tooltip("True if it doesn't move ever")] public bool isOriginalTerrain;
    private Sprite sprite => spriteRenderer.sprite;
    private Texture2D texture => spriteRenderer.sprite.texture;
    // Flood fill maps
    private Dictionary<Vector2Int, bool> _solid;
    private Dictionary<Vector2Int, bool> _visited;


    private void OnValidate()
    {
        polyCollider = GetComponent<PolygonCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        Texture2D newTexture = Instantiate(solidSpritePrerfab.texture);
        spriteRenderer.sprite = 
            Sprite.Create(
                newTexture, new Rect(0, 0, newTexture.width, newTexture.height), new Vector2(0.5f, 0.5f));
    }

    private void Awake()
    {
        texture.Apply();
        InputManager.MouseDown += UpdateTerrain;
        // print($"Original pivot: {spriteRenderer.sprite.pivot}");
    }
    
    private void OnDestroy()
    {
        InputManager.MouseDown -= UpdateTerrain;
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
        //Dig
        DeleteSquare();
        
        SeparateRegions();

        //Assuming this object is one contiguous region, create a new poly collider 
        UpdatePolyCol();
    }

        
    /// <summary>
    /// Separate contiguous regions into separate objects
    /// </summary>
    private void SeparateRegions()
    {
        List<List<Vector2Int>> regions = FindRegions();
        if (regions.Count == 1) return;     //No need to create new objects if there is only one region
        if (regions.Count == 0)
        {
            print($"No solid regions in {gameObject.name}. Detroying");
            Destroy(gameObject);
        }

        foreach (List<Vector2Int> region in regions)
        {
            //Find the center of mass
            Vector2 center = region.Aggregate(Vector2.zero, (current, pixel) => current + PixelToWorld(pixel)) / region.Count;
            // MyDebug.DrawX(center, .1f, Color.red, 1f);
            
            //Create a new object at the center of mass
            GameObject newObject = Instantiate(selfPrefab, center, transform.rotation);
            DestructibleTerrainTest newTerrain = newObject.GetComponent<DestructibleTerrainTest>();
            newTerrain.enabled = true;
            newObject.GetComponent<PolygonCollider2D>().enabled = true;
            SpriteRenderer newSpriteRenderer = newObject.GetComponent<SpriteRenderer>();
            Vector2 newPivot = WorldToPixel(center);
            Rect spriteRect = spriteRenderer.sprite.rect;
            Vector2 pivotNormalized = new Vector2(
                (newPivot.x - spriteRect.x) / spriteRect.width,
                (newPivot.y - spriteRect.y) / spriteRect.height
            );
            // print($"new pivot: {newPivot}");
            newSpriteRenderer.sprite = 
                Sprite.Create(
                    Instantiate(emptySpritePrerfab.texture),
                    spriteRenderer.sprite.rect,
                    pivotNormalized);
            // newTerrain.texture = Instantiate(emptySpritePrerfab.texture);
            
            // Only the relevant region is solid
             foreach (Vector2Int pixel in region)
             {
                 newSpriteRenderer.sprite.texture.SetPixel(pixel.x, pixel.y, Color.white);
                 // spriteRenderer.sprite.texture.SetPixel(pixel.x, pixel.y, Color.clear);
             }
            newSpriteRenderer.sprite.texture.Apply();
            
            //Update collider on the new object
            newTerrain.UpdatePolyCol();
            
            //Self destruct
            Destroy(gameObject);
            
            // Color color = MyColors.RandomColor;
            // foreach (Vector2Int pixel in region)
            // {
            //     texture.SetPixel(pixel.x, pixel.y, color);
            // }
            // texture.Apply();
        }
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

    public void UpdatePolyCol()
    {
        // polyCollider.pathCount = 0;
        // polyCollider.SetPath(0, new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) });
        List<Vector2> edge = GetEdge()?.Select(PixelToWorld).ToList();
        polyCollider.pathCount = 0;
        if (edge == null) return;
        polyCollider.SetPath(0,edge);
    }

    /// <summary>
    /// Gets the edge of the texture, ordered.
    /// </summary>
    /// <returns></returns>
    [CanBeNull]
    private List<Vector2> GetEdge()
    {
        List<Vector2> edge = new();
        
        //Use marching squares
        //Look for a junction of pixels that implies an edge
        Rect rect = spriteRenderer.sprite.rect;
        Vector2Int startPosition = Vector2Int.zero;
        bool foundEdge = false;
        for (int x = (int)rect.min.x; x < rect.max.x - 1; x++)
        {
            for (int y = (int)rect.min.y; y < rect.max.y - 1; y++)
            {
                //Get the values of the four pixels that touch in a corner
                bool[] junctionPixels = { false, false, false, false };
                GetJunctionPixelsPadded(junctionPixels, x, y);

                //  0*  *0  **  00  mean nothing. discard them
                //  *0  0*  **  00
                
                //check for the first two
                int totalTrue = junctionPixels.Aggregate(0, (cur, value) => cur + (value ? 1 : 0));
                if (junctionPixels[0] != junctionPixels[1] && junctionPixels[0] != junctionPixels[2] && totalTrue == 2)
                {
                    continue;
                }
                //check for the last two
                if (totalTrue == 4 || totalTrue == 0)
                {
                    continue;
                }
                
                //If the code reached here it means the junction implies an edge
                startPosition = new(x, y);
                foundEdge = true;
                break;
            }
            if (foundEdge) break;
        }
        
        if (!foundEdge) return null;

        
        Vector2Int curPosition = startPosition;
        do
        {
            edge.Add(curPosition);
            MyDebug.DrawX(PixelToWorld(curPosition), .01f, Color.red, 1f);
            bool[] junctionPixels = {false, false, false, false};
            GetJunctionPixelsPadded(junctionPixels, curPosition.x, curPosition.y);
            //If the march reached out of the rect, go counterclockwise on the edge of the rect
            // if (curPosition.x < rect.min.x)
            // {
            //     curPosition.y--;
            //     if (curPosition.y < rect.min.y) curPosition.x++;
            //     continue;
            // }
            // if (curPosition.y < rect.min.y)
            // {
            //     curPosition.x++;
            //     if (curPosition.x >= rect.max.x - 1) curPosition.y++;
            //     continue;
            // }
            // if (curPosition.x >= rect.max.x - 1)
            // {
            //     curPosition.y++;
            //     if (curPosition.y >= rect.max.y - 1) curPosition.x--;
            //     continue;
            // }
            // if (curPosition.y >= rect.max.y - 1)
            // {
            //     curPosition.x--;
            //     if (curPosition.x < rect.min.x) curPosition.y--;
            //     continue;
            // }
            
            //Any of the following patterns imply an edge
            // *0   *0  *0  mean go up
            // *0   00  **
            if (junctionPixels[2] && !junctionPixels[3])
            {
                //go up
                curPosition.y++;
            }
            // **   **  0*  mean go right
            // *0   00  00
            else if (!junctionPixels[1] && junctionPixels[3])
            {
                //go right
                curPosition.x++;
            }
            // **   0*  00  mean go down
            // 0*   0*  0*
            else if (!junctionPixels[0] && junctionPixels[1])
            {
                //go down
                curPosition.y--;
            }
            //  00  0*  00  mean go left
            //  *0  **  **
            else if (junctionPixels[0] && !junctionPixels[2])
            {
                //go left
                curPosition.x--;
            }
        } while (curPosition != startPosition);

        return edge;
    }

    /// <summary>
    /// Gets the four pixels that touch in a corner. Treats the edges of the texture as 1-wide 0-padding 
    /// </summary>
    /// <param name="junctionPixels"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    private void GetJunctionPixelsPadded(bool[] junctionPixels, int x, int y)
    {
        Rect rect = spriteRenderer.sprite.rect;
        bool leftEdge = x == rect.min.x;
        bool rightEdge = x == rect.max.x - 1;
        bool bottomEdge = y == rect.min.y;
        bool topEdge = y == rect.max.y - 1;
        junctionPixels[0] = (!leftEdge && !bottomEdge) && AlphaThreshold(texture.GetPixel(x, y).a);
        junctionPixels[1] = (!rightEdge && !bottomEdge) && AlphaThreshold(texture.GetPixel(x + 1, y).a);
        junctionPixels[2] = (!leftEdge && !topEdge) && AlphaThreshold(texture.GetPixel(x, y + 1).a);
        junctionPixels[3] = (!rightEdge && !topEdge) && AlphaThreshold(texture.GetPixel(x + 1, y + 1).a);
    }

    /// <summary>
    /// Find all connected regions
    /// </summary>
    private List<List<Vector2Int>> FindRegions()
    {
        //Construct boolean version of the textureKeep track of visited pixels
        _solid = new Dictionary<Vector2Int, bool>();
        _visited = new Dictionary<Vector2Int, bool>();
        for (int i = (int)sprite.rect.x; i < sprite.rect.width; i++)
        {
            for (int j = (int)sprite.rect.y; j < sprite.rect.height; j++)
            {
                _solid.Add(new Vector2Int(i, j), AlphaThreshold(texture.GetPixel(i, j).a));
                _visited.Add(new Vector2Int(i, j), false);
            }
        }
        
        //Get all regions
        List<List<Vector2Int>> regions = new List<List<Vector2Int>>();
        foreach (KeyValuePair<Vector2Int, bool> pixelSolid in _solid)
        {
            if (_visited[pixelSolid.Key]) continue;   // Skip visited
            if (!pixelSolid.Value)      // Skip non-solid pixels
            {
                _visited[pixelSolid.Key] = true;
                continue;
            }
            
            regions.Add(FloodFill(pixelSolid.Key));
        }
        
        return regions;
    }

    private bool AlphaThreshold(float alpha) => alpha > solidAlphaThreshold;

    private List<Vector2Int> FloodFill(Vector2Int start)
    {
        if (!_solid[start]) Debug.LogError("Trying to flood fill from non-solid pixel");

        List<Vector2Int> region = new();
        Stack<Vector2Int> currentSearch = new();
        
        //The stack contains pixels that may have unvisited neighbors
        currentSearch.Push(start);
        _visited[start] = true;
        while (currentSearch.Count > 0)
        {
            List<Vector2Int> neighbors = NeighborsOfPixel(currentSearch.Peek());
            region.Add(currentSearch.Pop());

            foreach (Vector2Int neighbor in neighbors)
            {
                if (_visited[neighbor]) continue;
                _visited[neighbor] = true;
                
                if (!_solid[neighbor]) continue;
                currentSearch.Push(neighbor);
            }
            
        }
        return region;
    }

    private List<Vector2Int> NeighborsOfPixel(Vector2Int peek)
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
