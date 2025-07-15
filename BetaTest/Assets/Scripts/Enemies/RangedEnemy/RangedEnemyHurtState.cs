using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemyHurtState : IState
{
    private RangedEnemyP manager;
    private RangedEnemyParameter parameter;
    private float hurtTimer;
    private Vector2 knockbackDirection;
    private bool hasAppliedKnockback;

    public RangedEnemyHurtState(RangedEnemyP manager, RangedEnemyParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }

    public void OnEnter()
    {
        Debug.Log("RangedEnemy: Entering Hurt State");
        
        // �������˱��
        parameter.isHit = true;
        parameter.isCharging = false;
        parameter.isAttacking = false;
        hasAppliedKnockback = false;
        
        // �������˼�ʱ��
        hurtTimer = 0f;
        
        // �������˶���
        if (parameter.animator != null)
        {
            parameter.animator.SetTrigger("Hurt");
        }
        
        // ����������ɫЧ��
        manager.SetSpriteColor(Color.red);
        
        // ������˷���
        if (parameter.target != null)
        {
            Vector2 directionFromPlayer = (manager.transform.position - parameter.target.position).normalized;
            knockbackDirection = directionFromPlayer;
        }
        else
        {
            // ���û��Ŀ�꣬���ѡ��һ������
            knockbackDirection = new Vector2(Random.Range(-1f, 1f), 0).normalized;
        }
        
        // ֹͣ��ǰ�ƶ�
        Rigidbody2D rb = manager.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    public void OnUpdate()
    {
        // �����������������ת��������״̬
        if (manager.IsDead || parameter.isDead)
        {
            parameter.isHit = false;
            manager.TransitionState(RangedEnemyStateType.Dead);
            return;
        }

        // ������״̬��ʼʱӦ�û���Ч��
        if (!hasAppliedKnockback && parameter.hurtKnockbackForce > 0)
        {
            Rigidbody2D rb = manager.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 knockbackForce = knockbackDirection * parameter.hurtKnockbackForce;
                rb.AddForce(knockbackForce, ForceMode2D.Impulse);
                hasAppliedKnockback = true;
                Debug.Log($"RangedEnemy: Applied knockback force {knockbackForce}");
            }
        }

        // �������˼�ʱ��
        hurtTimer += Time.deltaTime;

        // ����״̬���������ش���״̬
        if (hurtTimer >= parameter.hurtDuration)
        {
            parameter.isHit = false;
            manager.TransitionState(RangedEnemyStateType.Idle);
        }
    }

    public void OnExit()
    {
        Debug.Log("RangedEnemy: Exiting Hurt State");
        
        // �������˱��
        parameter.isHit = false;
        hasAppliedKnockback = false;
        
        // �ָ�ԭʼ��ɫ
        manager.ResetSpriteColor();
        
        // ֹͣ����Ч��
        Rigidbody2D rb = manager.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }
}