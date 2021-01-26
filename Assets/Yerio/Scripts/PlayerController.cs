using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [Header("Stats")]
    [SerializeField] float moveSpeed = 4;

    [Header("--Jumping--")]
    [Tooltip("Height of the jump in meters")]
    public float jumpHeight = 4f;
    [Tooltip("the amount of time that needs to acceed so the player can jump again (to prevent spamming the jump)")]
    public float jumpCooldownTime = 0.1f;
    [Tooltip("Transform for the GroundCheck object in the player")]
    public Transform groundCheck;
    [Tooltip("Radius of how far it will check is there is ground below the player")]
    public float groundDistance = 0.5f;
    [Tooltip("LayerMask for the ground")]
    public LayerMask groundLayer;
    [Tooltip("This will show if the player is on the ground or not")]
    public bool isGrounded;

    [Header("--Gravity--")]
    [Tooltip("This is the gravity applied to the player when hes falling down from a jump")]
    public float fallGravityMultiplier = 1.5f;

    [HideInInspector]
    public Rigidbody rb;

    //exposed variables
    [HideInInspector]
    public Vector3 gForceVector;
    [HideInInspector]
    public bool isStationary;

    //private variables
    float jumpCooldownTimer;
    bool jumpCooldown = false;
    bool wantToJump = false;
    Vector3 velocity;
    float horiontal;
    float vertical;

    public override void Attached()
    {
        base.Attached();
        state.SetTransforms(state.PlayerTransform, transform);
        rb = GetComponent<Rigidbody>();
    }

    public override void SimulateOwner()
    {
        base.SimulateOwner();
        Movement();
    }

    private void FixedUpdate()
    {
        Jumping();
        CheckIfGround();
    }

    public void Movement()
    {
        horiontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horiontal, 0, vertical);

        transform.Translate(movement * moveSpeed * BoltNetwork.FrameDeltaTime, Space.Self);
    }

    void Jumping()
    {
        wantToJump = Input.GetButtonDown("Jump");

        //when the cooldown is active add it up with time and if it has exceeded the cooldown time you can jump again
        if (jumpCooldown && isGrounded && !wantToJump)
        {
            jumpCooldownTimer += Time.deltaTime;
            if (jumpCooldownTimer >= jumpCooldownTime)
            {
                jumpCooldownTimer = 0;
                jumpCooldown = false;
            }
        }

        if (wantToJump && isGrounded && !jumpCooldown)
        {
            //calculate the force needed to jump the height given
            var jumpVelocity = Mathf.Sqrt(2 * -Physics.gravity.y * jumpHeight);

            //if player is moving down reset the velocity to zero so it always reaches full height when jumping     
            if (rb.velocity.y < 0)
            {
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            }

            rb.AddForce(0, jumpVelocity, 0, ForceMode.Impulse);
            jumpCooldown = true;
        }

    }
    void CheckIfGround()
    {
        //this checks if the player is standing on the ground
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundLayer);

        //so there is always a little velocity down on the player for when it falls
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

}
