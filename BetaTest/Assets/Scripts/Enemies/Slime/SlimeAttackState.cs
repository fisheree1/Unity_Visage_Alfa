using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeAttackState : IState
{
    private SlimeP manager;
    private SlimeParameter parameter;
    private AnimatorStateInfo info;
    private int AttackCount = 0;
    private bool isInAttackRange = false;

    public SlimeAttackState(SlimeP manager, SlimeParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }
    
    public void OnEnter()
    {
        PlayAttackAnimation();
    }

    public void OnUpdate()
    {
        if (parameter.isHit)
        {
            manager.TransitionState(SlimeStateType.Hit);
            return; // ��������У������л�״̬
        }
        
        isInAttackRange = CheckAttackRange();
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        
        if (info.normalizedTime >= 0.95f)
        {
            if (isInAttackRange)
            {
                manager.TransitionState(SlimeStateType.Attack);
            }
            else
            {
                manager.TransitionState(SlimeStateType.Idle);
            }
        }
    }

    private void PlayAttackAnimation()
    {
        if (parameter?.animator == null) return;

        // ���Բ��Ų�ͬ�Ĺ��������������ȼ�˳��
        string[] attackAnimations = {
            "Slime_attack",
            "slime_attack",
            "Attack",
            "attack",
            "SlimeAttack",
            "Hit",
            "hit",
            "SlimeHit",
            "Strike",
            "strike",
            "SlimeStrike"
        };

        foreach (string animName in attackAnimations)
        {
            if (HasAnimationState(animName))
            {
                parameter.animator.Play(animName);
                return;
            }
        }

        // ���û���ҵ��κι�������������ʹ��Animator����
        if (parameter.animator.parameters.Length > 0)
        {
            try
            {
                parameter.animator.SetTrigger("Attack");
            }
            catch { }
            
            try
            {
                parameter.animator.SetBool("isAttacking", true);
            }
            catch { }
            
            try
            {
                parameter.animator.SetInteger("State", 4); // 4 ͨ����ʾattack
            }
            catch { }
        }

        Debug.LogWarning($"SlimeAttackState: No suitable attack animation found for {manager.gameObject.name}");
    }

    private bool CheckAttackRange()
    {
        if (parameter.attackPoint == null) return false;
        
        return Physics2D.OverlapCircle(
            parameter.attackPoint.position,
            parameter.attackArea,
            parameter.targetLayer
        );
    }

    private bool HasAnimationState(string stateName)
    {
        if (parameter?.animator == null) return false;

        // ��鶯���������Ƿ���ָ��״̬
        var controller = parameter.animator.runtimeAnimatorController;
        if (controller == null) return false;

        foreach (var clip in controller.animationClips)
        {
            if (clip.name == stateName)
                return true;
        }

        return false;
    }

    public void OnExit()
    {
        Debug.Log("Attack Count: " + AttackCount);
        
        // ���ù�����ص�animator����
        if (parameter?.animator != null && parameter.animator.parameters.Length > 0)
        {
            try
            {
                parameter.animator.SetBool("isAttacking", false);
            }
            catch { }
        }
    }
}


