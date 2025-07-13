using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeAttackState : IState
{
    private SlimeP manager;
    private SlimeParameter parameter;
    private AnimatorStateInfo info;
    private int AttackCount = 0;

    private bool isInAttackRange = false;

    public SlimeAttackState(SlimeP manager, SlimeParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;

    }
    public void OnEnter()
    {

        parameter.animator.Play("Slime_attack");

    }

    public void OnUpdate()
    {
        if (parameter.isHit)
        {
            manager.TransitionState(SlimeStateType.Hit);
            return; // 如果被击中，立即切换状态
        }
        isInAttackRange = CheckAttackRange();
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        if (info.normalizedTime >= 0.95f)
        {

            if (isInAttackRange)
            {
                manager.TransitionState(SlimeStateType.Attack);


            }
            else
            {
                manager.TransitionState(SlimeStateType.Idle);
            }
        }



    }


    public void OnExit()
    {
        Debug.Log("Attack Count: " + AttackCount);
    }
    private bool CheckAttackRange()
    {
        return Physics2D.OverlapCircle(
            parameter.attackPoint.position,
            parameter.attackArea,
            parameter.targetLayer
        );
    }
}


