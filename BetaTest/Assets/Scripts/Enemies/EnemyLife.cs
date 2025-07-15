using System;
using UnityEngine;

public class EnemyLife : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 20;
    [SerializeField] private int currentHealth;
    
    // �¼�
    public event Action OnDeath;
    public event Action<int, int> OnHealthChanged; // (currentHealth, maxHealth)
    public event Action<int> OnDamageTaken; // (damage)
    public event Action<int> OnHealed; // (healAmount)
    
    // ����
    public bool IsDead { get; private set; }
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public float HealthPercentage => maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
    
    private void Start()
    {
        currentHealth = maxHealth;
        IsDead = false;
        
        // ������ʼѪ���¼�
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    public void TakeDamage(int damage)
    {
        if (IsDead) return;
        
        int actualDamage = Mathf.Min(damage, currentHealth);
        currentHealth -= actualDamage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        // �����˺��¼�
        OnDamageTaken?.Invoke(actualDamage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        if (IsDead) return;
        
        IsDead = true;
        Debug.Log($"{gameObject.name} has died!");
        
        // ���������¼�
        OnDeath?.Invoke();
    }
    
    public void Heal(int amount)
    {
        if (IsDead) return;
        
        int actualHeal = Mathf.Min(amount, maxHealth - currentHealth);
        if (actualHeal > 0)
        {
            currentHealth += actualHeal;
            
            // ���������¼�
            OnHealed?.Invoke(actualHeal);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }
    
    // ǿ�������������ⲿ���ã�
    public void ForceDeath()
    {
        if (!IsDead)
        {
            currentHealth = 0;
            Die();
        }
    }
    
    // �������Ѫ��
    public void SetMaxHealth(int newMaxHealth)
    {
        maxHealth = Mathf.Max(1, newMaxHealth);
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    // �ָ���Ѫ
    public void RestoreToFullHealth()
    {
        if (IsDead) return;
        
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    // ��ȡѪ������
    public bool IsHealthLow(float threshold = 0.3f)
    {
        return HealthPercentage <= threshold;
    }
    
    public bool IsHealthCritical(float threshold = 0.1f)
    {
        return HealthPercentage <= threshold;
    }
}