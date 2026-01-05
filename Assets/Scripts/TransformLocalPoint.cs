using System;using JetBrains.Annotations;
using UnityEngine;

public class TransformLocalPoint
{
    public Vector2 point { get; private set; }
    public Transform transform { get; private set; }
    
    public TransformLocalPoint(Transform transform, Vector2 point)
    {
        this.point = point;
        this.transform = transform;
    }
}
