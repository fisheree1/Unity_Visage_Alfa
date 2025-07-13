using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 魔法攻击类型枚举
public enum MagicAttackType
{
    Single,     // 单发
    MultiShot,  // 连续多发
    Fan,        // 扇形
    Barrage,    // 弹幕
    Homing,     // 追踪
    Lightning   // 雷电攻击
}

public class BossAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private int meleeDamage = 20;
    [SerializeField] private int magicDamage = 15;
    [SerializeField] private float meleeAttackRange = 1.5f;
    [SerializeField] private float meleeAttackRadius = 1f;
    
    [Header("Attack HitBox")]
    [SerializeField] private GameObject attackHitBoxObject;
    [SerializeField] private float meleeAttackDuration = 0.2f; // 减少攻击持续时间
    
    [Header("Magic Attack Settings")]
    [SerializeField] private GameObject magicProjectilePrefab;
    [SerializeField] private Transform[] magicSpawnPoints; // 多个生成点
    [SerializeField] private float magicProjectileSpeed = 8f;
    [SerializeField] private float magicProjectileLifetime = 5f;
    
    [Header("Lightning Attack Settings")]
    [SerializeField] private GameObject lightningPrefab; // 雷电攻击预制体
    [SerializeField] private bool useSimpleLightning = true; // 使用简化版雷电
    [SerializeField] private int lightningDamage = 25; // 雷电伤害
    [SerializeField] private float lightningRange = 2f; // 雷电攻击范围
    [SerializeField] private float lightningWarningTime = 1.5f; // 警告时间
    [SerializeField] private int lightningStrikeCount = 3; // 连击次数
    [SerializeField] private float lightningStrikeDelay = 0.3f; // 连击间隔
    
    [Header("Magic Attack Types")]
    [SerializeField] private MagicAttackType magicAttackType = MagicAttackType.Single;
    [SerializeField] private int multiShotCount = 3; // 多发射击数量
    [SerializeField] private float multiShotDelay = 0.2f; // 多发射击间隔
    [SerializeField] private float fanAngle = 30f; // 扇形攻击角度
    [SerializeField] private int fanProjectileCount = 5; // 扇形弹射物数量

    [Header("Effects")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private GameObject magicChargeEffectPrefab;

    private Animator anim;
    private BossMove bossMove;
    private LayerMask playerLayer;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        bossMove = GetComponent<BossMove>();
        playerLayer = LayerMask.GetMask("Player");

        // 查找或设置攻击HitBox
        if (attackHitBoxObject == null)
        {
            // 尝试在子对象中查找
            Transform hitboxTransform = transform.Find("BossAttackHitBox");
            if (hitboxTransform != null)
            {
                attackHitBoxObject = hitboxTransform.gameObject;
            }
        }

        // 设置魔法生成点
        if (magicSpawnPoints == null || magicSpawnPoints.Length == 0)
        {
            InitializeMagicSpawnPoints();
        }
    }

    // 初始化魔法生成点
    private void InitializeMagicSpawnPoints()
    {
        List<Transform> spawnPointsList = new List<Transform>();
        
        // 查找现有的魔法生成点
        for (int i = 0; i < 5; i++) // 最多查找5个生成点
        {
            Transform existingSpawnPoint = transform.Find($"MagicSpawnPoint_{i}");
            if (existingSpawnPoint != null)
            {
                spawnPointsList.Add(existingSpawnPoint);
                Debug.Log($"Found existing magic spawn point {i} at: " + existingSpawnPoint.localPosition);
            }
        }
        
        magicSpawnPoints = spawnPointsList.ToArray();
        Debug.Log($"Initialized {magicSpawnPoints.Length} magic spawn points");
    }


    // 创建单个生成点
    private void CreateSpawnPoint(string name, Vector3 localPosition, List<Transform> spawnPointsList)
    {
        GameObject spawnPointObj = new GameObject(name);
        spawnPointObj.transform.SetParent(transform);
        spawnPointObj.transform.localPosition = localPosition;
        spawnPointsList.Add(spawnPointObj.transform);
        Debug.Log($"Created {name} at: {localPosition}");
    }

    // Called from animation event or from BossMove during attack state
    public void MeleeAttack()
    {
        Debug.Log("Boss performing Melee Attack");
        
        // 使用新的HitBox系统
        if (attackHitBoxObject != null)
        {
            Debug.Log("Found attack hitbox object, attempting to activate");
            
            // 直接获取BossAttackHitBox组件
            BossAttackHitBox hitBoxComponent = attackHitBoxObject.GetComponent<BossAttackHitBox>();
            if (hitBoxComponent != null)
            {
                Debug.Log($"Activating Boss Attack HitBox for {meleeAttackDuration} seconds");
                hitBoxComponent.ActivateForDuration(meleeAttackDuration);
                return;
            }
            else
            {
                Debug.LogWarning("BossAttackHitBox component not found on hitbox object!");
            }
        }
        else
        {
            Debug.LogWarning("Attack hitbox object not assigned!");
        }
    }


    // Called from BossMove when entering magic cast state
    public void MagicAttack()
    {
        Debug.Log("Performing Magic Attack");
        StartCoroutine(CastMagicSequence());
    }

    private IEnumerator CastMagicSequence()
    {
        Debug.Log("Starting magic attack sequence");
        
        // 更新所有魔法生成点的位置根据朝向
        UpdateMagicSpawnPointsPosition();
        
        // 根据攻击类型执行不同的魔法攻击
        switch (magicAttackType)
        {
            case MagicAttackType.Single:
                yield return StartCoroutine(CastSingleMagic());
                break;
            case MagicAttackType.MultiShot:
                yield return StartCoroutine(CastMultiShotMagic());
                break;
            case MagicAttackType.Fan:
                yield return StartCoroutine(CastFanMagic());
                break;
            case MagicAttackType.Barrage:
                yield return StartCoroutine(CastBarrageMagic());
                break;
            case MagicAttackType.Homing:
                yield return StartCoroutine(CastHomingMagic());
                break;
            case MagicAttackType.Lightning:
                yield return StartCoroutine(CastLightningMagic());
                break;
        }
    }

    // 单发魔法攻击
    private IEnumerator CastSingleMagic()
    {
        Transform spawnPoint = GetRandomSpawnPoint();
        if (spawnPoint == null) yield break;

        // Show charge effect
        GameObject chargeEffect = CreateChargeEffect(spawnPoint);
        
        // Wait for charge time
        yield return new WaitForSeconds(0.7f);
        
        // Remove charge effect
        if (chargeEffect != null) Destroy(chargeEffect);
        
        // Launch single projectile
        Vector2 direction = bossMove.IsFacingRight ? Vector2.right : Vector2.left;
        CreateProjectile(spawnPoint.position, direction, magicDamage);
    }

    // 连续多发魔法攻击
    private IEnumerator CastMultiShotMagic()
    {
        Transform spawnPoint = GetRandomSpawnPoint();
        if (spawnPoint == null) yield break;

        // Show charge effect
        GameObject chargeEffect = CreateChargeEffect(spawnPoint);
        yield return new WaitForSeconds(0.5f);
        if (chargeEffect != null) Destroy(chargeEffect);
        
        // Launch multiple projectiles in sequence
        Vector2 direction = bossMove.IsFacingRight ? Vector2.right : Vector2.left;
        for (int i = 0; i < multiShotCount; i++)
        {
            CreateProjectile(spawnPoint.position, direction, magicDamage);
            yield return new WaitForSeconds(multiShotDelay);
        }
    }

    // 扇形魔法攻击
    private IEnumerator CastFanMagic()
    {
        Transform spawnPoint = GetRandomSpawnPoint();
        if (spawnPoint == null) yield break;

        // Show charge effect
        GameObject chargeEffect = CreateChargeEffect(spawnPoint);
        yield return new WaitForSeconds(0.8f);
        if (chargeEffect != null) Destroy(chargeEffect);
        
        // Launch fan of projectiles
        Vector2 baseDirection = bossMove.IsFacingRight ? Vector2.right : Vector2.left;
        float angleStep = fanAngle / (fanProjectileCount - 1);
        float startAngle = -fanAngle / 2;
        
        for (int i = 0; i < fanProjectileCount; i++)
        {
            float angle = startAngle + (angleStep * i);
            Vector2 direction = RotateVector(baseDirection, angle);
            CreateProjectile(spawnPoint.position, direction, magicDamage);
        }
    }

    // 弹幕魔法攻击
    private IEnumerator CastBarrageMagic()
    {
        // Use all spawn points for barrage
        GameObject[] chargeEffects = new GameObject[magicSpawnPoints.Length];
        
        // Create charge effects at all spawn points
        for (int i = 0; i < magicSpawnPoints.Length; i++)
        {
            chargeEffects[i] = CreateChargeEffect(magicSpawnPoints[i]);
        }
        
        yield return new WaitForSeconds(1.0f);
        
        // Remove charge effects
        foreach (var effect in chargeEffects)
        {
            if (effect != null) Destroy(effect);
        }
        
        // Launch projectiles from all spawn points
        Vector2 direction = bossMove.IsFacingRight ? Vector2.right : Vector2.left;
        foreach (var spawnPoint in magicSpawnPoints)
        {
            CreateProjectile(spawnPoint.position, direction, magicDamage);
            yield return new WaitForSeconds(0.1f); // Small delay between each shot
        }
    }

    // 追踪魔法攻击
    private IEnumerator CastHomingMagic()
    {
        Transform spawnPoint = GetRandomSpawnPoint();
        if (spawnPoint == null) yield break;

        // Show charge effect
        GameObject chargeEffect = CreateChargeEffect(spawnPoint);
        yield return new WaitForSeconds(0.6f);
        if (chargeEffect != null) Destroy(chargeEffect);
        
        // Create homing projectile
        Vector2 direction = bossMove.IsFacingRight ? Vector2.right : Vector2.left;
        GameObject projectile = CreateProjectile(spawnPoint.position, direction, magicDamage);
        
        // Add homing behavior (simplified version)
        if (projectile != null)
        {
            var magicScript = projectile.GetComponent<MagicProjectile>();
            if (magicScript != null)
            {
                magicScript.EnableHoming(true);
            }
        }
    }

    // 雷电魔法攻击
    private IEnumerator CastLightningMagic()
    {
        Debug.Log("Casting Lightning Magic Attack");
        
        // 获取玩家位置
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("Player not found for lightning attack!");
            yield break;
        }
        
        // 连续雷击
        for (int i = 0; i < lightningStrikeCount; i++)
        {
            Vector3 targetPosition = player.transform.position;
            targetPosition.y += 2f; // 调整目标位置高度
            
            // 添加一些随机偏移，让攻击更有趣且不那么精确
            if (i > 0)
            {
                float randomOffset = Random.Range(-2f, 2f);
                targetPosition.x += randomOffset;
            }
            
            // 创建雷电攻击
            yield return StartCoroutine(CreateLightningStrike(targetPosition));
            
            // 等待下次雷击
            if (i < lightningStrikeCount - 1)
            {
                yield return new WaitForSeconds(lightningStrikeDelay);
            }
        }
    }
    
    // 创建单次雷电攻击
    private IEnumerator CreateLightningStrike(Vector3 targetPosition)
    {
        // 创建警告指示器
        GameObject warningIndicator = CreateLightningWarning(targetPosition);
        
        // 等待警告时间
        yield return new WaitForSeconds(lightningWarningTime);
        
        // 移除警告指示器
        if (warningIndicator != null)
        {
            Destroy(warningIndicator);
        }
        
        // 创建雷电攻击
        Vector3 lightningPosition = new Vector3(targetPosition.x, targetPosition.y + 10f, targetPosition.z);
        GameObject lightning = null;
        

        if (lightningPrefab != null)
        {
            // 使用预制体雷电攻击
            lightning = Instantiate(lightningPrefab, lightningPosition, Quaternion.identity);
            
            // 配置雷电攻击 - 使用反射或组件查找来避免编译错误
            MonoBehaviour lightningScript = lightning.GetComponent<MonoBehaviour>();
            if (lightningScript == null)
            {
                // 尝试添加LightningStrike组件
                System.Type lightningType = System.Type.GetType("LightningStrike");
                if (lightningType != null)
                {
                    lightningScript = (MonoBehaviour)lightning.AddComponent(lightningType);
                }
            }
            
            // 如果找到了组件，使用反射调用Initialize方法
            if (lightningScript != null)
            {
                System.Reflection.MethodInfo initMethod = lightningScript.GetType().GetMethod("Initialize");
                if (initMethod != null)
                {
                    initMethod.Invoke(lightningScript, new object[] { lightningDamage, targetPosition, lightningRange, playerLayer });
                }
            }
        }
        else
        {
            Debug.LogWarning("No lightning prefab assigned and simple lightning is disabled!");
        }
        
        Debug.Log($"Lightning strike created at {targetPosition}");
    }
    

    
    // 创建雷电警告指示器
    private GameObject CreateLightningWarning(Vector3 position)
    {
        // 创建简单的警告指示器
        GameObject warning = new GameObject("LightningWarning");
        warning.transform.position = position;
        
        // 添加视觉组件
        SpriteRenderer spriteRenderer = warning.AddComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(1f, 1f, 0f, 0.5f); // 半透明黄色
        
        // 创建简单的圆形精灵（如果没有专门的警告精灵）
        Texture2D texture = new Texture2D(64, 64);
        Color[] colors = new Color[64 * 64];
        Vector2 center = new Vector2(32, 32);
        
        for (int x = 0; x < 64; x++)
        {
            for (int y = 0; y < 64; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance <= 30)
                {
                    colors[y * 64 + x] = new Color(1f, 1f, 0f, 0.3f);
                }
                else
                {
                    colors[y * 64 + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        Sprite warningSprite = Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
        spriteRenderer.sprite = warningSprite;
        spriteRenderer.sortingOrder = 10; // 确保警告在其他对象之上
        
        // 添加脉冲动画
        StartCoroutine(PulseWarning(warning.transform));
        
        Debug.Log($"Lightning warning created at {position}");
        return warning;
    }
    
    // 警告指示器脉冲动画
    private IEnumerator PulseWarning(Transform warningTransform)
    {
        if (warningTransform == null) yield break;
        
        Vector3 originalScale = warningTransform.localScale;
        float elapsedTime = 0f;
        
        while (warningTransform != null && elapsedTime < lightningWarningTime)
        {
            float pulseScale = 1f + Mathf.Sin(elapsedTime * 8f) * 0.2f;
            warningTransform.localScale = originalScale * pulseScale;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    // 更新所有魔法生成点位置根据Boss朝向
    private void UpdateMagicSpawnPointsPosition()
    {
        if (magicSpawnPoints == null || bossMove == null) return;
        
        foreach (var spawnPoint in magicSpawnPoints)
        {
            if (spawnPoint != null)
            {
                Vector3 localPos = spawnPoint.localPosition;
                float targetX = bossMove.IsFacingRight ? Mathf.Abs(localPos.x) : -Mathf.Abs(localPos.x);
                spawnPoint.localPosition = new Vector3(targetX, localPos.y, localPos.z);
            }
        }
        
        Debug.Log("All magic spawn points updated for facing direction");
    }

    // 获取随机生成点
    private Transform GetRandomSpawnPoint()
    {
        if (magicSpawnPoints == null || magicSpawnPoints.Length == 0)
        {
            Debug.LogWarning("No magic spawn points available!");
            return null;
        }
        
        int randomIndex = Random.Range(0, magicSpawnPoints.Length);
        return magicSpawnPoints[randomIndex];
    }

    // 创建蓄力特效
    private GameObject CreateChargeEffect(Transform spawnPoint)
    {
        if (magicChargeEffectPrefab != null && spawnPoint != null)
        {
            GameObject chargeEffect = Instantiate(magicChargeEffectPrefab, spawnPoint.position, Quaternion.identity);
            chargeEffect.transform.SetParent(spawnPoint);
            Debug.Log("Magic charge effect created");
            return chargeEffect;
        }
        return null;
    }

    // 创建弹射物
    private GameObject CreateProjectile(Vector3 position, Vector2 direction, int damage)
    {
        if (magicProjectilePrefab == null)
        {
            Debug.LogWarning("Magic projectile prefab not assigned!");
            return null;
        }

        GameObject projectile = Instantiate(magicProjectilePrefab, position, Quaternion.identity);
        Debug.Log($"Magic projectile created at {position} with direction {direction}");
        
        // Configure projectile
        Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();
        if (projectileRb != null)
        {
            projectileRb.velocity = direction * magicProjectileSpeed;
        }
        else
        {
            // 如果没有Rigidbody2D，添加一个
            projectileRb = projectile.AddComponent<Rigidbody2D>();
            projectileRb.gravityScale = 0f; // 魔法弹不受重力影响
            projectileRb.velocity = direction * magicProjectileSpeed;
        }
        
        // 确保有碰撞体
        if (projectile.GetComponent<Collider2D>() == null)
        {
            CircleCollider2D collider = projectile.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.5f;
        }
        
        // Add projectile script
        MagicProjectile magicProjectileScript = projectile.GetComponent<MagicProjectile>();
        if (magicProjectileScript == null)
        {
            magicProjectileScript = projectile.AddComponent<MagicProjectile>();
        }
        magicProjectileScript.Initialize(damage, direction, playerLayer);
        
        // Destroy projectile after lifetime
        Destroy(projectile, magicProjectileLifetime);
        
        return projectile;
    }

    // 旋转向量
    private Vector2 RotateVector(Vector2 vector, float angle)
    {
        float rad = angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        
        return new Vector2(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos
        );
    }

    // New methods: Attack methods for BossMove to call
    public void PerformMeleeAttack()
    {
        MeleeAttack();
    }

    public void PerformMagicAttack()
    {
        MagicAttack();
    }

    // Visualize attack range in editor
    private void OnDrawGizmosSelected()
    {
        if (bossMove != null)
        {
            // 绘制近战攻击范围
            Vector2 attackDirection = bossMove.IsFacingRight ? Vector2.right : Vector2.left;
            Vector2 attackPosition = (Vector2)transform.position + attackDirection * meleeAttackRange;
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPosition, meleeAttackRadius);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, attackDirection * meleeAttackRange);
        }
        
        // 绘制所有魔法生成点
        if (magicSpawnPoints != null && magicSpawnPoints.Length > 0)
        {
            for (int i = 0; i < magicSpawnPoints.Length; i++)
            {
                if (magicSpawnPoints[i] != null)
                {
                    // 不同颜色表示不同的生成点
                    Color[] colors = { Color.blue, Color.cyan, Color.magenta, Color.green, Color.yellow };
                    Gizmos.color = colors[i % colors.Length];
                    
                    Gizmos.DrawWireSphere(magicSpawnPoints[i].position, 0.2f);
                    
                    // 显示发射方向
                    Vector3 direction = bossMove != null && bossMove.IsFacingRight ? Vector3.right : Vector3.left;
                    Gizmos.DrawRay(magicSpawnPoints[i].position, direction * 1.5f);
                    
                    // 绘制从Boss到魔法生成点的连线
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(transform.position, magicSpawnPoints[i].position);
                }
            }
        }
    }
}

// Simple projectile script for magic projectiles
public class MagicProjectile : MonoBehaviour
{
    private int damage;
    private Vector2 direction;
    private LayerMask targetLayer;
    private bool hasHitTarget = false;
    private bool isHoming = false;
    private Transform target;
    private float homingSpeed = 5f;
    
    public void Initialize(int damage, Vector2 direction, LayerMask targetLayer)
    {
        this.damage = damage;
        this.direction = direction;
        this.targetLayer = targetLayer;
        
        Debug.Log($"Magic projectile initialized - Damage: {damage}, Direction: {direction}, TargetLayer: {targetLayer}");
    }
    
    public void EnableHoming(bool enable)
    {
        isHoming = enable;
        if (isHoming)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                Debug.Log("Homing enabled for magic projectile");
            }
        }
    }
    
    private void Update()
    {
        if (isHoming && target != null)
        {
            // Simple homing behavior
            Vector2 targetDirection = (target.position - transform.position).normalized;
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.Lerp(rb.velocity, targetDirection * homingSpeed, Time.deltaTime * 2f);
            }
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHitTarget) return; // 防止重复伤害
        
        Debug.Log($"Magic projectile hit: {collision.name}, Layer: {collision.gameObject.layer}, Tag: {collision.tag}");
        
        // 检查是否击中玩家
        bool hitPlayer = false;
        
        // 方法1: 检查层级
        if (((1 << collision.gameObject.layer) & targetLayer) != 0)
        {
            hitPlayer = true;
        }
        
        // 方法2: 检查标签 (备用)
        if (collision.CompareTag("Player"))
        {
            hitPlayer = true;
        }
        
        if (hitPlayer)
        {
            // Hit player
            HeroLife playerLife = collision.GetComponent<HeroLife>();
            if (playerLife != null)
            {
                playerLife.TakeDamage(damage);
                hasHitTarget = true;
                Debug.Log($"Magic projectile dealt {damage} damage to player");
                
                // 可以在这里添加击中特效
                // if (hitEffectPrefab != null)
                // {
                //     Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                // }
            }
            else
            {
                Debug.LogWarning("Hit player but no HeroLife component found!");
            }
            
            // Destroy the projectile
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Ground") || collision.CompareTag("Wall") || collision.CompareTag("Platform"))
        {
            // Hit environment, destroy projectile
            Debug.Log("Magic projectile hit environment, destroying");
            Destroy(gameObject);
        }
    }
    
    private void OnBecameInvisible()
    {
        // 当弹射物离开屏幕时销毁
        Destroy(gameObject);
    }
}
