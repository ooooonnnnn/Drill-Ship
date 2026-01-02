using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations;

public class GrapplingHookVisuals : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private ParentConstraint endPointConstraint;
    [SerializeField] private GrapplingHook hookScript;
    [SerializeField] private int numPoints;
    [SerializeField] private float maxAmplitude;
    [SerializeField] private float minDist;

    private void OnValidate()
    {
        lineRenderer.useWorldSpace = false;
        lineRenderer.positionCount = numPoints;
        lineRenderer.enabled = false;
    }

    public void OnGrabStateChange(TransformLocalPoint data)
    {
        if (data == null)
        {
            lineRenderer.enabled = false;
            return;
        }
        lineRenderer.enabled = true;


        SetEndPointConstraints(data);

    }

    private void SetEndPointConstraints(TransformLocalPoint data)
    {
        endPointConstraint.constraintActive = false;
        endPointConstraint.locked = false;
        var constraintSources = new List<ConstraintSource>
        {
            new ConstraintSource
            {
                sourceTransform = data.transform,
                weight = 1
            }
        };
        endPointConstraint.SetSources(constraintSources);
        endPointConstraint.SetTranslationOffset(0, data.point);
        endPointConstraint.locked = true;
        endPointConstraint.constraintActive = true;
    }

    private void Update()
    {
        float maxJointDist = hookScript.JointLength;
        float currentDist = Vector2.Distance(lineRenderer.transform.position, endPointConstraint.transform.position);

        Func<int, float> lineShape = currentDist >= maxJointDist ?
            _ => 0 :
            currentDist >= minDist ?
                ind => math.sin(2 * math.PI * ind / (numPoints - 1)) * math.lerp(maxAmplitude, 0, math.pow((currentDist - minDist) / (maxJointDist - minDist), 4)) :
                ind => math.sin(2 * math.PI * ind / (numPoints - 1)) * maxAmplitude;

        Vector3[] positions = new Vector3[numPoints];
        for (int i = 0; i < numPoints; i++)
        {
            positions[i] = new Vector3(currentDist * i / (numPoints - 1), lineShape(i));
        }
        
        lineRenderer.SetPositions(positions);
    }
}
