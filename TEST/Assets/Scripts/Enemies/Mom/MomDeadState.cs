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
        
        // 验证动画器
        if (parameter?.animator == null)
        {
            Debug.LogError("MomDeadState: Animator is null!");
            DestroyMom();
            return;
        }
        
        // 安全播放死亡动画
        if (HasAnimationState("Mom_death"))
        {
            parameter.animator.Play("Mom_death");
        }
        else
        {
            Debug.LogWarning("MomDeadState: Animation state 'Mom_death' not found!");
        }
        
        // 禁用碰撞器，防止死亡后还能被攻击
        
        
        // 停止移动
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
        
        // 可以在这里添加死亡效果，比如掉落物品、播放音效等
        // DropLoot();
        // PlayDeathSound();
        
        Debug.Log("Mom destroyed after death animation");
        Object.Destroy(manager.gameObject);
    }
    
    private bool HasAnimationState(string stateName)
    {
        if (parameter?.animator == null) return false;
        
        // 检查动画控制器是否有指定状态
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
        // 死亡状态通常不会退出，但为了完整性保留
        Debug.Log("MomDeadState: OnExit called (this should not happen)");
    }
}