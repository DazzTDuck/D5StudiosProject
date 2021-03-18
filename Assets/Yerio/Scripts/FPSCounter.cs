using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    TMP_Text fpsText;
    float _hudRefreshRate = .25f;

    private float timer;

    private void Awake()
    {
        fpsText = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        if (Time.unscaledTime > timer)
        {
            int fps = (int)(1f / Time.unscaledDeltaTime);
            fpsText.text = "FPS: " + fps;
            timer = Time.unscaledTime + _hudRefreshRate;
        }
    }
}
