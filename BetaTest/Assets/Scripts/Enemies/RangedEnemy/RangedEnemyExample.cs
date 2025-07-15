using UnityEngine;

/// <summary>
/// Զ�̹�������ϵͳʹ��ʾ����˵��
/// չʾ������ú�ʹ��Զ�̹�������
/// </summary>
public class RangedEnemyExample : MonoBehaviour
{
    [Header("ϵͳ���˵��")]
    [SerializeField] private string[] systemComponents = {
        "RangedEnemyP.cs - ��Ҫ���˿�����",
        "RangedEnemyIdleState.cs - ����״̬",
        "RangedEnemyChargeState.cs - ����״̬", 
        "RangedEnemyAttackState.cs - ����״̬",
        "TrackingProjectile.cs - ׷�ٵ�����",
        "RangedProjectile.cs - ��ͨ������"
    };
    
    [Header("״̬������")]
    [SerializeField] private string[] stateFlow = {
        "1. Idle - ������Ѱ��Ŀ��",
        "2. Charge - ����Ŀ���ʼ����",
        "3. Attack - ������ɺ��䵯Ļ",
        "4. ����Idle - ������ɣ�������ȴ"
    };
    
    [Header("����ģʽ")]
    [SerializeField] private string[] attackModes = {
        "׷�ٵ� - ��������׷�����",
        "���ε�Ļ - �෢�����ηֲ�",
        "����ģʽ����ʹ��"
    };
    
    [Header("����ʾ��")]
    [SerializeField] private GameObject rangedEnemyPrefab;
    [SerializeField] private GameObject trackingProjectilePrefab;
    [SerializeField] private GameObject normalProjectilePrefab;
    
    void Start()
    {
        Debug.Log("=== Զ�̹�������ϵͳ ===");
        Debug.Log("ϵͳ�ص㣺");
        Debug.Log("- ��״̬��ƣ�����������������");
        Debug.Log("- ˫����ģʽ��׷�ٵ� + ���ε�Ļ");
        Debug.Log("- ����Ŀ����͹�����Χ����");
        Debug.Log("- ��������ײ�����˺�ϵͳ");
        Debug.Log("========================");
        
        // ��ʾ�������Զ�̵���
        DemonstrateRangedEnemySetup();
    }
    
    private void DemonstrateRangedEnemySetup()
    {
        Debug.Log("=== Զ�̵�������ָ�� ===");
        
        // ���ҳ����е�Զ�̵���
        RangedEnemyP[] rangedEnemies = FindObjectsOfType<RangedEnemyP>();
        
        if (rangedEnemies.Length > 0)
        {
            Debug.Log($"�ҵ� {rangedEnemies.Length} ��Զ�̵���");
            
            foreach (RangedEnemyP enemy in rangedEnemies)
            {
                Debug.Log($"- Զ�̵���: {enemy.name}");
                CheckEnemyConfiguration(enemy);
            }
        }
        else
        {
            Debug.Log("δ�ҵ�Զ�̵��ˣ���ʾ����ָ�ϣ�");
            ShowCreationGuide();
        }
    }
    
    private void CheckEnemyConfiguration(RangedEnemyP enemy)
    {
        Debug.Log($"��� {enemy.name} �����ã�");
        
        // ���EnemyLife���
        EnemyLife enemyLife = enemy.GetComponent<EnemyLife>();
        if (enemyLife != null)
        {
            Debug.Log($"? EnemyLife������ڣ�����ֵ: {enemyLife.CurrentHealth}/{enemyLife.MaxHealth}");
        }
        else
        {
            Debug.Log("? ȱ��EnemyLife���");
        }
        
        // ����������
        if (enemy.parameter != null)
        {
            Debug.Log($"? �������ô���");
            Debug.Log($"  - ��ⷶΧ: {enemy.parameter.detectionRange}");
            Debug.Log($"  - ������Χ: {enemy.parameter.attackRange}");
            Debug.Log($"  - ������ȴ: {enemy.parameter.attackCooldown}");
            Debug.Log($"  - ����ʱ��: {enemy.parameter.chargeDuration}");
            Debug.Log($"  - �������˺�: {enemy.parameter.damage}");
            
            // ��鵯����Ԥ����
            if (enemy.parameter.projectilePrefab != null)
            {
                Debug.Log($"? ��ͨ������Ԥ����: {enemy.parameter.projectilePrefab.name}");
            }
            else
            {
                Debug.Log("? ȱ����ͨ������Ԥ����");
            }
            
            if (enemy.parameter.trackingProjectilePrefab != null)
            {
                Debug.Log($"? ׷�ٵ�����Ԥ����: {enemy.parameter.trackingProjectilePrefab.name}");
            }
            else
            {
                Debug.Log("? ȱ��׷�ٵ�����Ԥ����");
            }
        }
        else
        {
            Debug.Log("? ȱ�ٲ�������");
        }
    }
    
    private void ShowCreationGuide()
    {
        Debug.Log("=== Զ�̵��˴���ָ�� ===");
        Debug.Log("1. ������GameObject������Ϊ'RangedEnemy'");
        Debug.Log("2. ������������");
        Debug.Log("   - RangedEnemyP (���ű�)");
        Debug.Log("   - EnemyLife (����ϵͳ)");
        Debug.Log("   - Rigidbody2D (����)");
        Debug.Log("   - Collider2D (��ײ����ΪTrigger)");
        Debug.Log("   - SpriteRenderer (��Ⱦ)");
        Debug.Log("   - Animator (����)");
        Debug.Log("3. ���ò�����");
        Debug.Log("   - Detection Range: 8");
        Debug.Log("   - Attack Range: 6");
        Debug.Log("   - Charge Duration: 1");
        Debug.Log("   - Attack Cooldown: 2");
        Debug.Log("   - Damage: 15");
        Debug.Log("4. ���䵯����Ԥ����");
        Debug.Log("5. ����Ŀ��㼶 (Player)");
        Debug.Log("========================");
    }
    
    [ContextMenu("����ʾ��Զ�̵���")]
    public void CreateExampleRangedEnemy()
    {
        // ����Զ�̵���GameObject
        GameObject enemyObj = new GameObject("RangedEnemy_Example");
        enemyObj.transform.position = transform.position + Vector3.right * 3f;
        
        // ��ӻ������
        RangedEnemyP rangedEnemy = enemyObj.AddComponent<RangedEnemyP>();
        EnemyLife enemyLife = enemyObj.AddComponent<EnemyLife>();
        Rigidbody2D rb = enemyObj.AddComponent<Rigidbody2D>();
        CapsuleCollider2D col = enemyObj.AddComponent<CapsuleCollider2D>();
        SpriteRenderer sr = enemyObj.AddComponent<SpriteRenderer>();
        
        // �����������
        rb.freezeRotation = true;
        rb.gravityScale = 1f;
        col.isTrigger = true;
        
        // ���õ��˱�ǩ
        enemyObj.tag = "Enemy";
        
        // ���ò���
        if (rangedEnemy.parameter == null)
        {
            rangedEnemy.parameter = new RangedEnemyParameter();
        }
        
        rangedEnemy.parameter.detectionRange = 8f;
        rangedEnemy.parameter.attackRange = 6f;
        rangedEnemy.parameter.chargeDuration = 1f;
        rangedEnemy.parameter.attackCooldown = 2f;
        rangedEnemy.parameter.damage = 15;
        rangedEnemy.parameter.projectileSpeed = 5f;
        rangedEnemy.parameter.fanProjectileCount = 5;
        rangedEnemy.parameter.fanAngle = 45f;
        rangedEnemy.parameter.targetLayer = LayerMask.GetMask("Player");
        
        // ���䵯����Ԥ���壨����еĻ���
        if (trackingProjectilePrefab != null)
        {
            rangedEnemy.parameter.trackingProjectilePrefab = trackingProjectilePrefab;
        }
        
        if (normalProjectilePrefab != null)
        {
            rangedEnemy.parameter.projectilePrefab = normalProjectilePrefab;
        }
        
        Debug.Log("ʾ��Զ�̵��˴�����ɣ�");
    }
    
    [ContextMenu("����Զ�̵��˹���")]
    public void TestRangedEnemyAttack()
    {
        RangedEnemyP[] enemies = FindObjectsOfType<RangedEnemyP>();
        
        foreach (RangedEnemyP enemy in enemies)
        {
            if (enemy.parameter != null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    enemy.parameter.target = player.transform;
                    enemy.TransitionState(RangedEnemyStateType.Charge);
                    Debug.Log($"���� {enemy.name} �Ĺ�������");
                }
            }
        }
    }
} 