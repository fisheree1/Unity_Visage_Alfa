using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SorcererProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    private int damage = 1;
    private float speed = 5f;
    private Vector2 direction;
    private LayerMask targetLayer;
    
    [Header("Projectile Properties")]
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private bool destroyOnHit = true;
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private bool canHitMultipleTargets = false;
    
    [Header("Visual Effects")]
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private ParticleSystem particles;
    
    // Cached components
    private Rigidbody2D rb;
    private Collider2D col;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    
    // State tracking
    private bool isInitialized = false;
    private bool hasHitTarget = false;
    private HashSet<GameObject> hitTargets = new HashSet<GameObject>();
    
    // Object pooling support
    private bool isPooled = false;
    private System.Action<SorcererProjectile> returnToPool;
    
    // Lifecycle timer
    private float currentLifetime;
    
    // Initialization delay handling
    private bool initializationPending = false;
    private float initializationDelay = 0.1f; // Small delay to allow proper initialization
    
    void Awake()
    {
        CacheComponents();
    }
    
    void Start()
    {
        // Give a small delay for initialization if not already initialized
        if (!isInitialized && !initializationPending)
        {
            initializationPending = true;
            StartCoroutine(WaitForInitialization());
        }
    }
    
    private IEnumerator WaitForInitialization()
    {
        float waitTime = 0f;
        
        // Wait for initialization or timeout
        while (!isInitialized && waitTime < initializationDelay)
        {
            yield return null;
            waitTime += Time.deltaTime;
        }
        
        initializationPending = false;
        
        // If still not initialized after waiting, destroy or handle gracefully
        if (!isInitialized)
        {
            // Check if this is a prefab component that doesn't need initialization
            if (IsPrefabComponent())
            {
                Debug.Log("SorcererProjectile is a prefab component, skipping initialization requirement");
                yield break; // Exit coroutine properly
            }
            
            Debug.LogWarning($"SorcererProjectile on {gameObject.name} was not properly initialized within {initializationDelay} seconds!");
            
            // Try auto-initialization with default values
            if (TryAutoInitialize())
            {
                Debug.Log("Successfully auto-initialized SorcererProjectile with default values");
            }
            else
            {
                Debug.LogError("Auto-initialization failed, destroying projectile");
                Destroy(gameObject);
            }
        }
    }
    
    private bool IsPrefabComponent()
    {
        // Check if this is a prefab asset or prefab component that doesn't need runtime initialization
        return gameObject.scene.name == null || gameObject.scene.name == "";
    }
    
    private bool TryAutoInitialize()
    {
        try
        {
            // Try to find player for auto-targeting
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            Vector2 direction = player != null ? 
                (player.transform.position - transform.position).normalized : 
                Vector2.right;
            
            LayerMask playerLayer = LayerMask.GetMask("Player");
            
            // Auto-initialize with default values
            Initialize(1, direction, 5f, playerLayer);
            Debug.Log("Auto-initialized SorcererProjectile with default values");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Auto-initialization failed: {e.Message}");
            return false;
        }
    }
    
    void Update()
    {
        if (!isInitialized) return;
        
        UpdateMovement();
        UpdateRotation();
        UpdateLifetime();
    }
    
    private void CacheComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        
        // Cache trail and particles if not assigned
        if (trail == null) trail = GetComponent<TrailRenderer>();
        if (particles == null) particles = GetComponent<ParticleSystem>();
    }
    
    public void Initialize(int damage, Vector2 direction, float speed, LayerMask targetLayer, System.Action<SorcererProjectile> returnToPoolCallback = null)
    {
        // Store parameters
        this.damage = damage;
        this.direction = direction.normalized;
        this.speed = speed;
        this.targetLayer = targetLayer;
        this.returnToPool = returnToPoolCallback;
        this.isPooled = returnToPoolCallback != null;
        
        // Reset state
        ResetState();
        
        // Set up physics
        SetupPhysics();
        
        // Set up collider
        SetupCollider();
        
        // Start lifetime countdown
        currentLifetime = lifetime;
        isInitialized = true;
        
        Debug.Log($"Sorcerer projectile initialized - Damage: {damage}, Direction: {direction}, Speed: {speed}");
    }
    
    /// <summary>
    /// Immediate initialization without waiting - for use right after instantiation
    /// </summary>
    public void InitializeImmediate(int damage, Vector2 direction, float speed, LayerMask targetLayer, System.Action<SorcererProjectile> returnToPoolCallback = null)
    {
        // Stop any pending initialization coroutine
        if (initializationPending)
        {
            StopAllCoroutines();
            initializationPending = false;
        }
        
        Initialize(damage, direction, speed, targetLayer, returnToPoolCallback);
    }
    
    private void ResetState()
    {
        hasHitTarget = false;
        hitTargets.Clear();
        currentLifetime = lifetime;
        
        // Reset visual effects
        if (trail != null)
        {
            trail.Clear();
            trail.emitting = true;
        }
        
        if (particles != null)
        {
            particles.Play();
        }
        
        // Reset renderer
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        
        // Reset collider
        if (col != null)
        {
            col.enabled = true;
        }
    }
    
    private void SetupPhysics()
    {
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.drag = 0f;
            rb.angularDrag = 0f;
            rb.velocity = direction * speed;
        }
    }
    
    private void SetupCollider()
    {
        if (col != null)
        {
            col.isTrigger = true;
        }
    }
    
    private void UpdateMovement()
    {
        // Keep moving in the direction (fallback if no Rigidbody2D)
        if (rb == null)
        {
            transform.Translate(direction * speed * Time.deltaTime);
        }
        else
        {
            // Ensure consistent velocity
            rb.velocity = direction * speed;
        }
    }
    
    private void UpdateRotation()
    {
        // Rotate to face movement direction
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }
    
    private void UpdateLifetime()
    {
        currentLifetime -= Time.deltaTime;
        if (currentLifetime <= 0f)
        {
            DestroyProjectile();
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isInitialized || other == null) return;
        
        // Check if it's a valid target
        if (IsValidTarget(other))
        {
            HandleTargetHit(other);
        }
        // Check for walls/obstacles
        else if (IsObstacle(other))
        {
            HandleObstacleHit();
        }
    }
    
    private bool IsValidTarget(Collider2D other)
    {
        // Check if we've already hit this target
        if (!canHitMultipleTargets && hitTargets.Contains(other.gameObject))
        {
            return false;
        }
        
        // Check if it's the player
        if (other.CompareTag("Player"))
        {
            return true;
        }
        
        // Check layer mask
        if (targetLayer.value != -1 && (targetLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            return true;
        }
        
        return false;
    }
    
    private bool IsObstacle(Collider2D other)
    {
        return other.CompareTag("Ground") || other.CompareTag("Wall") || other.CompareTag("Obstacle");
    }
    
    private void HandleTargetHit(Collider2D other)
    {
        // Deal damage to player
        HeroLife heroLife = other.GetComponent<HeroLife>();
        if (heroLife == null)
        {
            // Fallback: try to find HeroLife in parent
            heroLife = other.GetComponentInParent<HeroLife>();
        }
        
        if (heroLife != null)
        {
            heroLife.TakeDamage(damage);
            Debug.Log($"Sorcerer projectile hit player for {damage} damage");
        }
        else
        {
            Debug.LogWarning($"Hit target {other.name} but no HeroLife component found!");
        }
        
        // Add to hit targets
        hitTargets.Add(other.gameObject);
        hasHitTarget = true;
        
        // Spawn hit effect
        SpawnHitEffect();
        
        // Stop movement
        StopMovement();
        
        // Destroy projectile if configured to do so
        if (destroyOnHit)
        {
            DestroyProjectile();
        }
    }
    
    private void HandleObstacleHit()
    {
        Debug.Log("Sorcerer projectile hit obstacle");
        
        // Spawn hit effect
        SpawnHitEffect();
        
        // Destroy projectile
        DestroyProjectile();
    }
    
    private void SpawnHitEffect()
    {
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }
    }
    
    private void StopMovement()
    {
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }
    
    private void DestroyProjectile()
    {
        // Stop visual effects
        StopVisualEffects();
        
        // Return to pool or destroy
        if (isPooled && returnToPool != null)
        {
            returnToPool(this);
        }
        else
        {
            Destroy(gameObject, 0.1f); // Small delay to allow effects to finish
        }
    }
    
    private void StopVisualEffects()
    {
        // Stop particle systems
        if (particles != null)
        {
            particles.Stop();
        }
        
        // Stop trail
        if (trail != null)
        {
            trail.emitting = false;
        }
        
        // Disable renderer
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
        
        // Disable collider
        if (col != null)
        {
            col.enabled = false;
        }
    }
    
    // Optional: Add homing behavior
    public void EnableHoming(Transform target, float homingStrength = 2f)
    {
        if (target != null && isInitialized)
        {
            StartCoroutine(HomingBehavior(target, homingStrength));
        }
    }
    
    private IEnumerator HomingBehavior(Transform target, float homingStrength)
    {
        while (target != null && !hasHitTarget && gameObject != null && isInitialized)
        {
            Vector2 directionToTarget = (target.position - transform.position).normalized;
            direction = Vector2.Lerp(direction, directionToTarget, homingStrength * Time.deltaTime);
            
            if (rb != null)
            {
                rb.velocity = direction * speed;
            }
            
            yield return null;
        }
    }
    
    // Public methods for external control
    public void SetLifetime(float newLifetime)
    {
        this.lifetime = newLifetime;
        this.currentLifetime = newLifetime;
    }
    
    public void ModifySpeed(float speedMultiplier)
    {
        this.speed *= speedMultiplier;
        if (rb != null)
        {
            rb.velocity = direction * speed;
        }
    }
    
    public void ChangeDirection(Vector2 newDirection)
    {
        this.direction = newDirection.normalized;
        if (rb != null)
        {
            rb.velocity = direction * speed;
        }
    }
    
    // Getters for debugging
    public bool IsInitialized => isInitialized;
    public Vector2 Direction => direction;
    public float Speed => speed;
    public int Damage => damage;
    public float RemainingLifetime => currentLifetime;
    
    // Manual initialization for testing
    [ContextMenu("Test Initialize")]
    private void TestInitialize()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Vector2 direction = player != null ? 
            (player.transform.position - transform.position).normalized : 
            Vector2.right;
        
        LayerMask playerLayer = LayerMask.GetMask("Player");
        InitializeImmediate(1, direction, 5f, playerLayer);
    }
}