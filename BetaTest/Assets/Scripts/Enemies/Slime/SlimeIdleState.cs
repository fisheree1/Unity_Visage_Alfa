using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeIdleState : IState
{
    private SlimeP manager;
    private SlimeParameter parameter;
    private float idleTimer = 0f;
    private Rigidbody2D rb;

    public SlimeIdleState(SlimeP manager, SlimeParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
        rb = manager.GetComponent<Rigidbody2D>();
    }

    public void OnEnter()
    {
        // ���Բ���idle������������������Ա��ö���
        PlayIdleAnimation();
        
        idleTimer = 0f;
        StopMovement();
    }

    public void OnUpdate()
    {
        if (parameter.isHit)
        {
            manager.TransitionState(SlimeStateType.Hit);
            return; // ��������У������л�״̬
        }
        
        // 1. ���ȼ�⣺ȷ��target����
        if (parameter.target != null)
        {
            // 2. ���Ŀ���Ƿ���׷����Χ
            if (parameter.chasePoints != null && parameter.chasePoints.Length >= 2)
            {
                bool inChaseZone =
                    parameter.target.position.x >= parameter.chasePoints[0].position.x &&
                    parameter.target.position.x <= parameter.chasePoints[1].position.x;

                if (inChaseZone && parameter.target.CompareTag("Player"))
                {
                    manager.TransitionState(SlimeStateType.Chase);
                    return; // �����˳�״̬����
                }
            }
        }

        // 3. ��ʱ�ȴ�
        idleTimer += Time.deltaTime;

        // 4. ��ʱ�������л���Ѳ��״̬���������Ѳ�ߵ㣩
        if (idleTimer >= parameter.IdleTime)
        {
            // ����Ƿ�����Ч��Ѳ�ߵ�
            if (parameter.patrolPoints != null && parameter.patrolPoints.Length > 0)
            {
                manager.TransitionState(SlimeStateType.Patrol);
            }
            else
            {
                // û��Ѳ�ߵ�ͱ��ִ��������ü�ʱ��
                idleTimer = 0;
            }
        }
    }

    private void PlayIdleAnimation()
    {
        if (parameter?.animator == null) return;

        // ���Բ��Ų�ͬ��idle�����������ȼ�˳��
        string[] idleAnimations = {
            "Slime_idle",
            "slime_idle", 
            "Idle",
            "idle",
            "SlimeIdle",
            "Default"
        };

        foreach (string animName in idleAnimations)
        {
            if (HasAnimationState(animName))
            {
                parameter.animator.Play(animName);
                return;
            }
        }

        // ���û���ҵ��κ�idle����������ʹ��Animator����
        if (parameter.animator.parameters.Length > 0)
        {
            // �������ó�����animator����
            try
            {
                parameter.animator.SetBool("isMoving", false);
            }
            catch { }
            
            try
            {
                parameter.animator.SetInteger("State", 0); // 0 ͨ����ʾidle
            }
            catch { }
            
            try
            {
                parameter.animator.SetTrigger("Idle");
            }
            catch { }
        }

        Debug.LogWarning($"SlimeIdleState: No suitable idle animation found for {manager.gameObject.name}");
    }

    private void StopMovement()
    {
        if (rb != null)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
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
        // �˳�ʱ����״̬����
        idleTimer = 0;
    }
}
