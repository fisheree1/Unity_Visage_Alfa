using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MGoBlinDeadState : IState
{
    private MGoBlinP manager;
    private MGoBlinParameter parameter;
    private float destroyTimer = 0f;
    private readonly float destroyDelay = 0.6f;
    private bool isDestroying = false;

    public MGoBlinDeadState(MGoBlinP manager, MGoBlinParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }
    
    public void OnEnter()
    {
        Debug.Log("MGoBlin entering Dead State");
        
        // ��֤������
        if (parameter?.animator == null)
        {
            Debug.LogError("MGoBlinDeadState: Animator is null!");
            DestroyMGoBlin();
            return;
        }
        
        // ��ȫ������������
        if (HasAnimationState("MGoBlin_death"))
        {
            parameter.animator.Play("MGoBlin_death");
        }
        else
        {
            Debug.LogWarning("MGoBlinDeadState: Animation state 'MGoBlin_death' not found!");
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
            DestroyMGoBlin();
        }
    }
    
    private void DestroyMGoBlin()
    {
        if (isDestroying) return;
        
        isDestroying = true;
        
        // �����������������Ч�������������Ʒ��������Ч��
        // DropLoot();
        // PlayDeathSound();
        
        Debug.Log("MGoBlin destroyed after death animation");
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
        Debug.Log("MGoBlinDeadState: OnExit called (this should not happen)");
    }
}