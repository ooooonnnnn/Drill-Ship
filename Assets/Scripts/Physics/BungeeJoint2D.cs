using System;
using UnityEngine;

/// <summary>
/// Like a spring joint but only active when the distance is greater than the requires distance. IE only pulls, doesn't
/// push.
/// </summary>
[RequireComponent(typeof(SpringJoint2D), typeof(Rigidbody2D))]
public class BungeeJoint2D : MonoBehaviour
{
    public float distance {get => springJoint.distance; set => springJoint.distance = value;}
    public Rigidbody2D connectedBody {get => springJoint.connectedBody; set => springJoint.connectedBody = value;}
    public Vector2 connectedAnchor {get => springJoint.connectedAnchor; set => springJoint.connectedAnchor = value;}
    public Vector2 anchor {get => springJoint.anchor; set => springJoint.anchor = value;}
    
    [SerializeField] private SpringJoint2D springJoint;
    [SerializeField] [HideInInspector] private Rigidbody2D rb;

    private void OnValidate()
    {
        //spring = GetComponent<SpringJoint2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnDisable()
    {
        springJoint.enabled = false;
    }
    
    private void OnEnable()
    {
        springJoint.enabled = true;
    }

    private void FixedUpdate()
    {
        if (!springJoint.connectedBody) return;
        
        Vector2 selfToConnected = connectedBody.transform.TransformPoint(springJoint.connectedAnchor) -
                                    transform.TransformPoint(springJoint.anchor);
        
        //Turn the spring on if it should be stretched 
        float currentSquareDist = selfToConnected.sqrMagnitude;
        float requiredSquareDist = distance * distance;
        springJoint.enabled = currentSquareDist > requiredSquareDist;
    }
}
