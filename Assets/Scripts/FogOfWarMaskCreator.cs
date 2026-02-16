using System;
using UnityEngine;

public class FogOfWarMaskCreator : MonoBehaviour
{
    [SerializeField, Tooltip("When digging a block, masks will be created in a radius around where it was")]
    private int radius;
    [SerializeField] private GameObject maskPrefab;
    [SerializeField] private float blockSize;

    private void Start()
    {
        blockSize = BreakableBlockManager.Instance.blockSize;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="position"></param>
    public void GenerateMasks(Vector2 position)
    {
        for (int i = -radius; i <= radius; i++)
        {
            for (int j = -radius; j <= radius; j++)
            {
                Vector2 maskPosition = position + new Vector2(i, j) * blockSize;
                Vector2 maskScale = Vector2.one * blockSize;
                Instantiate(maskPrefab, maskPosition, Quaternion.identity).transform.localScale = maskScale;
            }
        }
    }
}
