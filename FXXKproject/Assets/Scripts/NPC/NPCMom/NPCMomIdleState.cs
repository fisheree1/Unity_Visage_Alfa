using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCMomIdleState : IState
{
    private NPCMomP manager;
    private NPCMomParameter parameter;
    private float idleTimer = 0f;

    public NPCMomIdleState(NPCMomP manager, NPCMomParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }

    public void OnEnter()
    {
        parameter.animator.Play("Mom_idle");
        idleTimer = 0f;
    }

    public void OnUpdate()
    {
        // ������ʱ
        idleTimer += Time.deltaTime;

        // ��ʱ�������л���Ѳ��״̬���������Ѳ�ߵ㣩
        if (idleTimer >= parameter.IdleTime)
        {
            // ����Ƿ������Ч��Ѳ�ߵ�
            if (parameter.patrolPoints != null && parameter.patrolPoints.Length > 0)
            {
                manager.TransitionState(NPCMomStateType.Patrol);
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
