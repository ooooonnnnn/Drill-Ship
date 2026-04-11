using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

public class BreakableBlockManager : MonoBehaviour
{
    [Header("Blocks")]
    [SerializeField] private BreakableBlock blockPrefab;

    [Header("Ground Types")]
    [SerializeField] private GroundTypeChance[] groundTypeChances;
    [SerializeField] private SerializedDictionary<GroundType, GroundData> groundDataLookup;

    [Header("Resource Types")]
    [SerializeField] private ResourceTypeChance[] resourceTypeChances;
    [SerializeField] private SerializedDictionary<ResourceType, ResourceData> resourceDataLookup;

    [Header("Grid")]
    [SerializeField] private int gridWidth;
    [SerializeField] private int gridHeight;
    public float blockSize => _blockSize;
    [SerializeField] private float _blockSize;

    public static BreakableBlockManager Instance { get; private set; }

    [SerializeField] private SerializedDictionary<GameObject, BreakableBlock> breakableBlockComponents = new();

    [Tooltip("Passed with the position of the block")]
    public UnityEvent<Vector2> OnBlockDestroyed;

    private void OnValidate()
    {
        // //Initialize data lookup dictionaries
        // groundDataLookup = new();
        // foreach (GroundType groundType in Enum.GetValues(typeof(GroundType)))
        // {
        //     groundDataLookup.Add(groundType, null);
        // }
        // resourceDataLookup = new();
        // foreach (ResourceType resourceType in Enum.GetValues(typeof(ResourceType)))
        // {
        //     resourceDataLookup.Add(resourceType, null);
        // }
        
        breakableBlockComponents = new();
        BreakableBlock[] components = FindObjectsByType<BreakableBlock>(FindObjectsSortMode.None);
        foreach (BreakableBlock component in components)
        {
            breakableBlockComponents.Add(component.gameObject, component);
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>Returns the cached BreakableBlock component for the given GameObject.</summary>
    public BreakableBlock GetBreakableBlockComponent(GameObject go)
    {
        return breakableBlockComponents[go];
    }

    /// <summary>Invokes the OnBlockDestroyed event at the given world position.</summary>
    public void LogBlockDestroyed(Vector2 position)
    {
        OnBlockDestroyed.Invoke(position);
    }

#if UNITY_EDITOR

    /// <summary>Destroys all child blocks and regenerates the grid using current settings.</summary>
    public void GenerateBlocks()
    {
        // Destroy old blocks
        List<Transform> toDestroy = new List<Transform>();
        foreach (Transform child in transform)
        {
            toDestroy.Add(child);
        }
        foreach (Transform child in toDestroy) DestroyImmediate(child.gameObject);

        NormalizeGroundSpawnChances();
        NormalizeResourceSpawnChances();

        // Create new blocks
        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++)
            {
                BreakableBlock newBlock = PrefabUtility.InstantiatePrefab(blockPrefab) as BreakableBlock;
                newBlock.transform.SetParent(transform);

                newBlock.transform.localPosition = new Vector3(i * blockSize, j * blockSize);
                newBlock.transform.localScale = Vector3.one * blockSize;

                GroundType groundType = ChooseRandomGroundType();
                ResourceType resourceType = ChooseRandomResourceType();

                newBlock.Initialize(groundType, resourceType, groundDataLookup[groundType], resourceDataLookup[resourceType]);
                PrefabUtility.RecordPrefabInstancePropertyModifications(newBlock);
            }
        }
    }

    private void NormalizeGroundSpawnChances()
    {
        float total = groundTypeChances.Select(x => x.chance).Sum();
        foreach (GroundTypeChance entry in groundTypeChances)
        {
            entry.chance /= total;
        }
    }

    private void NormalizeResourceSpawnChances()
    {
        float total = resourceTypeChances.Select(x => x.chance).Sum();
        foreach (ResourceTypeChance entry in resourceTypeChances)
        {
            entry.chance /= total;
        }
    }

    private GroundType ChooseRandomGroundType()
    {
        float random = UnityEngine.Random.Range(0f, 1f);
        float accumulated = 0f;

        foreach (GroundTypeChance entry in groundTypeChances)
        {
            accumulated += entry.chance;
            if (accumulated >= random)
                return entry.groundType;
        }

        return groundTypeChances[groundTypeChances.Length - 1].groundType;
    }

    private ResourceType ChooseRandomResourceType()
    {
        float random = UnityEngine.Random.Range(0f, 1f);
        float accumulated = 0f;

        foreach (ResourceTypeChance entry in resourceTypeChances)
        {
            accumulated += entry.chance;
            if (accumulated >= random)
                return entry.resourceType;
        }

        return resourceTypeChances[resourceTypeChances.Length - 1].resourceType;
    }

#endif
}
