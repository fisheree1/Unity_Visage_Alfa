using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MGoBlinHitState : IState
{
    private MGoBlinP manager;
    private MGoBlinParameter parameter;
    private AnimatorStateInfo info;
    
    public MGoBlinHitState(MGoBlinP manager, MGoBlinParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }
    public void OnEnter()
    {

        parameter.animator.Play("MGoBlin_hurt");
        
    }

    public void OnUpdate()
    {

        
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        if (info.normalizedTime >= 0.95f)
        {
            if (manager.currentHealth <= 0)
            {
                manager.TransitionState(MGoBlinStateType.Dead);
            }
            else
            {
                parameter.isHit = false; // ���ñ�����״̬
                parameter.target = GameObject.FindGameObjectWithTag("Player").transform; // ���»�ȡĿ��
                manager.TransitionState(MGoBlinStateType.Chase); // �л���׷��״̬
            }

        }

    }


    public void OnExit()
    {
       parameter.isHit = false; // ȷ���˳�ʱ���ñ�����״̬
    }
    
}


