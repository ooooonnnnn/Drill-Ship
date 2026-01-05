using System;
using UnityEngine;

/// <summary>
/// Like a spring joint but only active when the distance is greater than the requires distance. IE only pulls, doesn't
/// push.
/// </summary>
[RequireComponent(typeof(SpringJoint2D), typeof(Rigidbody2D))]
public class BungeeJoint2D : MonoBehaviour
{
    public float distance {get => spring.distance; set => spring.distance = value;}
    public Rigidbody2D connectedBody {get => spring.connectedBody; set => spring.connectedBody = value;}
    public Vector2 connectedAnchor {get => spring.connectedAnchor; set => spring.connectedAnchor = value;}
    
    [SerializeField] [HideInInspector] private SpringJoint2D spring;
    [SerializeField] [HideInInspector] private Rigidbody2D rb;

    private void OnValidate()
    {
        spring = GetComponent<SpringJoint2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnDisable()
    {
        spring.enabled = false;
    }
    
    private void OnEnable()
    {
        spring.enabled = true;
    }

    private void FixedUpdate()
    {
        if (!spring.connectedBody) return;
        
        Vector2 selfToConnected = connectedBody.transform.TransformPoint(spring.connectedAnchor) -
                                    transform.TransformPoint(spring.anchor);
        
        //Turn the spring on if it should be stretched 
        float currentSquareDist = selfToConnected.sqrMagnitude;
        float requiredSquareDist = distance * distance;
        spring.enabled = currentSquareDist > requiredSquareDist;
    }
}
