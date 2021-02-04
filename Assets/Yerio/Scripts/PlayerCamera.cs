using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [Header("--Settings--")]
    [Tooltip("Sensitivity of the camera movement")]
    public float sensitivity = 3f;
    public float maxRotX = 45f;
    public float minRotX = -55;
    public Vector3 camOffset;
    [SerializeField] float recoilDecreaseValue;
    [SerializeField] float recoilIncreaseValue;

    [Header("--References--")]
    public Transform player;

    [HideInInspector]
    public float rotCamX;
    [HideInInspector]
    public float rotCamY;

    public float recoilMax;
    public float recoilValue = 0;
    public bool increaseRecoil;

    private void Awake()
    {
        ShowCursor();
    }

    public void Update()
    {
        CameraPos();
        EnemyHealthbarLookat();

        if (increaseRecoil)
        {
            recoilValue = Mathf.Lerp(recoilValue, recoilMax, recoilIncreaseValue * BoltNetwork.FrameDeltaTime);
        }
        else
        {
            recoilValue = Mathf.Lerp(recoilValue, 0, recoilDecreaseValue * BoltNetwork.FrameDeltaTime);
        }

        if (recoilValue + .1 >= recoilMax)
            increaseRecoil = false;
        if (recoilValue <= .1)
            recoilMax = 0;
    }

    public void AddRecoil(float value)
    {
        recoilMax += value;
        increaseRecoil = true;
    }

    void EnemyHealthbarLookat()
    {
        foreach (var enemy in GameObject.FindGameObjectsWithTag("enemy"))
        {
            enemy.GetComponent<EnemyHealth>().CanvasLookAt(transform);
        }
    }

    void CameraPos()
    {
        if (!state.IsDead)
        {
            rotCamY += Input.GetAxis("Mouse X") * sensitivity;
            rotCamX -= Input.GetAxis("Mouse Y") * sensitivity;
        }

        //Clamping the rotX value
        rotCamX = Mathf.Clamp(rotCamX, minRotX, maxRotX);

        //EulerAngles for the camera rotation (this is so it rotates around the player)
        Quaternion rotPlayer = Quaternion.Euler(0, rotCamY, 0);
        Quaternion rotation = Quaternion.Euler(rotCamX - recoilValue, rotCamY, 0);
        transform.position = player.position + camOffset;
        transform.rotation = rotation;
        player.rotation = rotPlayer;
    }

    public static void HideCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public static void ShowCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

}
