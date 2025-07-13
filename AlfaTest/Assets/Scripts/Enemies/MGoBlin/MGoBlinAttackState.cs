using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MGoBlinAttackState : IState
{
    private MGoBlinP manager;
    private MGoBlinParameter parameter;
    private AnimatorStateInfo info;
    private int AttackCount = 0;  
    
    private bool isInAttackRange = false;

    public MGoBlinAttackState(MGoBlinP manager, MGoBlinParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
        
    }
    public void OnEnter()
    {
        
            parameter.animator.Play("MGoBlin_attack1");
            
    }

    public void OnUpdate()
    {
        
        
            isInAttackRange = CheckAttackRange();
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        if (parameter.isHit)
        {
            manager.TransitionState(MGoBlinStateType.Hit);
            return; // 如果被击中，立即切换状态
        }
        else
        if (info.normalizedTime >= 0.95f)
        {

            if (isInAttackRange)
            {
                manager.TransitionState(MGoBlinStateType.Attack);


            }
            else
            {
                manager.TransitionState(MGoBlinStateType.Idle);
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


