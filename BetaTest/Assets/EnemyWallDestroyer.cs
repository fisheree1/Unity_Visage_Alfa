using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWallDestroyer : MonoBehaviour
{
    [Header("Target Enemy")]
    [SerializeField] private GameObject targetEnemy; // ָ���ĵ���
    [SerializeField] private string enemyTag = "Enemy"; // ���˱�ǩ
    [SerializeField] private bool autoFindEnemy = true; // �Ƿ��Զ����ҵ���
    
    [Header("Wall Settings")]
    [SerializeField] private GameObject wallToDestroy; // Ҫ�ݻٵ�ǽ��
    [SerializeField] private bool destroyWall = true; // true: �ݻ�ǽ��, false: ����Ϊtrigger
    [SerializeField] private float destroyDelay = 0.5f; // �ݻ��ӳ�ʱ��
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject destructionEffect; // �ݻ���Ч
    [SerializeField] private AudioClip destructionSound; // �ݻ���Ч
    
    private EnemyLife enemyLife;
    private BossLife bossLife;
    private Collider2D wallCollider;
    private AudioSource audioSource;
    private bool hasProcessedDeath = false;
    
    void Start()
    {
        // ��ȡ���
        audioSource = GetComponent<AudioSource>();
        
        // ����Ҫ�ݻٵ�ǽ��
        if (wallToDestroy == null)
        {
            wallToDestroy = gameObject; // ���û��ָ����Ĭ�ϴݻ��Լ�
        }
        
        wallCollider = wallToDestroy.GetComponent<Collider2D>();
        
        // ����Ŀ�����
        FindTargetEnemy();
        
        // ���������¼�
        SubscribeToDeathEvents();
    }
    
    void Update()
    {
        // ���û���ҵ����ˣ��������²���
        if (targetEnemy == null && autoFindEnemy)
        {
            FindTargetEnemy();
            if (targetEnemy != null)
            {
                SubscribeToDeathEvents();
            }
        }
    }
    
    private void FindTargetEnemy()
    {
        if (targetEnemy == null && autoFindEnemy)
        {
            // ͨ����ǩ���ҵ���
            GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
            if (enemies.Length > 0)
            {
                targetEnemy = enemies[0]; // ѡ���һ���ҵ��ĵ���
                Debug.Log($"EnemyWallDestroyer: Auto-found enemy '{targetEnemy.name}'");
            }
        }
    }
    
    private void SubscribeToDeathEvents()
    {
        if (targetEnemy == null)
        {
            Debug.LogWarning("EnemyWallDestroyer: No target enemy assigned!");
            return;
        }
        
        // ���Ի�ȡ EnemyLife ���
        enemyLife = targetEnemy.GetComponent<EnemyLife>();
        if (enemyLife != null)
        {
            enemyLife.OnDeath += OnEnemyDeath;
            Debug.Log($"EnemyWallDestroyer: Successfully connected to EnemyLife on '{targetEnemy.name}'");
            return;
        }
        
        // ���Ի�ȡ BossLife ���
        bossLife = targetEnemy.GetComponent<BossLife>();
        if (bossLife != null)
        {
            bossLife.OnDeath += OnEnemyDeath;
            Debug.Log($"EnemyWallDestroyer: Successfully connected to BossLife on '{targetEnemy.name}'");
            return;
        }
        
        // ���û���ҵ�������������Լ�����Ϸ���������
        StartCoroutine(MonitorEnemyDestruction());
        Debug.Log($"EnemyWallDestroyer: No life component found, monitoring destruction of '{targetEnemy.name}'");
    }
    
    private IEnumerator MonitorEnemyDestruction()
    {
        while (targetEnemy != null && !hasProcessedDeath)
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        if (!hasProcessedDeath)
        {
            OnEnemyDeath();
        }
    }
    
    private void OnEnemyDeath()
    {
        if (hasProcessedDeath) return;
        
        hasProcessedDeath = true;
        
        Debug.Log($"EnemyWallDestroyer: Enemy '{targetEnemy?.name}' has died, processing wall destruction.");
        
        if (destroyWall)
        {
            DestroyWall();
        }
        else
        {
            SetWallAsTrigger();
        }
    }
    
    private void DestroyWall()
    {
        if (wallToDestroy == null) return;
        
        // ���Ŵݻ���Ч
        if (destructionEffect != null)
        {
            Instantiate(destructionEffect, wallToDestroy.transform.position, Quaternion.identity);
        }
        
        // ���Ŵݻ���Ч
        if (audioSource != null && destructionSound != null)
        {
            audioSource.PlayOneShot(destructionSound);
        }
        
        Debug.Log($"EnemyWallDestroyer: Wall '{wallToDestroy.name}' will be destroyed.");
        
        // �ӳٴݻ���ȷ����Ч����Ч�ܹ�����
        StartCoroutine(DestroyWallCoroutine());
    }
    
    private IEnumerator DestroyWallCoroutine()
    {
        yield return new WaitForSeconds(destroyDelay);
        
        if (wallToDestroy != null)
        {
            Destroy(wallToDestroy);
        }
    }
    
    private void SetWallAsTrigger()
    {
        if (wallCollider != null)
        {
            wallCollider.isTrigger = true;
            Debug.Log($"EnemyWallDestroyer: Wall '{wallToDestroy.name}' has been set as trigger.");
        }
        else
        {
            Debug.LogWarning("EnemyWallDestroyer: No collider found to set as trigger!");
        }
    }
    
    private void OnDestroy()
    {
        // ȡ�������¼�����ֹ�ڴ�й©
        if (enemyLife != null)
        {
            enemyLife.OnDeath -= OnEnemyDeath;
        }
        
        if (bossLife != null)
        {
            bossLife.OnDeath -= OnEnemyDeath;
        }
    }
    
    // �ڱ༭���п��ӻ�
    private void OnDrawGizmosSelected()
    {
        if (wallToDestroy != null)
        {
            Gizmos.color = hasProcessedDeath ? Color.red : Color.green;
            Gizmos.DrawWireCube(wallToDestroy.transform.position, wallToDestroy.transform.localScale);
        }
        
        if (targetEnemy != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(targetEnemy.transform.position, 1f);
            
            // �������ӵ��˺�ǽ��
            if (wallToDestroy != null)
            {
                Gizmos.DrawLine(targetEnemy.transform.position, wallToDestroy.transform.position);
            }
        }
    }
}