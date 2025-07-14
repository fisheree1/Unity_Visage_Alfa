using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DadDeadState : IState
{
    private DadP manager;
    private DadParameter parameter;
    private float destroyTimer = 0f;
    private readonly float destroyDelay = 0.6f;
    private bool isDestroying = false;

    public DadDeadState(DadP manager, DadParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }
    
    public void OnEnter()
    {
        Debug.Log("Dad entering Dead State");
        
        // ��֤������
        if (parameter?.animator == null)
        {
            Debug.LogError("DadDeadState: Animator is null!");
            DestroyDad();
            return;
        }
        
        // ��ȫ������������
        if (HasAnimationState("Dad_death"))
        {
            parameter.animator.Play("Dad_death");
        }
        else
        {
            Debug.LogWarning("DadDeadState: Animation state 'Dad_death' not found!");
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
            DestroyDad();
        }
    }
    
    private void DestroyDad()
    {
        if (isDestroying) return;
        
        isDestroying = true;
        
        // �����������������Ч�������������Ʒ��������Ч��
        // DropLoot();
        // PlayDeathSound();
        
        Debug.Log("Dad destroyed after death animation");
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
        Debug.Log("DadDeadState: OnExit called (this should not happen)");
    }
}