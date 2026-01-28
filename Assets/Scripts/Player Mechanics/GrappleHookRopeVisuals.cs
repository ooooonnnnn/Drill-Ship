using System;
using Unity.Mathematics;
using UnityEngine;

public class GrappleHookRopeVisuals : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] private GrappleHook hookScript;
    [SerializeField,
     Tooltip("Line renderer component that sits on the starting point")] private LineRenderer lineRenderer;
    [SerializeField] private Transform endPoint;
    
    [Header("Rope Shape Settings")]
    [SerializeField, Tooltip("The amplitude of the rope undulations is capped")] private float maxAmplitude;
    [SerializeField, 
     Tooltip("When the distance between the start and end points gets below this, the amplitude will stop increasing at maxAmplitude")] 
    private float minDistance;
    [SerializeField, 
     Tooltip("When launching, the rope will be longer than the distance between the start and end points")] private float ropeSlackWhenLaunching;
    
    [Header("Line Resolution")]
    [SerializeField] private int numPoints;

    private RopeState ropeState = RopeState.Inactive;

    private void OnValidate()
    {
        lineRenderer.useWorldSpace = false;
        lineRenderer.positionCount = numPoints;
        lineRenderer.enabled = false;
    }

    public void OnHookStateChange(HookState prevState, HookState newState)
    {
        if (newState == HookState.Stored)
        {
            ropeState = RopeState.Inactive;
            lineRenderer.enabled = false;
            return;
        }
        if(prevState == HookState.Stored)
        {
            lineRenderer.enabled = true;
        }

        ropeState = newState switch
        {
            HookState.Launched => RopeState.Slacking,
            _ => RopeState.Tight
        };
    }

    private void FixedUpdate()
    {
        float targetRopeLength = 0,
            ropeTightLength = 0;
        switch (ropeState)
        {
            case RopeState.Inactive:
                return;
            case RopeState.Slacking:
                ropeTightLength = Vector2.Distance(lineRenderer.transform.position, endPoint.position);
                targetRopeLength = ropeTightLength + ropeSlackWhenLaunching;
                break;
            case RopeState.Tight:
                ropeTightLength = Vector2.Distance(lineRenderer.transform.position, endPoint.position);
                targetRopeLength = hookScript.JointLength;
                break;
        }
        SetLineShape(targetRopeLength, ropeTightLength);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="targetRopeLength">Desired rope length</param>
    /// <param name="ropeTightLength">The length the rope would have had if it was tight</param>
    private void SetLineShape(float targetRopeLength, float ropeTightLength)
    {
        Func<int, float> lineShape = targetRopeLength >= ropeTightLength ?
            _ => 0 :
            targetRopeLength >= minDistance ?
                //Sine with variable amplitude: zero when the lengths are the same, increasing when currentDist decreases, 
                //limited by maxAmplitude when the distance reaches minDistance
                ind => math.sin(2 * math.PI * ind / (numPoints - 1)) * math.lerp(maxAmplitude, 0, math.pow((targetRopeLength - minDistance) / (ropeTightLength - minDistance), 4)) :
                ind => math.sin(2 * math.PI * ind / (numPoints - 1)) * maxAmplitude;

        Vector3[] positions = new Vector3[numPoints];
        for (int i = 0; i < numPoints; i++)
        {
            positions[i] = new Vector3(targetRopeLength * i / (numPoints - 1), lineShape(i));
        }
        
        lineRenderer.SetPositions(positions);
    }

    private enum RopeState
    {
        Inactive,
        /// <summary>
        /// The rope is longer than the length of the physical joint
        /// </summary>
        Slacking,
        /// <summary>
        /// The rope and the physical joint are the same length
        /// </summary>
        Tight
    }
}
