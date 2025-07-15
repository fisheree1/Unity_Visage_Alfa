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

        // ��ȫ������������
        if (HasAnimationState("Slime_death"))
        {
            parameter.animator.Play("Slime_death");
        }
        else
        {
            Debug.LogWarning("SlimeDeadState: Animation state 'Slime_death' not found!");
        }

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