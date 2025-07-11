using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderAttackState : IState
{
    private SpiderP manager;
    private SpiderParameter parameter;
    private AnimatorStateInfo info;
     

    public SpiderAttackState(SpiderP manager, SpiderParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
        
    }
    public void OnEnter()
    {
        parameter.animator.Play("Spider_attack");
    }

    public void OnUpdate()
    {
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        if (info.normalizedTime >= 0.95f)
        {
            
            
            
            
                manager.TransitionState(SpiderStateType.Chase);
                
                return; // 确保只在攻击动画结束后切换状态
			
            
            
        }
    }


    public void OnExit()
    {
        Debug.Log("Exiting Attack State");
    }
}


