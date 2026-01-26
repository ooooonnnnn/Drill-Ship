using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Flips the sprites under this gameobject on the y axis when it's rotation crosses a certain threshold (with play)
/// </summary>
public class FlipYToKeepUpright : MonoBehaviour
{
    [SerializeField] private float flipAngle, playRange;
    
    [SerializeField, HideInInspector] private SpriteRenderer[] renderers;
    private Dictionary<SpriteRenderer, bool> originalFlips = new ();
    private FlipState flipState = FlipState.Original;
    
    private void OnValidate()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>();
    }

    private void Awake()
    {
        foreach (SpriteRenderer renderer in renderers)
        {
            originalFlips.Add(renderer, renderer.flipY);
        }
    }

    private void Update()
    {
        float angle = transform.rotation.eulerAngles.z;
        switch (flipState)
        {
            case FlipState.Original:
                if (angle > flipAngle + playRange && angle < 360 - flipAngle - playRange) Flip();
                break;
            case FlipState.Flipped:
                if (angle < flipAngle - playRange || angle > 360 - flipAngle + playRange) Flip();
                break;
        }
    }

    private void Flip()
    {
        flipState = (FlipState)((int)flipState ^ 1);
        foreach (KeyValuePair<SpriteRenderer, bool> pair in originalFlips)
        {
            pair.Key.flipY = flipState == FlipState.Original ?
                pair.Value : !pair.Value;
        }
    }

    private enum FlipState
    {
        Original = 0,
        Flipped = 1
    }
}
