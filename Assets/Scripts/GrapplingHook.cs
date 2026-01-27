using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Helper;

public class GrapplingHook : MonoBehaviour
{
    [Header("Launching")] 
    [SerializeField] private float launchForce;
    [Header("Reeling In")]
    [SerializeField] private float pullRate;
    [SerializeField] [Tooltip("You can't reel in the hook if the difference between the" +
                              " spring distance and the actual" +
                              " distance is greater than this delta")] private float maxPullDelta;
    [SerializeField] private Rigidbody2D hookProjectile;
    /// <summary>
    /// The joint keeping the hook projectile stored on the ship
    /// </summary>
    [SerializeField, HideInInspector] private FixedJoint2D storingJoint;
    [SerializeField] private UnityEvent<TransformLocalPoint> OnGrabOrRelease;
    [SerializeField] [HideInInspector] private BungeeJoint2D joint;
    [SerializeField] [HideInInspector] private Rigidbody2D rb;
    
    private HookState hookState = HookState.Stored;

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
        joint = GetComponent<BungeeJoint2D>();
        rb = GetComponent<Rigidbody2D>();
        storingJoint = GetComponent<FixedJoint2D>();
    }

    private void Awake()
    {
        //SetCallbacks_NotGrabbing();
        InputManager.RMouseTap += LaunchHook;
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

    private void LaunchHook()
    {
        Destroy(storingJoint);
        rb.AddForceEqualReaction(hookProjectile, transform.right * launchForce, ForceMode2D.Impulse);
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

        float distanceToGrabbed = (transform.TransformPoint(joint.anchor) -
                                   joint.connectedBody.transform.TransformPoint(joint.connectedAnchor)).magnitude;
        
        canPull = distanceToGrabbed - joint.distance <= maxPullDelta;
    }

    //TODO: release object once it has been destroyed or mined

    private enum HookState
    {
        Stored,
        Launched,
        Grabbing,
        Reeling,
        ReturningNoCollision
    }
}
