using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MGoBlinDamagedState : IState
{
    private MGoBlinP manager;
    private MGoBlinParameter parameter;
    private AnimatorStateInfo info;
    public MGoBlinDamagedState(MGoBlinP manager, MGoBlinParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }
    public void OnEnter()
    {
        parameter.animator.Play("MGoBlin_hurt");
        Debug.Log("Entering Damaged State");
    }

    public void OnUpdate()
    {
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        

        if (info.normalizedTime >= 0.95f)
        {
            
             
            if ((manager.currentHealth<=manager.maxHealth)&&(manager.currentHealth>0))
            {
                if (manager.parameter.target != null)
                    manager.TransitionState(MGoBlinStateType.Chase);
                else
                    manager.TransitionState(MGoBlinStateType.Idle);
            }
           
            
        }
    }
    public void OnExit()
    {

    }
}
    
