using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SlimeDeadState : IState
{
    private SlimeP manager;
    private SlimeParameter parameter;
    private float destroyTimer = 0f;
    private readonly float destroyDelay = 0.6f;
    private bool isDestroying = false;

    public SlimeDeadState(SlimeP manager, SlimeParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }

    public void OnEnter()
    {
        Debug.Log("Slime entering Dead State");

        // ��֤������
        if (parameter?.animator == null)
        {
            Debug.LogError("SlimeDeadState: Animator is null!");
            DestroySlime();
            return;
        }

        // ���Բ�����������
        PlayDeathAnimation();

        // ������ײ������ֹ�������ܱ�����
        var colliders = manager.GetComponents<Collider2D>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }

        // ֹͣ�ƶ�
        var rb = manager.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        destroyTimer = 0f;
        isDestroying = false;
    }

    public void OnUpdate()
    {
        if (isDestroying) return;

        destroyTimer += Time.deltaTime;

        if (destroyTimer >= destroyDelay)
        {
            DestroySlime();
        }
    }

    private void PlayDeathAnimation()
    {
        if (parameter?.animator == null) return;

        // ���Բ��Ų�ͬ�����������������ȼ�˳��
        string[] deathAnimations = {
            "Slime_death",
            "slime_death",
            "Death",
            "death",
            "SlimeDeath",
            "Die",
            "die",
            "SlimeDie",
            "Dead",
            "dead",
            "SlimeDead"
        };

        foreach (string animName in deathAnimations)
        {
            if (HasAnimationState(animName))
            {
                parameter.animator.Play(animName);
                return;
            }
        }

        // ���û���ҵ��κ���������������ʹ��Animator����
        if (parameter.animator.parameters.Length > 0)
        {
            try
            {
                parameter.animator.SetTrigger("Death");
            }
            catch { }
            
            try
            {
                parameter.animator.SetTrigger("Die");
            }
            catch { }
            
            try
            {
                parameter.animator.SetBool("isDead", true);
            }
            catch { }
            
            try
            {
                parameter.animator.SetInteger("State", 5); // 5 ͨ����ʾdeath
            }
            catch { }
        }

        Debug.LogWarning($"SlimeDeadState: No suitable death animation found for {manager.gameObject.name}");
    }

    private void DestroySlime()
    {
        if (isDestroying) return;

        isDestroying = true;

        // �����������������Ч�������������Ʒ��������Ч��
        // DropLoot();
        // PlayDeathSound();

        Debug.Log("Slime destroyed after death animation");
        Object.Destroy(manager.gameObject);
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
        // ����״̬ͨ�������˳�����Ϊ�������Ա���
        Debug.Log("SlimeDeadState: OnExit called (this should not happen)");
    }
}