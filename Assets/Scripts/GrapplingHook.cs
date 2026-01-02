using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class GrapplingHook : MonoBehaviour
{
    [SerializeField] private float pullRate;
    [SerializeField] [HideInInspector] private DistanceJoint2D joint;
    [SerializeField] private UnityEvent<TransformLocalPoint> OnGrabOrRelease;

    public bool isGrabbing
    {
        get => _isGrabbing;
        set
        {
            if (_isGrabbing != value)
            {
                if (!value) SetCallbacks_NotGrabbing();
                else SetCallbacks_Grabbing();
            }
            _isGrabbing = value;
        }
    }
    
    public float JointLength => joint.distance; 
    
    private bool _isGrabbing;
    private bool canPull = true;
    
    private void OnValidate()
    {
        joint = GetComponent<DistanceJoint2D>();
    }

    private void Awake()
    {
        SetCallbacks_NotGrabbing();
    }

    /// <summary>
    /// Callbacks while not grabbing anything
    /// </summary>
    private void SetCallbacks_NotGrabbing()
    {
        InputManager.RMouseTap += Grab;
        InputManager.RMouseHold += Grab;
        InputManager.RMouseTap -= ReleaseGrab;
    }

    /// <summary>
    /// Callbacks while grabbing something
    /// </summary>
    private void SetCallbacks_Grabbing()
    {
        InputManager.RMouseTap -= Grab;
        InputManager.RMouseHold -= Grab;
        InputManager.RMouseTap += ReleaseGrab;
    }

    private void Grab() 
    {
        isGrabbing = TryGrab();
    }

    private bool TryGrab()
    {
        Vector2 worldMousePos = InputManager.WorldMousePos;
        Collider2D overlapCol = Physics2D.OverlapPoint(worldMousePos);
        if (!overlapCol) return false;

        joint.enabled = true;
        joint.connectedBody = overlapCol.attachedRigidbody;
        Transform grabbedTransform = overlapCol.transform;
        Vector2 grabbedLocalPoint = grabbedTransform.InverseTransformPoint(worldMousePos);
        joint.connectedAnchor = grabbedLocalPoint;
        joint.distance = Vector2.Distance(transform.position, worldMousePos);
        
        OnGrabOrRelease.Invoke(new TransformLocalPoint(grabbedTransform, grabbedLocalPoint * grabbedTransform.lossyScale));
        
        return true;
    }

    private void ReleaseGrab()
    {
        joint.enabled = false;
        isGrabbing = false;
        
        OnGrabOrRelease.Invoke(null);
    }

    private void FixedUpdate()
    {
        if (isGrabbing && Mouse.current.rightButton.isPressed && canPull)
        {
            joint.distance -= pullRate * Time.fixedDeltaTime;
            print("Pulling");
        }

        canPull = true;
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        if (isGrabbing && other.gameObject == joint.connectedBody.gameObject) canPull = false;
    }

    //TODO: release object once it has been destroyed or mined
}
