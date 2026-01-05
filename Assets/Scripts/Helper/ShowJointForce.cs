using System;
using Helper;
using UnityEngine;

public class ShowJointForce : MonoBehaviour
{
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
    }

    private Vector2 totalForce;
    
    private void OnDrawGizmos()
    {
        for (int i = 0; i < joints.Length; i++)
        {
            Gizmos.color = colors[i];
            //Gizmos.DrawRay(rb.position, joints[i].reactionForce);
        }
        
        Gizmos.color = Color.red;
        Gizmos.DrawRay(rb.position, totalForce);
        print(totalForce);
    }
}
