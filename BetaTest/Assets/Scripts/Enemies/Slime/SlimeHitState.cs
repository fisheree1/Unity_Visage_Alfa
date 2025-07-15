using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeHitState : IState
{
    private SlimeP manager;
    private SlimeParameter parameter;
    private AnimatorStateInfo info;
    private Rigidbody2D rb;
    private float knockbackTimer = 0f;
    private float knockbackDuration = 0.3f; // ���˳���ʱ��
    private Vector2 knockbackDirection;
    private float knockbackForce = 8f; // ��������

    public SlimeHitState(SlimeP manager, SlimeParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
        rb = manager.GetComponent<Rigidbody2D>();
    }
    
    public void OnEnter()
    {
        // ���Բ����ܻ�����
        PlayHurtAnimation();
        
        // ��ȡ���˷��� - �����λ�õ�����λ��
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector2 direction = (manager.transform.position - player.transform.position).normalized;
            knockbackDirection = new Vector2(direction.x, 0); // ֻ��ˮƽ�������
        }
        else
        {
            // ����Ҳ�����ң�ʹ�õ�ǰ����ķ�����
            knockbackDirection = new Vector2(manager.transform.localScale.x > 0 ? -1 : 1, 0);
        }
        
        knockbackTimer = 0f;
        
        // Ӧ�ó�ʼ������
        if (rb != null)
        {
            rb.velocity = new Vector2(knockbackDirection.x * knockbackForce, rb.velocity.y);
        }
    }

    public void OnUpdate()
    {
        knockbackTimer += Time.deltaTime;
        
        // �ڻ����ڼ��𽥼��ٻ����ٶ�
        if (knockbackTimer < knockbackDuration && rb != null)
        {
            float knockbackLerp = 1f - (knockbackTimer / knockbackDuration);
            float currentKnockbackSpeed = knockbackForce * knockbackLerp;
            rb.velocity = new Vector2(knockbackDirection.x * currentKnockbackSpeed, rb.velocity.y);
        }
        else if (rb != null)
        {
            // ���˽�����ֹͣˮƽ�ƶ�
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        if (info.normalizedTime >= 0.95f)
        {
            if (manager.IsDead)
            {
                manager.TransitionState(SlimeStateType.Dead);
            }
            else
            {
                parameter.isHit = false; // ���ñ�����״̬
                parameter.target = GameObject.FindGameObjectWithTag("Player").transform; // ���»�ȡĿ��
                manager.TransitionState(SlimeStateType.Chase); // �л���׷��״̬
            }
        }
    }

    private void PlayHurtAnimation()
    {
        if (parameter?.animator == null) return;

        // ���Բ��Ų�ͬ���ܻ������������ȼ�˳��
        string[] hurtAnimations = {
            "Slime_hurt",
            "slime_hurt",
            "Hurt",
            "hurt",
            "SlimeHurt",
            "Hit",
            "hit",
            "SlimeHit",
            "Damaged",
            "damaged",
            "SlimeDamaged"
        };

        foreach (string animName in hurtAnimations)
        {
            if (HasAnimationState(animName))
            {
                parameter.animator.Play(animName);
                return;
            }
        }

        // ���û���ҵ��κ��ܻ�����������ʹ��Animator����
        if (parameter.animator.parameters.Length > 0)
        {
            try
            {
                parameter.animator.SetTrigger("Hit");
            }
            catch { }
            
            try
            {
                parameter.animator.SetTrigger("Hurt");
            }
            catch { }
            
            try
            {
                parameter.animator.SetBool("isHit", true);
            }
            catch { }
            
            try
            {
                parameter.animator.SetInteger("State", 3); // 3 ͨ����ʾhurt
            }
            catch { }
        }

        Debug.LogWarning($"SlimeHitState: No suitable hurt animation found for {manager.gameObject.name}");
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
        parameter.isHit = false; // ȷ���˳�ʱ���ñ�����״̬
        
        // ֹͣ�����ƶ�
        if (rb != null)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        
        // ���û�����ص�animator����
        if (parameter?.animator != null && parameter.animator.parameters.Length > 0)
        {
            try
            {
                parameter.animator.SetBool("isHit", false);
            }
            catch { }
        }
    }
}


