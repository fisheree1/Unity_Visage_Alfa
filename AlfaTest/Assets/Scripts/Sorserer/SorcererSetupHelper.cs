using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Utility class to help set up Sorcerer projectile system
/// </summary>
public class SorcererSetupHelper : MonoBehaviour
{
    [Header("Quick Setup")]
    [SerializeField] private bool createDefaultConfig = true;
    [SerializeField] private bool setupProjectilePool = true;
    [SerializeField] private bool findAndFixMissingConfigs = true;
    
    [Header("Default Config Settings")]
    [SerializeField] private int defaultDamage = 1;
    [SerializeField] private float defaultSpeed = 5f;
    [SerializeField] private Color defaultColor = Color.red;
    [SerializeField] private float defaultSize = 0.5f;
    
    [ContextMenu("Auto Setup Sorcerer System")]
    public void AutoSetupSorcererSystem()
    {
        Debug.Log("Starting Sorcerer system auto-setup...");
        
        if (createDefaultConfig)
        {
            CreateDefaultConfiguration();
        }
        
        if (setupProjectilePool)
        {
            SetupProjectilePool();
        }
        
        if (findAndFixMissingConfigs)
        {
            FindAndFixMissingConfigs();
        }
        
        Debug.Log("Sorcerer system auto-setup completed!");
    }
    
    private void CreateDefaultConfiguration()
    {
        #if UNITY_EDITOR
        // Check if default config already exists
        var existingConfig = Resources.Load<SorcererProjectileConfig>("DefaultSorcererProjectileConfig");
        if (existingConfig != null)
        {
            Debug.Log("Default configuration already exists");
            return;
        }
        
        // Create Resources directory if it doesn't exist
        string resourcesPath = "Assets/Resources";
        if (!AssetDatabase.IsValidFolder(resourcesPath))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }
        
        // Create the default config
        var config = ScriptableObject.CreateInstance<SorcererProjectileConfig>();
        
        // Set default values
        config.damage = defaultDamage;
        config.speed = defaultSpeed;
        config.projectileColor = defaultColor;
        config.projectileSize = defaultSize;
        config.lifetime = 5f;
        config.destroyOnHit = true;
        config.addTrail = true;
        config.addParticles = true;
        config.addGlow = true;
        config.trailTime = 0.3f;
        config.trailStartWidth = 0.15f;
        config.trailEndWidth = 0f;
        config.audioVolume = 0.5f;
        
        // Save the asset
        string assetPath = "Assets/Resources/DefaultSorcererProjectileConfig.asset";
        AssetDatabase.CreateAsset(config, assetPath);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"Created default configuration at {assetPath}");
        #else
        Debug.LogWarning("Configuration creation is only available in the Unity Editor");
        #endif
    }
    
    private void SetupProjectilePool()
    {
        // Check if pool already exists
        var existingPool = FindObjectOfType<SorcererProjectilePool>();
        if (existingPool != null)
        {
            Debug.Log("Projectile pool already exists");
            return;
        }
        
        // Create pool object
        GameObject poolObject = new GameObject("Sorcerer Projectile Pool");
        var pool = poolObject.AddComponent<SorcererProjectilePool>();
        
        Debug.Log("Created projectile pool");
    }
    
    private void FindAndFixMissingConfigs()
    {
        var prefabComponents = FindObjectsOfType<SorcererProjectilePrefab>();
        int fixedCount = 0;
        
        foreach (var prefab in prefabComponents)
        {
            if (prefab.GetConfig() == null)
            {
                // Try to auto-assign default config
                var defaultConfig = Resources.Load<SorcererProjectileConfig>("DefaultSorcererProjectileConfig");
                if (defaultConfig != null)
                {
                    #if UNITY_EDITOR
                    // Use reflection to set the private config field
                    var configField = typeof(SorcererProjectilePrefab).GetField("config", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (configField != null)
                    {
                        configField.SetValue(prefab, defaultConfig);
                        EditorUtility.SetDirty(prefab);
                        fixedCount++;
                        Debug.Log($"Auto-assigned config to {prefab.gameObject.name}");
                    }
                    #endif
                }
            }
        }
        
        if (fixedCount > 0)
        {
            Debug.Log($"Fixed {fixedCount} missing configurations");
            #if UNITY_EDITOR
            AssetDatabase.SaveAssets();
            #endif
        }
        else
        {
            Debug.Log("No missing configurations found");
        }
    }
    
    [ContextMenu("Create Test Projectile")]
    public void CreateTestProjectile()
    {
        GameObject testProjectile = new GameObject("Test Sorcerer Projectile");
        testProjectile.transform.position = transform.position + Vector3.right;
        
        var prefabComponent = testProjectile.AddComponent<SorcererProjectilePrefab>();
        
        // Auto-assign default config if available
        var defaultConfig = Resources.Load<SorcererProjectileConfig>("DefaultSorcererProjectileConfig");
        if (defaultConfig != null)
        {
            #if UNITY_EDITOR
            var configField = typeof(SorcererProjectilePrefab).GetField("config", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            configField?.SetValue(prefabComponent, defaultConfig);
            #endif
        }
        
        prefabComponent.SetupProjectile();
        
        Debug.Log("Created test projectile");
    }
    
    [ContextMenu("Validate System")]
    public void ValidateSystem()
    {
        Debug.Log("Validating Sorcerer system...");
        
        // Check for default config
        var defaultConfig = Resources.Load<SorcererProjectileConfig>("DefaultSorcererProjectileConfig");
        Debug.Log($"Default config exists: {defaultConfig != null}");
        
        // Check for projectile pool
        var pool = FindObjectOfType<SorcererProjectilePool>();
        Debug.Log($"Projectile pool exists: {pool != null}");
        
        // Check for Sorcerer objects
        var sorcerers = FindObjectsOfType<SorcererP>();
        Debug.Log($"Found {sorcerers.Length} Sorcerer(s) in scene");
        
        // Check for projectile prefabs
        var prefabs = FindObjectsOfType<SorcererProjectilePrefab>();
        int configuredPrefabs = 0;
        foreach (var prefab in prefabs)
        {
            if (prefab.GetConfig() != null)
            {
                configuredPrefabs++;
            }
        }
        Debug.Log($"Found {prefabs.Length} projectile prefab(s), {configuredPrefabs} properly configured");
        
        Debug.Log("System validation completed");
    }
}