using System;
using UnityEngine;

public class GapplingHook : MonoBehaviour
{
    [SerializeField] [HideInInspector] private DistanceJoint2D joint;

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
    
    private bool _isGrabbing;
    
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
        joint.connectedAnchor = overlapCol.transform.InverseTransformPoint(worldMousePos);
        joint.distance = Vector2.Distance(transform.position, worldMousePos);
        return true;
    }

    private void ReleaseGrab()
    {
        joint.enabled = false;
        isGrabbing = false;
    }
    
    //TODO: disable grabbing of an object once it has been destroyed or mined
}
