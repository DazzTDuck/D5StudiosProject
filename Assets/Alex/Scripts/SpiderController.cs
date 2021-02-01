using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderController : MonoBehaviour
{
    Vector3 originalPosition; 
    public GameObject moveCube; 
    public float legMoveSpeed = 7f; 
    public float moveDistance = 0.7f; 
    public float moveStoppingDistance = 0.4f; 
    bool isMoving = false; 
    bool moving = false; 

    private void Start()
    {
        originalPosition = transform.position;  
    }

    private void Update()
    {
        float distanceToMoveCube = Vector3.Distance(transform.position, moveCube.transform.position);
        if ((distanceToMoveCube >= moveDistance && !oppsiteLeg.isItMoving()) || moving)
        {
            moving = true;
            transform.position = Vector3.Lerp(transform.position, moveCube.transform.position + new Vector3(0f, 0.3f, 0f), Time.deltaTime * legMoveSpeed); 
            originalPosition = transform.position; 
            isMoving = true; 

            if (distanceToMoveCube < moveStoppingDistance)
            {
                moving = false; 
            }
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, originalPosition + new Vector3(0f, -0.3f, 0f), Time.deltaTime * legMoveSpeed * 3f); 
            isMoving = false; 
        }
    }

    public bool isItMoving()
    {
        return isMoving;
    }

}
