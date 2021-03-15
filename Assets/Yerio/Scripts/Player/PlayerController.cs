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

    [Space, SerializeField] Animator characterAnimator;
    [Space, SerializeField] PauseMenuHandler pauseMenuHandler;
    [Space, SerializeField] GameObject cam;
    [SerializeField] GameObject weaponCam;
    [SerializeField] GameObject hudCanvas;
    [SerializeField] Shoot shoot;
    [SerializeField] Scout shotgun;

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

    [Space, SerializeField] float crouchRecoilDivider;
    bool canCrouch = true;
    bool waitingForCoroutine = false;
    [HideInInspector] public bool isCrouching;

    bool isGrappling;
    public bool shieldClosed;

    public bool isStimmed;

    WaitForHostScreen waitForHostScreen;

    [Space, SerializeField] string[] tags;

    public override void Attached()
    {
        base.Attached();
        state.SetTransforms(state.PlayerTransform, transform);
        state.SetAnimator(characterAnimator);
        if (entity.IsOwner)
        {
            state.IsHost = isHost;
            state.IsReady = false;
            state.IsWalking = false;
            state.AddCallback("IsHost", HostCallback);
        }
        rb = GetComponent<Rigidbody>();
        waitForHostScreen = GetComponent<WaitForHostScreen>();
    }

    public override void SimulateOwner()
    {
        if (entity.IsOwner && !isGrappling)
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

        if (!state.IsDead && !pauseMenuHandler.GetIfPaused() && !state.IsStunned)
            transform.Translate(movement * finalMoveSpeed * BoltNetwork.FrameDeltaTime, Space.Self);

        state.IsWalking = movement != Vector3.zero && !state.IsDead && !state.IsStunned && !pauseMenuHandler.GetIfPaused() && isGrounded;

        isCrouching = Input.GetButton("Crouch");

        if (!waitingForCoroutine)
        {
            Crouch();
        }

        state.Animator.SetFloat("velocityX", horizontal);
        state.Animator.SetFloat("velocityZ", vertical);
        state.Animator.SetBool("IsCrouching", isCrouching);
    }

    public void GrappleState(bool state)
    {
        isGrappling = state;
    }

    void Crouch()
    {
        if(!state.IsDead && !pauseMenuHandler.GetIfPaused())
        {
            if (isCrouching && canCrouch && !shieldClosed && !state.IsStunned)
            {
                ChangeCrouch(true);
                canCrouch = false;
            }
            else if (!isCrouching && !canCrouch)
            {
                ChangeCrouch(false);
                StartCoroutine(WaitForCrouch(nextCrouch));
            }
        }
    }

    void ChangeCrouch(bool state)
    {
        cam.GetComponent<PlayerCamera>().Crouch(state);
        weaponCam.GetComponent<PlayerCamera>().Crouch(state);

        if (!isStimmed)
        {
            if (shoot)
                shoot.CrouchRecoil(state, crouchRecoilDivider);
            else if (shotgun)
                shotgun.ReduceSpread(state);
        }

        if (state)
            moveSpeed /= 2;
        else
            moveSpeed *= 2;
    }

    IEnumerator WaitForCrouch(float time)
    {
        waitingForCoroutine = true;

        yield return new WaitForSeconds(time);

        canCrouch = true;
        waitingForCoroutine = false;

        StopCoroutine(nameof(WaitForCrouch));
    }

    void Jumping()
    {
        if (!state.IsDead && !pauseMenuHandler.GetIfPaused())
            wantToJump = Input.GetButtonDown("Jump") && !state.IsStunned;

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


    //Ability stuff
    public void EmpowerSpeed(bool started, float walkSpeed)
    {
        moveSpeed = !started ? moveSpeed *= walkSpeed : moveSpeed /= walkSpeed;
    }

    public void StartStun(float time) { StartCoroutine(Stunned(time)); }
    IEnumerator Stunned(float time)
    {
        if (entity.IsOwner)
            state.IsStunned = true;
        hudCanvas.GetComponent<AbilityHandler>().PlayerStunned(time);

        yield return new WaitForSeconds(time);

        if (entity.IsOwner)
            state.IsStunned = false;

        StopCoroutine(nameof(Stunned));
    }

    public void StartPowerUp(float time) { StartCoroutine(PowerUp(time)); }
    IEnumerator PowerUp(float time)
    {
        if (entity.IsOwner)
            state.IsPoweredUp = true;

        yield return new WaitForSeconds(time);

        if (entity.IsOwner)
            state.IsPoweredUp = false;

        StopCoroutine(nameof(PowerUp));
    }

    public void StartBleed(float time) { StartCoroutine(InflictBleed(time)); }
    IEnumerator InflictBleed(float time)
    {
        if(entity.IsOwner)
            state.CanBleedEnemies = true;

        yield return new WaitForSeconds(time);

        if (entity.IsOwner)
            state.CanBleedEnemies = false;

        StopCoroutine(nameof(InflictBleed));
    }


    //Connection stuff
    public void SetConnectionID(string id)
    {
        if (entity.IsOwner)
        {
            state.ConnectionID = id;
        }
    }

    public void SetTagForServer()
    {
        StartCoroutine(WaitForTag(1));    
    }

    public void SetTeamTag(int index)
    {
        if (entity.IsOwner)
        {
            state.PlayerTeamTag = tags[index];
            tag = state.PlayerTeamTag;
        }

        var request = TeamTagEvent.Create();
        request.Send();
    }

    IEnumerator WaitForTag(float time)
    {
        yield return new WaitForSeconds(time);

        tag = state.PlayerTeamTag;

        yield return new WaitForSeconds(0.2f);

        GetComponent<Health>().SetTags();
        if (GetComponent<Support>()) { GetComponent<Support>().SetTags(); }
        else if (GetComponentInChildren<Tank>()) { GetComponentInChildren<Tank>().SetTags(); }
        else if (GetComponentInChildren<Shoot>()) { GetComponentInChildren<Shoot>().SetTags(); }
        else if (GetComponentInChildren<Scout>()) { GetComponentInChildren<Scout>().SetTags(); }

        StopCoroutine(nameof(WaitForTag));
    }

    public void SetReady() { if (entity.IsOwner){ state.IsReady = true; waitForHostScreen.ReadyRequest(); } }
    public bool GetIfReady() { return state.IsReady; }
}

