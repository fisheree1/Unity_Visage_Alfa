using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Improved helper script for creating Sorcerer projectile prefabs
/// This script focuses on setup and configuration, delegating runtime behavior to other components
/// </summary>
public class SorcererProjectilePrefab : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private SorcererProjectileConfig config;
    [SerializeField] private bool useConfigDefaults = true;
    
    [Header("Override Settings (if not using config)")]
    [SerializeField] private Sprite projectileSprite;
    [SerializeField] private Color projectileColor = Color.blue;
    [SerializeField] private float projectileSize = 0.5f;
    [SerializeField] private bool addTrail = true;
    [SerializeField] private bool addParticles = true;
    [SerializeField] private bool addGlow = true;
    
    [Header("Audio")]
    [SerializeField] private AudioClip fireSound;
    [SerializeField] private AudioClip hitSound;
    
    [Header("Setup Options")]
    [SerializeField] private bool autoSetupOnAwake = true;
    [SerializeField] private bool validateComponents = true;
    
    // Cached components
    private SorcererProjectile projectileComponent;
    private Rigidbody2D rbComponent;
    private Collider2D colliderComponent;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    
    // Setup state
    private bool isSetupComplete = false;
    
    void Awake()
    {
        if (autoSetupOnAwake)
        {
            SetupProjectile();
        }
    }
    
    void Start()
    {
        if (validateComponents && !isSetupComplete)
        {
            Debug.LogWarning($"Projectile {gameObject.name} was not properly set up!");
        }
    }
    
    /// <summary>
    /// Main setup method that configures all projectile components
    /// </summary>
    public void SetupProjectile()
    {
        if (isSetupComplete) return;
        
        try
        {
            // Use config if available, otherwise use manual settings
            var settings = GetEffectiveSettings();
            
            // Setup core components
            SetupCoreComponents(settings);
            
            // Setup visual components
            SetupVisualComponents(settings);
            
            // Setup audio
            SetupAudio(settings);
            
            // Setup projectile component
            SetupProjectileComponent(settings);
            
            isSetupComplete = true;
            Debug.Log($"Projectile {gameObject.name} setup completed successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to setup projectile {gameObject.name}: {e.Message}");
        }
    }
    
    private ProjectileSettings GetEffectiveSettings()
    {
        var settings = new ProjectileSettings();
        
        if (useConfigDefaults && config != null)
        {
            // Use config values
            settings.projectileSprite = config.projectileSprite;
            settings.projectileColor = config.projectileColor;
            settings.projectileSize = config.projectileSize;
            settings.addTrail = config.addTrail;
            settings.addParticles = config.addParticles;
            settings.addGlow = config.addGlow;
            settings.fireSound = config.fireSound;
            settings.hitSound = config.hitSound;
            settings.audioVolume = config.audioVolume;
            settings.trailTime = config.trailTime;
            settings.trailStartWidth = config.trailStartWidth;
            settings.trailEndWidth = config.trailEndWidth;
            settings.particleLifetime = config.particleLifetime;
            settings.particleSpeed = config.particleSpeed;
            settings.particleSize = config.particleSize;
            settings.maxParticles = config.maxParticles;
            settings.particleEmissionRate = config.particleEmissionRate;
            settings.glowScale = config.glowScale;
            settings.glowAlpha = config.glowAlpha;
        }
        else if (useConfigDefaults && config == null)
        {
            // Config is missing but trying to use defaults - attempt auto-load
            Debug.LogWarning($"Config is missing for {gameObject.name}, attempting to auto-load default config...");
            TryLoadDefaultConfig();
            
            if (config != null)
            {
                // Recursively call with the loaded config
                return GetEffectiveSettings();
            }
            else
            {
                Debug.LogWarning($"No config available for {gameObject.name}, falling back to manual settings");
                // Fall through to manual values
            }
        }
        
        // Use manual values (either by choice or as fallback)
        if (!useConfigDefaults || config == null)
        {
            settings.projectileSprite = projectileSprite;
            settings.projectileColor = projectileColor;
            settings.projectileSize = projectileSize;
            settings.addTrail = addTrail;
            settings.addParticles = addParticles;
            settings.addGlow = addGlow;
            settings.fireSound = fireSound;
            settings.hitSound = hitSound;
            settings.audioVolume = 0.5f;
            settings.trailTime = 0.3f;
            settings.trailStartWidth = projectileSize * 0.3f;
            settings.trailEndWidth = 0f;
            settings.particleLifetime = 0.5f;
            settings.particleSpeed = 2f;
            settings.particleSize = projectileSize * 0.1f;
            settings.maxParticles = 20;
            settings.particleEmissionRate = 40f;
            settings.glowScale = 1.5f;
            settings.glowAlpha = 0.3f;
        }
        
        return settings;
    }
    
    private void TryLoadDefaultConfig()
    {
        if (config != null) return; // Already have a config
        
        // Try to find a default config in Resources
        var defaultConfig = Resources.Load<SorcererProjectileConfig>("DefaultSorcererProjectileConfig");
        if (defaultConfig != null)
        {
            config = defaultConfig;
            Debug.Log($"Auto-loaded default config for {gameObject.name}");
        }
        else
        {
            // Try alternative names
            string[] alternativeNames = {
                "SorcererProjectileConfig",
                "Default_SorcererProjectileConfig",
                "DefaultProjectileConfig"
            };
            
            foreach (string name in alternativeNames)
            {
                defaultConfig = Resources.Load<SorcererProjectileConfig>(name);
                if (defaultConfig != null)
                {
                    config = defaultConfig;
                    Debug.Log($"Auto-loaded config '{name}' for {gameObject.name}");
                    break;
                }
            }
        }
    }
    
    private void SetupCoreComponents(ProjectileSettings settings)
    {
        // Ensure SorcererProjectile component
        projectileComponent = GetComponent<SorcererProjectile>();
        if (projectileComponent == null)
        {
            projectileComponent = gameObject.AddComponent<SorcererProjectile>();
        }
        
        // Setup Rigidbody2D
        rbComponent = GetComponent<Rigidbody2D>();
        if (rbComponent == null)
        {
            rbComponent = gameObject.AddComponent<Rigidbody2D>();
        }
        
        rbComponent.gravityScale = 0f;
        rbComponent.drag = 0f;
        rbComponent.angularDrag = 0f;
        rbComponent.bodyType = RigidbodyType2D.Kinematic;
        
        // Setup Collider2D
        colliderComponent = GetComponent<Collider2D>();
        if (colliderComponent == null)
        {
            CircleCollider2D circleCol = gameObject.AddComponent<CircleCollider2D>();
            circleCol.radius = settings.projectileSize * 0.5f;
            colliderComponent = circleCol;
        }
        
        colliderComponent.isTrigger = true;
    }
    
    private void SetupVisualComponents(ProjectileSettings settings)
    {
        // Setup sprite renderer
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        if (settings.projectileSprite != null)
        {
            spriteRenderer.sprite = settings.projectileSprite;
        }
        
        spriteRenderer.color = settings.projectileColor;
        spriteRenderer.sortingOrder = 10;
        transform.localScale = Vector3.one * settings.projectileSize;
        
        // Setup trail effect
        if (settings.addTrail)
        {
            SetupTrailRenderer(settings);
        }
        
        // Setup particle effects
        if (settings.addParticles)
        {
            SetupParticleSystem(settings);
        }
        
        // Setup glow effect
        if (settings.addGlow)
        {
            SetupGlowEffect(settings);
        }
    }
    
    private void SetupTrailRenderer(ProjectileSettings settings)
    {
        TrailRenderer trail = GetComponent<TrailRenderer>();
        if (trail == null)
        {
            trail = gameObject.AddComponent<TrailRenderer>();
        }
        
        trail.time = settings.trailTime;
        trail.startWidth = settings.trailStartWidth;
        trail.endWidth = settings.trailEndWidth;
        trail.material = CreateTrailMaterial(settings.projectileColor);
        trail.sortingOrder = 9;
        // Note: useWorldSpace is not available in Unity 2018.4, using default behavior
    }
    
    private void SetupParticleSystem(ProjectileSettings settings)
    {
        ParticleSystem particles = GetComponent<ParticleSystem>();
        if (particles == null)
        {
            particles = gameObject.AddComponent<ParticleSystem>();
        }
        
        ConfigureParticleSystem(particles, settings);
    }
    
    private void SetupGlowEffect(ProjectileSettings settings)
    {
        Transform existingGlow = transform.Find("Glow");
        if (existingGlow != null)
        {
            DestroyImmediate(existingGlow.gameObject);
        }
        
        GameObject glowChild = new GameObject("Glow");
        glowChild.transform.SetParent(transform);
        glowChild.transform.localPosition = Vector3.zero;
        glowChild.transform.localScale = Vector3.one * settings.glowScale;
        
        SpriteRenderer glowRenderer = glowChild.AddComponent<SpriteRenderer>();
        glowRenderer.sprite = settings.projectileSprite;
        glowRenderer.color = new Color(settings.projectileColor.r, settings.projectileColor.g, settings.projectileColor.b, settings.glowAlpha);
        glowRenderer.sortingOrder = 9;
    }
    
    private void SetupAudio(ProjectileSettings settings)
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        audioSource.playOnAwake = false;
        audioSource.volume = settings.audioVolume;
        audioSource.spatialBlend = 0.5f;
        
        // Play fire sound if available
        if (settings.fireSound != null)
        {
            audioSource.clip = settings.fireSound;
            audioSource.Play();
        }
    }
    
    private void SetupProjectileComponent(ProjectileSettings settings)
    {
        if (projectileComponent == null) return;
        
        // Set lifetime if config is available
        if (config != null)
        {
            projectileComponent.SetLifetime(config.lifetime);
        }
    }
    
    private Material CreateTrailMaterial(Color color)
    {
        Material trailMaterial = new Material(Shader.Find("Sprites/Default"));
        trailMaterial.color = color;
        return trailMaterial;
    }
    
    private void ConfigureParticleSystem(ParticleSystem particles, ProjectileSettings settings)
    {
        var main = particles.main;
        main.startLifetime = settings.particleLifetime;
        main.startSpeed = settings.particleSpeed;
        main.startSize = settings.particleSize;
        main.startColor = settings.projectileColor;
        main.maxParticles = settings.maxParticles;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        var emission = particles.emission;
        emission.rateOverTime = settings.particleEmissionRate;
        
        var shape = particles.shape;
        shape.enabled = false;
        
        var velocityOverLifetime = particles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(1f);
        
        var sizeOverLifetime = particles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        var colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(settings.projectileColor, 0f), new GradientColorKey(settings.projectileColor, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = gradient;
        
        // Set renderer
        var renderer = particles.GetComponent<ParticleSystemRenderer>();
        renderer.material = CreateTrailMaterial(settings.projectileColor);
        renderer.sortingOrder = 11;
    }
    
    /// <summary>
    /// Get the configured projectile component
    /// </summary>
    public SorcererProjectile GetProjectileComponent()
    {
        return projectileComponent;
    }
    
    /// <summary>
    /// Check if setup is complete
    /// </summary>
    public bool IsSetupComplete => isSetupComplete;
    
    /// <summary>
    /// Get the configuration being used
    /// </summary>
    public SorcererProjectileConfig GetConfig()
    {
        return config;
    }
    
    // Editor helper methods
    [ContextMenu("Setup Projectile")]
    private void SetupProjectileEditor()
    {
        SetupProjectile();
    }
    
    [ContextMenu("Reset Setup")]
    private void ResetSetup()
    {
        isSetupComplete = false;
        SetupProjectile();
    }
    
    // Validation
    void OnValidate()
    {
        // Only show warning in play mode or when the object is in a scene (not a prefab asset)
        if (useConfigDefaults && config == null && !IsPrefabAsset())
        {
            Debug.LogWarning($"Projectile {gameObject.name} is set to use config defaults but no config is assigned!");
        }
        
        projectileSize = Mathf.Max(0.1f, projectileSize);
    }
    
    private bool IsPrefabAsset()
    {
        // Check if this is a prefab asset (not an instance in scene)
        return gameObject.scene.name == null || gameObject.scene.name == "";
    }
    
    /// <summary>
    /// Auto-assign a default config if available
    /// </summary>
    [ContextMenu("Auto-Assign Default Config")]
    private void AutoAssignDefaultConfig()
    {
        if (config == null)
        {
            // Try to find a default config in Resources
            var defaultConfig = Resources.Load<SorcererProjectileConfig>("DefaultSorcererProjectileConfig");
            if (defaultConfig != null)
            {
                config = defaultConfig;
            }
        }
    }
}

/// <summary>
/// Internal settings structure
/// </summary>
internal class ProjectileSettings
{
    public Sprite projectileSprite;
    public Color projectileColor;
    public float projectileSize;
    public bool addTrail;
    public bool addParticles;
    public bool addGlow;
    public AudioClip fireSound;
    public AudioClip hitSound;
    public float audioVolume;
    public float trailTime;
    public float trailStartWidth;
    public float trailEndWidth;
    public float particleLifetime;
    public float particleSpeed;
    public float particleSize;
    public int maxParticles;
    public float particleEmissionRate;
    public float glowScale;
    public float glowAlpha;
}