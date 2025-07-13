using UnityEngine;

[CreateAssetMenu(fileName = "New Sorcerer Projectile Config", menuName = "Sorcerer/Projectile Config")]
public class SorcererProjectileConfig : ScriptableObject
{
    [Header("Basic Settings")]
    public int damage = 1;
    public float speed = 5f;
    public float lifetime = 5f;
    public bool destroyOnHit = true;
    public bool canHitMultipleTargets = false;
    
    [Header("Visual Settings")]
    public Sprite projectileSprite;
    public Color projectileColor = Color.blue;
    public float projectileSize = 0.5f;
    
    [Header("Effects")]
    public bool addTrail = true;
    public bool addParticles = true;
    public bool addGlow = true;
    public GameObject hitEffect;
    
    [Header("Trail Settings")]
    public float trailTime = 0.3f;
    public float trailStartWidth = 0.15f;
    public float trailEndWidth = 0f;
    
    [Header("Particle Settings")]
    public float particleLifetime = 0.5f;
    public float particleSpeed = 2f;
    public float particleSize = 0.05f;
    public int maxParticles = 20;
    public float particleEmissionRate = 40f;
    
    [Header("Glow Settings")]
    public float glowScale = 1.5f;
    public float glowAlpha = 0.3f;
    
    [Header("Audio")]
    public AudioClip fireSound;
    public AudioClip hitSound;
    public float audioVolume = 0.5f;
    
    [Header("Advanced")]
    public bool enableHoming = false;
    public float homingStrength = 2f;
    public LayerMask targetLayer = -1;
    
    // Validation
    void OnValidate()
    {
        damage = Mathf.Max(1, damage);
        speed = Mathf.Max(0.1f, speed);
        lifetime = Mathf.Max(0.1f, lifetime);
        projectileSize = Mathf.Max(0.1f, projectileSize);
        trailTime = Mathf.Max(0f, trailTime);
        particleLifetime = Mathf.Max(0.1f, particleLifetime);
        particleSpeed = Mathf.Max(0.1f, particleSpeed);
        maxParticles = Mathf.Max(1, maxParticles);
        particleEmissionRate = Mathf.Max(0f, particleEmissionRate);
        glowScale = Mathf.Max(0.1f, glowScale);
        glowAlpha = Mathf.Clamp01(glowAlpha);
        audioVolume = Mathf.Clamp01(audioVolume);
        homingStrength = Mathf.Max(0.1f, homingStrength);
    }
}