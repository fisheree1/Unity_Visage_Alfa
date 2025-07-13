using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MGoBlinDeadState : IState
{
    private MGoBlinP manager;
    private MGoBlinParameter parameter;
    private float Timer = 0f;

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
        Timer += Time.deltaTime;
        if (Timer >= 0.6f)
        {
            // Ïú»ÙMGoBlin¶ÔÏó
            Object.Destroy(manager.gameObject);
            Debug.Log("MGoBlin has been destroyed after death");
        }

    }
    public void OnExit()
    {
        return;
    }
    
    
}