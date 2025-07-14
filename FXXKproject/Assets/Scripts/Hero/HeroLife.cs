using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HeroLife : MonoBehaviour
{
    [Header("Hero Health Settings")]
    [SerializeField] public int maxHealth = 3;
    [SerializeField] public int currentHealth;

    [Header("Damage Response")]
    [SerializeField] private float damageFlashDuration = 0.2f;
    [SerializeField] private Color damageFlashColor = Color.red;
    [SerializeField] private float invulnerabilityTime = 0.5f;

    [Header("Death Settings")]
    [SerializeField] private float deathAnimationDuration = 2.0f;
    [SerializeField] private float respawnDelay = 1.0f;
    [SerializeField] private bool useCheckPointRespawn = true;
    [SerializeField] private bool showGameOverOnNoCheckPoint = true;

    [Header("UI References")]
    [SerializeField] private GameObject gameOverUI;

    // Components
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private HeroMovement heroMovement;
    private HeroAttackController attackController;
    private Rigidbody2D rb;

    // State
    private bool isDead = false;
    private bool isInvulnerable = false;
    private Color originalColor;
    private Vector3 respawnPosition;

    // Properties
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => isDead;
    public bool IsInvulnerable => isInvulnerable;

    // Events
    public System.Action<int> OnHealthChanged;
    public System.Action OnDeath;
    public System.Action OnRespawn;
    public static System.Action OnPlayerRespawned; // 静态事件，用于通知所有游戏对象玩家复活

    void Start()
    {
        currentHealth = maxHealth;

        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        heroMovement = GetComponent<HeroMovement>();
        attackController = GetComponent<HeroAttackController>();
        rb = GetComponent<Rigidbody2D>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        respawnPosition = transform.position;

        OnHealthChanged?.Invoke(currentHealth);
    }

    void Update()
    {
        // 死亡时按R键重启游戏
        if (isDead && Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
        
        // 检查坠落死亡
        CheckFallDeath();
    }

    private void CheckFallDeath()
    {
        if (!isDead && transform.position.y < -20000f) // 你的 -200000f 太夸张了
        {
            TakeDamage(maxHealth);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead || isInvulnerable) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        OnHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(DamageResponse());
        }
    }

    private IEnumerator DamageResponse()
    {
        isInvulnerable = true;
        StartCoroutine(DamageFlash());
        yield return new WaitForSeconds(invulnerabilityTime);
        isInvulnerable = false;
    }

    private IEnumerator DamageFlash()
    {
        if (spriteRenderer != null)
        {
            float flashInterval = 0.1f;
            float elapsedTime = 0f;

            while (elapsedTime < damageFlashDuration)
            {
                spriteRenderer.color = damageFlashColor;
                yield return new WaitForSeconds(flashInterval);
                spriteRenderer.color = originalColor;
                yield return new WaitForSeconds(flashInterval);
                elapsedTime += flashInterval * 2;
            }

            spriteRenderer.color = originalColor;
        }
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;

        // 禁用移动和攻击控制器
        if (heroMovement != null) heroMovement.enabled = false;
        if (attackController != null) attackController.enabled = false;

        // 停止物理运动
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        // 设置死亡动画
        if (anim != null)
        {
            // 重置其他动画参数
            anim.SetBool("IsRunning", false);
            anim.SetBool("IsJumping", false);
            anim.SetBool("IsAttacking", false);
            anim.SetInteger("Movements", 0);
            anim.SetFloat("Speed", 0f);
            anim.SetFloat("VelocityY", 0f);
            
            // 设置死亡状态
            anim.SetBool("IsDead", true);
            anim.SetTrigger("Death");
        }

        OnDeath?.Invoke();

        // 总是显示GameOverUI
        if (gameOverUI != null)
        {
            StartCoroutine(ShowGameOverUI());
        }

        // 然后根据设置决定后续行为
        if (useCheckPointRespawn)
        {
            StartCoroutine(CheckPointRespawn());
        }
        else
        {
            StartCoroutine(AutoRestart());
        }
    }
    public void RespawnAfterDead()
    {
        if (useCheckPointRespawn)
        {
            StartCoroutine(CheckPointRespawn());
        }
    }

    private IEnumerator ShowGameOverUI()
    {
        Debug.Log("开始显示GameOverUI...");
        yield return new WaitForSeconds(deathAnimationDuration * 0.5f);
        
        if (gameOverUI != null) 
        {
            Debug.Log("激活GameOverUI对象");
            gameOverUI.SetActive(true);
            
            // 尝试调用GameOverUI脚本的Show方法
            var gameOverUIScript = gameOverUI.GetComponent<MonoBehaviour>();
            if (gameOverUIScript != null && gameOverUIScript.GetType().Name == "GameOverUI")
            {
                Debug.Log("调用GameOverUI.Show()方法");
                gameOverUIScript.SendMessage("Show", SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                Debug.LogWarning("未找到GameOverUI脚本组件，只激活了GameObject");
            }
        }
        else
        {
            Debug.LogError("GameOverUI引用为空！无法显示GameOverUI");
        }
    }

    private IEnumerator AutoRestart()
    {
        yield return new WaitForSeconds(deathAnimationDuration + respawnDelay);
        RestartGame();
    }

    private IEnumerator CheckPointRespawn()
    {
        yield return new WaitForSeconds(deathAnimationDuration);
        
        // 检查是否有设置复活点
        if (respawnPosition != Vector3.zero)
        {
            // 有复活点，等待玩家在GameOverUI中选择
            // GameOverUI会显示复活按钮，玩家可以选择复活或重启
            Debug.Log("有复活点可用，等待玩家选择...");
        }
        else
        {
            // 没有复活点，等待一段时间后自动重启（如果没有显示GameOverUI）
            if (!showGameOverOnNoCheckPoint)
            {
                yield return new WaitForSeconds(respawnDelay);
                RestartGame();
            }
            // 如果showGameOverOnNoCheckPoint为true，GameOverUI已经显示了
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Respawn()
    {
        // 开始复活流程
        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        // 步骤1：立即重置死亡状态
        isDead = false;
        isInvulnerable = false;
        currentHealth = maxHealth;

        // 步骤2：重置物理
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = false;
        }

        // 步骤3：重置位置
        transform.position = respawnPosition;

        // 步骤4：重置渲染
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        // 步骤5：重置动画
        if (anim != null)
        {
            // 清除所有必要参数
            anim.Play("Idle", 0, 0f);
            anim.SetInteger("Movements", 0);
            anim.SetBool("IsAttacking", false);
            anim.SetBool("IsDead", false);
            anim.SetFloat("Speed", 0f);
            anim.SetFloat("VelocityY", 0f);

            // 等几帧让动画状态机稳定
            for (int i = 0; i < 8; i++)
            {
                yield return null;
            }
        }

        // 步骤6：启用角色控制
        if (heroMovement != null) heroMovement.enabled = true;
        if (attackController != null) attackController.enabled = true;

        // 步骤7：隐藏死亡UI
        if (gameOverUI != null) 
        {
            gameOverUI.SetActive(false);
            
            // 尝试调用GameOverUI脚本的Hide方法
            var gameOverUIScript = gameOverUI.GetComponent<MonoBehaviour>();
            if (gameOverUIScript != null && gameOverUIScript.GetType().Name == "GameOverUI")
            {
                gameOverUIScript.SendMessage("Hide", SendMessageOptions.DontRequireReceiver);
            }
        }

        // 步骤8：触发事件
        OnRespawn?.Invoke();
        OnHealthChanged?.Invoke(currentHealth);
        
        // 触发静态复活事件，通知所有游戏对象
        OnPlayerRespawned?.Invoke();
        Debug.Log("触发玩家复活事件，通知所有游戏对象重置状态");

        // 步骤9：可选触发复活动画
        yield return null;
        
        if (anim != null)
        {
            anim.SetTrigger("Respawn");
        }

        // 触发静态复活事件
        OnPlayerRespawned?.Invoke();
    }


    public void Heal(int healAmount)
    {
        if (isDead) return;

        currentHealth += healAmount;
        currentHealth = Mathf.Min(maxHealth, currentHealth);
        OnHealthChanged?.Invoke(currentHealth);
    }

    /// <summary>
    /// 永久增加最大血量上限
    /// </summary>
    /// <param name="amount">要增加的血量值</param>
    public void IncreaseMaxHealth(int amount)
    {
        if (isDead) return;

        maxHealth += amount;
        currentHealth += amount; // 增加当前血量，相当于同时治疗
        
        // 确保当前血量不超过最大血量
        currentHealth = Mathf.Min(maxHealth, currentHealth);
        
        OnHealthChanged?.Invoke(currentHealth);
        
        Debug.Log($"玩家血量上限增加了 {amount}，当前血量: {currentHealth}/{maxHealth}");
    }

    /// <summary>
    /// 设置最大血量（用于存档/载入等场景）
    /// </summary>
    /// <param name="newMaxHealth">新的最大血量值</param>
    public void SetMaxHealth(int newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth);
        OnHealthChanged?.Invoke(currentHealth);
    }

    public void SetRespawnPoint(Vector3 newRespawnPoint)
    {
        respawnPosition = newRespawnPoint;
    }
    
    // 获取当前复活点
    public Vector3 GetRespawnPoint()
    {
        return respawnPosition;
    }
    
    // 检查是否有有效的复活点
    public bool HasValidRespawnPoint()
    {
        return respawnPosition != Vector3.zero;
    }

    /// <summary>
    /// 调试方法：检查GameOverUI设置
    /// </summary>
    [ContextMenu("调试GameOverUI设置")]
    public void DebugGameOverUISettings()
    {
        Debug.Log("=== GameOverUI 调试信息 ===");
        Debug.Log($"useCheckPointRespawn: {useCheckPointRespawn}");
        Debug.Log($"showGameOverOnNoCheckPoint: {showGameOverOnNoCheckPoint}");
        Debug.Log($"gameOverUI引用: {(gameOverUI != null ? "已设置" : "未设置")}");
        Debug.Log($"当前血量: {currentHealth}/{maxHealth}");
        Debug.Log($"是否死亡: {isDead}");
        Debug.Log($"复活点位置: {respawnPosition}");
        Debug.Log($"是否有有效复活点: {HasValidRespawnPoint()}");
        
        if (gameOverUI != null)
        {
            Debug.Log($"GameOverUI对象名称: {gameOverUI.name}");
            Debug.Log($"GameOverUI是否激活: {gameOverUI.activeInHierarchy}");
            
            var gameOverUIScript = gameOverUI.GetComponent<GameOverUI>();
            if (gameOverUIScript != null)
            {
                Debug.Log("找到GameOverUI脚本组件");
            }
            else
            {
                Debug.LogWarning("未找到GameOverUI脚本组件！");
            }
        }
        else
        {
            Debug.LogError("gameOverUI引用为空！请在Inspector中设置gameOverUI字段。");
        }
    }
    
    /// <summary>
    /// 调试方法：强制显示GameOverUI
    /// </summary>
    [ContextMenu("强制显示GameOverUI")]
    public void ForceShowGameOverUI()
    {
        if (gameOverUI != null)
        {
            StartCoroutine(ShowGameOverUI());
            Debug.Log("强制显示GameOverUI");
        }
        else
        {
            Debug.LogError("无法显示GameOverUI：gameOverUI引用为空！");
        }
    }
    
    /// <summary>
    /// 调试方法：测试死亡
    /// </summary>
    [ContextMenu("测试死亡")]
    public void TestDeath()
    {
        Debug.Log("测试死亡功能...");
        TakeDamage(maxHealth);
    }
}
