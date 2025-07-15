using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemyIdleState : IState
{
    private RangedEnemyP manager;
    private RangedEnemyParameter parameter;

    public RangedEnemyIdleState(RangedEnemyP manager, RangedEnemyParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }

    public void OnEnter()
    {
        Debug.Log("RangedEnemy: Entering Idle State");
        
        // ���Ŵ�������
        if (parameter.animator != null)
        {
            parameter.animator.SetTrigger("Idle");
        }
        
        // ֹͣ�ƶ�
        Rigidbody2D rb = manager.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        
        // ����״̬���
        parameter.isCharging = false;
        parameter.isAttacking = false;
    }

    public void OnUpdate()
    {
        // ��������У���������״̬
        if (parameter.isHit)
        {
            manager.TransitionState(RangedEnemyStateType.Hurt);
            return;
        }

        // ���������������������״̬
        if (manager.IsDead || parameter.isDead)
        {
            manager.TransitionState(RangedEnemyStateType.Dead);
            return;
        }

        // Ѱ��Ŀ��
        if (parameter.target == null)
        {
            FindTarget();
        }

        // �����Ŀ�꣬����Ƿ�Ӧ�ÿ�ʼ����
        if (parameter.target != null)
        {
            float distanceToTarget = Vector2.Distance(manager.transform.position, parameter.target.position);
            
            // ����Ŀ��
            manager.FlipTo(parameter.target);
            
            // ���Ŀ���ڹ�����Χ���ҿ��Թ�������ʼ����
            if (distanceToTarget <= parameter.attackRange && manager.CanAttack())
            {
                manager.TransitionState(RangedEnemyStateType.Charge);
            }
            // ���Ŀ��̫�������Կ��Ǻ��ˣ���ѡ���ܣ�
            else if (distanceToTarget < parameter.retreatRange)
            {
                // ������������Ӻ����߼�
                // Ŀǰ�����ڴ���״̬
            }
            // ���Ŀ��̫Զ��ʧȥĿ��
            else if (distanceToTarget > parameter.detectionRange)
            {
                parameter.target = null;
            }
        }
    }

    public void OnExit()
    {
        Debug.Log("RangedEnemy: Exiting Idle State");
    }

    private void FindTarget()
    {
        // Ѱ�����
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float distance = Vector2.Distance(manager.transform.position, player.transform.position);
            if (distance <= parameter.detectionRange)
            {
                parameter.target = player.transform;
                Debug.Log("RangedEnemy: Target found - Player");
            }
        }
    }
}