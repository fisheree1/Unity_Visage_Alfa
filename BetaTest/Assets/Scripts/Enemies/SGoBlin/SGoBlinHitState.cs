using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SGoBlinHitState : IState
{
    private SGoBlinP manager;
    private SGoBlinParameter parameter;
    private AnimatorStateInfo info;
    

    public SGoBlinHitState(SGoBlinP manager, SGoBlinParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;

    }
    public void OnEnter()
    {

        parameter.animator.Play("S_GoBlin_hurt");
        
    }

    public void OnUpdate()
    {

        
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        if (info.normalizedTime >= 0.95f)
        {
            if (manager.IsDead)
            {
                manager.TransitionState(SGoBlinStateType.Dead);
            }
            else
            {
                parameter.isHit = false; // ���ñ�����״̬
                parameter.target = GameObject.FindGameObjectWithTag("Player").transform; // ���»�ȡĿ��
                manager.TransitionState(SGoBlinStateType.Chase); // �л���׷��״̬
            }

        }

    }


    public void OnExit()
    {
       parameter.isHit = false; // ȷ���˳�ʱ���ñ�����״̬
    }
    
}


