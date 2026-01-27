using System;using JetBrains.Annotations;
using UnityEngine;

/// <summary>
/// A local-space point on a transform
/// </summary>
public class TransformLocalPoint
{
    /// <summary>
    /// Local space point
    /// </summary>
    public Vector2 point { get; private set; }
    
    /// <summary>
    /// The transform
    /// </summary>
    public Transform transform { get; private set; }
    
    public TransformLocalPoint(Transform transform, Vector2 point)
    {
        this.point = point;
        this.transform = transform;
    }
}
