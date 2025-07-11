using UnityEngine;

public class DeathZone : MonoBehaviour
{
    [Header("Death Zone Settings")]
    [SerializeField] private int damage = 999; // 致命伤害
    [SerializeField] private bool instantKill = true;
    
    [Header("Effects")]
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private ParticleSystem deathEffect;
    
    private AudioSource audioSource;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        // 确保是触发器
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 检查是否是玩家
        HeroLife heroLife = other.GetComponent<HeroLife>();
        if (heroLife != null && !heroLife.IsDead)
        {
            Debug.Log($"Player entered death zone: {gameObject.name}");
            
            // 播放音效
            if (audioSource != null && deathSound != null)
            {
                audioSource.PlayOneShot(deathSound);
            }
            
            // 播放粒子效果
            if (deathEffect != null)
            {
                Instantiate(deathEffect, other.transform.position, Quaternion.identity);
            }
            
            // 造成伤害
            if (instantKill)
            {
                heroLife.TakeDamage(heroLife.MaxHealth);
            }
            else
            {
                heroLife.TakeDamage(damage);
            }
        }
        
        // 也可以对敌人造成伤害
        SpiderP spider = other.GetComponent<SpiderP>();
        if (spider != null && !spider.IsDead)
        {
            spider.TakeDamage(spider.MaxHealth);
        }
    }
    
    // 在编辑器中可视化危险区域
    private void OnDrawGizmos()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
            
            // 绘制危险标识
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.DrawCube(transform.position, col.bounds.size);
        }
    }
}
