using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeHitState : IState
{
    private SlimeP manager;
    private SlimeParameter parameter;
    private AnimatorStateInfo info;

    public SlimeHitState(SlimeP manager, SlimeParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }
    public void OnEnter()
    {

        parameter.animator.Play("Slime_hurt");

    }

    public void OnUpdate()
    {


        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        if (info.normalizedTime >= 0.95f)
        {
            if (manager.currentHealth <= 0)
            {
                manager.TransitionState(SlimeStateType.Dead);
            }
            else
            {
                parameter.isHit = false; // ���ñ�����״̬
                parameter.target = GameObject.FindGameObjectWithTag("Player").transform; // ���»�ȡĿ��
                manager.TransitionState(SlimeStateType.Chase); // �л���׷��״̬
            }

        }

    }


    public void OnExit()
    {
        parameter.isHit = false; // ȷ���˳�ʱ���ñ�����״̬
    }

}


