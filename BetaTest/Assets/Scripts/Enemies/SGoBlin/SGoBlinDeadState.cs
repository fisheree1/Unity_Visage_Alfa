using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SGoBlinDeadState : IState
{
    private SGoBlinP manager;
    private SGoBlinParameter parameter;
    private float Timer = 0f;

    public SGoBlinDeadState(SGoBlinP manager, SGoBlinParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }
    public void OnEnter()
    {
        Debug.Log("Entering Dead State");
        parameter.animator.Play("S_GoBlin_death");
    }


    public void OnUpdate()
    {
        Debug.Log("SGoBlin is dead, cannot update state");
        Timer += Time.deltaTime;
        if (Timer >= 0.6f)
        {
            // Ïú»ÙSGoBlin¶ÔÏó
            Object.Destroy(manager.gameObject);
            Debug.Log("SGoBlin has been destroyed after death");
        }

    }
    public void OnExit()
    {
        return;
    }
    
    
}