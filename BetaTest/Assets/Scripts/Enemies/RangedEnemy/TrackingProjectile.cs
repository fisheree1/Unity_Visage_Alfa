using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackingProjectile : MonoBehaviour
{
    private int damage;
    private Transform target;
    private LayerMask targetLayer;
    private bool hasHitTarget = false;
    
    [Header("Tracking Settings")]
    [SerializeField] private float trackingSpeed = 3f;
    [SerializeField] private float trackingStrength = 2f;
    [SerializeField] private float maxTrackingDistance = 10f;
    
    private Rigidbody2D rb;
    private Vector2 initialVelocity;
    
    public void Initialize(int damage, Transform target, LayerMask targetLayer)
    {
        this.damage = damage;
        this.target = target;
        this.targetLayer = targetLayer;
        
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            initialVelocity = rb.velocity;
        }
        
        Debug.Log($"Tracking projectile initialized - Damage: {damage}, Target: {target?.name}");
    }
    
    private void Update()
    {
        if (hasHitTarget || target == null) return;
        
        // ���Ŀ�����
        float distanceToTarget = Vector2.Distance(transform.position, target.position);
        if (distanceToTarget > maxTrackingDistance)
        {
            // Ŀ��̫Զ��ֹͣ׷��
            return;
        }
        
        // ׷��Ŀ��
        Vector2 targetDirection = (target.position - transform.position).normalized;
        
        if (rb != null)
        {
            // �𽥵����ٶȷ�����Ŀ��
            Vector2 currentVelocity = rb.velocity;
            Vector2 desiredVelocity = targetDirection * trackingSpeed;
            
            // ʹ�ò�ֵ��ƽ��׷��
            Vector2 newVelocity = Vector2.Lerp(currentVelocity, desiredVelocity, trackingStrength * Time.deltaTime);
            rb.velocity = newVelocity;
            
            // ��ת�����������ƶ�����
            if (newVelocity.magnitude > 0.1f)
            {
                float angle = Mathf.Atan2(newVelocity.y, newVelocity.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHitTarget) return;
        
        Debug.Log($"Tracking projectile hit: {collision.name}, Layer: {collision.gameObject.layer}, Tag: {collision.tag}");
        
        // ����Ƿ����Ŀ��
        bool hitTarget = false;
        
        // ���㼶
        if (((1 << collision.gameObject.layer) & targetLayer) != 0)
        {
            hitTarget = true;
        }
        
        // ����ǩ
        if (collision.CompareTag("Player"))
        {
            hitTarget = true;
        }
        
        if (hitTarget)
        {
            // �������
            HeroLife playerLife = collision.GetComponent<HeroLife>();
            if (playerLife != null)
            {
                playerLife.TakeDamage(damage);
                hasHitTarget = true;
                Debug.Log($"Tracking projectile dealt {damage} damage to player");
                
                // ������������ӻ�����Ч
                CreateHitEffect();
            }
            else
            {
                Debug.LogWarning("Hit player but no HeroLife component found!");
            }
            
            // ���ٵ�����
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Wall"))
        {
            // ���л��������ٵ�����
            Debug.Log("Tracking projectile hit environment, destroying");
            Destroy(gameObject);
        }
    }
    
    private void CreateHitEffect()
    {
        // �����򵥵Ļ�����Ч
        GameObject effect = new GameObject("HitEffect");
        effect.transform.position = transform.position;
        
        // �������Ч���������Ӿ�Ч��
        // �������ʵ����Ԥ�Ƶ���Ч
        
        // �򵥵�������Ч
        Destroy(effect, 1f);
    }
    
    private void OnBecameInvisible()
    {
        // ���������뿪��Ļʱ����
        Destroy(gameObject);
    }
}