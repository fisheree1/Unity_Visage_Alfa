using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderHitState : IState
{
    private SpiderP manager;
    private SpiderParameter parameter;
    private AnimatorStateInfo info;
    

    public SpiderHitState(SpiderP manager, SpiderParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;

    }
    public void OnEnter()
    {

        parameter.animator.Play("Spider_hurt");
        
    }

    public void OnUpdate()
    {

        
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        if (info.normalizedTime >= 0.95f)
        {
            if (manager.IsDead)
            {
                manager.TransitionState(SpiderStateType.Dead);
            }
            else
            {
                parameter.isHit = false; // ���ñ�����״̬
                parameter.target = GameObject.FindGameObjectWithTag("Player").transform; // ���»�ȡĿ��
                manager.TransitionState(SpiderStateType.Chase); // �л���׷��״̬
            }

        }

    }


    public void OnExit()
    {
       parameter.isHit = false; // ȷ���˳�ʱ���ñ�����״̬
    }
    
}


