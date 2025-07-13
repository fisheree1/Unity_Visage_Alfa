using System.Collections;
using UnityEngine;

/// <summary>
/// Handles projectile creation and firing for Sorcerer characters
/// Integrates with object pool and configuration system
/// </summary>
public class SorcererProjectileLauncher : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private SorcererProjectileConfig defaultConfig;
    [SerializeField] private bool useObjectPool = true;
    [SerializeField] private LayerMask targetLayers = -1;
    
    [Header("Launch Settings")]
    [SerializeField] private Transform[] launchPoints;
    [SerializeField] private float launchCooldown = 0.5f;
    [SerializeField] private bool autoFindLaunchPoints = true;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private bool playLaunchSound = true;
    
    // State
    private float lastLaunchTime = 0f;
    private SorcererProjectilePool projectilePool;
    
    // Events
    public System.Action<SorcererProjectile> OnProjectileLaunched;
    public System.Action<Vector2> OnLaunchAttempt;
    
    void Start()
    {
        InitializeLauncher();
    }
    
    private void InitializeLauncher()
    {
        // Find project pool if using object pooling
        if (useObjectPool)
        {
            projectilePool = SorcererProjectilePool.Instance;
            if (projectilePool == null)
            {
                Debug.LogWarning("Object pool not found, creating projectiles directly");
                useObjectPool = false;
            }
        }
        
        // Auto-find launch points if enabled
        if (autoFindLaunchPoints)
        {
            FindLaunchPoints();
        }
        
        // Setup audio source
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        
        // Validate configuration
        if (defaultConfig == null)
        {
            Debug.LogWarning($"No default projectile config assigned to {gameObject.name}");
        }
    }
    
    private void FindLaunchPoints()
    {
        // Look for child objects with "LaunchPoint" in their name
        var foundPoints = new System.Collections.Generic.List<Transform>();
        
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.name.ToLower().Contains("launchpoint") || 
                child.name.ToLower().Contains("spawnpoint") ||
                child.name.ToLower().Contains("firepoint"))
            {
                foundPoints.Add(child);
            }
        }
        
        if (foundPoints.Count > 0)
        {
            launchPoints = foundPoints.ToArray();
            Debug.Log($"Found {launchPoints.Length} launch points automatically");
        }
        else
        {
            // Use transform as default launch point
            launchPoints = new Transform[] { transform };
            Debug.Log("No launch points found, using transform as default");
        }
    }
    
    /// <summary>
    /// Launch a projectile in the specified direction
    /// </summary>
    /// <param name="direction">Direction to launch the projectile</param>
    /// <param name="config">Optional config override</param>
    /// <returns>The launched projectile, or null if launch failed</returns>
    public SorcererProjectile LaunchProjectile(Vector2 direction, SorcererProjectileConfig config = null)
    {
        if (!CanLaunch())
        {
            return null;
        }
        
        // Use provided config or default
        var projectileConfig = config ?? defaultConfig;
        if (projectileConfig == null)
        {
            Debug.LogError("No projectile configuration available for launch");
            return null;
        }
        
        // Get launch point
        Transform launchPoint = GetLaunchPoint();
        if (launchPoint == null)
        {
            Debug.LogError("No launch point available");
            return null;
        }
        
        // Create projectile
        SorcererProjectile projectile = CreateProjectile(launchPoint.position, projectileConfig);
        if (projectile == null)
        {
            Debug.LogError("Failed to create projectile");
            return null;
        }
        
        // Initialize projectile
        InitializeProjectile(projectile, direction, projectileConfig);
        
        // Play launch sound
        if (playLaunchSound && audioSource != null && projectileConfig.fireSound != null)
        {
            audioSource.PlayOneShot(projectileConfig.fireSound, projectileConfig.audioVolume);
        }
        
        // Update cooldown
        lastLaunchTime = Time.time;
        
        // Fire events
        OnProjectileLaunched?.Invoke(projectile);
        OnLaunchAttempt?.Invoke(direction);
        
        return projectile;
    }
    
    /// <summary>
    /// Launch projectile towards a target
    /// </summary>
    /// <param name="target">Target to aim at</param>
    /// <param name="config">Optional config override</param>
    /// <returns>The launched projectile, or null if launch failed</returns>
    public SorcererProjectile LaunchProjectileAtTarget(Transform target, SorcererProjectileConfig config = null)
    {
        if (target == null)
        {
            Debug.LogWarning("Cannot launch projectile at null target");
            return null;
        }
        
        Vector2 direction = (target.position - transform.position).normalized;
        return LaunchProjectile(direction, config);
    }
    
    /// <summary>
    /// Launch multiple projectiles in different directions
    /// </summary>
    /// <param name="directions">Array of directions to launch projectiles</param>
    /// <param name="config">Optional config override</param>
    /// <returns>Array of launched projectiles</returns>
    public SorcererProjectile[] LaunchProjectileSpread(Vector2[] directions, SorcererProjectileConfig config = null)
    {
        if (directions == null || directions.Length == 0)
        {
            return new SorcererProjectile[0];
        }
        
        var projectiles = new SorcererProjectile[directions.Length];
        
        for (int i = 0; i < directions.Length; i++)
        {
            projectiles[i] = LaunchProjectile(directions[i], config);
            
            // Small delay between shots if launching multiple
            if (i < directions.Length - 1)
            {
                StartCoroutine(DelayedLaunch(directions[i + 1], config, 0.1f * (i + 1)));
            }
        }
        
        return projectiles;
    }
    
    private IEnumerator DelayedLaunch(Vector2 direction, SorcererProjectileConfig config, float delay)
    {
        yield return new WaitForSeconds(delay);
        LaunchProjectile(direction, config);
    }
    
    /// <summary>
    /// Check if launcher can currently fire
    /// </summary>
    /// <returns>True if launcher is ready to fire</returns>
    public bool CanLaunch()
    {
        return Time.time >= lastLaunchTime + launchCooldown;
    }
    
    private SorcererProjectile CreateProjectile(Vector3 position, SorcererProjectileConfig config)
    {
        SorcererProjectile projectile = null;
        
        if (useObjectPool && projectilePool != null)
        {
            // Get from pool
            projectile = projectilePool.GetProjectile();
            if (projectile != null)
            {
                projectile.transform.position = position;
                projectile.transform.rotation = Quaternion.identity;
            }
        }
        else
        {
            // Create new projectile
            GameObject projectileObj = new GameObject("Sorcerer Projectile");
            projectileObj.transform.position = position;
            
            // Add SorcererProjectilePrefab component for setup
            SorcererProjectilePrefab prefabComponent = projectileObj.AddComponent<SorcererProjectilePrefab>();
            
            // Configure the prefab component
            if (config != null)
            {
                // We would need to expose a way to set the config on the prefab component
                // For now, it will use its default settings
            }
            
            // Setup the projectile
            prefabComponent.SetupProjectile();
            projectile = prefabComponent.GetProjectileComponent();
        }
        
        return projectile;
    }
    
    private void InitializeProjectile(SorcererProjectile projectile, Vector2 direction, SorcererProjectileConfig config)
    {
        if (projectile == null || config == null) return;
        
        // Initialize projectile with config settings
        System.Action<SorcererProjectile> returnCallback = useObjectPool && projectilePool != null ? 
            projectilePool.ReturnProjectile : null;
        
        // Use InitializeImmediate to avoid timing issues
        projectile.InitializeImmediate(
            config.damage, 
            direction, 
            config.speed, 
            targetLayers, 
            returnCallback
        );
        
        // Enable homing if configured
        if (config.enableHoming)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                projectile.EnableHoming(player.transform, config.homingStrength);
            }
        }
    }
    
    private Transform GetLaunchPoint()
    {
        if (launchPoints == null || launchPoints.Length == 0)
        {
            return transform;
        }
        
        // For now, return a random launch point
        // Could be extended to use specific logic (closest to target, etc.)
        int randomIndex = Random.Range(0, launchPoints.Length);
        return launchPoints[randomIndex];
    }
    
    /// <summary>
    /// Set launch cooldown
    /// </summary>
    /// <param name="cooldown">New cooldown time</param>
    public void SetLaunchCooldown(float cooldown)
    {
        launchCooldown = Mathf.Max(0f, cooldown);
    }
    
    /// <summary>
    /// Set target layers for projectiles
    /// </summary>
    /// <param name="layers">New target layer mask</param>
    public void SetTargetLayers(LayerMask layers)
    {
        targetLayers = layers;
    }
    
    /// <summary>
    /// Get remaining cooldown time
    /// </summary>
    /// <returns>Remaining cooldown time in seconds</returns>
    public float GetRemainingCooldown()
    {
        return Mathf.Max(0f, (lastLaunchTime + launchCooldown) - Time.time);
    }
    
    // Validation and debugging
    void OnValidate()
    {
        launchCooldown = Mathf.Max(0f, launchCooldown);
    }
    
    void OnDrawGizmosSelected()
    {
        if (launchPoints != null)
        {
            Gizmos.color = Color.yellow;
            foreach (var point in launchPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, 0.2f);
                    Gizmos.DrawRay(point.position, point.right * 2f);
                }
            }
        }
    }
}