using System;
using UnityEngine;

public class Timer : MonoBehaviour
{
    private Action onTimerEnd;

    private float endTime;

    /// <summary>
    /// Starts the timer and 
    /// </summary>
    /// <param name="time"></param>
    /// <param name="callback"></param>
    public void StartTimer(float time, Action callback)
    {
        endTime = Time.time + time;
        enabled = true;
        onTimerEnd = callback;
    }

    public void StopTimer()
    {
        enabled = false;
    }
    
    private void FixedUpdate()
    {
        if (Time.time >= endTime)
        {
            onTimerEnd?.Invoke();
        }
    }

    private void OnDisable()
    {
        onTimerEnd = null;
    }
}
