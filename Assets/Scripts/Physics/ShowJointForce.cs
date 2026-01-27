using System;
using Helper;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShowJointForce : MonoBehaviour
{
    [SerializeField] private float scale;
    [SerializeField] [HideInInspector] private Joint2D[] joints;
    [SerializeField] [HideInInspector] private Rigidbody2D rb;
    [SerializeField] [HideInInspector] private Color[] colors;

    private void OnValidate()
    {
        joints = GetComponents<Joint2D>();
        rb = GetComponent<Rigidbody2D>();
        colors = new Color[joints.Length + 1];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = MyColors.RandomColor;
        }
    }

    private void FixedUpdate()
    {
        totalForce = rb.totalForce;

        if (!Mouse.current.rightButton.isPressed) return;
        
        foreach (Joint2D joint in joints)
        {
            if (joint is SpringJoint2D spring)
            {
                float distance = Vector2.Distance(transform.TransformPoint(spring.anchor), spring.connectedBody.transform.TransformPoint(spring.connectedAnchor));
                print($"current distance: {distance}\n" +
                      $"spring distance: {spring.distance}");
            }
        }
    }

    private Vector2 totalForce;
    
    private void OnDrawGizmos()
    {
        for (int i = 0; i < joints.Length; i++)
        {
            Gizmos.color = colors[i];
            Gizmos.DrawRay(rb.position, -joints[i].reactionForce * scale);
        }
        
        Gizmos.color = Color.red;
        Gizmos.DrawRay(rb.position, totalForce * scale);
    }
}
