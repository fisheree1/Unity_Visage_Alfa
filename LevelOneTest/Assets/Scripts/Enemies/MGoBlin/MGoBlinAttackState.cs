
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MGoBlinAttackState : IState
{
    private MGoBlinP manager;
    private MGoBlinParameter parameter;
    private AnimatorStateInfo info;
    private float AttackTimer = 0f;
    private float AttackCount = 0f;

    public MGoBlinAttackState(MGoBlinP manager, MGoBlinParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;

    }
    public void OnEnter()
    {
        parameter.animator.Play("MGoBlin_lightatk");
    }

    public void OnUpdate()
    {
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        if (info.normalizedTime >= 0.95f)
        {
            AttackCount += 1f; // 增加攻击计数
            if (AttackCount >= 3f)
            {
                manager.TransitionState(MGoBlinStateType.DashAtk);
                AttackCount = 0f; // 重置攻击计数
                return; // 切换状态后不再执行后续逻辑
            }
            if (AttackTimer >= 1f)
            {
                manager.TransitionState(MGoBlinStateType.Chase);
                AttackTimer = 0f; // 重置计时器
            }
            else
            {
                AttackTimer += Time.deltaTime;
            }

        }
    }


    public void OnExit()
    {
        Debug.Log("Exiting Attack State");
    }
}


