using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemyAttackState : IState
{
    private RangedEnemyP manager;
    private RangedEnemyParameter parameter;
    private float attackTimer;
    private bool hasAttacked;

    public RangedEnemyAttackState(RangedEnemyP manager, RangedEnemyParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }

    public void OnEnter()
    {
        Debug.Log("RangedEnemy: Entering Attack State");
        
        // ���ù������
        parameter.isAttacking = true;
        parameter.isCharging = false;
        hasAttacked = false;
        attackTimer = 0f;
        
        // ���Ź�������
        if (parameter.animator != null)
        {
            parameter.animator.SetTrigger("Attack");
        }
        
        // ֹͣ�ƶ�
        Rigidbody2D rb = manager.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        
        // ����Ŀ��
        if (parameter.target != null)
        {
            manager.FlipTo(parameter.target);
        }
    }

    public void OnUpdate()
    {
        // ��������У��жϹ�������������״̬
        if (parameter.isHit)
        {
            parameter.isAttacking = false;
            manager.TransitionState(RangedEnemyStateType.Hurt);
            return;
        }

        // ���������������������״̬
        if (manager.IsDead || parameter.isDead)
        {
            parameter.isAttacking = false;
            manager.TransitionState(RangedEnemyStateType.Dead);
            return;
        }

        // ���¹�����ʱ��
        attackTimer += Time.deltaTime;

        // �ڹ����������м�ʱ��ִ�й���
        if (!hasAttacked && attackTimer >= 0.3f)
        {
            PerformAttack();
            hasAttacked = true;
        }

        // �����������������ش���״̬
        if (attackTimer >= 0.6f)
        {
            parameter.isAttacking = false;
            manager.SetAttackTime(); // ���ù�����ȴʱ��
            manager.TransitionState(RangedEnemyStateType.Idle);
        }
    }

    public void OnExit()
    {
        Debug.Log("RangedEnemy: Exiting Attack State");
        parameter.isAttacking = false;
    }

    private void PerformAttack()
    {
        if (parameter.target == null)
        {
            Debug.LogWarning("RangedEnemy: No target for attack");
            return;
        }

        // ���Ŀ���Ƿ����ڹ�����Χ��
        float distanceToTarget = Vector2.Distance(manager.transform.position, parameter.target.position);
        if (distanceToTarget > parameter.attackRange)
        {
            Debug.LogWarning("RangedEnemy: Target out of range during attack");
            return;
        }

        // ���ݹ���ģʽִ�в�ͬ�Ĺ���
        if (manager.ShouldUseTrackingAttack())
        {
            // ִ��׷�ٵ�����
            manager.PerformTrackingAttack();
            Debug.Log("RangedEnemy: Performing tracking attack");
        }
        else
        {
            // ִ�����ε�Ļ����
            manager.PerformFanAttack();
            Debug.Log("RangedEnemy: Performing fan attack");
        }
    }
}