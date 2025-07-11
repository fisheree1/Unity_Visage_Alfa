using System.Collections;
using UnityEngine;

public class HeroAttackController : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackDuration = 0.5f;
    [SerializeField] private float upAttackDuration = 0.8f;
    [SerializeField] private float downAttackDuration = 0.6f;
    [SerializeField] private float specialAttackDuration = 1.2f;
    
    [Header("Attack Cooldowns")]
    [SerializeField] private float attackCooldown = 0.2f;
    [SerializeField] private float upAttackCooldown = 0.8f;
    [SerializeField] private float downAttackCooldown = 0.8f;
    [SerializeField] private float specialAttackCooldown = 3.0f;
    
    [Header("Combo Settings")]
    [SerializeField] private int maxComboCount = 3;
    [SerializeField] private float comboResetTime = 2.0f;
    
    // Components
    private Animator anim;
    private HeroMovement heroMovement;
    private Rigidbody2D rb;
    
    // Attack Hitboxes
    private GameObject basicAttackHitbox;
    private GameObject specialAttackHitbox;
    private GameObject upAttackHitbox;
    private GameObject downAttackHitbox;
    
    // Attack State
    public enum AttackType { None, Basic, UpAttack, DownAttack, Special }
    private AttackType currentAttackType = AttackType.None;
    private bool isAttacking = false;
    
    // Combo system
    private int comboCount = 0;
    private float comboResetTimer = 0f;
    
    // Cooldown timers
    private float attackTimer = 0f;
    private float upAttackTimer = 0f;
    private float downAttackTimer = 0f;
    private float specialAttackTimer = 0f;
    
    // Properties
    public bool IsAttacking => isAttacking;
    public AttackType CurrentAttackType => currentAttackType;
    public int ComboCount => comboCount;
    
    void Start()
    {
        anim = GetComponent<Animator>();
        heroMovement = GetComponent<HeroMovement>();
        rb = GetComponent<Rigidbody2D>();
        
        // 查找攻击碰撞盒
        basicAttackHitbox = transform.Find("AttackHitbox")?.gameObject;
        specialAttackHitbox = transform.Find("SpecialHitbox")?.gameObject;
        upAttackHitbox = transform.Find("UpAttackHitbox")?.gameObject;
        downAttackHitbox = transform.Find("DownHitbox")?.gameObject;
        
    }
    
    void Update()
    {
        // 更新冷却时间
        UpdateCooldowns();
        
        // 更新连击重置计时器
        UpdateComboTimer();
        
        // 处理攻击输入
        if (!heroMovement.IsClimbing && !isAttacking)
        {
            HandleAttackInput();
        }
    }
    
    private void UpdateCooldowns()
    {
        if (attackTimer > 0) attackTimer -= Time.deltaTime;
        if (upAttackTimer > 0) upAttackTimer -= Time.deltaTime;
        if (downAttackTimer > 0) downAttackTimer -= Time.deltaTime;
        if (specialAttackTimer > 0) specialAttackTimer -= Time.deltaTime;
    }
    
    private void UpdateComboTimer()
    {
        if (comboCount > 0 && !isAttacking)
        {
            comboResetTimer -= Time.deltaTime;
            if (comboResetTimer <= 0)
            {
                ResetCombo();
            }
        }
    }
    
    private void HandleAttackInput()
    {
        bool wPressed = Input.GetKey(KeyCode.W);
        bool sPressed = Input.GetKey(KeyCode.S);
        bool jPressed = Input.GetKeyDown(KeyCode.J);
        
        if (!jPressed) return;
        
        
        // 向上攻击 - W+J 在地面或梯子上
        if (wPressed && !heroMovement.IsClimbing && upAttackTimer <= 0)
        {
            TriggerUpAttack();
        }
        // 向下攻击 - S+J 在空中
        else if (sPressed && !heroMovement.IsGrounded() && downAttackTimer <= 0)
        {
            TriggerDownAttack();
        }
        // 普通攻击或特殊攻击 - 单独按J键
        else if (!wPressed && !sPressed && attackTimer <= 0)
        {
            // 如果已经连击3次，下一次触发特殊攻击
            if (comboCount >= maxComboCount && specialAttackTimer <= 0)
            {
                TriggerSpecialAttack();
            }
            else
            {
                TriggerBasicAttack();
            }
        }
    }
    
    private void TriggerBasicAttack()
    {
        currentAttackType = AttackType.Basic;
        isAttacking = true;
        StopHorizontalMovement();
        attackTimer = attackCooldown;
        
        // 增加连击数
        comboCount++;
        comboResetTimer = comboResetTime;
        
        anim.SetTrigger("Attack");
        
        // 激活基础攻击碰撞盒
        if (basicAttackHitbox != null)
        {
            StartCoroutine(ActivateHitboxTemporarily(basicAttackHitbox, 0.1f, 0.3f));
        }
        
        // 限制最大连击数
        if (comboCount >= maxComboCount)
        {
            comboCount = maxComboCount;
        }
                
        StartCoroutine(AttackCoroutine(attackDuration));
    }
    
    private void TriggerUpAttack()
    {
        currentAttackType = AttackType.UpAttack;
        isAttacking = true;
        upAttackTimer = upAttackCooldown;
        
        anim.SetTrigger("UpAttack");
        
        // 使用专门的向上攻击碰撞盒
        if (upAttackHitbox != null)
        {
            StartCoroutine(ActivateHitboxTemporarily(upAttackHitbox, 0.1f, 0.4f));
        }       
        
        StartCoroutine(AttackCoroutine(upAttackDuration));
    }
    
    private void TriggerDownAttack()
    {
        currentAttackType = AttackType.DownAttack;
        isAttacking = true;

        downAttackTimer = downAttackCooldown;
        
        anim.SetTrigger("DownAttack");
        
        // 使用专门的向下攻击碰撞盒
        if (downAttackHitbox != null)
        {
            StartCoroutine(ActivateHitboxTemporarily(downAttackHitbox, 0.1f, 0.4f));
        }
        
        StartCoroutine(AttackCoroutine(downAttackDuration));
    }
    
    private void TriggerSpecialAttack()
    {
        currentAttackType = AttackType.Special;
        isAttacking = true;
        StopHorizontalMovement();
        specialAttackTimer = specialAttackCooldown;
        
        anim.SetTrigger("SpecialAttack");
        
        // 激活特殊攻击碰撞盒
        if (specialAttackHitbox != null)
        {
            StartCoroutine(ActivateHitboxTemporarily(specialAttackHitbox, 0.2f, 0.5f));
        }
        
        // 重置连击数
        ResetCombo();
        
        StartCoroutine(AttackCoroutine(specialAttackDuration));
    }
    
    // 临时激活攻击碰撞盒的协程
    private IEnumerator ActivateHitboxTemporarily(GameObject hitbox, float startDelay, float duration)
    {
        if (hitbox == null) yield break;
        
        // 等待攻击动画开始
        yield return new WaitForSeconds(startDelay);
        
        // 激活攻击碰撞盒 - 使用AttackHitbox组件
        AttackHitbox attackHitbox = hitbox.GetComponent<AttackHitbox>();
        if (attackHitbox != null)
        {
            attackHitbox.SetHitboxActive(true);
            Debug.Log("Attack hitbox activated via AttackHitbox component");
        }  
        // 等待攻击持续时间
        yield return new WaitForSeconds(duration);
        
        // 关闭攻击碰撞盒
        if (attackHitbox != null)
        {
            attackHitbox.SetHitboxActive(false);
            Debug.Log("Attack hitbox deactivated via AttackHitbox component");
        }

    }

    private void StopHorizontalMovement()
    {
        if (rb != null)
            rb.velocity = new Vector2(0, rb.velocity.y);
    }
    
    private void ResetCombo()
    {
        comboCount = 0;
        comboResetTimer = 0f;
        Debug.Log("Combo reset!");
    }
    
    private IEnumerator AttackCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        
        isAttacking = false;
        currentAttackType = AttackType.None;
        
        Debug.Log($"Attack finished!");
    }
    
    
    // 公共方法供其他脚本调用
    public bool CanMove()
    {
        return !isAttacking;
    }
    public bool CanJump()
    {
        // 只有在攻击过程中才不能跳跃，攻击结束后立即可以跳跃
        return !isAttacking;
    }
    public bool CanClimb()
    {
        // 攻击时不能攀爬
        return !isAttacking;
    }
    
    public float GetMovementSpeedMultiplier()
    {
        if (!isAttacking) return 1f;
        
        switch (currentAttackType)
        {
            case AttackType.Basic:
                return 0.3f; // 基础攻击时移动速度降低
            case AttackType.UpAttack:
            case AttackType.DownAttack:
            case AttackType.Special:
                return 0f; // 其他攻击时不能移动
            default:
                return 1f;
        }
    }
}
