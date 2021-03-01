using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraStretch : MonoBehaviour
{
    [SerializeField] Camera cam;

    // Update is called once per frame
    void Update()
    {
        Vector2 resTarget = new Vector2(1920f, 1080f);
        Vector2 resViewport = new Vector2(Screen.width, Screen.height);
        Vector2 resNormalized = resTarget / resViewport; // target res in viewport space
        Vector2 size = resNormalized / Mathf.Max(resNormalized.x, resNormalized.y);
        cam.rect = new Rect(default, size) { center = new Vector2(0.5f, 0.5f) };
    }
}
