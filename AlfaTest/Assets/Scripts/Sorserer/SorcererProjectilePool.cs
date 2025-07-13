using System.Collections.Generic;
using UnityEngine;

public class SorcererProjectilePool : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private int initialPoolSize = 10;
    [SerializeField] private int maxPoolSize = 50;
    [SerializeField] private bool autoExpand = true;
    
    private Queue<SorcererProjectile> availableProjectiles = new Queue<SorcererProjectile>();
    private HashSet<SorcererProjectile> activeProjectiles = new HashSet<SorcererProjectile>();
    private Transform poolParent;
    
    // Singleton pattern
    public static SorcererProjectilePool Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializePool();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializePool()
    {
        // Create pool parent
        poolParent = new GameObject("Projectile Pool").transform;
        poolParent.SetParent(transform);
        
        // Pre-instantiate projectiles
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewProjectile();
        }
        
        Debug.Log($"Projectile pool initialized with {initialPoolSize} projectiles");
    }
    
    private SorcererProjectile CreateNewProjectile()
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("Projectile prefab is not assigned!");
            return null;
        }
        
        GameObject projectileObj = Instantiate(projectilePrefab, poolParent);
        projectileObj.SetActive(false);
        
        SorcererProjectile projectile = projectileObj.GetComponent<SorcererProjectile>();
        if (projectile == null)
        {
            projectile = projectileObj.AddComponent<SorcererProjectile>();
        }
        
        availableProjectiles.Enqueue(projectile);
        return projectile;
    }
    
    public SorcererProjectile GetProjectile()
    {
        SorcererProjectile projectile = null;
        
        // Try to get from available pool
        if (availableProjectiles.Count > 0)
        {
            projectile = availableProjectiles.Dequeue();
        }
        // Create new if pool is empty and auto-expand is enabled
        else if (autoExpand && (activeProjectiles.Count + availableProjectiles.Count) < maxPoolSize)
        {
            projectile = CreateNewProjectile();
            if (projectile != null)
            {
                availableProjectiles.Dequeue(); // Remove from available since we're using it
            }
        }
        
        if (projectile != null)
        {
            projectile.gameObject.SetActive(true);
            activeProjectiles.Add(projectile);
        }
        else
        {
            Debug.LogWarning("No projectiles available in pool!");
        }
        
        return projectile;
    }
    
    public void ReturnProjectile(SorcererProjectile projectile)
    {
        if (projectile == null) return;
        
        if (activeProjectiles.Remove(projectile))
        {
            projectile.gameObject.SetActive(false);
            projectile.transform.SetParent(poolParent);
            
            // Reset transform
            projectile.transform.localPosition = Vector3.zero;
            projectile.transform.localRotation = Quaternion.identity;
            projectile.transform.localScale = Vector3.one;
            
            availableProjectiles.Enqueue(projectile);
        }
    }
    
    public void ClearPool()
    {
        // Return all active projectiles
        var activeList = new List<SorcererProjectile>(activeProjectiles);
        foreach (var projectile in activeList)
        {
            ReturnProjectile(projectile);
        }
        
        Debug.Log($"Pool cleared. Available: {availableProjectiles.Count}, Active: {activeProjectiles.Count}");
    }
    
    public void PrewarmPool(int count)
    {
        count = Mathf.Min(count, maxPoolSize - (availableProjectiles.Count + activeProjectiles.Count));
        
        for (int i = 0; i < count; i++)
        {
            CreateNewProjectile();
        }
        
        Debug.Log($"Pool prewarmed with {count} additional projectiles");
    }
    
    // Debug information
    public int AvailableCount => availableProjectiles.Count;
    public int ActiveCount => activeProjectiles.Count;
    public int TotalCount => availableProjectiles.Count + activeProjectiles.Count;
    
    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    
    // Editor helper
    [ContextMenu("Clear Pool")]
    private void ClearPoolEditor()
    {
        if (Application.isPlaying)
        {
            ClearPool();
        }
    }
    
    [ContextMenu("Prewarm Pool")]
    private void PrewarmPoolEditor()
    {
        if (Application.isPlaying)
        {
            PrewarmPool(5);
        }
    }
}