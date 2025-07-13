using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallDelete : MonoBehaviour
{
    [Header("Boss Reference")]
    [SerializeField] private BossLife bossLife;
    
    [Header("Settings")]
    [SerializeField] private bool disableWhenBossDead = true; // true: Boss死亡时禁用墙体, false: Boss死亡时启用墙体
    [SerializeField] private bool autoFindBoss = true; // 是否自动查找Boss
    
    private bool lastBossDeadState = false;
    // Start is called before the first frame update
    void Start()
    {
        // 如果启用自动查找且没有手动指定BossLife，尝试自动查找
        if (autoFindBoss && bossLife == null)
        {
            bossLife = FindObjectOfType<BossLife>();
        }
        
        if (bossLife != null)
        {
            // 订阅Boss的死亡事件
            bossLife.OnDeath += OnBossDeath;
            
            // 订阅Boss的血量变化事件
            bossLife.OnHealthChanged += OnBossHealthChanged;
            
            // 初始化Boss死亡状态
            lastBossDeadState = bossLife.CurrentHealth <= 0 || bossLife.IsDead;
            
            // 初始检查Boss状态并设置墙体状态
            UpdateWallState();
            
            Debug.Log($"WallDelete: Successfully connected to Boss '{bossLife.name}', Initial boss dead state: {lastBossDeadState}");
        }
        else
        {
            Debug.LogWarning("WallDelete: No BossLife component found! Please assign it in the inspector or ensure a Boss exists in the scene.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 如果没有BossLife引用，尝试重新查找
        if (bossLife == null && autoFindBoss)
        {
            bossLife = FindObjectOfType<BossLife>();
            if (bossLife != null)
            {
                // 重新订阅事件
                bossLife.OnDeath += OnBossDeath;
                bossLife.OnHealthChanged += OnBossHealthChanged;
                UpdateWallState();
            }
        }
    }
    
    /// <summary>
    /// 当Boss死亡时调用的方法
    /// </summary>
    private void OnBossDeath()
    {
        Debug.Log("WallDelete: Boss has died, updating wall state.");
        UpdateWallState();
    }
    
    /// <summary>
    /// 当Boss血量变化时调用的方法
    /// </summary>
    /// <param name="currentHealth">当前血量</param>
    /// <param name="maxHealth">最大血量</param>
    private void OnBossHealthChanged(int currentHealth, int maxHealth)
    {
        bool isBossDead = currentHealth <= 0;
        
        // 只在Boss死亡状态发生变化时更新
        if (isBossDead != lastBossDeadState)
        {
            lastBossDeadState = isBossDead;
            UpdateWallState();
            Debug.Log($"WallDelete: Boss health changed to {currentHealth}/{maxHealth}, Boss dead: {isBossDead}");
        }
    }
    
    /// <summary>
    /// 更新墙体的启用状态
    /// </summary>
    private void UpdateWallState()
    {
        if (bossLife == null) return;
        
        bool isBossDead = bossLife.CurrentHealth <= 0 || bossLife.IsDead;
        bool shouldEnable;
        
        if (disableWhenBossDead)
        {
            // Boss死亡时禁用墙体 (Boss活着时墙体存在，Boss死亡时墙体消失)
            shouldEnable = !isBossDead;
        }
        else
        {
            // Boss死亡时启用墙体 (Boss活着时墙体消失，Boss死亡时墙体出现)
            shouldEnable = isBossDead;
        }
        
        Debug.Log($"WallDelete: Boss dead: {isBossDead}, Should enable wall: {shouldEnable}, Current active: {gameObject.activeSelf}, disableWhenBossDead: {disableWhenBossDead}");
        
        // 使用 activeSelf 而不是 activeInHierarchy，因为我们只关心这个对象本身的状态
        if (gameObject.activeSelf != shouldEnable)
        {
            gameObject.SetActive(shouldEnable);
            Debug.Log($"WallDelete: Wall state changed to {(shouldEnable ? "enabled" : "disabled")} (Boss dead: {isBossDead})");
        }
        else
        {
            Debug.Log($"WallDelete: Wall state unchanged, already {(shouldEnable ? "enabled" : "disabled")}");
        }
    }
    
    /// <summary>
    /// 手动设置BossLife引用
    /// </summary>
    /// <param name="boss">要监听的Boss对象</param>
    public void SetBoss(BossLife boss)
    {
        // 清理旧的事件订阅
        if (bossLife != null)
        {
            bossLife.OnDeath -= OnBossDeath;
            bossLife.OnHealthChanged -= OnBossHealthChanged;
        }
        
        bossLife = boss;
        
        if (bossLife != null)
        {
            // 订阅新的事件
            bossLife.OnDeath += OnBossDeath;
            bossLife.OnHealthChanged += OnBossHealthChanged;
            
            // 重新初始化状态
            lastBossDeadState = bossLife.CurrentHealth <= 0 || bossLife.IsDead;
            UpdateWallState();
        }
    }
    
    /// <summary>
    /// 手动触发墙体状态更新（用于测试）
    /// </summary>
    [ContextMenu("Force Update Wall State")]
    public void ForceUpdateWallState()
    {
        if (bossLife != null)
        {
            Debug.Log($"WallDelete: Force updating wall state. Boss health: {bossLife.CurrentHealth}, Boss dead: {bossLife.IsDead}");
            UpdateWallState();
        }
        else
        {
            Debug.LogWarning("WallDelete: Cannot force update - no Boss reference found!");
        }
    }
    
    /// <summary>
    /// 获取当前墙体应该的状态（用于调试）
    /// </summary>
    public bool GetExpectedWallState()
    {
        if (bossLife == null) return gameObject.activeSelf;
        
        bool isBossDead = bossLife.CurrentHealth <= 0 || bossLife.IsDead;
        return disableWhenBossDead ? !isBossDead : isBossDead;
    }
    
    /// <summary>
    /// 清理事件订阅
    /// </summary>
    private void OnDestroy()
    {
        if (bossLife != null)
        {
            bossLife.OnDeath -= OnBossDeath;
            bossLife.OnHealthChanged -= OnBossHealthChanged;
        }
    }
    

}
