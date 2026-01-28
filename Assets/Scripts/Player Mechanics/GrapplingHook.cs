using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Helper;
using UnityEngine.Animations;

public class GrapplingHook : MonoBehaviour
{
    [Header("Launching")] 
    [SerializeField] private float launchSpeed;
    [SerializeField,
     Tooltip("How long can the projectile fly before aborting and returning")] private float launchTimeout;
    
    [Header("Reeling In")]
    [SerializeField] private float pullRate;
    [SerializeField] [Tooltip("You can't reel in the hook if the difference between the" +
                              " spring distance and the actual" +
                              " distance is greater than this delta")] private float maxPullDelta;

    [Header("Returning Empty Handed")]
    [SerializeField] private float returnSpeed;
    
    [Header("Joints")]
    [SerializeField] private Joint2D storingJoint;
    [SerializeField] private BungeeJoint2D grappleJoint;
    //[SerializeField, Tooltip("For returning the hook when it's aborting grab")] private Joint2D returningJoint;
    
    [Header("Projectile")]
    [SerializeField] private Rigidbody2D hookProjectile;
    /// <summary>
    /// Used to track the flying time of the projectile 
    /// </summary>
    [SerializeField, HideInInspector] private Timer hookFlyTimer;
    [SerializeField, HideInInspector] private ColliderEvents hookColliderEvents;
    /// <summary>
    /// To orient to hook before launching
    /// </summary>
    [SerializeField, HideInInspector] private RotationConstraint hookRotationConstraint;
    /// <summary>
    /// The joint on the hook used to anchor it to the grabbed object
    /// </summary>
    [SerializeField, HideInInspector] private AnchoredJoint2D hookGrappleJoint;
    
    [SerializeField] private UnityEvent<TransformLocalPoint> OnGrabOrRelease;
    [SerializeField] [HideInInspector] private Rigidbody2D rb;

    private HookState hookState
    {
        get => _hookState;
        set
        {
            print($"Hook State: {value}");
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
    private const string NotGrabbableTag = "GrappleHook Not Grabbable";

    private void OnValidate()
    {
        rb = GetComponent<Rigidbody2D>();
        grappleJoint.connectedBody = hookProjectile;
        storingJoint.connectedBody = hookProjectile;
        //returningJoint.connectedBody = hookProjectile;
        hookColliderEvents = hookProjectile.GetComponent<ColliderEvents>();
        hookFlyTimer = hookProjectile.GetComponent<Timer>();
        hookRotationConstraint = hookProjectile.GetComponent<RotationConstraint>();
        hookGrappleJoint = hookProjectile.GetComponent<AnchoredJoint2D>();
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
                InputManager.OnRmbTap -= LaunchHook;
                break;
            case HookState.Launched:
                hookColliderEvents.OnCollisionEnter -= TryGrab;
                break;
            case HookState.ReturningNoCollision:
                hookColliderEvents.OnTriggerEnter -= TryStoreHook;
                break;
            case HookState.Grabbing:
                InputManager.OnRmbTap -= ReleaseGrab;
                InputManager.OnRmbHold -= StartReeling;
                break;
            case HookState.Reeling:
                InputManager.OnRmbUp -= StopReeling;
                break;
        }
        
        // Set callbacks for the new state
        switch (newState)
        {
            case HookState.Stored:
                InputManager.OnRmbTap += LaunchHook;
                break;
            case HookState.Launched:
                hookColliderEvents.OnCollisionEnter += TryGrab;
                break;
            case HookState.ReturningNoCollision:
                hookColliderEvents.OnTriggerEnter += TryStoreHook;
                break;
            case HookState.Grabbing:
                InputManager.OnRmbTap += ReleaseGrab;
                InputManager.OnRmbHold += StartReeling;
                break;
            case HookState.Reeling:
                InputManager.OnRmbUp += StopReeling;
                break;
        }
    }
    
    private void LaunchHook()
    {
        storingJoint.enabled = false;
        hookRotationConstraint.constraintActive = false;
        Vector2 desiredVelocity = transform.right * launchSpeed;
        Vector2 suggestedForce = hookProjectile.SuggestForceForVelocity(desiredVelocity);
        rb.AddForceEqualReaction(hookProjectile, suggestedForce, ForceMode2D.Impulse);
        hookFlyTimer.StartTimer(launchTimeout, ReturnHook);
        
        hookState = HookState.Launched;
    }

    private void ReturnHook()
    {
        hookState = HookState.ReturningNoCollision;
    }

    private void TryStoreHook(Collider2D other)
    {
        if (!other.CompareTag(PlayerTag)) return;
        
        storingJoint.enabled = true;
        hookRotationConstraint.constraintActive = true;
        
        hookState = HookState.Stored;
    }
    
    private void TryGrab(Collision2D collision)
    {
        Collider2D other = collision.collider;
        
        if (other.CompareTag(NotGrabbableTag) || other.CompareTag(PlayerTag)) return;
        
        hookFlyTimer.StopTimer();
        
        hookGrappleJoint.connectedBody = other.attachedRigidbody;
        grappleJoint.connectedBody = other.attachedRigidbody;
        Vector2 localPointOnOther = other.transform.InverseTransformPoint(hookProjectile.position);
        hookGrappleJoint.connectedAnchor = localPointOnOther;
        grappleJoint.connectedAnchor = localPointOnOther;
        hookGrappleJoint.enabled = true;
        grappleJoint.enabled = true;
        grappleJoint.distance = Vector2.Distance(transform.position, hookProjectile.position);
        
        hookState = HookState.Grabbing;
    }
    
    private void ReleaseGrab()
    {
        grappleJoint.enabled = false;
        hookGrappleJoint.enabled = false;

        ReturnHook();
    }

    private void StartReeling()
    {
        hookState = HookState.Reeling;
    }
    
    private void StopReeling()
    {
        hookState = HookState.Grabbing;
    }
    
    private void FixedUpdate()
    {
        switch (hookState)
        {
            case HookState.ReturningNoCollision:
                ApplyReturningForce();
                break;
            case HookState.Reeling:
                TryReel();
                break;
        }
    }

    /// <summary>
    /// Reels the hook in if it's not reeled beyond the allowable delta
    /// </summary>
    private void TryReel()
    {
        float distanceToGrabbed = (transform.TransformPoint(grappleJoint.anchor) -
                                   grappleJoint.connectedBody.transform.TransformPoint(grappleJoint.connectedAnchor)).magnitude;
        canPull = distanceToGrabbed - grappleJoint.distance <= maxPullDelta;
        if (!canPull) return;
        grappleJoint.distance -= pullRate * Time.fixedDeltaTime;
    }

    /// <summary>
    /// Applies a force from this to the hook projectile to give it velocity towards this 
    /// </summary>
    private void ApplyReturningForce()
    {
        Vector2 hookToMeDir = (transform.position - hookProjectile.transform.position).normalized;
        Vector2 desiredVelocity = hookToMeDir * returnSpeed;
        rb.AddForceEqualReaction(hookProjectile, hookProjectile.SuggestForceForVelocity(desiredVelocity),
            ForceMode2D.Impulse);
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
