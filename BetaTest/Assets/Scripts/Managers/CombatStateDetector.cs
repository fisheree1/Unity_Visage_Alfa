using UnityEngine;
using System.Collections;

/// <summary>
/// ս��״̬����� - �������Ƿ���ս��״̬
/// ��AudioManager��Ϲ������ṩ��ȷ��ս�����
/// </summary>
public class CombatStateDetector : MonoBehaviour
{
    [Header("�������")]
    [SerializeField] private float combatRadius = 12f;
    [SerializeField] private float combatExitDelay = 3f; // ����ս�����ӳ�ʱ��
    [SerializeField] private LayerMask enemyLayers = -1;
    [SerializeField] private LayerMask bossLayers = -1;
    
    [Header("��ǩ����")]
    [SerializeField] private string[] enemyTags = { "Enemy", "Boss" };
    [SerializeField] private string bossTag = "Boss";
    
    [Header("����")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private bool showGizmos = true;
    
    // �¼�
    public System.Action OnCombatStart;
    public System.Action OnCombatEnd;
    public System.Action OnBossBattleStart;
    public System.Action OnBossBattleEnd;
    
    // ״̬
    private bool isInCombat = false;
    private bool isInBossBattle = false;
    private Transform player;
    private Coroutine combatExitCoroutine;
    
    // ��⵽��Ŀ��
    private BossLife currentBoss;
    private int nearbyEnemyCount = 0;
    
    void Start()
    {
        // �����������
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("CombatStateDetector: Player not found!");
        }
    }
    
    void Update()
    {
        if (player != null)
        {
            CheckCombatState();
        }
    }
    
    private void CheckCombatState()
    {
        bool previousCombatState = isInCombat;
        bool previousBossState = isInBossBattle;
        
        // ���Bossս��
        BossLife detectedBoss = DetectNearestBoss();
        bool bossDetected = (detectedBoss != null && !detectedBoss.IsDead);
        
        // �����ͨ����
        int enemyCount = DetectNearbyEnemies();
        bool enemiesDetected = enemyCount > 0;
        
        // ����Bossս��״̬
        if (bossDetected != isInBossBattle)
        {
            isInBossBattle = bossDetected;
            currentBoss = bossDetected ? detectedBoss : null;
            
            if (isInBossBattle)
            {
                OnBossBattleStart?.Invoke();
                DebugLog($"Boss battle started with {currentBoss.name}");
                
                // Bossս��ʱ�Զ�����ս��״̬
                if (!isInCombat)
                {
                    isInCombat = true;
                    OnCombatStart?.Invoke();
                    DebugLog("Combat started (Boss detected)");
                }
            }
            else
            {
                OnBossBattleEnd?.Invoke();
                DebugLog("Boss battle ended");
            }
        }
        
        // ����һ��ս��״̬
        bool shouldBeInCombat = bossDetected || enemiesDetected;
        
        if (shouldBeInCombat && !isInCombat)
        {
            // ����ս��
            isInCombat = true;
            nearbyEnemyCount = enemyCount;
            
            // ȡ������ս����Э��
            if (combatExitCoroutine != null)
            {
                StopCoroutine(combatExitCoroutine);
                combatExitCoroutine = null;
            }
            
            OnCombatStart?.Invoke();
            DebugLog($"Combat started - Enemies: {enemyCount}, Boss: {bossDetected}");
        }
        else if (!shouldBeInCombat && isInCombat)
        {
            // ��������ս������ʼ�ӳټ��
            if (combatExitCoroutine == null)
            {
                combatExitCoroutine = StartCoroutine(DelayedCombatExit());
            }
        }
        else if (shouldBeInCombat && isInCombat)
        {
            // ����ս���У����µ�������
            nearbyEnemyCount = enemyCount;
        }
    }
    
    private BossLife DetectNearestBoss()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(player.position, combatRadius, bossLayers);
        
        BossLife nearestBoss = null;
        float nearestDistance = float.MaxValue;
        
        foreach (var collider in colliders)
        {
            if (collider.CompareTag(bossTag))
            {
                BossLife boss = collider.GetComponent<BossLife>();
                if (boss != null && !boss.IsDead)
                {
                    float distance = Vector2.Distance(player.position, collider.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestBoss = boss;
                    }
                }
            }
        }
        
        return nearestBoss;
    }
    
    private int DetectNearbyEnemies()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(player.position, combatRadius, enemyLayers);
        int aliveEnemyCount = 0;
        
        foreach (var collider in colliders)
        {
            // ����ǩ
            bool isEnemy = false;
            foreach (string tag in enemyTags)
            {
                if (collider.CompareTag(tag))
                {
                    isEnemy = true;
                    break;
                }
            }
            
            if (!isEnemy) continue;
            
            // �������Ƿ񻹻���
            if (IsEnemyAlive(collider.gameObject))
            {
                aliveEnemyCount++;
            }
        }
        
        return aliveEnemyCount;
    }
    
    private bool IsEnemyAlive(GameObject enemy)
    {
        // ���Boss�������
        BossLife bossLife = enemy.GetComponent<BossLife>();
        if (bossLife != null)
        {
            return !bossLife.IsDead;
        }
        
        // ���Enemy���
        Enemy enemyComponent = enemy.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            return true; // Enemy��������һ�Ծ
        }
        
        // �������������������Ը�����Ŀ��չ��
        var lifeComponents = enemy.GetComponentsInChildren<MonoBehaviour>();
        foreach (var component in lifeComponents)
        {
            // ����Ƿ��а���"Life"��"Health"�������
            string componentName = component.GetType().Name.ToLower();
            if (componentName.Contains("life") || componentName.Contains("health"))
            {
                return true;
            }
        }
        
        return false;
    }
    
    private IEnumerator DelayedCombatExit()
    {
        DebugLog($"Combat exit delay started ({combatExitDelay}s)");
        
        yield return new WaitForSeconds(combatExitDelay);
        
        // �ٴμ���Ƿ����������ս��
        BossLife boss = DetectNearestBoss();
        int enemies = DetectNearbyEnemies();
        
        if (boss == null && enemies == 0)
        {
            isInCombat = false;
            nearbyEnemyCount = 0;
            
            OnCombatEnd?.Invoke();
            DebugLog("Combat ended");
            
            // ���֮ǰ��Bossս���У�Ҳ����Bossս��״̬
            if (isInBossBattle)
            {
                isInBossBattle = false;
                currentBoss = null;
                OnBossBattleEnd?.Invoke();
                DebugLog("Boss battle ended (delayed)");
            }
        }
        else
        {
            DebugLog("Still in combat, canceling exit");
        }
        
        combatExitCoroutine = null;
    }
    
    public void ForceEnterCombat()
    {
        if (!isInCombat)
        {
            isInCombat = true;
            OnCombatStart?.Invoke();
            DebugLog("Combat forced to start");
        }
    }
    
    public void ForceExitCombat()
    {
        if (isInCombat)
        {
            if (combatExitCoroutine != null)
            {
                StopCoroutine(combatExitCoroutine);
                combatExitCoroutine = null;
            }
            
            isInCombat = false;
            isInBossBattle = false;
            nearbyEnemyCount = 0;
            currentBoss = null;
            
            OnCombatEnd?.Invoke();
            OnBossBattleEnd?.Invoke();
            DebugLog("Combat forced to end");
        }
    }
    
    private void DebugLog(string message)
    {
        if (showDebugInfo)
        {
            Debug.Log($"[CombatDetector] {message}");
        }
    }
    
    // ���ӻ�����
    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;
        
        Vector3 center = player != null ? player.position : transform.position;
        
        // ���Ƽ�ⷶΧ
        Gizmos.color = isInCombat ? Color.red : Color.green;
        Gizmos.DrawWireSphere(center, combatRadius);
        
        // ����Bossս��ָʾ
        if (isInBossBattle)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(center, combatRadius * 0.8f);
        }
        
        // ���Ƶ���λ��
        if (Application.isPlaying && player != null)
        {
            Collider2D[] enemies = Physics2D.OverlapCircleAll(player.position, combatRadius, enemyLayers);
            Gizmos.color = Color.yellow;
            foreach (var enemy in enemies)
            {
                if (IsEnemyAlive(enemy.gameObject))
                {
                    Gizmos.DrawWireCube(enemy.transform.position, Vector3.one * 0.5f);
                }
            }
        }
    }
    
    // ��������
    public bool IsInCombat => isInCombat;
    public bool IsInBossBattle => isInBossBattle;
    public BossLife CurrentBoss => currentBoss;
    public int NearbyEnemyCount => nearbyEnemyCount;
}