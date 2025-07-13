using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public enum SorcererStateType
{
    Idle,
    Attack,
    Hit,
    Dead
}

[System.Serializable]
public class SorcererParameter
{
    [Header("Detection Settings")]
    public float detectionRange = 8f;
    public float attackRange = 6f;
    public Transform target;
    public LayerMask targetLayer;
    
    [Header("Attack Settings")]
    public Transform[] attackPoints;
    public float attackCooldown = 2f;
    public int damage = 1;
    public GameObject projectilePrefab;
    public float projectileSpeed = 5f;
    
    [Header("Components")]
    public Animator animator;
    
    [Header("State Flags")]
    public bool isHit = false;
    public bool isAttacking = false;
}

public class SorcererP : MonoBehaviour
{
    [Header("Sorcerer Health")]
    [SerializeField] public int maxHealth = 3;
    [SerializeField] public int currentHealth;

    [Header("Damage Response")]
    [SerializeField] private float damageFlashDuration = 0.2f;
    [SerializeField] private Color damageFlashColor = Color.red;

    [Header("Camera Shake Settings")]
    [SerializeField] private float shakeDuration = 0.15f;
    [SerializeField] private float shakeIntensity = 0.5f;
    [SerializeField] private CinemachineImpulseSource impulseSource;

    [Header("Projectile System")]
    [SerializeField] private SorcererProjectileLauncher projectileLauncher;
    [SerializeField] private SorcererProjectileConfig projectileConfig;
    [SerializeField] private bool useNewProjectileSystem = true;

    // Components
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Rigidbody2D rb;
    private Collider2D col;

    // Health properties
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    
    public SorcererParameter parameter;
    private IState currentState;
    private Dictionary<SorcererStateType, IState> states = new Dictionary<SorcererStateType, IState>();

    void Start()
    {
        InitializeComponents();
        InitializeStates();
        InitializeHealth();
        InitializeProjectileSystem();
        
        // Start in Idle state
        TransitionState(SorcererStateType.Idle);
    }

    void Update()
    {
        currentState?.OnUpdate();
    }

    private void InitializeComponents()
    {
        if (parameter == null)
            parameter = new SorcererParameter();

        parameter.animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        impulseSource = GetComponent<CinemachineImpulseSource>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        // Setup physics components
        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.gravityScale = 1f;
            rb.bodyType = RigidbodyType2D.Static; // Sorcerer doesn't move
        }

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;
            rb.gravityScale = 1f;
            rb.bodyType = RigidbodyType2D.Static;
            Debug.Log("Added Rigidbody2D to Sorcerer");
        }

        if (col == null)
        {
            col = gameObject.AddComponent<CapsuleCollider2D>();
            col.isTrigger = true;
            Debug.Log("Added CapsuleCollider2D to Sorcerer");
        }
    }

    private void InitializeProjectileSystem()
    {
        if (useNewProjectileSystem)
        {
            // Initialize new projectile launcher system
            if (projectileLauncher == null)
            {
                projectileLauncher = GetComponent<SorcererProjectileLauncher>();
                if (projectileLauncher == null)
                {
                    projectileLauncher = gameObject.AddComponent<SorcererProjectileLauncher>();
                    Debug.Log("Added SorcererProjectileLauncher to Sorcerer");
                }
            }

            // If no config is assigned, try to find one in Resources
            if (projectileConfig == null)
            {
                projectileConfig = Resources.Load<SorcererProjectileConfig>("SorcererProjectileConfig");
                if (projectileConfig == null)
                {
                    Debug.LogWarning("No SorcererProjectileConfig found! Please create one or assign it manually.");
                }
            }

            // Set up attack points if not already set
            if (parameter.attackPoints == null || parameter.attackPoints.Length == 0)
            {
                SetupAttackPoints();
            }
        }
        else
        {
            // Ensure old system has required components
            if (parameter.projectilePrefab == null)
            {
                Debug.LogWarning("Projectile prefab is not assigned! Please assign it in the inspector.");
            }

            if (parameter.attackPoints == null || parameter.attackPoints.Length == 0)
            {
                SetupAttackPoints();
            }
        }
    }

    private void SetupAttackPoints()
    {
        var attackPointsList = new List<Transform>();

        // Look for existing attack points
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.name.ToLower().Contains("attackpoint") || 
                child.name.ToLower().Contains("firepoint") ||
                child.name.ToLower().Contains("spawnpoint"))
            {
                attackPointsList.Add(child);
            }
        }

        // If no attack points found, create a default one
        if (attackPointsList.Count == 0)
        {
            GameObject attackPoint = new GameObject("AttackPoint");
            attackPoint.transform.SetParent(transform);
            attackPoint.transform.localPosition = new Vector3(1f, 0f, 0f); // Default position
            attackPointsList.Add(attackPoint.transform);
            Debug.Log("Created default attack point for Sorcerer");
        }

        parameter.attackPoints = attackPointsList.ToArray();
        Debug.Log($"Sorcerer initialized with {parameter.attackPoints.Length} attack points");
    }

    private void InitializeStates()
    {
        states.Add(SorcererStateType.Idle, new SorcererIdleState(this, parameter));
        states.Add(SorcererStateType.Attack, new SorcererAttackState(this, parameter));
        states.Add(SorcererStateType.Hit, new SorcererHitState(this, parameter));
        states.Add(SorcererStateType.Dead, new SorcererDeadState(this, parameter));
    }

    private void InitializeHealth()
    {
        currentHealth = maxHealth;
    }

    public void TransitionState(SorcererStateType type)
    {
        if (currentState != null)
            currentState.OnExit();
        
        currentState = states[type];
        currentState.OnEnter();
        
        Debug.Log($"Sorcerer state changed to: {type}");
    }

    public void FlipTo(Transform target)
    {
        if (target == null)
        {
            Debug.LogWarning("FlipTo called with null target!");
            return;
        }

        Vector3 direction = target.position - transform.position;
        float newScaleX = Mathf.Sign(direction.x) < 0 ? 1f : -1f;
        transform.localScale = new Vector3(newScaleX, 1f, 1f);
    }

    public float GetDistanceToPlayer()
    {
        if (parameter.target == null)
            return float.MaxValue;
        
        return Vector2.Distance(transform.position, parameter.target.position);
    }

    public bool IsPlayerInRange(float range)
    {
        return GetDistanceToPlayer() <= range;
    }

    public void FireProjectile()
    {
        Debug.Log("FireProjectile called");

        if (parameter.target == null)
        {
            Debug.LogWarning("Sorcerer: No target to fire at!");
            return;
        }

        if (useNewProjectileSystem)
        {
            FireProjectileNewSystem();
        }
        else
        {
            FireProjectileOldSystem();
        }
    }

    private void FireProjectileNewSystem()
    {
        

        if (projectileConfig == null)
        {
            Debug.LogError("Sorcerer: ProjectileConfig is not assigned!");
            return;
        }

        Debug.Log("Using new projectile system");

        // Calculate direction to player
        Vector2 direction = (parameter.target.position - transform.position).normalized;

        // Launch projectile using new system
        SorcererProjectile projectile = projectileLauncher.LaunchProjectile(direction, projectileConfig);

        if (projectile != null)
        {
            Debug.Log("Sorcerer successfully fired projectile using new system");
        }
        else
        {
            Debug.LogWarning("Failed to fire projectile using new system");
        }
    }

    private void FireProjectileOldSystem()
    {
        if (parameter.projectilePrefab == null || parameter.attackPoints == null || parameter.attackPoints.Length == 0)
        {
            Debug.LogWarning("Sorcerer: Missing projectile prefab or attack points!");
            return;
        }

        Debug.Log("Using old projectile system");

        // Use the first attack point or random one
        Transform attackPoint = parameter.attackPoints[0];
        if (parameter.attackPoints.Length > 1)
        {
            attackPoint = parameter.attackPoints[Random.Range(0, parameter.attackPoints.Length)];
        }

        // Calculate direction to player
        Vector2 direction = (parameter.target.position - attackPoint.position).normalized;

        // Create projectile
        GameObject projectile = Instantiate(parameter.projectilePrefab, attackPoint.position, Quaternion.identity);
        
        // Setup projectile immediately
        SorcererProjectile projectileScript = projectile.GetComponent<SorcererProjectile>();
        if (projectileScript != null)
        {
            // Use InitializeImmediate to avoid timing issues
            projectileScript.InitializeImmediate(parameter.damage, direction, parameter.projectileSpeed, parameter.targetLayer);
            Debug.Log("Sorcerer fired projectile using old system with SorcererProjectile component");
        }
        else
        {
            // Fallback: use Rigidbody2D
            Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();
            if (projectileRb != null)
            {
                projectileRb.velocity = direction * parameter.projectileSpeed;
                Debug.Log("Sorcerer fired projectile using old system with Rigidbody2D fallback");
            }
            else
            {
                Debug.LogError("Projectile prefab has no SorcererProjectile component or Rigidbody2D!");
            }
        }
    }

    #region Trigger Detection
    private void OnTriggerEnter2D(Collider2D other)
    {
        GameObject hero = GameObject.FindGameObjectWithTag("Player");
        
        // Detect player entering range
        if (other.CompareTag("Player"))
        {
            parameter.target = other.transform;
            
            if (currentState is SorcererIdleState && !parameter.isAttacking)
            {
                TransitionState(SorcererStateType.Attack);
            }
        }
        
        // Detect player attack
        if (other.CompareTag("PlayerAttack"))
        {
            AttackHitbox attackHitbox = other.GetComponent<AttackHitbox>();
            if (attackHitbox != null)
            {
                int damage = attackHitbox.damage;
                TakeDamage(damage);

                // Apply knockback effect
                if (hero != null)
                {
                    if (hero.transform.localRotation.y == 0)
                    {
                        GetComponent<Enemy>()?.GetHit(Vector2.right);
                    }
                    else if (hero.transform.localRotation.y == -1)
                    {
                        GetComponent<Enemy>()?.GetHit(Vector2.left);
                    }
                }
            }
            else
            {
                Debug.LogWarning("PlayerAttack object found but no AttackHitbox component!");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            float distance = GetDistanceToPlayer();
            
            // If player is still in detection range, keep attacking
            if (distance <= parameter.detectionRange)
            {
                // Continue attacking
                return;
            }
            else
            {
                // Player left range, return to idle
                parameter.target = null;
                if (currentState is SorcererAttackState)
                {
                    TransitionState(SorcererStateType.Idle);
                }
            }
        }
    }
    #endregion

    #region Damage System
    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return; // Already dead

        int previousHealth = currentHealth;
        currentHealth -= damage;
        parameter.isHit = true;

        // Trigger camera shake
        if (impulseSource != null)
        {
            impulseSource.GenerateImpulse();
        }
        else if (CameraShake.instance != null)
        {
            CameraShake.instance.ShakeCamera(shakeDuration, shakeIntensity);
        }

        // Start damage flash effect
        StartCoroutine(DamageFlash());

        // Ensure health doesn't go below 0
        currentHealth = Mathf.Max(0, currentHealth);

        if (currentHealth <= 0)
        {
            // Death state
            TransitionState(SorcererStateType.Dead);
        }
        else
        {
            // Hit state
            TransitionState(SorcererStateType.Hit);
        }

        Debug.Log($"Sorcerer took {damage} damage. Health: {currentHealth}/{maxHealth}");
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
    #endregion

    #region Public Methods for Testing
    /// <summary>
    /// Test method to manually fire projectile
    /// </summary>
    [ContextMenu("Test Fire Projectile")]
    public void TestFireProjectile()
    {
        // Find player for testing
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            parameter.target = player.transform;
            FireProjectile();
        }
        else
        {
            Debug.LogWarning("No player found for testing!");
        }
    }

    /// <summary>
    /// Toggle between old and new projectile system
    /// </summary>
    [ContextMenu("Toggle Projectile System")]
    public void ToggleProjectileSystem()
    {
        useNewProjectileSystem = !useNewProjectileSystem;
        Debug.Log($"Projectile system switched to: {(useNewProjectileSystem ? "New" : "Old")} system");
        InitializeProjectileSystem();
    }
    #endregion

    #region Gizmos
    private void OnDrawGizmos()
    {
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, parameter.detectionRange);

        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, parameter.attackRange);

        // Draw attack points
        if (parameter.attackPoints != null)
        {
            Gizmos.color = Color.blue;
            foreach (Transform attackPoint in parameter.attackPoints)
            {
                if (attackPoint != null)
                {
                    Gizmos.DrawWireSphere(attackPoint.position, 0.2f);
                    Gizmos.DrawLine(transform.position, attackPoint.position);
                }
            }
        }

        // Draw line to target
        if (parameter.target != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, parameter.target.position);
        }
    }
    #endregion
}
