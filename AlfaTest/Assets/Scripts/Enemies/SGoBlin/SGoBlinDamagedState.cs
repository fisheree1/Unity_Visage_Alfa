using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SGoBlinDamagedState : IState
{
    private SGoBlinP manager;
    private SGoBlinParameter parameter;
    private AnimatorStateInfo info;
    public SGoBlinDamagedState(SGoBlinP manager, SGoBlinParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }
    public void OnEnter()
    {
        parameter.animator.Play("SGoBlin_hurt");
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
                    manager.TransitionState(SGoBlinStateType.Chase);
                else
                    manager.TransitionState(SGoBlinStateType.Idle);
            }
           
            
        }
    }
    public void OnExit()
    {

    }
}
    
