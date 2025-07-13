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
        if (!isDead && transform.position.y < -200f) // 你的 -200000f 太夸张了
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

        if (useCheckPointRespawn)
        {
            StartCoroutine(CheckPointRespawn());
        }
        else
        {
            if (gameOverUI != null)
                StartCoroutine(ShowGameOverUI());
            
            StartCoroutine(AutoRestart());
        }
    }

    private IEnumerator ShowGameOverUI()
    {
        yield return new WaitForSeconds(deathAnimationDuration * 0.5f);
        if (gameOverUI != null) gameOverUI.SetActive(true);
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
            // 使用CheckPoint复活
            Respawn();
        }
        else
        {
            // 没有复活点，显示游戏结束UI或重启游戏
            if (showGameOverOnNoCheckPoint && gameOverUI != null)
            {
                StartCoroutine(ShowGameOverUI());
            }
            else
            {
                yield return new WaitForSeconds(respawnDelay);
                RestartGame();
            }
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
        if (gameOverUI != null) gameOverUI.SetActive(false);

        // 步骤8：触发事件
        OnRespawn?.Invoke();
        OnHealthChanged?.Invoke(currentHealth);

        // 步骤9：可选触发复活动画
        yield return null;
        
        if (anim != null)
        {
            anim.SetTrigger("Respawn");
        }
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
}
