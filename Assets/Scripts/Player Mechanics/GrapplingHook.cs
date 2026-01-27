using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Helper;

public class GrapplingHook : MonoBehaviour
{
    [Header("Launching")] 
    [SerializeField] private float launchForce;
    [SerializeField,
     Tooltip("How long can the projectile fly before aborting and returning")] private float launchTimeout;
    
    [Header("Reeling In")]
    [SerializeField] private float pullRate;
    [SerializeField] [Tooltip("You can't reel in the hook if the difference between the" +
                              " spring distance and the actual" +
                              " distance is greater than this delta")] private float maxPullDelta;
    [SerializeField] private Rigidbody2D hookProjectile;
    [SerializeField, HideInInspector] private TriggerEnterEvent hookTriggerEvent;
    /// <summary>
    /// The joint keeping the hook projectile stored on the ship
    /// </summary>
    [SerializeField, HideInInspector] private FixedJoint2D storingJoint;
    [SerializeField] private UnityEvent<TransformLocalPoint> OnGrabOrRelease;
    [SerializeField] [HideInInspector] private BungeeJoint2D joint;
    [SerializeField] [HideInInspector] private Rigidbody2D rb;
    
    private HookState hookState = HookState.Stored;

    public bool IsGrabbing => hookState == HookState.Grabbing;
    
    // public bool isGrabbing
    // {
    //     get => _isGrabbing;
    //     set
    //     {
    //         if (_isGrabbing != value)
    //         {
    //             if (!value) SetCallbacks_NotGrabbing();
    //             else SetCallbacks_Grabbing();
    //         }
    //         _isGrabbing = value;
    //     }
    // }
    
    public float JointLength => joint.distance; 
    
    // private bool _isGrabbing;
    private bool canPull = true;
    
    private void OnValidate()
    {
        joint = GetComponent<BungeeJoint2D>();
        rb = GetComponent<Rigidbody2D>();
        storingJoint = GetComponent<FixedJoint2D>();
        hookTriggerEvent = hookProjectile.GetComponent<TriggerEnterEvent>();
    }

    private void Awake()
    {
        //SetCallbacks_NotGrabbing();
        InputManager.RMouseTap += LaunchHook;
    }


    /// <summary>
    /// Remove event subscriptions of the old state and set them for the new state
    /// </summary>
    /// <param name="oldState">Optional, the previous state</param>
    /// <param name="newState">The new state</param>
    private void SetCallbackForState(HookState newState, HookState oldState = HookState.None)
    {
        // Remove callbacks for the old state
        switch (oldState)
        {
            case HookState.Stored:
                InputManager.RMouseTap -= LaunchHook;
                InputManager.RMouseHold -= LaunchHook;
                break;
        }
        
        // Set callbacks for the new state
        switch (newState)
        {
            case HookState.Stored:
                InputManager.RMouseTap += LaunchHook;
                InputManager.RMouseHold += LaunchHook;
                break;
            case HookState.Launched:
                hookTriggerEvent.OnTriggerEnter += Grab;
                break;
        }
    }

    private void LaunchHook()
    {
        storingJoint.enabled = false;
        rb.AddForceEqualReaction(hookProjectile, transform.right * launchForce, ForceMode2D.Impulse);
    }
    
    private void Grab(Collider2D other) 
    {
        //IsGrabbing = TryGrab();
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
        //IsGrabbing = false;
        
        OnGrabOrRelease.Invoke(null);
    }

    private void FixedUpdate()
    {
        if (IsGrabbing && Mouse.current.rightButton.isPressed && canPull)
        {
            joint.distance -= pullRate * Time.fixedDeltaTime;
            print("Pulling");
        }

        float distanceToGrabbed = (transform.TransformPoint(joint.anchor) -
                                   joint.connectedBody.transform.TransformPoint(joint.connectedAnchor)).magnitude;
        
        canPull = distanceToGrabbed - joint.distance <= maxPullDelta;
    }

    //TODO: release object once it has been destroyed or mined

    private enum HookState
    {
        None,
        Stored,
        Launched,
        Grabbing,
        Reeling,
        ReturningNoCollision
    }
}
