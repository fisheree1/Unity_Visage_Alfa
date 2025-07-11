using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MGoBlinStateType
{
    Idle,
    Patrol,
    Attack,
    Chase,
    Damaged,
    Dead,
    DashAtk
}
[System.Serializable] public class MGoBlinParameter
{
    public float PatrolSpeed;
    public float ChaseSpeed;
    public float DashSpeed;
    public float IdleTime;
    public Transform[] patrolPoints;
    public Transform[] chasePoints;
    public Animator animator;
    public Transform target;
    public LayerMask targetLayer;
    public Transform attackPoint;
    public float attackArea;
    public float sightArea;
    public int attackCount = 0; // 攻击计数器


}

public class MGoBlinP
: MonoBehaviour
{
    [Header("Spider Health")]
    [SerializeField] public int maxHealth = 3;
    [SerializeField] public int currentHealth;

    [Header("Damage Response")]
    [SerializeField] private float damageFlashDuration = 0.2f;
    [SerializeField] private Color damageFlashColor = Color.red;

    // Components
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Rigidbody2D rb;
    private Collider2D col;

    // Health properties
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => currentHealth <= 0;
    public MGoBlinParameter parameter;
    private IState currentState;
    private Dictionary<MGoBlinStateType, IState> states = new Dictionary<MGoBlinStateType, IState>();
    

    // 在动画事件调用的方法
    

    void Start()
    {
        if (parameter == null)
            parameter = new MGoBlinParameter();

        parameter.animator = GetComponent<Animator>();

        // 获取物理组件
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        // 设置物理属性确保敌人能站立在地面上
        if (rb != null)
        {
            rb.freezeRotation = true; // 防止旋转
            rb.gravityScale = 1f; // 设置重力
            rb.drag = 5f; // 设置阻力，防止滑动
        }

        // 如果没有Rigidbody2D，自动添加
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;
            rb.gravityScale = 1f;
            rb.drag = 5f;
            Debug.Log("Added Rigidbody2D to Spider");
        }

        // 如果没有Collider2D，自动添加
        if (col == null)
        {
            col = gameObject.AddComponent<CapsuleCollider2D>();
            Debug.Log("Added CapsuleCollider2D to Spider");
        }

        states.Add(MGoBlinStateType.Idle, new MGoBlinIdleState(this, parameter));
        states.Add(MGoBlinStateType.Patrol, new MGoBlinPatrolState(this, parameter));
        states.Add(MGoBlinStateType.Attack, new MGoBlinAttackState(this, parameter));
        states.Add(MGoBlinStateType.Chase, new MGoBlinChaseState(this, parameter));
        states.Add(MGoBlinStateType.Damaged, new MGoBlinDamagedState(this, parameter));
        states.Add(MGoBlinStateType.Dead, new MGoBlinDeadState(this, parameter));
        states.Add(MGoBlinStateType.DashAtk, new MGoBlinDashAtkState(this, parameter));

        TransitionState(MGoBlinStateType.Idle);
        
        // 初始化血量
        currentHealth = maxHealth;

        // 获取精灵渲染器
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        else
        {
            Debug.LogError("SpriteRenderer not found on Spider!");
        }

        Debug.Log($"Spider initialized: Health={currentHealth}/{maxHealth}, Physics components added");
        
        // 添加敌人伤害组件，能对Hero造成伤害
        
    }
    void Update()
    {
        currentState.OnUpdate();
    }

    public void TransitionState(MGoBlinStateType type)
    {
        if (currentState != null)
            currentState.OnExit();
        currentState = states[type];
        currentState.OnEnter(); 
    }

    public void FlipTo(Transform target)
    {
        // �ؼ��޸������ӿ�ֵ���
        if (target == null)
        {
            Debug.LogWarning("FlipTo called with null target!");
            return;
        }

        Vector3 direction = target.position - transform.position;

        // �Ż���ʹ����Ԫ��������߼�
        float newScaleX = Mathf.Sign(direction.x) < 0 ? -1f : 1f;
        transform.localScale = new Vector3(newScaleX, 1f, 1f);

        // ��ѡ�����ӵ��Կ��ӻ�
        Debug.DrawRay(transform.position, direction, Color.yellow, 0.1f);
    }
    #region sight_test
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 检测玩家进入视野
        if (other.CompareTag("Player"))
        {
            parameter.target = other.transform;

            if (parameter.attackCount > 0)
            {
                if (currentState is MGoBlinIdleState || currentState is MGoBlinPatrolState)
                    TransitionState(MGoBlinStateType.Chase); // 进入追逐状态
                else if (currentState is MGoBlinAttackState)
                    TransitionState(MGoBlinStateType.Attack); // 如果已经在攻击状态，保持攻击状态
                Debug.Log("Player detected, setting as target");
                parameter.attackCount=(parameter.attackCount+1)%3; 
            }
            else if (parameter.attackCount ==0)
            {
                TransitionState(MGoBlinStateType.DashAtk);
                parameter.attackCount = (parameter.attackCount + 1) % 3;
                Debug.Log("Player detected, initiating dash attack");
            }
            else
            {
                TransitionState(MGoBlinStateType.Idle); 
                Debug.Log("Player detected, but no attack count, staying idle");
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
                Debug.Log($"MGoBlin took {damage} damage from Hero's attack");
                
                // 可以在这里添加击退效果或其他反应
                StartCoroutine(DamageFlash());
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
            if (currentState is MGoBlinAttackState)
                TransitionState(MGoBlinStateType.Attack);

            else
            {
                parameter.target = null;

                TransitionState(MGoBlinStateType.Patrol); // �˳�ʱ�л���Idle״̬
            }
        }
    }
    #endregion


    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(parameter.attackPoint.position, parameter.attackArea);
    }
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
            // ������˸
            

            if (IsDead)
            {
                TransitionState(MGoBlinStateType.Dead);
            }
            else
            {
                // �л�������״̬
                TransitionState(MGoBlinStateType.Damaged);
            }
            // ����Ƿ�����

        


    }
    private IEnumerator DamageFlash()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = damageFlashColor;
            yield return new WaitForSeconds(damageFlashDuration);
            spriteRenderer.color = originalColor;
        }
    }
    
    
    // �������������ⲿ��ѯ
    public void Heal(int healAmount)
    {
        if (IsDead) return;
        
        currentHealth += healAmount;
        currentHealth = Mathf.Min(maxHealth, currentHealth); // ȷ�����������Ѫ��
        
        Debug.Log($"Spider healed {healAmount}! Health: {currentHealth}/{maxHealth}");
    }
    // 在管理器类中添加动画完成事件
    
        public void AnimationEvent_AttackCompleted()
        {
            
                TransitionState(MGoBlinStateType.Chase);
            
        }
    

}
