using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("--Settings--")]
    [Tooltip("Sensitivity of the camera movement")]
    public float sensitivity = 3f;
    public float maxRotX = 45f;
    public float minRotX = -55;
    public Vector3 camOffset;

    [Header("--References--")]
    public Transform player;

    [HideInInspector]
    public float rotCamX;
    [HideInInspector]
    public float rotCamY;

    private void Awake()
    {
        HideCursor();
    }

    public void Update()
    {
        CameraPos();
    }

    void CameraPos()
    {
        rotCamY += Input.GetAxis("Mouse X") * sensitivity;
        rotCamX -= Input.GetAxis("Mouse Y") * sensitivity;

        //Clamping the rotX value
        rotCamX = Mathf.Clamp(rotCamX, minRotX, maxRotX);

        //EulerAngles for the camera rotation (this is so it rotates around the player)
        Quaternion rotPlayer = Quaternion.Euler(0, rotCamY, 0);
        Quaternion rotation = Quaternion.Euler(rotCamX, rotCamY, 0);
        transform.position = player.position + camOffset;
        transform.rotation = rotation;
        player.rotation = rotPlayer;
    }

    public void HideCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ShowCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

}
