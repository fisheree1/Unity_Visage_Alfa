using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimePatrolState : IState
{
    private SlimeP manager;
    private SlimeParameter parameter;
    private int patrolPosition;
    private float timer = 0f;
    private Rigidbody2D rb;
    private bool isWaitingAtPoint; // ��Ѳ�ߵ�ȴ�״̬��־

    public SlimePatrolState(SlimeP manager, SlimeParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
        this.patrolPosition = 0;
        rb = manager.GetComponent<Rigidbody2D>();
    }
    
    public void OnEnter()
    {
        // ���Բ������߶���
        PlayWalkAnimation();
        
        isWaitingAtPoint = false;
        timer = 0f;
    }
    
    public void OnUpdate()
    {
        if (parameter.isHit)
        {
            manager.TransitionState(SlimeStateType.Hit);
            return; // ��������У������л�״̬
        }
        
        // ���Ѳ�ߵ�����
        if (parameter.patrolPoints == null || parameter.patrolPoints.Length == 0)
        {
            manager.TransitionState(SlimeStateType.Idle);
            return;
        }

        // ����Ƿ�Ӧ���л���׷��״̬
        if (parameter.target != null && parameter.chasePoints != null && parameter.chasePoints.Length >= 2)
        {
            if (parameter.target.position.x >= parameter.chasePoints[0].position.x &&
                parameter.target.position.x <= parameter.chasePoints[1].position.x)
            {
                manager.TransitionState(SlimeStateType.Chase);
                return;
            }
        }

        // ����ǰѲ�ߵ�
        if (parameter.patrolPoints[patrolPosition] != null)
        {
            manager.FlipTo(parameter.patrolPoints[patrolPosition]);
        }

        // ����Ƿ񵽴�Ѳ�ߵ�
        if (Vector2.Distance(manager.transform.position, parameter.patrolPoints[patrolPosition].position) < 0.1f)
        {
            if (!isWaitingAtPoint)
            {
                // �յ���Ѳ�ߵ㣬��ʼ�ȴ�
                isWaitingAtPoint = true;
                timer = 0f;
                PlayIdleAnimation();
                StopMovement();
            }
            else
            {
                // ��Ѳ�ߵ�ȴ�
                timer += Time.deltaTime;
                if (timer >= parameter.IdleTime)
                {
                    // �ȴ�ʱ��������ƶ�����һ��Ѳ�ߵ�
                    patrolPosition = (patrolPosition + 1) % parameter.patrolPoints.Length;
                    isWaitingAtPoint = false;
                    PlayWalkAnimation();
                }
            }
        }
        else
        {
            // �ƶ���Ѳ�ߵ�
            if (isWaitingAtPoint)
            {
                isWaitingAtPoint = false;
                PlayWalkAnimation();
            }
            
            MoveTowardsPatrolPoint();
        }
    }

    private void PlayWalkAnimation()
    {
        if (parameter?.animator == null) return;

        // ���Բ��Ų�ͬ�����߶����������ȼ�˳��
        string[] walkAnimations = {
            "Slime_walk",
            "slime_walk",
            "Walk",
            "walk",
            "SlimeWalk",
            "Move",
            "move",
            "SlimeMove"
        };

        foreach (string animName in walkAnimations)
        {
            if (HasAnimationState(animName))
            {
                parameter.animator.Play(animName);
                return;
            }
        }

        // ���û���ҵ��κ����߶���������ʹ��Animator����
        if (parameter.animator.parameters.Length > 0)
        {
            try
            {
                parameter.animator.SetBool("isMoving", true);
            }
            catch { }
            
            try
            {
                parameter.animator.SetInteger("State", 1); // 1 ͨ����ʾwalking
            }
            catch { }
            
            try
            {
                parameter.animator.SetTrigger("Walk");
            }
            catch { }
        }

        Debug.LogWarning($"SlimePatrolState: No suitable walk animation found for {manager.gameObject.name}");
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
        }
    }

    private void MoveTowardsPatrolPoint()
    {
        if (parameter.patrolPoints[patrolPosition] == null) return;
        
        Vector3 targetPosition = parameter.patrolPoints[patrolPosition].position;
        Vector3 moveDir = (targetPosition - manager.transform.position).normalized;

        // �����ƶ�
        if (rb != null)
        {
            rb.velocity = new Vector2(
                moveDir.x * parameter.PatrolSpeed,
                rb.velocity.y
            );
        }
        else // �������ƶ�
        {
            manager.transform.position = Vector2.MoveTowards(
                manager.transform.position,
                targetPosition,
                parameter.PatrolSpeed * Time.deltaTime
            );
        }
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
        StopMovement();
        isWaitingAtPoint = false;
    }
}
