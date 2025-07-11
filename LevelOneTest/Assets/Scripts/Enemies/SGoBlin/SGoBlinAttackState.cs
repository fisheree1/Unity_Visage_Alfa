using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SGoBlinAttackState : IState
{
    private SGoBlinP manager;
    private SGoBlinParameter parameter;
    private AnimatorStateInfo info;
    private float AttackTimer = 0f;   

    public SGoBlinAttackState(SGoBlinP manager, SGoBlinParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
        
    }
    public void OnEnter()
    {
        parameter.animator.Play("S_GoBlin_attack1");
    }

    public void OnUpdate()
    {
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        if (info.normalizedTime >= 0.95f)
        {
            
            
            if (AttackTimer >= 1f)
            {
                manager.TransitionState(SGoBlinStateType.Chase);
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


