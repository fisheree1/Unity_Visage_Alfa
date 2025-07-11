using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderIdleState : IState
{
    private SpiderP manager;
    private SpiderParameter parameter;
    private float idleTimer;

    public SpiderIdleState(SpiderP manager, SpiderParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }

    public void OnEnter()
    {
        // ���Ŵ�������
        parameter.animator.Play("Spider_idle");
        idleTimer = 0;
    }

    public void OnUpdate()
    {
        // 1. �����ü�飺ȷ��target����
        if (parameter.target != null)
        {
            // 2. ���ȼ������Ƿ����׷����Χ
            bool inChaseZone =
                parameter.target.position.x >= parameter.chasePoints[0].position.x &&
                parameter.target.position.x <= parameter.chasePoints[1].position.x;

            if (inChaseZone && parameter.target.CompareTag("Player"))
            {
                manager.TransitionState(SpiderStateType.Chase);
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
                manager.TransitionState(SpiderStateType.Patrol);
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
