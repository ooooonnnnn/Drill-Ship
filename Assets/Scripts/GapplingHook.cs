using System;
using UnityEngine;

public class GapplingHook : MonoBehaviour
{
    [SerializeField] [HideInInspector] private Joint2D joint;

    private void OnValidate()
    {
        joint = GetComponent<Joint2D>();
    }
    
    
}
