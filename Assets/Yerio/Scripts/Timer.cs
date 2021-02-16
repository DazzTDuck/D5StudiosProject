using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{
    Action timerCallback;
    Action timerActiveAction;
    float timer;

    public void SetTimer(float time, Action actionAfterTimer = null, Action actionWhileTimerActive = null)
    {
        timer = time;
        timerCallback = actionAfterTimer;
        timerActiveAction = actionWhileTimerActive;
    }

    private void Update()
    {
        if (IsTimerActive())
        {
            timer -= Time.deltaTime;

            if (IsTimerComplete())
                timerCallback?.Invoke();

            if (IsTimerActive())
                timerActiveAction?.Invoke();
        }
    }

    public bool IsTimerComplete()
    {
        return timer <= 0;
    }
    public bool IsTimerActive()
    {
        return timer > 0;
    }

    public float GetTimerValue()
    {
        return timer;
    }
}
