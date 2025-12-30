using System;
using UnityEngine;

public class CreateMasksWhileTrigger : MonoBehaviour
{
    [SerializeField] private GameObject maskPrefab;


    private void OnTriggerStay2D(Collider2D other)
    {
        GameObject newMask = Instantiate(maskPrefab, other.transform);
        newMask.transform.position = transform.position;
    }
}
