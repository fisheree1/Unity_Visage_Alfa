using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HeroMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float climbSpeed = 3f;
    [SerializeField] private float ladderDetectionDistance = 0.3f;

    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Double Jump Settings")]
    [SerializeField] private float doubleJumpForce = 8f;
    [SerializeField] private bool doubleJumpUnlocked = false;

    [Header("Slide Settings")]
    [SerializeField] private float slideSpeed = 8f;
    [SerializeField] private float slideDuration = 0.5f;
    [SerializeField] private bool slideUnlocked = false;

    [Header("Capsule Collider Settings")]
    [SerializeField] private CapsuleCollider2D capsuleCollider;
    [SerializeField] private Vector2 slideSize = new Vector2(0.13f, 0.2f);
    [SerializeField] private Vector2 slideOffset = new Vector2(0f, -0.1f);


    // Components
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator anim;
    private HeroAttackController attackController;
    private HeroLife heroLife;

    // Movement state
    private float dirx = 0f;
    private bool isOnLadder = false;
    private bool isClimbing = false;
    private bool isJumping = false;
    private bool hasDoubleJumped = false;
    private bool isSliding = false;
    private MovementState lastState = MovementState.HeroIdle;

    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;

    [field: SerializeField]
    public bool isFacingRight { get; private set; } = true;

    [field: SerializeField] private GameObject _cameraFollowObject;

    private enum MovementState
    {
        HeroIdle,
        HeroRun,
        PlayerJump,
        PlayerFall,
        HeroLadder
    }

    private float _fallSpeedYDampingChangeThreshold;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        attackController = GetComponent<HeroAttackController>();
        heroLife = GetComponent<HeroLife>();
        originalColliderSize = capsuleCollider.size;
        originalColliderOffset = capsuleCollider.offset;

        _fallSpeedYDampingChangeThreshold = CameraManager.instance._fallSpeedYDampingChangeThreshold;
    }

    void Update()
    {
        if (heroLife != null && heroLife.IsDead)
            return;

        dirx = Input.GetAxis("Horizontal");
        float diry = Input.GetAxis("Vertical");

        bool wasJumping = isJumping;
        bool currentlyGrounded = IsGrounded();

        if (wasJumping && currentlyGrounded && rb.velocity.y <= 0.1f)
        {
            isJumping = false;
            hasDoubleJumped = false;
            Debug.Log("Landed - isJumping reset to false, doubleJump reset");
        }

        if (rb.velocity.y < _fallSpeedYDampingChangeThreshold && !CameraManager.instance.IsLerpingYDamping && !CameraManager.instance.LerpedFromPlayerFalling)
        {
            CameraManager.instance.LerpYDamping(true);
        }

        if (rb.velocity.y >= 0f && !CameraManager.instance.IsLerpingYDamping && CameraManager.instance.LerpedFromPlayerFalling)
        {
            CameraManager.instance.LerpYDamping(false);
            CameraManager.instance.LerpedFromPlayerFalling = false;
        }

        HandleClimbing(diry);
        HandleHorizontalMovement();
        HandleJump();
        HandleSliding();
        HandleFacingDirection();
        UpdateAnimationState();
    }

    private void HandleHorizontalMovement()
    {
        bool canMove = attackController == null || attackController.CanMove();
        float speedMultiplier = attackController != null ? attackController.GetMovementSpeedMultiplier() : 1f;

        if (!isClimbing && canMove)
        {
            rb.velocity = new Vector2(dirx * moveSpeed * speedMultiplier, rb.velocity.y);
        }
    }

    private void HandleJump()
    {
        bool canJump = attackController == null || attackController.CanJump();
        bool isGrounded = IsGrounded();

        if (Input.GetButtonDown("Jump") && isGrounded && !isClimbing && !isJumping && canJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            isJumping = true;
            hasDoubleJumped = false;
            Debug.Log("Jump triggered!");
        }
        else if (Input.GetButtonDown("Jump") && !isGrounded && !isClimbing && !hasDoubleJumped && doubleJumpUnlocked && canJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, doubleJumpForce);
            hasDoubleJumped = true;
            Debug.Log("Double Jump triggered!");
        }
        else if (Input.GetButtonDown("Jump") && isClimbing)
        {
            isClimbing = false;
            isOnLadder = false;
            rb.gravityScale = 1f;
            rb.velocity = new Vector2(dirx * moveSpeed, jumpForce * 0.7f);
            isJumping = true;
            hasDoubleJumped = false;
            Debug.Log("Jump off ladder!");
        }
    }

    private void HandleClimbing(float verticalInput)
    {
        bool canClimb = attackController == null || attackController.CanClimb();
        CheckLadderProximity();

        if (isOnLadder && Mathf.Abs(verticalInput) > 0 && canClimb)
        {
            isClimbing = true;
            rb.gravityScale = 0f;
            rb.velocity = new Vector2(0, verticalInput * climbSpeed);
        }
        else if (isClimbing)
        {
            if (!isOnLadder || !canClimb)
            {
                isClimbing = false;
                rb.gravityScale = 1f;
            }
            else if (Mathf.Abs(verticalInput) == 0)
            {
                rb.velocity = new Vector2(0, 0);
            }
        }
        else
        {
            rb.gravityScale = 1f;
        }
    }

    private void HandleSliding()
    {
        if (slideUnlocked && Input.GetKeyDown(KeyCode.LeftShift) && !isSliding && IsGrounded())
        {
            StartCoroutine(Slide());
        }   
    }

    private void HandleFacingDirection()
    {
        if (Mathf.Abs(dirx) > 0.1f)
        {
            bool stateChange = transform.rotation.eulerAngles.y == 180f && dirx > 0f
                             || transform.rotation.eulerAngles.y == 0f && dirx < 0f;

            if (dirx < 0f)
            {
                transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                isFacingRight = false;
            }
            else
            {
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                isFacingRight = true;
            }

            if (stateChange)
            {
                Debug.Log($"Hero facing direction changed: {isFacingRight}");
                _cameraFollowObject.GetComponent<CameraFollowObject>()?.CallTurn();
            }
        }
    }

    private void CheckLadderProximity()
    {
        Vector2 heroCenter = transform.position;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(heroCenter, ladderDetectionDistance);

        bool foundLadder = false;

        foreach (Collider2D col in colliders)
        {
            if (col.gameObject.name.Contains("ClimbMap") || col.CompareTag("Ladder"))
            {
                foundLadder = true;
                break;
            }
        }

        isOnLadder = foundLadder;

        if (!isOnLadder && isClimbing)
        {
            isClimbing = false;
            rb.gravityScale = 1f;
        }
    }

    private void UpdateAnimationState()
    {
        if (heroLife != null && heroLife.IsDead)
            return;
        

        MovementState state;
        if (isSliding)
        {
            state = MovementState.HeroRun; 
            return; 
        }
        else if (isClimbing)
        {
            state = MovementState.HeroLadder;
        }
        else if (IsGrounded())
        {
            // 在地面上，根据水平速度判断
            state = Mathf.Abs(dirx) > 0.1f ? MovementState.HeroRun : MovementState.HeroIdle;
            isJumping = false; // 落地安全归零
        }
        else
        {
            // 不在地面，根据 y 速度判断跳跃或下落
            state = rb.velocity.y > 0.1f ? MovementState.PlayerJump : MovementState.PlayerFall;
        }

        anim.SetInteger("Movements", (int)state);


        if (state != lastState)
        {
            Debug.Log($"Movement State Changed: {lastState} → {state} (IsGrounded: {IsGrounded()}, Velocity.y: {rb.velocity.y:F2})");
            lastState = state;
        }
    }

    private IEnumerator Slide()
    {
        isSliding = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        capsuleCollider.size = slideSize;
        capsuleCollider.offset = slideOffset;

        rb.velocity = new Vector2(isFacingRight ? slideSpeed : -slideSpeed, 0f);

        anim.SetTrigger("Slide"); 

        yield return new WaitForSeconds(slideDuration);

        capsuleCollider.size = originalColliderSize;
        capsuleCollider.offset = originalColliderOffset;

        rb.gravityScale = originalGravity;
        isSliding = false;
    }



    public bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    public bool IsClimbing => isClimbing;
    public bool IsJumping => isJumping;
    public float GetVelocityY() => rb.velocity.y;
    public float GetVelocityX() => rb.velocity.x;
    public bool HasDoubleJumped => hasDoubleJumped;
    public bool IsDoubleJumpUnlocked => doubleJumpUnlocked;

    public void UnlockDoubleJump()
    {
        doubleJumpUnlocked = true;
        Debug.Log("Double Jump skill unlocked!");
    }

    public void LockDoubleJump()
    {
        doubleJumpUnlocked = false;
        hasDoubleJumped = false;
        Debug.Log("Double Jump skill locked!");
    }

    public void UnlockSlide()
    {
        slideUnlocked = true;
        Debug.Log("Slide skill unlocked!");
    }
}
