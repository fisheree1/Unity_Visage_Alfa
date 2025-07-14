using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;

public class BossLife : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 300;
    [SerializeField] private int currentHealth;
    
    // Events for UI updates
    public event Action<int, int> OnHealthChanged; // currentHealth, maxHealth
    public event Action OnDeath;

    [Header("Damage Response")]
    [SerializeField] private float damageFlashDuration = 0.2f;
    [SerializeField] private Color damageFlashColor = Color.red;
    [SerializeField] private float invulnerabilityTime = 0.8f;

    [Header("Death Settings")]
    [SerializeField] private float deathAnimationDuration = 2.0f;
    [SerializeField] private GameObject deathEffect;

    [Header("UI References")]
    [SerializeField] private Slider healthBar;

    [Header("Drop Settings")]
    [SerializeField] private GameObject[] possibleDrops;
    [SerializeField] private float dropChance = 0.8f;

    // Components
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private BossMove bossMove;
    private Color originalColor;
    private bool isDead = false;
    private bool isInvulnerable = false;

    // Properties
    public bool IsDead => isDead;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsInvulnerable => isInvulnerable;

    private void Start()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        bossMove = GetComponent<BossMove>();
        
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
            
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(int damage)
    {
        if (isDead || isInvulnerable)
            return;
            
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        UpdateHealthBar();
        
        // 触发血量变化事件
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        // Flash red when taking damage
        if (spriteRenderer != null)
            StartCoroutine(DamageFlashEffect());
            
        // Check if boss is hurt and apply hurt animation
        if (bossMove != null)
            bossMove.TakeHurt();

        // Start invulnerability
        StartCoroutine(InvulnerabilitySequence());
            
        // Check if boss is dead
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.value = (float)currentHealth / maxHealth;
        }
    }

    private IEnumerator InvulnerabilitySequence()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerabilityTime);
        isInvulnerable = false;
    }

    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        
        // 触发死亡事件
        OnDeath?.Invoke();
        
        // Spawn death effect if available
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        
        // Disable components to prevent further actions
        if (bossMove != null)
            bossMove.enabled = false;
            
        if (GetComponent<BossAttack>() != null)
            GetComponent<BossAttack>().enabled = false;
            
        // Disable collider to prevent further interactions
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;
            
        // Drop item with specified chance
        if (possibleDrops.Length > 0 && UnityEngine.Random.value <= dropChance)
        {
            int dropIndex = UnityEngine.Random.Range(0, possibleDrops.Length);
            Instantiate(possibleDrops[dropIndex], transform.position, Quaternion.identity);
        }
        
        // Destroy boss after delay
        Destroy(gameObject, deathAnimationDuration);
    }

    private IEnumerator DamageFlashEffect()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = damageFlashColor;
            yield return new WaitForSeconds(damageFlashDuration);
            spriteRenderer.color = originalColor;
        }
    }

    public void Heal(int amount)
    {
        if (isDead)
            return;
            
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateHealthBar();
        
        // 触发血量变化事件
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}