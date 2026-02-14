using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class BreakableBlockManager : MonoBehaviour
{
    [Header("Blocks")]
    [SerializeField] private BreakableBlock blockPrefab;
    [SerializeField] private BlockTypeChance[] blockTypeChances;
    
    [Header("Grid")]
    [SerializeField] private int gridWidth, gridHeight;
    [SerializeField] private float blockSize;
    public static BreakableBlockManager Instance { get; private set; }
    [SerializeField] private SerializedDictionary<GameObject, BreakableBlock> breakableBlockComponents = new ();

    private void OnValidate()
    {
        if (breakableBlockComponents == null)
            breakableBlockComponents = new();

        breakableBlockComponents.Clear();
        
        BreakableBlock[] components = FindObjectsByType<BreakableBlock>(FindObjectsSortMode.None);
        foreach (BreakableBlock component in components)
        {
            breakableBlockComponents[component.gameObject] = component;
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    public BreakableBlock GetBreakableBlockComponent(GameObject go)
    {
        return breakableBlockComponents[go];
    }

#if UNITY_EDITOR

    public void GenerateBlocks()
    {
        //Destroy old blocks
        List<Transform> toDestroy = new List<Transform>();
        foreach (Transform child in transform)
        {
            toDestroy.Add(child);
        }
        foreach (Transform child in toDestroy) DestroyImmediate(child.gameObject);

        NormalizeBlockSpawnChances();
        
        //Create new blocks
        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++)
            {
                BreakableBlock newBlock = PrefabUtility.InstantiatePrefab(blockPrefab) as BreakableBlock;
                newBlock.transform.SetParent(transform);
                
                newBlock.transform.localPosition = new Vector3(i * blockSize, j * blockSize);
                newBlock.transform.localScale = Vector3.one * blockSize;
                
                //Choose and apply settings
                BreakableBlockSettings settings = ChooseRandomBlockType();
                newBlock.health = settings.Health;
                newBlock.resourceType = settings.ResourceType;
                newBlock.fullHealthColor = settings.ColorFullHealth;
                newBlock.oneHealthColor = settings.ColorOneHealth;
                
                //To apply colors immediately
                newBlock.OnValidate();
            }
        }
    }
    
#endif

    private void NormalizeBlockSpawnChances()
    {
        //Normalize Block spawn chances
        float totalChance = blockTypeChances.Select(x => x.chance).Sum();
        foreach (BlockTypeChance chance in blockTypeChances)
        {
            chance.chance /= totalChance;
        }
    }

    private BreakableBlockSettings ChooseRandomBlockType()
    {
        float random = UnityEngine.Random.Range(0f, 1f);
        float totalChance = 0;

        BreakableBlockSettings chosenSettings = null;
        foreach (BlockTypeChance blockTypeChance in blockTypeChances)
        {
            totalChance += blockTypeChance.chance;
            if (totalChance >= random)
            {
                chosenSettings = blockTypeChance.blockSettings;
                break;
            }
        }

        return chosenSettings;
    }
}
