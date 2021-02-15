﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [Space, SerializeField] PauseMenuHandler pauseMenuHandler;

    [Header("--Settings--")]
    [Tooltip("Sensitivity of the camera movement")]
    public float sensitivity = 3f;
    public float maxRotX = 45f;
    public float minRotX = -55;
    public Vector3 camOffset;
    [SerializeField] float recoilDecreaseValue;
    [SerializeField] float recoilIncreaseValue;
    [SerializeField] float recoilMaxClamp;

    [Header("--References--")]
    public Transform player;
    public CapsuleCollider capsuleCollider;
    public Shoot weapon;

    [HideInInspector]
    public float rotCamX;
    [HideInInspector]
    public float rotCamY;

    Vector3 colliderCenter;
    [SerializeField] float crouchDivider = 2;

    float recoilMaxX;
    float recoilMaxY;
    float recoilValueX = 0;
    float recoilValueY = 0;
    bool increaseRecoil = false;

    private void Awake()
    {
        ShowCursor();
        colliderCenter = capsuleCollider.center;
    }

    public void Update()
    {
        CameraPos();
        EnemyHealthbarLookat();

        if (increaseRecoil)
        {
            recoilValueX = Mathf.Lerp(recoilValueX, recoilMaxX, recoilIncreaseValue * BoltNetwork.FrameDeltaTime);
            recoilValueY = Mathf.Lerp(recoilValueY, recoilMaxY, recoilIncreaseValue * BoltNetwork.FrameDeltaTime);
        }
        else
        {
            recoilValueX = Mathf.Lerp(recoilValueX, 0, recoilDecreaseValue * BoltNetwork.FrameDeltaTime);
            recoilValueY = Mathf.Lerp(recoilValueY, 0, recoilDecreaseValue * BoltNetwork.FrameDeltaTime);
        }

        if (recoilValueX + .1 > recoilMaxX)
            increaseRecoil = false;
        if (recoilValueX < .1)
        {
            recoilMaxX = 0;
            recoilMaxY = 0;
        }
    }

    public void Crouch(bool crouching)
    {
        if (crouching)
        {
            camOffset /= crouchDivider;
            capsuleCollider.height /= crouchDivider;
            colliderCenter = new Vector3(colliderCenter.x, colliderCenter.y / crouchDivider, colliderCenter.z);
            capsuleCollider.center = colliderCenter;
        }
        else
        {
            camOffset *= crouchDivider;
            capsuleCollider.height *= crouchDivider;
            colliderCenter = new Vector3(colliderCenter.x, colliderCenter.y * crouchDivider, colliderCenter.z);
            capsuleCollider.center = colliderCenter;
        }
    }

    public void AddRecoil(float valueX, float valueY)
    {
        recoilMaxX += valueX;
        recoilMaxX = Mathf.Clamp(recoilMaxX, 0, recoilMaxClamp);
        recoilMaxY = valueY;
        increaseRecoil = true;
    }

    void EnemyHealthbarLookat()
    {
        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            enemy.GetComponent<EnemyHealth>().CanvasLookAt(transform);
        }
    }

    void CameraPos()
    {
        if (!state.IsDead && !pauseMenuHandler.GetIfPaused())
        {
            rotCamY += Input.GetAxis("Mouse X") * sensitivity;
            rotCamX -= Input.GetAxis("Mouse Y") * sensitivity;
        }

        //Clamping the rotX value
        rotCamX = Mathf.Clamp(rotCamX, minRotX, maxRotX);

        //EulerAngles for the camera rotation (this is so it rotates around the player)
        Quaternion rotPlayer = Quaternion.Euler(0, rotCamY, 0);
        Quaternion rotation = Quaternion.Euler(rotCamX - recoilValueX, rotCamY + recoilValueY, 0);
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
