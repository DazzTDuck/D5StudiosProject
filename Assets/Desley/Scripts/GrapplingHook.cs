using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingHook : MonoBehaviour
{
    [SerializeField] Transform playerTransform;
    [SerializeField] Rigidbody rb;
    [SerializeField] Camera cam;
    [SerializeField] AbilityHandler abilityHandler;

    [Space, SerializeField] float range;
    [SerializeField] float hookSpeed;
    [SerializeField] float stoppingDistance;
    
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
            hitPoint = hit.point;
            direction = (hit.point - playerTransform.position).normalized;

            isGrappling = true;

            DetermineStopDistance(Vector3.Distance(playerTransform.position, hitPoint));
            playerTransform.GetComponent<PlayerController>().GrappleState(isGrappling);
        }
        else
        {
            abilityHandler.ResetAbility2Timer();
        }
    }

    void DetermineStopDistance(float distance)
    {
        stoppingDistance = distance < 15 ? stoppingDistance = 1f : stoppingDistance = 3f;
    }

    private void Update()
    {
        if (isGrappling)
        {
            distance = Vector3.Distance(playerTransform.position, hitPoint);
            if (distance < stoppingDistance || Input.GetButtonDown("Jump"))
                StopGrapple();
        }
    }

    private void FixedUpdate()
    {
        if (isGrappling)
            rb.MovePosition(playerTransform.position + direction * hookSpeed * Time.fixedDeltaTime);
    }

    public void StopGrapple()
    {
        isGrappling = false;
        playerTransform.GetComponent<PlayerController>().GrappleState(isGrappling);
    }
}
