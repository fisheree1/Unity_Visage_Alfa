using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HeroLifeUI : MonoBehaviour
{
    [Header("Manual Hero Reference")]
    [SerializeField] private GameObject manualHeroReference;
    
    [Header("UI Components")]
    [SerializeField] private Image healthBorder;
    [SerializeField] private Image healthFill;
    [SerializeField] private Image powerBorder;
    [SerializeField] private Image powerFill;
    [SerializeField] private Image heartIcon;
    
    [Header("Health Bar Settings")]
    [SerializeField] private float healthBarSmoothTime = 0.2f;
    [SerializeField] private bool enableHeartBeat = true;
    [SerializeField] private float heartBeatSpeed = 1.5f;
    [SerializeField] private float heartBeatScale = 1.2f;
    
    [Header("Power Bar Settings")]
    [SerializeField] private float powerBarSmoothTime = 0.1f;
    [SerializeField] private float powerRegenRate = 0.5f;
    [SerializeField] private float maxPower = 100f;
    
    [Header("Visual Effects")]
    [SerializeField] private Color lowHealthColor = Color.red;
    [SerializeField] private Color normalHealthColor = Color.green;
    [SerializeField] private float lowHealthThreshold = 0.3f;
    [SerializeField] private bool enableWarningFlash = true;
    [SerializeField] private float flashSpeed = 3f;
    
    // References
    private HeroLife heroLife;
    private HeroAttackController attackController;
    
    // Runtime values
    private float currentDisplayHealth = 1f;
    private float currentDisplayPower = 1f;
    private float currentPower = 100f;
    private Vector3 originalHeartScale;
    private bool isFlashing = false;
    
    void Start()
    {
        InitializeUI();
        FindHeroComponents();
        LoadUISprites();
        
        if (heartIcon != null)
        {
            originalHeartScale = heartIcon.transform.localScale;
        }
    }
    
    void Update()
    {
        UpdateHealthDisplay();
        UpdatePowerDisplay();
        UpdateVisualEffects();
    }
    
    private void InitializeUI()
    {
        // 如果没有手动赋值，尝试自动查找UI组件
        if (healthBorder == null) healthBorder = transform.Find("HealthBar/Border")?.GetComponent<Image>();
        if (healthFill == null) healthFill = transform.Find("HealthBar/Fill")?.GetComponent<Image>();
        if (powerBorder == null) powerBorder = transform.Find("PowerBar/Border")?.GetComponent<Image>();
        if (powerFill == null) powerFill = transform.Find("PowerBar/Fill")?.GetComponent<Image>();
        if (heartIcon == null) heartIcon = transform.Find("HeartIcon")?.GetComponent<Image>();
    }
    
    private void FindHeroComponents()
    {
        // 首先检查手动引用
        if (manualHeroReference != null)
        {
            heroLife = manualHeroReference.GetComponent<HeroLife>();
            attackController = manualHeroReference.GetComponent<HeroAttackController>();
        }
        else
        {
            // 尝试从当前对象获取组件
            heroLife = GetComponent<HeroLife>();
            attackController = GetComponent<HeroAttackController>();
            
            // 如果没找到，通过Player标签查找
            if (heroLife == null)
            {
                GameObject hero = GameObject.FindGameObjectWithTag("Player");
                if (hero != null)
                {
                    heroLife = hero.GetComponent<HeroLife>();
                    attackController = hero.GetComponent<HeroAttackController>();
                }
            }
        }
        
        // 订阅血量变化事件
        if (heroLife != null)
        {
            heroLife.OnHealthChanged += OnHealthChanged;
            heroLife.OnDeath += OnHeroDeath;
            heroLife.OnRespawn += OnHeroRespawn;
            
            // 初始化显示值
            currentDisplayHealth = (float)heroLife.CurrentHealth / heroLife.MaxHealth;
        }
    }
    
    private void LoadUISprites()
    {
        // 从Resources加载图片
        Sprite borderSprite = Resources.Load<Sprite>("Art/border");
        Sprite fillSprite = Resources.Load<Sprite>("Art/fill");
        Sprite heartSprite = Resources.Load<Sprite>("Art/heart");
        Sprite expSprite = Resources.Load<Sprite>("Art/exp"); // 用作发力条
        
        // 应用血条图片
        if (healthBorder != null && borderSprite != null)
        {
            healthBorder.sprite = borderSprite;
            healthBorder.type = Image.Type.Sliced;
        }
        
        if (healthFill != null && fillSprite != null)
        {
            healthFill.sprite = fillSprite;
            healthFill.type = Image.Type.Filled;
            healthFill.fillMethod = Image.FillMethod.Horizontal;
        }
        
        // 应用发力条图片
        if (powerBorder != null && borderSprite != null)
        {
            powerBorder.sprite = borderSprite;
            powerBorder.type = Image.Type.Sliced;
        }
        
        if (powerFill != null && expSprite != null)
        {
            powerFill.sprite = expSprite;
            powerFill.type = Image.Type.Filled;
            powerFill.fillMethod = Image.FillMethod.Horizontal;
        }
        
        // 应用心脏图标
        if (heartIcon != null && heartSprite != null)
        {
            heartIcon.sprite = heartSprite;
        }
    }
    
    private void UpdateHealthDisplay()
    {
        if (heroLife == null || healthFill == null) return;
        
        // 计算目标血量比例
        float targetHealth = (float)heroLife.CurrentHealth / heroLife.MaxHealth;
        
        // 平滑过渡血量显示
        currentDisplayHealth = Mathf.Lerp(currentDisplayHealth, targetHealth, Time.deltaTime / healthBarSmoothTime);
        healthFill.fillAmount = currentDisplayHealth;
        
        // 更新血条颜色
        UpdateHealthColor();
    }
    
    private void UpdatePowerDisplay()
    {
        if (powerFill == null) return;
        
        // 发力值回复逻辑
        if (currentPower < maxPower)
        {
            currentPower = Mathf.Min(maxPower, currentPower + powerRegenRate * maxPower * Time.deltaTime);
        }
        
        // 检查攻击消耗（如果有攻击控制器）
        if (attackController != null)
        {
            // 这里可以添加攻击消耗发力值的逻辑
            // 例如：if (attackController.IsAttacking) currentPower -= attackCost;
        }
        
        // 计算目标发力比例
        float targetPower = currentPower / maxPower;
        
        // 平滑过渡发力显示
        currentDisplayPower = Mathf.Lerp(currentDisplayPower, targetPower, Time.deltaTime / powerBarSmoothTime);
        powerFill.fillAmount = currentDisplayPower;
    }
    
    private void UpdateHealthColor()
    {
        if (healthFill == null) return;
        
        float healthPercent = currentDisplayHealth;
        
        // 低血量时变红
        if (healthPercent <= lowHealthThreshold)
        {
            // 血量警告闪烁效果
            if (enableWarningFlash && !isFlashing)
            {
                StartCoroutine(HealthWarningFlash());
            }
            healthFill.color = Color.Lerp(lowHealthColor, normalHealthColor, healthPercent / lowHealthThreshold);
        }
        else
        {
            healthFill.color = normalHealthColor;
            isFlashing = false;
        }
    }
    
    private void UpdateVisualEffects()
    {
        // 心跳效果
        if (enableHeartBeat && heartIcon != null && heroLife != null && !heroLife.IsDead)
        {
            float heartBeat = 1f + Mathf.Sin(Time.time * heartBeatSpeed) * 0.1f * heartBeatScale;
            heartIcon.transform.localScale = originalHeartScale * heartBeat;
        }
    }
    
    private IEnumerator HealthWarningFlash()
    {
        isFlashing = true;
        
        while (heroLife != null && (float)heroLife.CurrentHealth / heroLife.MaxHealth <= lowHealthThreshold && !heroLife.IsDead)
        {
            // 闪烁效果
            float alpha = (Mathf.Sin(Time.time * flashSpeed) + 1f) * 0.5f;
            Color flashColor = Color.Lerp(lowHealthColor, Color.white, alpha);
            
            if (healthFill != null)
            {
                healthFill.color = flashColor;
            }
            
            yield return null;
        }
        
        isFlashing = false;
    }
    
    // 事件处理
    private void OnHealthChanged(int newHealth)
    {
        // 血量变化时的额外效果可以在这里添加
    }
    
    private void OnHeroDeath()
    {
        // 死亡时隐藏心跳效果
        if (heartIcon != null)
        {
            heartIcon.transform.localScale = originalHeartScale;
        }
    }
    
    private void OnHeroRespawn()
    {
        // 重生时重置UI状态
        currentPower = maxPower;
        isFlashing = false;
    }
    
    // 公共方法供外部调用
    public void ConsumePower(float amount)
    {
        currentPower = Mathf.Max(0, currentPower - amount);
    }
    
    public void AddPower(float amount)
    {
        currentPower = Mathf.Min(maxPower, currentPower + amount);
    }
    
    public bool HasEnoughPower(float amount)
    {
        return currentPower >= amount;
    }
    
    public float GetCurrentPowerPercent()
    {
        return currentPower / maxPower;
    }
    
    void OnDestroy()
    {
        // 取消事件订阅
        if (heroLife != null)
        {
            heroLife.OnHealthChanged -= OnHealthChanged;
            heroLife.OnDeath -= OnHeroDeath;
            heroLife.OnRespawn -= OnHeroRespawn;
        }
    }
}
