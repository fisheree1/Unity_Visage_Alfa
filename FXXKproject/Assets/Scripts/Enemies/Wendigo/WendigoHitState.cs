using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WendigoHitState : IState
{
    private WendigoP manager;
    private WendigoParameter parameter;
    private AnimatorStateInfo info;
    

    public WendigoHitState(WendigoP manager, WendigoParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;

    }
    public void OnEnter()
    {

        parameter.animator.Play("Wendigo_hurt");
        
    }

    public void OnUpdate()
    {

        
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        if (info.normalizedTime >= 0.95f)
        {
            if (manager.currentHealth <= 0)
            {
                manager.TransitionState(WendigoStateType.Dead);
            }
            else
            {
                parameter.isHit = false; // ���ñ�����״̬
                parameter.target = GameObject.FindGameObjectWithTag("Player").transform; // ���»�ȡĿ��
                manager.TransitionState(WendigoStateType.Chase); // �л���׷��״̬
            }

        }

    }


    public void OnExit()
    {
       parameter.isHit = false; // ȷ���˳�ʱ���ñ�����״̬
    }
    
}


