using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MGoBlinDeadState : IState
{
    private MGoBlinP manager;
    private MGoBlinParameter parameter;
    private AnimatorStateInfo info;

    public MGoBlinDeadState(MGoBlinP manager, MGoBlinParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }
    public void OnEnter()
    {
        Debug.Log("Entering Dead State");
        parameter.animator.Play("MGoBlin_death");
    }


    public void OnUpdate()
    {
        Debug.Log("MGoBlin is dead, cannot update state");
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        if (info.normalizedTime >= 0.95)
            Die();
        return;

    }
    public void OnExit()
    {

    }
    public void Die()
    {         if (manager.currentHealth <= 0)
        {
            Debug.Log("MGoBlin has died");
            
            GameObject.Destroy(manager.gameObject, 0.5f);
        }
    }
}