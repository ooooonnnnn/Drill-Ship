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
    [SerializeField] private PrefabReference selfPrefabRef;
    private GameObject selfPrefab;
    [SerializeField] private float solidAlphaThreshold;
    [SerializeField] private Sprite solidSpritePrefab;
    [SerializeField] private Sprite emptySpritePrefab;
    [HideInInspector] [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Camera mainCam;
    [HideInInspector] [SerializeField] private PolygonCollider2D polyCollider;
    [SerializeField] [Tooltip("True if it doesn't move ever")] public bool isOriginalTerrain;
    private Sprite sprite => spriteRenderer.sprite;
    private Texture2D texture => sprite.texture;

    // Flood fill maps
    bool[,] visited;
    private bool[,] solidTextureSnapshot;
    
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
        texture.Apply();
        solidTextureSnapshot = new bool[(int)sprite.rect.width, (int)sprite.rect.height];
        visited = new bool[(int)sprite.rect.width, (int)sprite.rect.height];

        selfPrefab = selfPrefabRef.prefab;
        // InputManager.MouseDown += UpdateTerrain;
        mainCam = Camera.main;
    }

    private void OnDestroy()
    {
        // InputManager.MouseDown -= UpdateTerrain;
    }

    private void FixedUpdate()
    {
        if (InputManager.LmbDown) UpdateTerrain();
    }

    private static ProfilerMarker m_dig = new("m_dig");
    private static ProfilerMarker m_read = new("read");
    private static ProfilerMarker m_write = new("write");
    private static ProfilerMarker m_separate = new("separate");
    private static ProfilerMarker m_updateCOl = new("updateCOl");
    
    private void UpdateTerrain()
    {
        //Read entire texture
        m_read.Begin();
        TextureUtils.ReadTextureToBoolArr(sprite, solidTextureSnapshot, solidAlphaThreshold);
        m_read.End();
        
        //Dig
        m_dig.Begin();
        DeleteSquare(out bool textureChanged);
        m_dig.End();
        
        if (!textureChanged) return;
        
        m_separate.Begin();
        SeparateRegions();
        m_separate.End();

        //Assuming this object is one contiguous region, create a new poly collider
        m_updateCOl.Begin();
        UpdatePolyCol();
        m_updateCOl.End();
        
        //Write entire texture
        m_write.Begin();
        TextureUtils.WriteBoolArrToTexture(sprite, solidTextureSnapshot);
        m_write.End();
    }

    /// <summary>
    /// Separate contiguous regions into separate objects
    /// </summary>
    private void SeparateRegions()
    {
        print($"{gameObject.name} Separate Regions");
        
        List<List<Vector2Int>> regions = FindRegions();
        if (regions.Count == 1) return;     //No need to create new objects if there is only one region
        if (regions.Count == 0)
        {
            print($"No solid regions in {gameObject.name}. Destroying");
            Destroy(gameObject);
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
            
            bool[,] newTexture = new bool[solidTextureSnapshot.GetLength(0), solidTextureSnapshot.GetLength(1)];
            // Only the relevant region is solid
            foreach (Vector2Int pixel in region) 
            {
                newTexture[pixel.x, pixel.y] = true;
            }
            newTerrain.solidTextureSnapshot = newTexture;
            TextureUtils.WriteBoolArrToTexture(newSpriteRenderer.sprite, newTexture);
            
            //Update collider on the new object
            newTerrain.UpdatePolyCol();
        }
        
        //Self destruct
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
            if (x < 0 || x >= texture.width) continue;
            for (int y = (int)start.y; y < (int)end.y; y++)
            {
                if (y < 0 || y >= texture.height) continue;
                solidTextureSnapshot[x, y] = false;
                textureEdited = true;
            }
        }
    }
    
    private void DeleteSquare()
    {
        DeleteSquare(out var _);
    }
    
    private static readonly ProfilerMarker m_updateCol = new ProfilerMarker("UpdatePolyCol");

    public void UpdatePolyCol()
    {
        m_updateCol.Begin();
        
        print($"{gameObject.GetInstanceID()} UpdatePolyCol");
        List<Vector2> edge = GetEdge()?.
            Select(
                pix => (Vector2)transform.InverseTransformPoint(
                    PixelToWorld(pix))).
            ToList();
        polyCollider.pathCount = 0;
        if (edge == null) return;
        
        polyCollider.SetPath(0,edge);
        m_updateCol.End();
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
        
        //CodeMarker2.End();
        
        //CodeMarker4.Begin();
        Direction lastDirection = Direction.None;
        Direction currentDirection = Direction.None;
        //Once found an initial pixel, start marching
        Vector2Int curPosition = startPosition;
        bool quit = false;
        do
        {
            if (currentDirection == Direction.None || lastDirection == Direction.None
                                                   || lastDirection != currentDirection)
                edge.Add(curPosition);
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
        } while (curPosition != startPosition && !quit);
        
        //CodeMarker4.End();
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
        junctionPixels[0] = (!leftEdge && !bottomEdge) && solidTextureSnapshot[x,y];
        junctionPixels[1] = (!rightEdge && !bottomEdge) && solidTextureSnapshot[x + 1,y];
        junctionPixels[2] = (!leftEdge && !topEdge) && solidTextureSnapshot[x,y + 1];
        junctionPixels[3] = (!rightEdge && !topEdge) && solidTextureSnapshot[x + 1,y + 1];
    }

    
    static ProfilerMarker m_findRegions = new("FindRegions");
    static ProfilerMarker m_createDict = new("Clear visited");
    static ProfilerMarker m_lookForSolid = new("Algorithm");
    static ProfilerMarker m_addFloodFill = new("Add flood fill");
    static ProfilerMarker m_initRegionList = new("init Region List");
    /// <summary>
    /// Find all connected regions
    /// </summary>
    private List<List<Vector2Int>> FindRegions()
    {
        m_findRegions.Begin();
        m_createDict.Begin();
        //Construct boolean version of the textureKeep track of visited pixels
        visited = new bool[(int)sprite.rect.width, (int)sprite.rect.height];
        m_createDict.End();
        
        //Get all regions
        m_initRegionList.Begin();
        List<List<Vector2Int>> regions = new List<List<Vector2Int>>();
        m_initRegionList.End();
        m_lookForSolid.Begin();
        for (int i = 0; i < sprite.rect.width; i++)
        {
            for (int j = 0; j < sprite.rect.height; j++)
            {
                // Vector2Int coords = new Vector2Int(i, j);
                if (visited[i, j])
                {
                    continue; // Skip visited
                }
                if (!solidTextureSnapshot[i,j])      // Skip non-solid pixels
                {
                    visited[i, j] = true;
                    continue;
                }

                m_addFloodFill.Begin();
                regions.Add(FloodFill(new Vector2Int(i, j)));
                m_addFloodFill.End();
            }
        }
        m_lookForSolid.End();
        
        m_findRegions.End();
        return regions;
    }

    
    static ProfilerMarker m_floodFill = new("FloodFill");
    private List<Vector2Int> FloodFill(Vector2Int start)
    {
        m_floodFill.Begin();
        if (!solidTextureSnapshot[start.x, start.y]) Debug.LogError("Trying to flood fill from non-solid pixel");

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
                
                if (!solidTextureSnapshot[neighbor.x, neighbor.y]) continue;
                currentSearch.Push(neighbor);
            }
            
        }
        m_floodFill.End();
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
