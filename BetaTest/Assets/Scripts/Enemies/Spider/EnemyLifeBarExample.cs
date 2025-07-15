using UnityEngine;

/// <summary>
/// ʾ���ű�����ʾ���ʹ�ü��ɺ�� EnemyLifeBar �� EnemyLife ϵͳ
/// </summary>
public class EnemyLifeBarExample : MonoBehaviour
{
    [Header("ʾ������")]
    [SerializeField] private bool enableExample = true;
    [SerializeField] private KeyCode damageKey = KeyCode.D;
    [SerializeField] private KeyCode healKey = KeyCode.H;
    [SerializeField] private KeyCode resetKey = KeyCode.R;
    
    [Header("���Բ���")]
    [SerializeField] private int damageAmount = 5;
    [SerializeField] private int healAmount = 3;
    
    private EnemyLife enemyLife;
    private EnemyLifeBar enemyLifeBar;
    
    void Start()
    {
        if (!enableExample) return;
        
        // ��ȡ���
        enemyLife = GetComponent<EnemyLife>();
        enemyLifeBar = GetComponent<EnemyLifeBar>();
        
        if (enemyLife == null)
        {
            Debug.LogWarning($"EnemyLifeBarExample on {gameObject.name} couldn't find EnemyLife component!");
            return;
        }
        
        if (enemyLifeBar == null)
        {
            Debug.LogWarning($"EnemyLifeBarExample on {gameObject.name} couldn't find EnemyLifeBar component!");
            return;
        }
        
        // �����¼�����ʾ��־
        SubscribeToEvents();
        
        // ��ʾʹ��˵��
        Debug.Log($"=== EnemyLifeBar Integration Example ===");
        Debug.Log($"Press {damageKey} to damage enemy ({damageAmount} damage)");
        Debug.Log($"Press {healKey} to heal enemy ({healAmount} heal)");
        Debug.Log($"Press {resetKey} to reset enemy to full health");
        Debug.Log($"Enemy: {enemyLife.CurrentHealth}/{enemyLife.MaxHealth} HP");
        Debug.Log($"=========================================");
    }
    
    void Update()
    {
        if (!enableExample) return;
        
        // ��������
        if (Input.GetKeyDown(damageKey))
        {
            TestDamage();
        }
        
        if (Input.GetKeyDown(healKey))
        {
            TestHeal();
        }
        
        if (Input.GetKeyDown(resetKey))
        {
            TestReset();
        }
    }
    
    private void TestDamage()
    {
        if (enemyLife != null && !enemyLife.IsDead)
        {
            enemyLife.TakeDamage(damageAmount);
            Debug.Log($"Applied {damageAmount} damage to {gameObject.name}");
        }
    }
    
    private void TestHeal()
    {
        if (enemyLife != null && !enemyLife.IsDead)
        {
            enemyLife.Heal(healAmount);
            Debug.Log($"Applied {healAmount} healing to {gameObject.name}");
        }
    }
    
    private void TestReset()
    {
        if (enemyLife != null)
        {
            enemyLife.RestoreToFullHealth();
            Debug.Log($"Restored {gameObject.name} to full health");
        }
    }
    
    private void SubscribeToEvents()
    {
        if (enemyLife != null)
        {
            enemyLife.OnHealthChanged += OnHealthChanged;
            enemyLife.OnDamageTaken += OnDamageTaken;
            enemyLife.OnHealed += OnHealed;
            enemyLife.OnDeath += OnDeath;
        }
    }
    
    private void UnsubscribeFromEvents()
    {
        if (enemyLife != null)
        {
            enemyLife.OnHealthChanged -= OnHealthChanged;
            enemyLife.OnDamageTaken -= OnDamageTaken;
            enemyLife.OnHealed -= OnHealed;
            enemyLife.OnDeath -= OnDeath;
        }
    }
    
    // �¼�����
    private void OnHealthChanged(int currentHealth, int maxHealth)
    {
        float healthPercent = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
        Debug.Log($"[{gameObject.name}] Health: {currentHealth}/{maxHealth} ({healthPercent:P1}) - Bar width should be {healthPercent:P1}");
    }
    
    private void OnDamageTaken(int damage)
    {
        Debug.Log($"[{gameObject.name}] Took {damage} damage - Bar width should decrease");
    }
    
    private void OnHealed(int healAmount)
    {
        Debug.Log($"[{gameObject.name}] Healed {healAmount} HP - Bar width should increase");
    }
    
    private void OnDeath()
    {
        Debug.Log($"[{gameObject.name}] Died - Bar should be hidden");
    }
    
    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    
    // �������������ⲿ����
    public void DamageEnemy(int damage)
    {
        if (enemyLife != null)
        {
            enemyLife.TakeDamage(damage);
        }
    }
    
    public void HealEnemy(int heal)
    {
        if (enemyLife != null)
        {
            enemyLife.Heal(heal);
        }
    }
    
    public void ResetEnemy()
    {
        if (enemyLife != null)
        {
            enemyLife.RestoreToFullHealth();
        }
    }
}