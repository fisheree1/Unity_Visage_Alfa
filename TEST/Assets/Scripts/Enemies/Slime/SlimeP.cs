using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using Cinemachine;

public enum SlimeStateType
{
    Idle,
    Patrol,
    Attack,
    Chase,
    Hit,
    Dead
}
[System.Serializable]
public class SlimeParameter
{
    public float PatrolSpeed;
    public float ChaseSpeed;
    public float IdleTime;
    public Transform[] patrolPoints;
    public Transform[] chasePoints;
    public Animator animator;
    public Transform target;
    public LayerMask targetLayer;
    public Transform attackPoint;
    public float attackArea;
    public float sightArea;
    public int damage = 1; // 攻击伤害
    public float attackCooldown = 0.7f; // 攻击冷却时间
    public bool isHit = false; // 是否被击中

}

public class SlimeP
: MonoBehaviour
{
    [Header("Slime Health")]
    [SerializeField] public int maxHealth = 3;
    [SerializeField] public int currentHealth;

    [Header("Damage Response")]
    [SerializeField] private float damageFlashDuration = 0.2f;

    [Header("Camera Shake Settings")]
    [SerializeField] private float shakeDuration = 0.15f;
    [SerializeField] private float shakeIntensity = 0.5f;

    // Components
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Rigidbody2D rb;
    private Collider2D col;

    // Health properties
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    public SlimeParameter parameter;
    private IState currentState;
    private Dictionary<SlimeStateType, IState> states = new Dictionary<SlimeStateType, IState>();
    private CinemachineImpulseSource impulseSource;


    // 在动画事件调用的方法


    void Start()
    {
        if (parameter == null)
            parameter = new SlimeParameter();

        parameter.animator = GetComponent<Animator>();

        // 获取物理组件
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        // 设置物理属性确保敌人能站立在地面上
        if (rb != null)
        {
            rb.freezeRotation = true; // 防止旋转
            rb.gravityScale = 1f; // 设置重力

        }

        // 如果没有Rigidbody2D，自动添加
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;
            rb.gravityScale = 1f;
            rb.drag = 5f;
            Debug.Log("Added Rigidbody2D to Slime");
        }

        // 如果没有Collider2D，自动添加
        if (col == null)
        {
            col = gameObject.AddComponent<CapsuleCollider2D>();
            Debug.Log("Added CapsuleCollider2D to Slime");
        }

        states.Add(SlimeStateType.Idle, new SlimeIdleState(this, parameter));
        states.Add(SlimeStateType.Patrol, new SlimePatrolState(this, parameter));
        states.Add(SlimeStateType.Attack, new SlimeAttackState(this, parameter));
        states.Add(SlimeStateType.Chase, new SlimeChaseState(this, parameter));
        states.Add(SlimeStateType.Hit, new SlimeHitState(this, parameter));
        states.Add(SlimeStateType.Dead, new SlimeDeadState(this, parameter));


        TransitionState(SlimeStateType.Idle);

        // 初始化血量
        currentHealth = maxHealth;
        //初始化相机震动源
        impulseSource = GetComponent<CinemachineImpulseSource>();


    }
    void Update()
    {
        currentState.OnUpdate();

    }

    public void TransitionState(SlimeStateType type)
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


        float newScaleX = Mathf.Sign(direction.x) < 0 ? 1f : -1f;
        transform.localScale = new Vector3(newScaleX, 1f, 1f);


        Debug.DrawRay(transform.position, direction, Color.yellow, 0.1f);
    }
    #region sight_test
    private void OnTriggerEnter2D(Collider2D other)
    {
        GameObject hero = GameObject.FindGameObjectWithTag("Player");
        // 检测玩家进入视野
        if (other.CompareTag("Player"))
        {
            parameter.target = other.transform;
            if (currentState is SlimeAttackState)
                TransitionState(SlimeStateType.Attack); // 如果已经在攻击状态，保持攻击状态
            else if (currentState is SlimeIdleState || currentState is SlimePatrolState || currentState is SlimeChaseState)
            {
                TransitionState(SlimeStateType.Chase); // 进入追逐状态

            }
            else
            {
                TransitionState(SlimeStateType.Idle);
                return;
            }
        }

        // 检测玩家攻击
        if (other.CompareTag("PlayerAttack"))
        {
            AttackHitbox attackHitbox = other.GetComponent<AttackHitbox>();
            if (attackHitbox != null)
            {
                int damage = attackHitbox.damage;
                TakeDamage(damage);




                if (hero.transform.localRotation.y == 0)
                {

                    GetComponent<Enemy>().GetHit(Vector2.right);
                }
                else if (hero.transform.localRotation.y == -1)
                {

                    Debug.Log(hero.transform.localRotation.y);
                    GetComponent<Enemy>().GetHit(Vector2.left);
                }
            }
            else
            {
                Debug.LogWarning("PlayerAttack object found but no AttackHitbox component!");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        float distance = transform.position.x - other.transform.position.x;
        if (other.CompareTag("Player"))
        {
            if (parameter.target != null && (distance < parameter.sightArea) || (distance > -parameter.sightArea))
                TransitionState(SlimeStateType.Chase);

            else
            {
                parameter.target = null;

                TransitionState(SlimeStateType.Patrol); // �˳�ʱ�л���Idle״̬
            }
        }
    }
    #endregion


    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(parameter.attackPoint.position, parameter.attackArea);
    }
    //受到伤害
    #region Damage System
    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return; // 已死亡时不再受到伤害

        int previousHealth = currentHealth;
        currentHealth -= damage;
        parameter.isHit = true;

        // 触发屏幕震动
        CamaraShakeManager.Instance.CamaraShake(impulseSource);

        // 确保血量不会变为负数
        currentHealth = Mathf.Max(0, currentHealth);
        if (currentHealth <= 0)
        {
            // 死亡状态
            TransitionState(SlimeStateType.Dead);
        }
        else
        {
            // 受击状态
            TransitionState(SlimeStateType.Hit);
        }
    }


    #endregion







}
