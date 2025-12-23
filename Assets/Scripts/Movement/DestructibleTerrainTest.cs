using System;
using System.Collections.Generic;
using System.Linq;
using Helper;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;
using Unity.Profiling;

public class DestructibleTerrainTest : MonoBehaviour
{
    [SerializeField] private GameObject selfPrefab;
    [SerializeField] private float solidAlphaThreshold;
    [SerializeField] private Sprite solidSpritePrefab;
    [SerializeField] private Sprite emptySpritePrefab;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Camera mainCam;
    [HideInInspector] [SerializeField] private PolygonCollider2D polyCollider;
    [SerializeField] [Tooltip("True if it doesn't move ever")] public bool isOriginalTerrain;
    private Sprite sprite => spriteRenderer.sprite;
    private Texture2D texture => sprite.texture;
    private DestructibleTerrainManager manager;

    // Flood fill maps
    bool[,] visited;
    private bool[,] boolTextureSnapshot;
    
    private void OnValidate()
    {
        polyCollider = GetComponent<PolygonCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        Texture2D newTexture = Instantiate(solidSpritePrefab.texture);
        spriteRenderer.sprite = 
            Sprite.Create(
                newTexture, new Rect(0, 0, newTexture.width, newTexture.height), new Vector2(0.5f, 0.5f));
    }

    private void Awake()
    {
        if (!sprite)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        boolTextureSnapshot = new bool[(int)sprite.rect.width, (int)sprite.rect.height];
        visited = new bool[(int)sprite.rect.width, (int)sprite.rect.height];

        mainCam = Camera.main;
    }

    private void Start()
    {
        manager = DestructibleTerrainManager.Instance;
        manager.AddTerrain(this);
    }

    // private void Update()
    // {
    //     if (InputManager.LmbDown)
    //     {
    //         Vector2 mousePos = Mouse.current.position.ReadValue();
    //         mousePos = mainCam.ScreenToWorldPoint(mousePos);
    //         float radius = 0.2f;
    //         UpdateTerrain(mousePos, radius);
    //     }
    // }
    
    public void DigSphere(Vector2 worldCenter, float radius)
    {
        //Read entire texture
        TextureUtils.ReadTextureToBoolArr(sprite, boolTextureSnapshot, solidAlphaThreshold);
        
        //Dig
        DoDeleteSphere(worldCenter, radius, out bool textureChanged);
        
        if (!textureChanged) return;
        
        SeparateRegions();

        //Assuming this object is one contiguous region, create a new poly collider
        UpdatePolyCol();
        
        //Write entire texture
        TextureUtils.WriteBoolArrToTexture(sprite, boolTextureSnapshot);
    }
    
    public void DigCollider(Collider2D destructiveCollider)
    {
        //Read entire texture
        TextureUtils.ReadTextureToBoolArr(sprite, boolTextureSnapshot, solidAlphaThreshold);
        
        //Dig
        DoDeleteInCollider(destructiveCollider, out bool textureChanged);
        
        if (!textureChanged) return;
        
        SeparateRegions();

        //Assuming this object is one contiguous region, create a new poly collider
        UpdatePolyCol();
        
        //Write entire texture
        TextureUtils.WriteBoolArrToTexture(sprite, boolTextureSnapshot);
    }

    /// <summary>
    /// Separate contiguous regions into separate objects
    /// </summary>
    private void SeparateRegions()
    {
        // print($"{gameObject.name} Separate Regions");
        
        List<List<Vector2Int>> regions = FindRegions();
        if (regions.Count == 1) return;     //No need to create new objects if there is only one region
        if (regions.Count == 0)
        {
            // print($"No solid regions in {gameObject.name}. Destroying");
            DestroyAndRemoveFromManager();
        }

        foreach (List<Vector2Int> region in regions)
        {
            //Find the center of mass
            Vector2 center = region.Aggregate(Vector2.zero, (current, pixel) => current + PixelToWorld(pixel)) / region.Count;
            MyDebug.DrawX(center, .1f, Color.red, 1f);
            
            //Create a new object at the center of mass
            GameObject newObject = Instantiate(selfPrefab, center, transform.rotation);
            DestructibleTerrainTest newTerrain = newObject.GetComponent<DestructibleTerrainTest>();
            SpriteRenderer newSpriteRenderer = newObject.GetComponent<SpriteRenderer>();
            Vector2 newPivot = WorldToPixel(center);
            Rect spriteRect = spriteRenderer.sprite.rect;
            Vector2 pivotNormalized = new Vector2(
                (newPivot.x - spriteRect.x) / spriteRect.width,
                (newPivot.y - spriteRect.y) / spriteRect.height
            );
            newSpriteRenderer.sprite = 
                Sprite.Create(
                    Instantiate(emptySpritePrefab.texture),
                    sprite.rect,
                    pivotNormalized);
            
            bool[,] newTexture = new bool[boolTextureSnapshot.GetLength(0), boolTextureSnapshot.GetLength(1)];
            // Only the relevant region is solid
            foreach (Vector2Int pixel in region) 
            {
                newTexture[pixel.x, pixel.y] = true;
            }
            newTerrain.boolTextureSnapshot = newTexture;
            TextureUtils.WriteBoolArrToTexture(newSpriteRenderer.sprite, newTexture);
            
            //Update collider on the new object
            newTerrain.UpdatePolyCol();
        }
        
        //Self destruct
        DestroyAndRemoveFromManager();
    }

    private void DestroyAndRemoveFromManager()
    {
        manager.RemoveTerrain(this);
        Destroy(gameObject);
    }

    private void DeleteSquare(out bool textureEdited)
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        mousePos = mainCam.ScreenToWorldPoint(mousePos);
        
        Vector2 texturePos = WorldToPixel(mousePos);
        
        int width = 50;
        Vector2 start = new Vector2(texturePos.x - width * 0.5f, texturePos.y - width * 0.5f);
        Vector2 end = new Vector2(texturePos.x + width * 0.5f, texturePos.y + width * 0.5f);
        
        textureEdited = false;
        for (int x = (int)start.x; x < (int)end.x; x++)
        {
            if (x < 0 || x >= sprite.rect.width) continue;
            for (int y = (int)start.y; y < (int)end.y; y++)
            {
                if (y < 0 || y >= sprite.rect.height) continue;
                if (!boolTextureSnapshot[x, y]) continue;
                
                boolTextureSnapshot[x, y] = false;
                textureEdited = true;
            }
        }
    }

    private void DoDeleteSphere(Vector2 worldCenter, float radius, out bool textureEdited)
    {
        textureEdited = false;
        
        //Check center is in range of the sprite bounds
        float sqrDistance = spriteRenderer.bounds.SqrDistance(worldCenter);
        float sqrRadius = radius * radius;
        if (sqrDistance > sqrRadius) return;
        
        Vector2Int spriteSpaceCenter = WorldToPixel(worldCenter).RoundToInt();
        float spriteSpaceSquaredRadius = sqrRadius * sprite.pixelsPerUnit * sprite.pixelsPerUnit;
        
        for (int i = 0; i < boolTextureSnapshot.GetLength(0); i++)
        {
            for (int j = 0; j < boolTextureSnapshot.GetLength(1); j++)
            {
                float pixelSqrDistance = (new Vector2Int(i,j) - spriteSpaceCenter).sqrMagnitude;
                if (pixelSqrDistance > spriteSpaceSquaredRadius) continue;
                if (!boolTextureSnapshot[i, j]) continue;
                
                boolTextureSnapshot[i, j] = false;
                textureEdited = true;
            }
        }
    }
    
    private void DoDeleteInCollider(Collider2D destructiveCollider, out bool textureEdited)
    {
        textureEdited = false;
        
        //Check collider is in range of the sprite bounds
        if (!spriteRenderer.bounds.Intersects(destructiveCollider.bounds)) return;
        
        for (int i = 0; i < boolTextureSnapshot.GetLength(0); i++)
        {
            for (int j = 0; j < boolTextureSnapshot.GetLength(1); j++)
            {
                if (!boolTextureSnapshot[i, j]) continue;
                if (!destructiveCollider.OverlapPoint(PixelToWorld(new Vector2(i, j)))) continue;
                
                boolTextureSnapshot[i, j] = false;
                textureEdited = true;
            }
        }
    }
    
    // private static readonly ProfilerMarker m_updateCol = new ProfilerMarker("UpdatePolyCol");

    public void UpdatePolyCol()
    {
        // m_updateCol.Begin();
        
        // print($"{gameObject.GetInstanceID()} UpdatePolyCol");
        List<Vector2> edge = GetEdge()?.
            Select(
                pix => (Vector2)transform.InverseTransformPoint(
                    PixelToWorld(pix))).
            ToList();
        polyCollider.pathCount = 0;
        if (edge == null) return;
        
        polyCollider.SetPath(0,edge);
        // m_updateCol.End();
    }

    private enum Direction
    {
        Up,
        Down,
        Left,
        Right,
        None
    }
    
    // private static readonly ProfilerMarker CodeMarker2 = new ProfilerMarker("GetEdge.FindStartingPoint");
    // private static readonly ProfilerMarker CodeMarker4 = new ProfilerMarker("GetEdge.MarchingSquares");
    // private static readonly ProfilerMarker CodeMarker5 = new ProfilerMarker("GetJunctionPixelsPadded");
    // private static readonly ProfilerMarker CodeMarker6 = new ProfilerMarker("Sum of junction pixels");
    // private static readonly ProfilerMarker CodeMarker7 = new ProfilerMarker("");
    
    /// <summary>
    /// Gets the edge of the texture, ordered ccw. Using marching squares
    /// </summary>
    /// <returns></returns>
    [CanBeNull]
    private List<Vector2> GetEdge()
    {
        //CodeMarker2.Begin();
        List<Vector2> edge = new();
        
        //Look for a junction of pixels that implies an edge
        Rect rect = sprite.rect;
        Vector2Int startPosition = Vector2Int.zero;
        bool foundEdge = false;
        for (int x = 0; x < rect.width; x++)
        {
            for (int y = 0; y < rect.height; y++)
            {
                //CodeMarker5.Begin();
                //Get the values of the four pixels that touch in a corner
                bool[] junctionPixels = { false, false, false, false };
                GetJunctionPixelsPadded(junctionPixels, x, y);
                //CodeMarker5.End();

                //  0*  *0  **  00  mean nothing. discard them
                //  *0  0*  **  00

                //CodeMarker6.Begin();
                int totalTrue = junctionPixels.Sum(pixel => pixel ? 1 : 0);
                //CodeMarker6.End();
                //check for the last two
                if (totalTrue == 4 || totalTrue == 0)
                {
                    continue;
                }
                //check for the first two
                if (junctionPixels[0] != junctionPixels[1] && junctionPixels[0] != junctionPixels[2] && totalTrue == 2)
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
        
        Direction lastDirection = Direction.None;
        Direction currentDirection = Direction.None;
        //Once found an initial pixel, start marching
        Vector2Int curPosition = startPosition;
        Vector2Int lastPosition = startPosition;
        int loops = 0;
        do
        {
            loops++;
            MyDebug.DrawX(PixelToWorld(curPosition), .01f, Color.red, 1f);
            //Determine whether to save the new position
            if (currentDirection == Direction.None || lastDirection == Direction.None)
            {
                lastPosition = curPosition;
                edge.Add(curPosition);
            }
            else if (currentDirection != lastDirection && (curPosition - lastPosition).sqrMagnitude > 100)
            {
                lastPosition = curPosition;
                edge.Add(curPosition);
            }
            
            // MyDebug.DrawX(PixelToWorld(curPosition), .01f, Color.red, 1f);
            bool[] junctionPixels = {false, false, false, false};
            if (curPosition.x < 0 || curPosition.x >= rect.width || curPosition.y < 0 || curPosition.y >= rect.height)
                Debug.LogError("Out of bounds");
            GetJunctionPixelsPadded(junctionPixels, curPosition.x, curPosition.y);
            
            lastDirection = currentDirection;
            //Any of the following patterns imply an edge
            // *0   *0  *0  mean go up
            // *0   00  **
            if (junctionPixels[2] && !junctionPixels[3])
            {
                //go up
                curPosition.y++;
                currentDirection = Direction.Up;
            }
            // **   **  0*  mean go right
            // *0   00  00
            else if (!junctionPixels[1] && junctionPixels[3])
            {
                //go right
                curPosition.x++;
                currentDirection = Direction.Right;
            }
            // **   0*  00  mean go down
            // 0*   0*  0*
            else if (!junctionPixels[0] && junctionPixels[1])
            {
                //go down
                curPosition.y--;
                currentDirection = Direction.Down;
            }
            //  00  0*  00  mean go left
            //  *0  **  **
            else if (junctionPixels[0] && !junctionPixels[2])
            {
                //go left
                curPosition.x--;
                currentDirection = Direction.Left;
            }
        } while (curPosition != startPosition && loops < 10_000);
        
        // print($"looped {loops} times to find {edge.Count} edge points");
        if (loops >= 10_000) print("Edge looped too many times");
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
        Rect rect = sprite.rect;
        bool leftEdge = x == 0;
        bool rightEdge = x == (int)rect.width - 1;
        bool bottomEdge = y == 0;
        bool topEdge = y == (int)rect.height - 1;
        junctionPixels[0] = (!leftEdge && !bottomEdge) && boolTextureSnapshot[x,y];
        junctionPixels[1] = (!rightEdge && !bottomEdge) && boolTextureSnapshot[x + 1,y];
        junctionPixels[2] = (!leftEdge && !topEdge) && boolTextureSnapshot[x,y + 1];
        junctionPixels[3] = (!rightEdge && !topEdge) && boolTextureSnapshot[x + 1,y + 1];
    }

    /// <summary>
    /// Find all connected regions
    /// </summary>
    private List<List<Vector2Int>> FindRegions()
    {
        //Construct boolean version of the textureKeep track of visited pixels
        visited = new bool[(int)sprite.rect.width, (int)sprite.rect.height];
        
        //Get all regions
        List<List<Vector2Int>> regions = new List<List<Vector2Int>>();
        for (int i = 0; i < sprite.rect.width; i++)
        {
            for (int j = 0; j < sprite.rect.height; j++)
            {
                // Vector2Int coords = new Vector2Int(i, j);
                if (visited[i, j])
                {
                    continue; // Skip visited
                }
                if (!boolTextureSnapshot[i,j])      // Skip non-solid pixels
                {
                    visited[i, j] = true;
                    continue;
                }

                regions.Add(FloodFill(new Vector2Int(i, j)));
            }
        }
        
        return regions;
    }
    
    private List<Vector2Int> FloodFill(Vector2Int start)
    {
        if (!boolTextureSnapshot[start.x, start.y]) Debug.LogError("Trying to flood fill from non-solid pixel");

        List<Vector2Int> region = new();
        Stack<Vector2Int> currentSearch = new();
        
        //The stack contains pixels that may have unvisited neighbors
        currentSearch.Push(start);
        visited[start.x, start.y] = true;
        while (currentSearch.Count > 0)
        {
            List<Vector2Int> neighbors = NeighborsOfPixel(currentSearch.Peek());
            region.Add(currentSearch.Pop());

            foreach (Vector2Int neighbor in neighbors)
            {
                if (visited[neighbor.x, neighbor.y]) continue;
                visited[neighbor.x, neighbor.y] = true;
                
                if (!boolTextureSnapshot[neighbor.x, neighbor.y]) continue;
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
