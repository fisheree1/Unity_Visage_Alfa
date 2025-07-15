using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyLifeBar : MonoBehaviour
{
    [Header("Health Bar Components")]
    [SerializeField] private RectTransform topBar;
    [SerializeField] private RectTransform bottomBar;
    
    [Header("Health Bar Settings")]
    [SerializeField] private float animationSpeed = 10f;
    [SerializeField] private bool enableSmoothAnimation = true;
    
    [Header("Auto-Find EnemyLife")]
    [SerializeField] private bool autoFindEnemyLife = true;
    [SerializeField] private EnemyLife enemyLife;
    
    private float fullWidth;
    private float TargetWidth => enemyLife != null ? enemyLife.HealthPercentage * fullWidth : 0f;
    
    private Coroutine adjustBarWidthCoroutine;
    
    private void Start()
    {
        InitializeComponents();
        InitializeHealthBar();
    }
    
    private void InitializeComponents()
    {
        // �Զ����� EnemyLife ���
        if (autoFindEnemyLife && enemyLife == null)
        {
            enemyLife = GetComponent<EnemyLife>();
            if (enemyLife == null)
            {
                enemyLife = GetComponentInParent<EnemyLife>();
            }
            
            if (enemyLife == null)
            {
                Debug.LogWarning($"EnemyLifeBar on {gameObject.name} couldn't find EnemyLife component!");
                return;
            }
        }
        
        // ��֤��Ҫ���
        if (topBar == null || bottomBar == null)
        {
            Debug.LogError($"EnemyLifeBar on {gameObject.name} missing topBar or bottomBar references!");
            return;
        }
        
        // ��ȡѪ������������
        fullWidth = topBar.rect.width;
        
        // �����¼�
        SubscribeToEnemyLifeEvents();
    }
    
    private void SubscribeToEnemyLifeEvents()
    {
        if (enemyLife != null)
        {
            enemyLife.OnHealthChanged += OnHealthChanged;
            enemyLife.OnDamageTaken += OnDamageTaken;
            enemyLife.OnHealed += OnHealed;
            enemyLife.OnDeath += OnDeath;
        }
    }
    
    private void UnsubscribeFromEnemyLifeEvents()
    {
        if (enemyLife != null)
        {
            enemyLife.OnHealthChanged -= OnHealthChanged;
            enemyLife.OnDamageTaken -= OnDamageTaken;
            enemyLife.OnHealed -= OnHealed;
            enemyLife.OnDeath -= OnDeath;
        }
    }
    
    private void InitializeHealthBar()
    {
        if (enemyLife != null)
        {
            // ���ó�ʼѪ������
            float initialWidth = TargetWidth;
            topBar.SetWidth(initialWidth);
            bottomBar.SetWidth(initialWidth);
            
            Debug.Log($"EnemyLifeBar initialized for {gameObject.name}: {enemyLife.CurrentHealth}/{enemyLife.MaxHealth}");
        }
    }
    
    private void OnHealthChanged(int currentHealth, int maxHealth)
    {
        float healthPercent = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
        Debug.Log($"Enemy {gameObject.name} health changed: {currentHealth}/{maxHealth} ({healthPercent:P1})");
        
        // ����Ѫ������
        UpdateHealthBarWidth();
    }
    
    private void OnDamageTaken(int damage)
    {
        Debug.Log($"Enemy {gameObject.name} took {damage} damage");
        // �˺�ʱ�Ķ���Ч����������������
    }
    
    private void OnHealed(int healAmount)
    {
        Debug.Log($"Enemy {gameObject.name} healed {healAmount} HP");
        // ����ʱ�Ķ���Ч����������������
    }
    
    private void OnDeath()
    {
        Debug.Log($"Enemy {gameObject.name} died - hiding health bar");
        // ����ʱ����Ѫ���򲥷���������
        gameObject.SetActive(false);
    }
    
    private void UpdateHealthBarWidth()
    {
        if (enemyLife == null || topBar == null || bottomBar == null) return;
        
        if (adjustBarWidthCoroutine != null)
        {
            StopCoroutine(adjustBarWidthCoroutine);
        }
        
        if (enableSmoothAnimation)
        {
            adjustBarWidthCoroutine = StartCoroutine(AdjustBarWidthCoroutine());
        }
        else
        {
            // ��������
            float targetWidth = TargetWidth;
            topBar.SetWidth(targetWidth);
            bottomBar.SetWidth(targetWidth);
        }
    }
    
    private IEnumerator AdjustBarWidthCoroutine()
    {
        float targetWidth = TargetWidth;
        
        // ���������ϲ�Ѫ����������Ӧ��
        topBar.SetWidth(targetWidth);
        
        // ƽ�������²�Ѫ�����ӳٶ�����
        while (Mathf.Abs(bottomBar.rect.width - targetWidth) > 0.1f)
        {
            float newWidth = Mathf.Lerp(bottomBar.rect.width, targetWidth, Time.deltaTime * animationSpeed);
            bottomBar.SetWidth(newWidth);
            yield return null;
        }
        
        // ȷ�����տ���׼ȷ
        bottomBar.SetWidth(targetWidth);
        adjustBarWidthCoroutine = null;
    }
    
    // ��������
    public void SetEnemyLife(EnemyLife newEnemyLife)
    {
        if (enemyLife != null)
        {
            UnsubscribeFromEnemyLifeEvents();
        }
        
        enemyLife = newEnemyLife;
        
        if (enemyLife != null)
        {
            SubscribeToEnemyLifeEvents();
            InitializeHealthBar();
        }
    }
    
    public void SetAnimationSpeed(float speed)
    {
        animationSpeed = Mathf.Max(0.1f, speed);
    }
    
    public void SetSmoothAnimation(bool enabled)
    {
        enableSmoothAnimation = enabled;
    }
    
    // ���ڲ��Եķ���������ɾ����
    private void Update()
    {
        
    }
    
    private void OnDestroy()
    {
        UnsubscribeFromEnemyLifeEvents();
    }
    
    // ���Է���
    private void OnValidate()
    {
        if (Application.isPlaying) return;
        
        // �༭���е���֤
        if (autoFindEnemyLife && enemyLife == null)
        {
            enemyLife = GetComponent<EnemyLife>();
            if (enemyLife == null)
            {
                enemyLife = GetComponentInParent<EnemyLife>();
            }
        }
    }
}
