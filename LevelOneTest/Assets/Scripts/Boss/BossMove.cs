using System.Collections;
using UnityEngine;

public class BossMove : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float rollSpeed = 7f;
    [SerializeField] private float rollDistance = 4f;

    [Header("Detection Settings")]
    [SerializeField] private float detectionRange = 12f;
    [SerializeField] private float closeRange = 6f;
    [SerializeField] private float attackRange = 2.5f;
    [SerializeField] private float magicRange = 8f;

    [Header("AI Behavior")]
    [SerializeField] private float idleTime = 0.5f; // 减少idle时间
    [SerializeField] private float actionCooldown = 0.3f; // 减少行动冷却
    [SerializeField] private float crouchTime = 0.5f; // 减少蹲下时间

    [Header("Distance-Based Probabilities")]
    [Range(0, 100)]
    [SerializeField] private int farRollChance = 40;
    [Range(0, 100)]
    [SerializeField] private int farMagicChance = 60;
    [Range(0, 100)]
    [SerializeField] private int midRangeRunChance = 70;
    [Range(0, 100)]
    [SerializeField] private int midRangeMagicChance = 30;

    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayer = -1;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;

    // Components
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private Animator anim;
    private BossAttack attackController;
    private BossLife bossLife;

    // Target
    private Transform player;

    // State
    private BossMovementState currentState = BossMovementState.Idle;
    private bool isFacingRight = true;
    private float actionTimer = 0f;
    private float stateTimer = 0f;
    private bool isGrounded = true;

    // State flags
    private bool isAttacking = false;
    private bool isHurt = false;
    private bool isCrouching = false;
    private bool isCastingMagic = false;
    private bool isRolling = false;

    private enum BossMovementState
    {
        Idle,
        Run,
        Roll,
        Crouch,
        CastMagic
    }

    // Properties
    public bool IsFacingRight => isFacingRight;
    public bool IsAttacking => isAttacking;
    public bool IsHurt => isHurt;
    public bool IsCrouching => isCrouching;
    public bool IsCastingMagic => isCastingMagic;
    public bool IsRolling => isRolling;
    public Transform Player => player;
    public float GetDistanceToPlayer() => player ? Vector2.Distance(transform.position, player.position) : float.MaxValue;

    void Start()
    {
        InitializeComponents();
        FindPlayer();
        SetupGroundCheck();
    }

    void Update()
    {
        if (bossLife != null && bossLife.IsDead)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        UpdateTimers();
        CheckGrounded();
        
        if (player != null)
        {
            UpdateFacingDirection();
            HandleAI();
        }

        UpdateAnimationState();
    }

    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();
        attackController = GetComponent<BossAttack>();
        bossLife = GetComponent<BossLife>();
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("Boss: No Player found with 'Player' tag!");
        }
    }

    private void SetupGroundCheck()
    {
        if (groundCheck == null)
        {
            groundCheck = new GameObject("GroundCheck").transform;
            groundCheck.SetParent(transform);
            groundCheck.localPosition = new Vector3(0, -boxCollider.bounds.extents.y - 0.1f, 0);
        }
    }

    private void UpdateTimers()
    {
        if (actionTimer > 0) actionTimer -= Time.deltaTime;
        if (stateTimer > 0) stateTimer -= Time.deltaTime;
    }

    private void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void UpdateFacingDirection()
    {
        if (player == null || isAttacking || isRolling) return;

        bool shouldFaceRight = player.position.x > transform.position.x;
        if (shouldFaceRight != isFacingRight)
        {
            Flip();
        }
    }

    private void HandleAI()
    {
        // 调试信息
        if (Time.frameCount % 60 == 0) // 每秒打印一次
        {
            Debug.Log($"Boss AI State - Hurt: {isHurt}, Attacking: {isAttacking}, Crouching: {isCrouching}, Magic: {isCastingMagic}, Rolling: {isRolling}, ActionTimer: {actionTimer:F2}");
        }

        // 如果处于特殊状态，不进行AI决策
        if (isHurt || isAttacking || isCrouching || isCastingMagic)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            return;
        }

        // 如果正在翻滚，不要重置速度，让翻滚继续
        if (isRolling)
        {
            return;
        }

        // 如果冷却时间未结束，保持当前状态
        if (actionTimer > 0)
        {
            if (currentState == BossMovementState.Idle)
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
            return;
        }

        float distanceToPlayer = GetDistanceToPlayer();
        if (Time.frameCount % 120 == 0) // 每两秒打印一次
        {
            Debug.Log($"Boss deciding action - Distance to player: {distanceToPlayer:F2}");
        }
        DecideAction(distanceToPlayer);
    }

    private void DecideAction(float distance)
    {
        // 在攻击范围内：直接攻击
        if (distance <= attackRange)
        {
            StartCoroutine(PerformMeleeAttack());
        }
        // 在近距离范围内：跑向玩家
        else if (distance <= closeRange)
        {
            SetState(BossMovementState.Run);
            MoveTowardsPlayer();
        }
        // 在魔法范围内：根据概率选择行动
        else if (distance <= magicRange)
        {
            int decision = Random.Range(0, 100);
            if (decision < midRangeRunChance)
            {
                SetState(BossMovementState.Run);
                MoveTowardsPlayer();
            }
            else
            {
                StartCoroutine(PerformCrouchAndMagic());
            }
        }
        // 在检测范围内但距离较远：根据概率选择翻滚接近或魔法攻击
        else if (distance <= detectionRange)
        {
            int decision = Random.Range(0, 100);
            if (decision < farRollChance)
            {
                StartCoroutine(PerformRoll());
            }
            else if (decision < farRollChance + farMagicChance)
            {
                StartCoroutine(PerformCrouchAndMagic());
            }
            else
            {
                // 保持Idle状态
                SetState(BossMovementState.Idle);
                rb.velocity = new Vector2(0, rb.velocity.y);
                actionTimer = idleTime;
            }
        }
        // 超出检测范围：Idle
        else
        {
            SetState(BossMovementState.Idle);
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    private void MoveTowardsPlayer()
    {
        if (player == null) return;

        float direction = player.position.x > transform.position.x ? 1f : -1f;
        rb.velocity = new Vector2(direction * moveSpeed, rb.velocity.y);
    }

    private IEnumerator PerformMeleeAttack()
    {
        isAttacking = true;
        rb.velocity = Vector2.zero;

        // 触发攻击动画
        anim.SetTrigger("isAttack");

        // 执行攻击
        if (attackController != null)
            attackController.PerformMeleeAttack();

        yield return new WaitForSeconds(0.5f); // 进一步减少攻击持续时间

        isAttacking = false;
        SetState(BossMovementState.Idle);
        actionTimer = actionCooldown * 0.3f; // 进一步减少攻击后的冷却时间
    }

    private IEnumerator PerformRoll()
    {
        isRolling = true;
        SetState(BossMovementState.Roll);

        float rollDirection = isFacingRight ? 1f : -1f;
        float currentRollSpeed = this.rollSpeed;
        
        // 检查前方是否有障碍物，如果有则降低速度
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right * rollDirection, rollDistance, groundLayer);
        if (hit.collider != null)
        {
            currentRollSpeed *= 0.5f; // 减速
        }

        // 确保Rigidbody2D是Dynamic类型
        if (rb != null)
        {
            if (rb.bodyType != RigidbodyType2D.Dynamic)
            {
                Debug.LogWarning("Boss Rigidbody2D should be set to Dynamic for rolling to work!");
                rb.bodyType = RigidbodyType2D.Dynamic; // 自动设置为Dynamic
            }
            
            // 设置翻滚速度 - 不重置y轴速度，保持重力效果
            rb.velocity = new Vector2(rollDirection * currentRollSpeed, rb.velocity.y);
            Debug.Log($"Boss rolling with velocity: {rb.velocity}");
        }

        // 持续翻滚一段时间
        float rollTime = 0.4f; // 减少翻滚时间
        float elapsedTime = 0f;
        
        while (elapsedTime < rollTime && isRolling)
        {
            // 在翻滚过程中保持速度
            if (rb != null)
            {
                rb.velocity = new Vector2(rollDirection * currentRollSpeed, rb.velocity.y);
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 翻滚结束
        isRolling = false;
        if (rb != null)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        SetState(BossMovementState.Idle);
        actionTimer = actionCooldown * 0.3f; // 减少翻滚后的冷却时间
    }

    private IEnumerator PerformCrouchAndMagic()
    {
        // 蹲下阶段
        isCrouching = true;
        SetState(BossMovementState.Crouch);
        rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(crouchTime);

        // 魔法攻击阶段
        isCrouching = false;
        isCastingMagic = true;
        SetState(BossMovementState.CastMagic);

        if (attackController != null)
            attackController.PerformMagicAttack();

        yield return new WaitForSeconds(1.0f); // 减少魔法攻击持续时间

        isCastingMagic = false;
        SetState(BossMovementState.Idle);
        actionTimer = actionCooldown * 1.5f; // 减少魔法攻击后的冷却时间
    }

    public void TakeHurt()
    {
        if (isHurt || (bossLife != null && bossLife.IsDead))
            return;

        StartCoroutine(HurtSequence());
    }

    private IEnumerator HurtSequence()
    {
        // 首先记录当前状态，但不要停止所有协程
        var previousState = currentState;
        
        // 只重置状态标志，不停止所有协程
        ResetAllStateFlags();

        isHurt = true;
        rb.velocity = Vector2.zero;

        // 触发受伤动画 - 确保trigger被正确设置
        if (anim != null)
        {
            anim.SetTrigger("isHurt");
            Debug.Log("Boss hurt animation triggered");
        }

        // 等待受伤动画播放完成
        yield return new WaitForSeconds(0.5f);

        // 清除受伤状态
        isHurt = false;
        
        // 强制回到Idle状态
        SetState(BossMovementState.Idle);
        
        // 重置动画参数确保状态正确
        if (anim != null)
        {
            anim.SetInteger("BossMoveState", (int)BossMovementState.Idle);
            Debug.Log("Boss animation state reset to Idle");
        }
        
        // 重置冷却时间，让Boss可以立即开始新的行动
        actionTimer = 0f;
        
        Debug.Log("Boss hurt sequence completed, returning to Idle, AI should resume");
    }

    private void ResetAllStateFlags()
    {
        isAttacking = false;
        isCrouching = false;
        isCastingMagic = false;
        isRolling = false;
    }

    private void SetState(BossMovementState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            stateTimer = 0f;
        }
    }

    private void UpdateAnimationState()
    {
        if (bossLife != null && bossLife.IsDead)
            return;

        anim.SetInteger("BossMoveState", (int)currentState);
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        spriteRenderer.flipX = !isFacingRight;
    }

    private void OnDrawGizmosSelected()
    {
        // 绘制检测范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = new Color(1f, 0.5f, 0f); // Orange color
        Gizmos.DrawWireSphere(transform.position, closeRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, magicRange);

        // 绘制地面检测
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
