using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [Header("Stats")]
    [SerializeField] float moveSpeed = 4;
    [SerializeField] float reducedMovement = 2.5f;
    [SerializeField] float jumpBoost;
    [SerializeField] float nextCrouch = .2f;

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

    [Space, SerializeField] GameObject cam;
    [SerializeField] GameObject weaponCam;
    [SerializeField] Shoot shoot;
    [SerializeField] Shotgun shotgun;

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
    float horizontal;
    float vertical;

    bool isHost;

    bool canCrouch = true;
    bool waitingForCourotine = false;
    bool isCrouching;

    public override void Attached()
    {
        base.Attached();
        state.SetTransforms(state.PlayerTransform, transform);
        if (entity.IsOwner)
        {
            state.IsHost = isHost;
            state.AddCallback("IsHost", HostCallback);
        }
        rb = GetComponent<Rigidbody>();
    }

    public override void SimulateOwner()
    {
        if (entity.IsOwner)
            Movement();
    }

    private void Update()
    {
        CheckIfGround();
        Jumping();
    }

    void HostCallback()
    {
        isHost = state.IsHost;
    }

    public void SetHost()
    {
        if (entity.IsOwner)
            state.IsHost = true;
    }

    public bool GetIfHost()
    {
        return state.IsHost;
    }

    public void Movement()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontal, 0, vertical);
        float finalMoveSpeed = 0;

        if (!state.IsDead && isGrounded)
            finalMoveSpeed = moveSpeed;
        if (!state.IsDead && !isGrounded)
            finalMoveSpeed = moveSpeed / reducedMovement;

        if (!state.IsDead)
            transform.Translate(movement * finalMoveSpeed * BoltNetwork.FrameDeltaTime, Space.Self);

        isCrouching = Input.GetButton("Crouch");

        if (!waitingForCourotine)
        {
            Crouch();
        }
    }

    void Crouch()
    {
        if (isCrouching && !state.IsDead && canCrouch)
        {
            cam.GetComponent<PlayerCamera>().Crouch(true);
            weaponCam.GetComponent<PlayerCamera>().Crouch(true);
            CrouchMoveSpeed(true);
            if (shoot)
                shoot.ReduceRecoil(true);
            else if (shotgun)
                shotgun.ReduceSpread(true);
                    

            canCrouch = false;
        }
        else if (!isCrouching && !state.IsDead && !canCrouch)
        {
            cam.GetComponent<PlayerCamera>().Crouch(false);
            weaponCam.GetComponent<PlayerCamera>().Crouch(false);
            CrouchMoveSpeed(false);
            if(shoot)
                shoot.ReduceRecoil(false);
            else if (shotgun)
                shotgun.ReduceSpread(false);

            StartCoroutine(WaitForCrouch(nextCrouch));
        }
    }

    void Jumping()
    {
        if (!state.IsDead)
            wantToJump = Input.GetButtonDown("Jump");

        //when the cooldown is active add it up with time and if it has exceeded the cooldown time you can jump again
        if (jumpCooldown && isGrounded && !wantToJump && entity.IsOwner)
        {
            jumpCooldownTimer += Time.deltaTime;
            if (jumpCooldownTimer >= jumpCooldownTime)
            {
                jumpCooldownTimer = 0;
                jumpCooldown = false;
            }
        }

        if (wantToJump && isGrounded && !jumpCooldown && entity.IsOwner)
        {
            //calculate the force needed to jump the height given
            var jumpVelocityY = Mathf.Sqrt(2 * -Physics.gravity.y * jumpHeight);
            var jumpVelocityX = jumpBoost * horizontal;
            var jumpVelocityZ = jumpBoost * vertical;

            //if player is moving down reset the velocity to zero so it always reaches full height when jumping     
            if (rb.velocity.y < 0)
            {
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            }

            rb.AddRelativeForce(jumpVelocityX, jumpVelocityY, jumpVelocityZ, ForceMode.Impulse);
            jumpCooldown = true;
        }

    }
    void CheckIfGround()
    {
        //this checks if the player is standing on the ground
        if (entity.IsOwner)
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundLayer);

        //so there is always a little velocity down on the player for when it falls
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    public void CrouchMoveSpeed(bool crouching)
    {
        if (crouching)
            moveSpeed /= 2;
        else
            moveSpeed *= 2;
    }

    public IEnumerator WaitForCrouch(float time)
    {
        waitingForCourotine = true;
        yield return new WaitForSeconds(time);

        canCrouch = true;
        waitingForCourotine = false;

        StopCoroutine(nameof(WaitForCrouch));
    }
}

