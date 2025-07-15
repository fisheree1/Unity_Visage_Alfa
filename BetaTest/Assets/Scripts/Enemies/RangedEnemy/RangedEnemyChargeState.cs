using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemyChargeState : IState
{
    private RangedEnemyP manager;
    private RangedEnemyParameter parameter;
    private float chargeTimer;

    public RangedEnemyChargeState(RangedEnemyP manager, RangedEnemyParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }

    public void OnEnter()
    {
        Debug.Log("RangedEnemy: Entering Charge State");
        
        // �����������
        parameter.isCharging = true;
        parameter.isAttacking = false;
        
        // ����������ʱ��
        chargeTimer = 0f;
        
        // ������������
        if (parameter.animator != null)
        {
            parameter.animator.SetTrigger("Charge");
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
        // ��������У��ж���������������״̬
        if (parameter.isHit)
        {
            parameter.isCharging = false;
            manager.TransitionState(RangedEnemyStateType.Hurt);
            return;
        }

        // ���������������������״̬
        if (manager.IsDead || parameter.isDead)
        {
            parameter.isCharging = false;
            manager.TransitionState(RangedEnemyStateType.Dead);
            return;
        }

        // ���û��Ŀ�꣬���ش���״̬
        if (parameter.target == null)
        {
            parameter.isCharging = false;
            manager.TransitionState(RangedEnemyStateType.Idle);
            return;
        }

        // ���Ŀ���Ƿ����ڹ�����Χ��
        float distanceToTarget = Vector2.Distance(manager.transform.position, parameter.target.position);
        if (distanceToTarget > parameter.attackRange)
        {
            parameter.isCharging = false;
            manager.TransitionState(RangedEnemyStateType.Idle);
            return;
        }

        // ��������Ŀ��
        manager.FlipTo(parameter.target);

        // ����������ʱ��
        chargeTimer += Time.deltaTime;

        // ������ɣ����빥��״̬
        if (chargeTimer >= parameter.chargeDuration)
        {
            parameter.isCharging = false;
            manager.TransitionState(RangedEnemyStateType.Attack);
        }
    }

    public void OnExit()
    {
        Debug.Log("RangedEnemy: Exiting Charge State");
        parameter.isCharging = false;
    }
}