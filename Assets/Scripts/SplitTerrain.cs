using System;
using System.Collections.Generic;
using Helper;
using UnityEngine;
using UnityEngine.U2D;

/// <summary>
/// Track the composite collider to see when it splits to more than 1 collider. Use these shapes to create new terrains
/// instead of this one
/// </summary>
public class SplitTerrain : MonoBehaviour
{
    private const string TagMask = "Mask";
    [SerializeField] private CompositeCollider2D compositeCollider;
    [SerializeField] [InspectorName("Min square dist in spline")] private float sqrDistThreshold;
    [SerializeField] private SpriteShapeController shape;
    // [SerializeField] private Rigidbody2D rigidbody2D;
    
    private void OnValidate()
    {
        compositeCollider = GetComponent<CompositeCollider2D>();
        shape = GetComponentInChildren<SpriteShapeController>();
        // rigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        ClearMasks();
    }

    private void FixedUpdate()
    {
        //Check the number of colliders in the composite collider
        if (compositeCollider.pathCount > 1)
        {
            Split();
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Destroy all child masks 
    /// </summary>
    private void ClearMasks()
    {
        foreach (Transform child in transform)
        {
            if (child.CompareTag(TagMask)) Destroy(child.gameObject);
        }
    }
    
    private void Split()
    {
        print("Splitting terrain");
        
        for (int i = 0; i < compositeCollider.pathCount; i++)
        {
            //Create a new terrain
            SplitTerrain newTerrain = Instantiate(this, transform.position, transform.rotation);
            
            //Give it the correct shape
            List<Vector2> points = new List<Vector2>();
            compositeCollider.GetPath(i, points);

            Spline newSpline = newTerrain.shape.spline;
            newSpline.Clear();
            Vector2? prevPoint = null;
            foreach (var point in points)
            {
                if (prevPoint != null)
                {
                    float sqrDist = (point - (Vector2)prevPoint).sqrMagnitude;
                    if (sqrDist < sqrDistThreshold)
                    {
                        continue;
                    }
                }
                newSpline.InsertPointAt(newSpline.GetPointCount(), point);
                prevPoint = point;
                
                int index = newSpline.GetPointCount() - 1;
            
                newSpline.SetTangentMode(index, ShapeTangentMode.Linear);
                newSpline.SetHeight(index, 0.1f);
                newSpline.SetCorner(index, false);
            }
            
            //TODO: Give it the correct velocity and angular velocity
        }
    }
}
