using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedProjectile : MonoBehaviour
{
    private int damage;
    private LayerMask targetLayer;
    private bool hasHitTarget = false;
    
    public void Initialize(int damage, LayerMask targetLayer)
    {
        this.damage = damage;
        this.targetLayer = targetLayer;
        
        Debug.Log($"Ranged projectile initialized - Damage: {damage}");
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHitTarget) return;
        
        Debug.Log($"Ranged projectile hit: {collision.name}, Layer: {collision.gameObject.layer}, Tag: {collision.tag}");
        
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
                Debug.Log($"Ranged projectile dealt {damage} damage to player");
                
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
        else if (collision.CompareTag("Ground") || collision.CompareTag("Wall") || collision.CompareTag("Platform"))
        {
            // ���л��������ٵ�����
            Debug.Log("Ranged projectile hit environment, destroying");
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