using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MomHitState : IState
{
    private MomP manager;
    private MomParameter parameter;
    private AnimatorStateInfo info;
    
    public MomHitState(MomP manager, MomParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }
    public void OnEnter()
    {

        parameter.animator.Play("Mom_hurt");
        
    }

    public void OnUpdate()
    {

        
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        if (info.normalizedTime >= 0.95f)
        {
            if (manager.currentHealth <= 0)
            {
                manager.TransitionState(MomStateType.Dead);
            }
            else
            {
                parameter.isHit = false; // ���ñ�����״̬
                parameter.target = GameObject.FindGameObjectWithTag("Player").transform; // ���»�ȡĿ��
                manager.TransitionState(MomStateType.Chase); // �л���׷��״̬
            }

        }

    }


    public void OnExit()
    {
       parameter.isHit = false; // ȷ���˳�ʱ���ñ�����״̬
    }
    
}


