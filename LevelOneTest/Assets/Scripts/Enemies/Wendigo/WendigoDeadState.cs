using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WendigoDeadState : IState
{
    private WendigoP manager;
    private WendigoParameter parameter;
    
    public WendigoDeadState(WendigoP manager, WendigoParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }
    public void OnEnter()
    {
        Debug.Log("Entering Dead State");
        Die();
    }


    public void OnUpdate()
    {
        Debug.Log("Wendigo is dead, cannot update state");
        

    }
    public void OnExit()
    {

    }
    public void Die()
    {         if (manager.currentHealth <= 0)
        {
            Debug.Log("Wendigo has died");
            parameter.animator.Play("Wendigo_dead");
            GameObject.Destroy(manager.gameObject, 0.5f);
        }
    }
}