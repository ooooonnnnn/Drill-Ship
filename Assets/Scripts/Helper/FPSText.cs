using System;
using TMPro;
using UnityEngine;

public class FPSText : MonoBehaviour
{
    [HideInInspector] [SerializeField] TMP_Text text;
    [SerializeField] private int averageOverFrames;
    private float[] times;
    private int frameCount = 0;

    private void OnValidate()
    {
        text = GetComponent<TMP_Text>();
    }

    private void Awake()
    {
        times = new float[averageOverFrames];
    }

    private void Update()
    {
        for (int i = 0; i < times.Length - 1; i++)
        {
            times[i] = times[i + 1];
        }
        times[averageOverFrames - 1] = Time.unscaledTime;
        text.text = $"FPS: {Mathf.RoundToInt((averageOverFrames - 1) / (times[averageOverFrames - 1] -  times[0]))}";
    }
}
