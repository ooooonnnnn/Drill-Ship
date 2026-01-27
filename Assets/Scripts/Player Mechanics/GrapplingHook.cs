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
    
    [Header("Joints")]
    [SerializeField] private FixedJoint2D storingJoint;
    [SerializeField] private BungeeJoint2D grappleJoint;
    [SerializeField, Tooltip("For returning the hook when it's aborting grab")] private Joint2D returningJoint;
    
    [SerializeField] private Rigidbody2D hookProjectile;
    /// <summary>
    /// Used to track the flying time of the projectile 
    /// </summary>
    [SerializeField, HideInInspector] private Timer launchTimer;
    [SerializeField, HideInInspector] private TriggerEnterEvent hookTriggerEvent;
    /// <summary>
    /// The joint keeping the hook projectile stored on the ship
    /// </summary>
    [SerializeField] private UnityEvent<TransformLocalPoint> OnGrabOrRelease;
    [SerializeField] [HideInInspector] private Rigidbody2D rb;

    private HookState hookState
    {
        get => _hookState;
        set
        {
            SetCallbackForState(value, _hookState);
            _hookState = value;
        }
    }
    private HookState _hookState = HookState.None;

    public bool IsGrabbing => hookState == HookState.Grabbing;
    
    public float JointLength => grappleJoint.distance; 
    
    // private bool _isGrabbing;
    private bool canPull = true;
    
    private const string PlayerTag = "Player";
    
    private void OnValidate()
    {
        rb = GetComponent<Rigidbody2D>();
        grappleJoint.connectedBody = hookProjectile;
        storingJoint.connectedBody = hookProjectile;
        returningJoint.connectedBody = hookProjectile;
        hookTriggerEvent = hookProjectile.GetComponent<TriggerEnterEvent>();
        launchTimer = hookProjectile.GetComponent<Timer>();
    }

    private void Awake()
    {
        hookState = HookState.Stored;
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
            case HookState.Launched:
                hookTriggerEvent.OnTriggerEnter -= TryGrab;
                break;
            case HookState.ReturningNoCollision:
                hookTriggerEvent.OnTriggerEnter -= TryStoreHook;
                break;
            case HookState.Grabbing:
                InputManager.RMouseTap -= ReleaseGrab;
                InputManager.RMouseDown -= StartReeling;
                break;
            case HookState.Reeling:
                InputManager.RMouseUp -= StopReeling;
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
                hookTriggerEvent.OnTriggerEnter += TryGrab;
                break;
            case HookState.ReturningNoCollision:
                hookTriggerEvent.OnTriggerEnter += TryStoreHook;
                break;
            case HookState.Grabbing:
                InputManager.RMouseTap += ReleaseGrab;
                InputManager.RMouseDown += StartReeling;
                break;
            case HookState.Reeling:
                InputManager.RMouseUp += StopReeling;
                break;
        }
    }
    
    private void LaunchHook()
    {
        storingJoint.enabled = false;
        rb.AddForceEqualReaction(hookProjectile, transform.right * launchForce, ForceMode2D.Impulse);
        launchTimer.StartTimer(launchTimeout, ReturnHook);
        
        hookState = HookState.Launched;
    }

    private void ReturnHook()
    {
        print("Returning hook");
        returningJoint.enabled = true;
        
        hookState = HookState.ReturningNoCollision;
    }

    private void TryStoreHook(Collider2D other)
    {
        print("Trying to store hook");
        
        if (!other.CompareTag(PlayerTag)) return;
        
        storingJoint.enabled = true;
        returningJoint.enabled = false;
        
        hookState = HookState.Stored;
    }
    
    private void TryGrab(Collider2D other)
    {

        hookState = HookState.Grabbing;
    }
    
    private void ReleaseGrab()
    {
        grappleJoint.enabled = false;
        //IsGrabbing = false;
        
        OnGrabOrRelease.Invoke(null);
    }

    private void StartReeling()
    {
        
        hookState = HookState.Reeling;
    }
    
    private void StopReeling()
    {
        
        hookState = HookState.Grabbing;
    }

    private bool Grab()
    {
        Vector2 worldMousePos = InputManager.WorldMousePos;
        Collider2D overlapCol = Physics2D.OverlapPoint(worldMousePos);
        if (!overlapCol) return false;

        grappleJoint.enabled = true;
        grappleJoint.connectedBody = overlapCol.attachedRigidbody;
        Transform grabbedTransform = overlapCol.transform;
        Vector2 grabbedLocalPoint = grabbedTransform.InverseTransformPoint(worldMousePos);
        grappleJoint.connectedAnchor = grabbedLocalPoint;
        grappleJoint.distance = Vector2.Distance(transform.position, worldMousePos);
        
        OnGrabOrRelease.Invoke(new TransformLocalPoint(grabbedTransform, grabbedLocalPoint * grabbedTransform.lossyScale));
        
        return true;
    }

    

    private void FixedUpdate()
    {
        if (IsGrabbing && Mouse.current.rightButton.isPressed && canPull)
        {
            grappleJoint.distance -= pullRate * Time.fixedDeltaTime;
            print("Pulling");
        }

        float distanceToGrabbed = (transform.TransformPoint(grappleJoint.anchor) -
                                   grappleJoint.connectedBody.transform.TransformPoint(grappleJoint.connectedAnchor)).magnitude;
        
        canPull = distanceToGrabbed - grappleJoint.distance <= maxPullDelta;
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
