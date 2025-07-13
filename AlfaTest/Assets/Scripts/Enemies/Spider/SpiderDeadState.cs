using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpiderDeadState : IState
{
    private SpiderP manager;
    private SpiderParameter parameter;
    private float Timer = 0f;

    public SpiderDeadState(SpiderP manager, SpiderParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }
    public void OnEnter()
    {
        Debug.Log("Entering Dead State");
        parameter.animator.Play("Spider_death");
    }


    public void OnUpdate()
    {
        Debug.Log("Spider is dead, cannot update state");
        Timer += Time.deltaTime;
        if (Timer >= 0.6f)
        {
            // Ïú»ÙSpider¶ÔÏó
            Object.Destroy(manager.gameObject);
            Debug.Log("Spider has been destroyed after death");
        }

    }
    public void OnExit()
    {
        return;
    }
    
    
}