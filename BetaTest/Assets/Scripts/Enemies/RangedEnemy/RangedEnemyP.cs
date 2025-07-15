using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public enum RangedEnemyStateType
{
    Idle,
    Charge,
    Attack,
    Hurt,
    Dead
}

[System.Serializable]
public class RangedEnemyParameter
{
    [Header("��������")]
    public float detectionRange = 8f;
    public float attackRange = 6f;
    public float retreatRange = 3f;
    public Animator animator;
    public Transform target;
    public LayerMask targetLayer;
    
    [Header("��������")]
    public Transform attackPoint;
    public float attackCooldown = 2f;
    public float chargeDuration = 1f;
    public int damage = 15;
    
    [Header("��Ļ����")]
    public GameObject projectilePrefab;
    public GameObject trackingProjectilePrefab;
    public float projectileSpeed = 5f;
    public float projectileLifetime = 3f;
    
    [Header("���ε�Ļ����")]
    public int fanProjectileCount = 5;
    public float fanAngle = 45f;
    
    [Header("״̬���")]
    public bool isCharging = false;
    public bool isAttacking = false;
    public bool isHit = false;
    public bool isDead = false;
    
    [Header("���˲���")]
    public float hurtDuration = 0.5f;
    public float hurtKnockbackForce = 5f;
}

public class RangedEnemyP : MonoBehaviour
{
    [Header("Camera Shake Settings")]
    [SerializeField] private float shakeDuration = 0.15f;
    [SerializeField] private float shakeIntensity = 0.5f;

    // Components
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Rigidbody2D rb;
    private Collider2D col;
    private EnemyLife enemyLife;

    // Health properties - ͨ��EnemyLife�����ȡ
    public int CurrentHealth => enemyLife != null ? enemyLife.CurrentHealth : 0;
    public int MaxHealth => enemyLife != null ? enemyLife.MaxHealth : 0;
    public bool IsDead => enemyLife != null ? enemyLife.IsDead : false;

    public RangedEnemyParameter parameter;
    private IState currentState;
    private Dictionary<RangedEnemyStateType, IState> states = new Dictionary<RangedEnemyStateType, IState>();
    private CinemachineImpulseSource impulseSource;

    // ����ģʽ����
    private bool useTrackingAttack = true; // ����ʹ��׷�ٵ������ε�
    private float lastAttackTime = 0f;

    void Start()
    {
        if (parameter == null)
            parameter = new RangedEnemyParameter();

        parameter.animator = GetComponent<Animator>();

        // ��ȡ�������
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        // ������������
        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.gravityScale = 1f;
        }

        // ���û��Rigidbody2D���Զ����
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;
            rb.gravityScale = 1f;
            rb.drag = 5f;
            Debug.Log("Added Rigidbody2D to RangedEnemy");
        }

        // ���û��Collider2D���Զ����
        if (col == null)
        {
            col = gameObject.AddComponent<CapsuleCollider2D>();
            Debug.Log("Added CapsuleCollider2D to RangedEnemy");
        }

        // ��ȡ�����EnemyLife���
        enemyLife = GetComponent<EnemyLife>();
        if (enemyLife == null)
        {
            enemyLife = gameObject.AddComponent<EnemyLife>();
        }

        // ���ù�����
        if (parameter.attackPoint == null)
        {
            GameObject attackPointObj = new GameObject("AttackPoint");
            attackPointObj.transform.SetParent(transform);
            attackPointObj.transform.localPosition = new Vector3(1f, 0f, 0f);
            parameter.attackPoint = attackPointObj.transform;
        }

        // ��ʼ��״̬��
        states.Add(RangedEnemyStateType.Idle, new RangedEnemyIdleState(this, parameter));
        states.Add(RangedEnemyStateType.Charge, new RangedEnemyChargeState(this, parameter));
        states.Add(RangedEnemyStateType.Attack, new RangedEnemyAttackState(this, parameter));
        states.Add(RangedEnemyStateType.Hurt, new RangedEnemyHurtState(this, parameter));
        states.Add(RangedEnemyStateType.Dead, new RangedEnemyDeadState(this, parameter));

        TransitionState(RangedEnemyStateType.Idle);

        // ��ʼ�������Դ
        impulseSource = GetComponent<CinemachineImpulseSource>();
        
        // ��ȡ������Ⱦ��������ɫ�仯
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    void Update()
    {
        currentState.OnUpdate();
    }

    public void TransitionState(RangedEnemyStateType type)
    {
        if (currentState != null)
            currentState.OnExit();
        currentState = states[type];
        currentState.OnEnter();
    }

    public void FlipTo(Transform target)
    {
        if (target == null)
        {
            Debug.LogWarning("FlipTo called with null target!");
            return;
        }

        Vector3 direction = target.position - transform.position;
        float newScaleX = Mathf.Sign(direction.x) < 0 ? -1f : 1f;
        transform.localScale = new Vector3(newScaleX, 1f, 1f);

        // ���¹�����λ��
        if (parameter.attackPoint != null)
        {
            Vector3 attackPos = parameter.attackPoint.localPosition;
            attackPos.x = Mathf.Abs(attackPos.x) * newScaleX;
            parameter.attackPoint.localPosition = attackPos;
        }
    }

    // ִ��׷�ٵ�����
    public void PerformTrackingAttack()
    {
        if (parameter.target == null || parameter.trackingProjectilePrefab == null) return;

        Vector3 spawnPos = parameter.attackPoint.position;
        Vector2 direction = (parameter.target.position - spawnPos).normalized;

        GameObject projectile = Instantiate(parameter.trackingProjectilePrefab, spawnPos, Quaternion.identity);
        
        // ���õ�����
        Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();
        if (projectileRb == null)
        {
            projectileRb = projectile.AddComponent<Rigidbody2D>();
            projectileRb.gravityScale = 0f;
        }
        
        projectileRb.velocity = direction * parameter.projectileSpeed;

        // ���׷�ٵ��ű�
        TrackingProjectile trackingScript = projectile.GetComponent<TrackingProjectile>();
        if (trackingScript == null)
        {
            trackingScript = projectile.AddComponent<TrackingProjectile>();
        }
        trackingScript.Initialize(parameter.damage, parameter.target, parameter.targetLayer);

        // ȷ������ײ��
        if (projectile.GetComponent<Collider2D>() == null)
        {
            CircleCollider2D collider = projectile.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.3f;
        }

        Destroy(projectile, parameter.projectileLifetime);
        
        Debug.Log("RangedEnemy: Tracking projectile launched");
    }

    // ִ�����ε�Ļ����
    public void PerformFanAttack()
    {
        if (parameter.target == null || parameter.projectilePrefab == null) return;

        Vector3 spawnPos = parameter.attackPoint.position;
        Vector2 baseDirection = (parameter.target.position - spawnPos).normalized;

        float angleStep = parameter.fanAngle / (parameter.fanProjectileCount - 1);
        float startAngle = -parameter.fanAngle / 2;

        for (int i = 0; i < parameter.fanProjectileCount; i++)
        {
            float angle = startAngle + (angleStep * i);
            Vector2 direction = RotateVector(baseDirection, angle);

            GameObject projectile = Instantiate(parameter.projectilePrefab, spawnPos, Quaternion.identity);
            
            // ���õ�����
            Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();
            if (projectileRb == null)
            {
                projectileRb = projectile.AddComponent<Rigidbody2D>();
                projectileRb.gravityScale = 0f;
            }
            
            projectileRb.velocity = direction * parameter.projectileSpeed;

            // �����ͨ������ű�
            RangedProjectile projectileScript = projectile.GetComponent<RangedProjectile>();
            if (projectileScript == null)
            {
                projectileScript = projectile.AddComponent<RangedProjectile>();
            }
            projectileScript.Initialize(parameter.damage, parameter.targetLayer);

            // ȷ������ײ��
            if (projectile.GetComponent<Collider2D>() == null)
            {
                CircleCollider2D collider = projectile.AddComponent<CircleCollider2D>();
                collider.isTrigger = true;
                collider.radius = 0.3f;
            }

            Destroy(projectile, parameter.projectileLifetime);
        }

        Debug.Log("RangedEnemy: Fan attack launched");
    }

    private Vector2 RotateVector(Vector2 vector, float angle)
    {
        float rad = angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        
        return new Vector2(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos
        );
    }

    // ����Ƿ���Թ���
    public bool CanAttack()
    {
        return Time.time - lastAttackTime >= parameter.attackCooldown;
    }

    // ���ù���ʱ��
    public void SetAttackTime()
    {
        lastAttackTime = Time.time;
        useTrackingAttack = !useTrackingAttack; // ���湥��ģʽ
    }

    // ��ȡ��ǰ����ģʽ
    public bool ShouldUseTrackingAttack()
    {
        return useTrackingAttack;
    }

    // ���þ�����ɫ
    public void ResetSpriteColor()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    // ���þ�����ɫ����������Ч����
    public void SetSpriteColor(Color color)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
    }

    #region ���ߺ��˺����
    private void OnTriggerEnter2D(Collider2D other)
    {
        GameObject hero = GameObject.FindGameObjectWithTag("Player");
        
        // �����ҽ�����Ұ
        if (other.CompareTag("Player"))
        {
            parameter.target = other.transform;
            
            // ����Ѿ�����������Ӧ���
            if (parameter.isDead) return;
            
            // ���ݵ�ǰ״̬�;������״̬ת��
            float distance = Vector2.Distance(transform.position, other.transform.position);
            
            if (distance <= parameter.attackRange && CanAttack())
            {
                TransitionState(RangedEnemyStateType.Charge);
            }
            else if (distance <= parameter.detectionRange)
            {
                TransitionState(RangedEnemyStateType.Idle);
            }
        }
        
        // �����ҹ���
        if (other.CompareTag("PlayerAttack"))
        {
            // ����Ѿ�����������Ӧ����
            if (parameter.isDead) return;
            
            AttackHitbox attackHitbox = other.GetComponent<AttackHitbox>();
            if (attackHitbox != null)
            {
                int damage = attackHitbox.damage;
                TakeDamage(damage);

                if (hero != null)
                {
                    if (hero.transform.localRotation.y == 0)
                    {
                        GetComponent<Enemy>()?.GetHit(Vector2.right);
                    }
                    else if (hero.transform.localRotation.y == -1)
                    {
                        GetComponent<Enemy>()?.GetHit(Vector2.left);
                    }
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // ����Ѿ�����������Ӧ����뿪
            if (parameter.isDead) return;
            
            float distance = Vector2.Distance(transform.position, other.transform.position);
            
            if (distance > parameter.detectionRange)
            {
                parameter.target = null;
                TransitionState(RangedEnemyStateType.Idle);
            }
        }
    }
    #endregion

    #region �˺�ϵͳ
    public void TakeDamage(int damage)
    {
        if (enemyLife == null || enemyLife.IsDead || parameter.isDead) return;

        parameter.isHit = true;

        // ������Ļ��
        if (impulseSource != null)
        {
            CamaraShakeManager.Instance.CamaraShake(impulseSource);
        }

        // ʹ��EnemyLife��������˺�
        enemyLife.TakeDamage(damage);
        
        // ����Ƿ�����
        if (enemyLife.IsDead)
        {
            parameter.isDead = true;
            // ��������״̬
            TransitionState(RangedEnemyStateType.Dead);
        }
        else
        {
            // ��������״̬
            TransitionState(RangedEnemyStateType.Hurt);
        }
    }
    #endregion

    private void OnDrawGizmosSelected()
    {
        // ���Ƽ�ⷶΧ
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, parameter.detectionRange);
        
        // ���ƹ�����Χ
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, parameter.attackRange);
        
        // ���ƺ��˷�Χ
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, parameter.retreatRange);
        
        // ���ƹ�����
        if (parameter.attackPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(parameter.attackPoint.position, 0.2f);
        }
    }
}