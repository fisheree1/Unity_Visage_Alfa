using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using Cinemachine;

public enum DadStateType
{
    Idle,
    Patrol,
    Attack,
    Chase,
    Hit,
    Dead
}
[System.Serializable] public class DadParameter
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

public class DadP
: MonoBehaviour
{
    [Header("Camera Shake Settings")]
    [SerializeField] private float shakeDuration = 0.15f;
    [SerializeField] private float shakeIntensity = 0.5f;

    // Components
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Rigidbody2D rb;
    private Collider2D col;
    private EnemyLife enemyLife; // EnemyLife组件引用

    // Health properties - 现在通过EnemyLife组件获取
    public int CurrentHealth => enemyLife != null ? enemyLife.CurrentHealth : 0;
    public int MaxHealth => enemyLife != null ? enemyLife.MaxHealth : 0;
    public bool IsDead => enemyLife != null ? enemyLife.IsDead : false;
    
    public DadParameter parameter;
    private IState currentState;
    private Dictionary<DadStateType, IState> states = new Dictionary<DadStateType, IState>();
    private CinemachineImpulseSource impulseSource;


    // 在动画事件调用的方法


    void Start()
    {
        if (parameter == null)
            parameter = new DadParameter();

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
            Debug.Log("Added Rigidbody2D to Dad");
        }

        // 如果没有Collider2D，自动添加
        if (col == null)
        {
            col = gameObject.AddComponent<CapsuleCollider2D>();
            Debug.Log("Added CapsuleCollider2D to Dad");
        }

        // 获取或添加EnemyLife组件
        enemyLife = GetComponent<EnemyLife>();
        if (enemyLife == null)
        {
            enemyLife = gameObject.AddComponent<EnemyLife>();
        }

        states.Add(DadStateType.Idle, new DadIdleState(this, parameter));
        states.Add(DadStateType.Patrol, new DadPatrolState(this, parameter));
        states.Add(DadStateType.Attack, new DadAttackState(this, parameter));
        states.Add(DadStateType.Chase, new DadChaseState(this, parameter));
        states.Add(DadStateType.Hit, new DadHitState(this, parameter));
        states.Add(DadStateType.Dead, new DadDeadState(this, parameter));
        

        TransitionState(DadStateType.Idle);
        
        //初始化相机震动源
        impulseSource = GetComponent<CinemachineImpulseSource>();


    }
    void Update()
    {
        currentState.OnUpdate();
        
    }

    public void TransitionState(DadStateType type)
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
            if (currentState is DadAttackState)
                TransitionState(DadStateType.Attack); // 如果已经在攻击状态，保持攻击状态
            else if (currentState is DadIdleState || currentState is DadPatrolState || currentState is DadChaseState)
            {
                TransitionState(DadStateType.Chase); // 进入追逐状态
                
            }
            else
            {
                TransitionState(DadStateType.Idle);
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
        float distance = transform.position.x-other.transform.position.x;
        if (other.CompareTag("Player"))
        {
            if (parameter.target != null && (distance <parameter.sightArea) || (distance > -parameter.sightArea))
                TransitionState(DadStateType.Chase);

            else
            {
                parameter.target = null;

                TransitionState(DadStateType.Patrol); // 退出时切换到Idle状态
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
        if (enemyLife == null || enemyLife.IsDead) return; // 已死亡时不再受到伤害
        
        parameter.isHit = true;
        
        // 触发屏幕震动
        CamaraShakeManager.Instance.CamaraShake(impulseSource);

        // 使用EnemyLife组件处理伤害
        enemyLife.TakeDamage(damage);
        
        // 检查是否死亡
        if (enemyLife.IsDead)
        {
            // 死亡状态
            TransitionState(DadStateType.Dead);
        }
        else
        {
            // 受击状态
            TransitionState(DadStateType.Hit);
        }
    }
    
   
    #endregion
    
    
    
    
       

}
