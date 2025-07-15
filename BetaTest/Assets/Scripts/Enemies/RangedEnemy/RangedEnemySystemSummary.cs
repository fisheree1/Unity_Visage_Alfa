using UnityEngine;

/// <summary>
/// Զ�̹�������ϵͳ�ܽ�
/// ����ű��ܽ�������Զ�̹�������ϵͳ�Ĺ��ܺ�ʹ�÷���
/// </summary>
public class RangedEnemySystemSummary : MonoBehaviour
{
    [Header("ϵͳ�������")]
    [TextArea(3, 10)]
    public string systemOverview = @"
Զ�̹�������ϵͳ - �������ܸ���:

?? ��������:
? ��״̬״̬��: ���� �� ���� �� ���� �� ���� �� ����
? ˫����ģʽ: ׷�ٵ� + ���ε�Ļ
? ����Ŀ����ͷ�Χ����
? ���������˺�����״̬����

?? ����ϵͳ:
? ׷�ٵ�: �������Զ�׷�����
? ���ε�Ļ: �෢�����ηֲ�
? ����ģʽ�л�: ���ֹ�������ʹ��

?? AIϵͳ:
? �Զ�Ŀ���� (8�׷�Χ)
? ����������� (6�׷�Χ)
? ����״̬�жϻ���
? ����״̬��������

?? ״̬ϵͳ:
? Idle: ������Ѱ��Ŀ��
? Charge: ����׼������
? Attack: ���䵯Ļ
? Hurt: ���ˣ�����Ч��
? Dead: ��������������";

    [Header("��������ָ��")]
    [TextArea(3, 10)]
    public string quickSetupGuide = @"
?? ���ٿ�ʼ:

1. ����GameObject����Ϊ 'RangedEnemy'
2. ��ӱ������:
   ? RangedEnemyP (���ű�)
   ? EnemyLife (����ϵͳ)
   ? Rigidbody2D (����)
   ? Collider2D (��ײ���)
   ? SpriteRenderer (��Ⱦ)
   ? Animator (����)

3. ���û�������:
   ? Detection Range: 8
   ? Attack Range: 6
   ? Charge Duration: 1
   ? Attack Cooldown: 2
   ? Damage: 15
   ? Hurt Duration: 0.5
   ? Hurt Knockback Force: 5

4. ����������Ԥ���岢�������Ӧ�ֶ�

5. ���ö���������:
   ? Idle, Charge, Attack, Hurt, Death";

    [Header("ϵͳ�ļ��ṹ")]
    [TextArea(3, 10)]
    public string fileStructure = @"
?? �ļ��ṹ:

Assets/Scripts/Enemies/RangedEnemy/
������ RangedEnemyP.cs              (��������)
������ RangedEnemyIdleState.cs      (����״̬)
������ RangedEnemyChargeState.cs    (����״̬)
������ RangedEnemyAttackState.cs    (����״̬)
������ RangedEnemyHurtState.cs      (����״̬) ?����
������ RangedEnemyDeadState.cs      (����״̬) ?����
������ TrackingProjectile.cs       (׷�ٵ�����)
������ RangedProjectile.cs         (��ͨ������)
������ RangedEnemyExample.cs       (ʹ��ʾ��)
������ RangedEnemySystemSummary.cs (���ļ�)
������ README_RangedEnemy.md       (�����ĵ�)";

    void Start()
    {
        Debug.Log("=== Զ�̹�������ϵͳ�Ѽ��� ===");
        Debug.Log("ϵͳ����:");
        Debug.Log("? ��״̬״̬�� (����/����/����/����/����)");
        Debug.Log("? ˫����ģʽ (׷�ٵ�/���ε�Ļ)");
        Debug.Log("? ����AI����ײ���");
        Debug.Log("? ���������˺���������");
        Debug.Log("? ������EnemyLife����");
        Debug.Log("=====================================");
        
        // ��鳡���е�Զ�̵���
        CheckSceneRangedEnemies();
    }

    private void CheckSceneRangedEnemies()
    {
        RangedEnemyP[] enemies = FindObjectsOfType<RangedEnemyP>();
        
        if (enemies.Length > 0)
        {
            Debug.Log($"? �����з��� {enemies.Length} ��Զ�̵���");
            
            foreach (var enemy in enemies)
            {
                string status = enemy.IsDead ? "������" : "���";
                string hurtStatus = enemy.parameter.isHit ? " (������)" : "";
                Debug.Log($"  - {enemy.name}: {status}{hurtStatus}, ����: {enemy.CurrentHealth}/{enemy.MaxHealth}");
            }
        }
        else
        {
            Debug.Log("? ������û�з���Զ�̵���");
            Debug.Log("?? ��ʾ: ʹ�� RangedEnemyExample.cs �е� '����ʾ��Զ�̵���' ����");
        }
    }

    [ContextMenu("��ʾϵͳͳ��")]
    public void ShowSystemStats()
    {
        Debug.Log("=== Զ�̹�������ϵͳͳ�� ===");
        
        // ͳ�Ƹ������
        RangedEnemyP[] enemies = FindObjectsOfType<RangedEnemyP>();
        TrackingProjectile[] trackingProjectiles = FindObjectsOfType<TrackingProjectile>();
        RangedProjectile[] rangedProjectiles = FindObjectsOfType<RangedProjectile>();
        
        Debug.Log($"��Ծ��Զ�̵���: {enemies.Length}");
        Debug.Log($"��Ծ��׷�ٵ�: {trackingProjectiles.Length}");
        Debug.Log($"��Ծ����ͨ������: {rangedProjectiles.Length}");
        
        // ͳ�Ƶ���״̬
        int idleCount = 0, chargeCount = 0, attackCount = 0, hurtCount = 0, deadCount = 0;
        
        foreach (var enemy in enemies)
        {
            if (enemy.parameter.isDead)
            {
                deadCount++;
            }
            else if (enemy.parameter.isHit)
            {
                hurtCount++;
            }
            else if (enemy.parameter.isAttacking)
            {
                attackCount++;
            }
            else if (enemy.parameter.isCharging)
            {
                chargeCount++;
            }
            else
            {
                idleCount++;
            }
        }
        
        Debug.Log($"״̬�ֲ� - ����: {idleCount}, ����: {chargeCount}, ����: {attackCount}, ����: {hurtCount}, ����: {deadCount}");
        Debug.Log("==============================");
    }

    [ContextMenu("��������Զ�̵���")]
    public void TestAllRangedEnemies()
    {
        RangedEnemyP[] enemies = FindObjectsOfType<RangedEnemyP>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player == null)
        {
            Debug.LogWarning("δ�ҵ���ң��޷�����");
            return;
        }
        
        Debug.Log($"��ʼ���� {enemies.Length} ��Զ�̵���");
        
        foreach (var enemy in enemies)
        {
            if (!enemy.IsDead && !enemy.parameter.isDead)
            {
                // �������ΪĿ��
                enemy.parameter.target = player.transform;
                
                // ǿ�ƽ�������״̬
                enemy.TransitionState(RangedEnemyStateType.Charge);
                
                Debug.Log($"? ���� {enemy.name} �Ĺ�������");
            }
        }
    }

    [ContextMenu("��������״̬")]
    public void TestHurtState()
    {
        RangedEnemyP[] enemies = FindObjectsOfType<RangedEnemyP>();
        
        foreach (var enemy in enemies)
        {
            if (!enemy.IsDead && !enemy.parameter.isDead)
            {
                // ģ������
                enemy.TakeDamage(1);
                Debug.Log($"? ���� {enemy.name} �����˲���");
            }
        }
    }

    [ContextMenu("��������Զ�̵���")]
    public void ResetAllRangedEnemies()
    {
        RangedEnemyP[] enemies = FindObjectsOfType<RangedEnemyP>();
        
        foreach (var enemy in enemies)
        {
            if (!enemy.IsDead && !enemy.parameter.isDead)
            {
                enemy.parameter.target = null;
                enemy.parameter.isCharging = false;
                enemy.parameter.isAttacking = false;
                enemy.parameter.isHit = false;
                enemy.TransitionState(RangedEnemyStateType.Idle);
            }
        }
        
        Debug.Log($"������ {enemies.Length} ��Զ�̵��˵�����״̬");
    }

    [ContextMenu("�������е�����")]
    public void ClearAllProjectiles()
    {
        TrackingProjectile[] trackingProjectiles = FindObjectsOfType<TrackingProjectile>();
        RangedProjectile[] rangedProjectiles = FindObjectsOfType<RangedProjectile>();
        
        foreach (var projectile in trackingProjectiles)
        {
            DestroyImmediate(projectile.gameObject);
        }
        
        foreach (var projectile in rangedProjectiles)
        {
            DestroyImmediate(projectile.gameObject);
        }
        
        Debug.Log($"������ {trackingProjectiles.Length} ��׷�ٵ��� {rangedProjectiles.Length} ����ͨ������");
    }

    private void OnDrawGizmosSelected()
    {
        // ����ϵͳ����
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 1f);
        
        // ��ʾ����������Զ�̵��˵�����
        RangedEnemyP[] enemies = FindObjectsOfType<RangedEnemyP>();
        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                Color lineColor = Color.green;
                if (enemy.parameter.isDead) lineColor = Color.red;
                else if (enemy.parameter.isHit) lineColor = Color.green;
                else if (enemy.parameter.isAttacking) lineColor = Color.yellow;
                else if (enemy.parameter.isCharging) lineColor = Color.blue;
                
                Gizmos.color = lineColor;
                Gizmos.DrawLine(transform.position, enemy.transform.position);
            }
        }
    }
}