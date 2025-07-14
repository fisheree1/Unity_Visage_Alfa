using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MomDeadState : IState
{
    private MomP manager;
    private MomParameter parameter;
    private float destroyTimer = 0f;
    private readonly float destroyDelay = 0.6f;
    private bool isDestroying = false;

    public MomDeadState(MomP manager, MomParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }
    
    public void OnEnter()
    {
        Debug.Log("Mom entering Dead State");
        
        // ��֤������
        if (parameter?.animator == null)
        {
            Debug.LogError("MomDeadState: Animator is null!");
            DestroyMom();
            return;
        }
        
        // ��ȫ������������
        if (HasAnimationState("Mom_death"))
        {
            parameter.animator.Play("Mom_death");
        }
        else
        {
            Debug.LogWarning("MomDeadState: Animation state 'Mom_death' not found!");
        }
        
        // ������ײ������ֹ�������ܱ�����
        
        
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
            DestroyMom();
        }
    }
    
    private void DestroyMom()
    {
        if (isDestroying) return;
        
        isDestroying = true;
        
        // �����������������Ч�������������Ʒ��������Ч��
        // DropLoot();
        // PlayDeathSound();
        
        Debug.Log("Mom destroyed after death animation");
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
        Debug.Log("MomDeadState: OnExit called (this should not happen)");
    }
}