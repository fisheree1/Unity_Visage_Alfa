using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemyDeadState : IState
{
    private RangedEnemyP manager;
    private RangedEnemyParameter parameter;
    private float deathTimer;
    private bool hasInitialized;

    public RangedEnemyDeadState(RangedEnemyP manager, RangedEnemyParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }

    public void OnEnter()
    {
        Debug.Log("RangedEnemy: Entering Dead State");
        
        // �����������
        parameter.isDead = true;
        parameter.isHit = false;
        parameter.isCharging = false;
        parameter.isAttacking = false;
        hasInitialized = false;
        
        // ����������ʱ��
        deathTimer = 0f;
        
        // ������������
        if (parameter.animator != null)
        {
            parameter.animator.SetTrigger("Death");
        }
        
        // ֹͣ�����ƶ�
        Rigidbody2D rb = manager.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true; // ����Ϊ�˶�ѧģʽ����������Ӱ��
        }
        
        // ������ײ���Է�ֹ��һ���Ľ���
        Collider2D col = manager.GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }
        
        // ����������ɫЧ��
        manager.SetSpriteColor(Color.gray);
        
        // ���Ŀ��
        parameter.target = null;
        
        // �������������������Ч
        CreateDeathEffect();
    }

    public void OnUpdate()
    {
        // ��ʼ������Ч����ִֻ��һ�Σ�
        if (!hasInitialized)
        {
            InitializeDeathEffects();
            hasInitialized = true;
        }
        
        // ����������ʱ��
        deathTimer += Time.deltaTime;
        
        // ��������������Ϻ����ٵ���
        if (deathTimer >= 2f) // 2�������
        {
            DestroyEnemy();
        }
    }

    public void OnExit()
    {
        Debug.Log("RangedEnemy: Exiting Dead State");
        // ����״̬ͨ�������˳����������˷����Է���Ҫ
    }

    private void InitializeDeathEffects()
    {
        // �����������������ʱ������Ч��
        Debug.Log("RangedEnemy: Initializing death effects");
        
        // ���磺����������Ч������������Ч��
        // AudioSource audioSource = manager.GetComponent<AudioSource>();
        // if (audioSource != null && deathSound != null)
        // {
        //     audioSource.PlayOneShot(deathSound);
        // }
    }

    private void CreateDeathEffect()
    {
        // �����򵥵�������Ч
        Debug.Log("RangedEnemy: Creating death effect");
        
        // ����������ʵ����������ЧԤ����
        // if (deathEffectPrefab != null)
        // {
        //     GameObject effect = Object.Instantiate(deathEffectPrefab, manager.transform.position, Quaternion.identity);
        //     Object.Destroy(effect, 3f);
        // }
    }

    private void DestroyEnemy()
    {
        Debug.Log("RangedEnemy: Destroying enemy");
        
        // �����������������ǰ��������
        // ���磺������Ʒ������ֵ��������
        
        // ���ٵ���GameObject
        if (manager != null)
        {
            Object.Destroy(manager.gameObject);
        }
    }
}