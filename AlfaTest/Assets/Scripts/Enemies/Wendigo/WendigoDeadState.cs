using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WendigoDeadState : IState
{
    private WendigoP manager;
    private WendigoParameter parameter;
    private float Timer = 0f;

    public WendigoDeadState(WendigoP manager, WendigoParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }
    public void OnEnter()
    {
        Debug.Log("Entering Dead State");
        parameter.animator.Play("Wendigo_death");
    }


    public void OnUpdate()
    {
        Debug.Log("Wendigo is dead, cannot update state");
        Timer += Time.deltaTime;
        if (Timer >= 0.6f)
        {
            // Ïú»ÙWendigo¶ÔÏó
            Object.Destroy(manager.gameObject);
            Debug.Log("Wendigo has been destroyed after death");
        }

    }
    public void OnExit()
    {
        return;
    }
    
    
}