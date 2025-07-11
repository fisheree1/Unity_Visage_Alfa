using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderDamagedState : IState
{
    private SpiderP manager;
    private SpiderParameter parameter;
    private AnimatorStateInfo info;
    public SpiderDamagedState(SpiderP manager, SpiderParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }
    public void OnEnter()
    {
        parameter.animator.Play("Spider_damage");
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
                    manager.TransitionState(SpiderStateType.Chase);
                else
                    manager.TransitionState(SpiderStateType.Idle);
            }
           
            
        }
    }
    public void OnExit()
    {

    }
}
    
