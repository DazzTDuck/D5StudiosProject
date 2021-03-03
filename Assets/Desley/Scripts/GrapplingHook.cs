﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;
using Photon;

public class GrapplingHook : MonoBehaviour
{
    [SerializeField] int ungrapplableLayerIndex;

    [Space, SerializeField] Transform playerTransform;
    [SerializeField] Rigidbody rb;
    [SerializeField] Camera cam;
    [SerializeField] AbilityHandler abilityHandler;
    [SerializeField] LineRenderer lr;
    [SerializeField] Transform grappleGunPoint;

    [Space, SerializeField] float range;
    [SerializeField] float hookSpeed;
    [SerializeField] float stoppingDistance;
    [SerializeField] float jumpForce;
    
    Vector3 direction;
    Vector3 hitPoint;
    float distance;
    bool isGrappling;

    public void ShootGrapplingHook()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, range))
        {
            if (hit.collider.gameObject.layer == ungrapplableLayerIndex)
            {
                abilityHandler.ResetAbility2Timer();
                return;
            }
                

            hitPoint = hit.point;
            direction = (hit.point - playerTransform.position).normalized;

            isGrappling = true;

            DetermineStopDistance(Vector3.Distance(playerTransform.position, hitPoint));
            playerTransform.GetComponent<PlayerController>().GrappleState(isGrappling);
            GetComponent<Scout>().isGrappling = true;
            lr.positionCount = 2;
        }
        else
        {
            abilityHandler.ResetAbility2Timer();
        }
    }

    void DetermineStopDistance(float distance)
    {
        stoppingDistance = distance < 15 ? stoppingDistance = 2f : stoppingDistance = 3f;
    }

    void DrawGrappleRope()
    {
        lr.SetPosition(0, grappleGunPoint.position);
        lr.SetPosition(1, hitPoint);
    }

    private void Update()
    {
        if (isGrappling)
        {
            distance = Vector3.Distance(playerTransform.position, hitPoint);

            if (distance < stoppingDistance)
                StopGrapple(false);
            else if (Input.GetButtonDown("Jump"))
                StopGrapple(true);
                
        }
    }

    private void FixedUpdate()
    {
        if (isGrappling)
            rb.velocity = (direction * hookSpeed * Time.fixedDeltaTime);
    }

    private void LateUpdate()
    {
        if (isGrappling)
        {
            DrawGrappleRope();
        }
    }

    public void StopGrapple(bool stoppedByJump)
    {
        isGrappling = false;
        rb.velocity = new Vector3(0,0,0);
        playerTransform.GetComponent<PlayerController>().GrappleState(isGrappling);
        GetComponent<Scout>().isGrappling = false;
        lr.positionCount = 0;

        if (stoppedByJump)
            rb.AddRelativeForce(0, 0, jumpForce, ForceMode.Impulse);
    }
}
