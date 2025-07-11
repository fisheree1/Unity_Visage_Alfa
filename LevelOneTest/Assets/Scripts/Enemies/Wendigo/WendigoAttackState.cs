using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WendigoAttackState : IState
{
    private WendigoP manager;
    private WendigoParameter parameter;
    private AnimatorStateInfo info;
    private float AttackTimer = 0f;   

    public WendigoAttackState(WendigoP manager, WendigoParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
        
    }
    public void OnEnter()
    {
        parameter.animator.Play("Wendigo_attack");
    }

    public void OnUpdate()
    {
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        if (info.normalizedTime >= 1.0f)
        {
            
            
            if (AttackTimer >= 1f)
            {
                manager.TransitionState(WendigoStateType.Chase);
                AttackTimer = 0f; // ÷ÿ÷√º∆ ±∆˜
            }
            else
            {
                AttackTimer += Time.deltaTime;
            }
            
        }
    }


    public void OnExit()
    {
        Debug.Log("Exiting Attack State");
    }
}


