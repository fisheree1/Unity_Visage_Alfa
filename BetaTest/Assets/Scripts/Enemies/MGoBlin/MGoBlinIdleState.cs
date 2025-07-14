using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MGoBlinIdleState : IState
{
    private MGoBlinP manager;
    private MGoBlinParameter parameter;
    private float idleTimer=0f;

    public MGoBlinIdleState(MGoBlinP manager, MGoBlinParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }

    public void OnEnter()
    {
        
        parameter.animator.Play("MGoBlin_idle");
        idleTimer = 0f;
    }

    public void OnUpdate()
    {
        if (parameter.isHit)
        {
            manager.TransitionState(MGoBlinStateType.Hit);
            return; // ��������У������л�״̬
        }
        // 1. �����ü�飺ȷ��target����
        if (parameter.target != null)
        {
            // 2. ���ȼ������Ƿ����׷����Χ
            bool inChaseZone =
                parameter.target.position.x >= parameter.chasePoints[0].position.x &&
                parameter.target.position.x <= parameter.chasePoints[1].position.x;

            if (inChaseZone && parameter.target.CompareTag("Player"))
            {
                manager.TransitionState(MGoBlinStateType.Chase);
                return; // �����˳�״̬����
            }
        }

        // 3. ������ʱ
        idleTimer += Time.deltaTime;

        // 4. ��ʱ�������л���Ѳ��״̬���������Ѳ�ߵ㣩
        if (idleTimer >= parameter.IdleTime)
        {
            // ����Ƿ������Ч��Ѳ�ߵ�
            if (parameter.patrolPoints != null && parameter.patrolPoints.Length > 0)
            {
                manager.TransitionState(MGoBlinStateType.Patrol);
            }
            else
            {
                // û��Ѳ�ߵ��򱣳ִ��������ü�ʱ��
                idleTimer = 0;
            }
        }
    }

    public void OnExit()
    {
        // �˳�ʱ����״̬����
        idleTimer = 0;
    }
}
