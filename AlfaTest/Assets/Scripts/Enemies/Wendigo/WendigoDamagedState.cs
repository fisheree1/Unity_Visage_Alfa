using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WendigoDamagedState : IState
{
    private WendigoP manager;
    private WendigoParameter parameter;
    private AnimatorStateInfo info;
    public WendigoDamagedState(WendigoP manager, WendigoParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }
    public void OnEnter()
    {
        parameter.animator.Play("Wendigo_damaged");
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
                    manager.TransitionState(WendigoStateType.Chase);
                else
                    manager.TransitionState(WendigoStateType.Idle);
            }
           
            
        }
    }
    public void OnExit()
    {

    }
}
    
