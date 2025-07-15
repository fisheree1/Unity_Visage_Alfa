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
        
        // 验证动画器
        if (parameter?.animator == null)
        {
            Debug.LogError("MGoBlinDeadState: Animator is null!");
            DestroyMGoBlin();
            return;
        }
        
        // 安全播放死亡动画
        if (HasAnimationState("MGoBlin_death"))
        {
            parameter.animator.Play("MGoBlin_death");
        }
        else
        {
            Debug.LogWarning("MGoBlinDeadState: Animation state 'MGoBlin_death' not found!");
        }
        
        // 禁用碰撞器，防止死亡后还能被攻击
        var colliders = manager.GetComponents<Collider2D>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }
        
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
            DestroyMGoBlin();
        }
    }
    
    private void DestroyMGoBlin()
    {
        if (isDestroying) return;
        
        isDestroying = true;
        
        // 可以在这里添加死亡效果，比如掉落物品、播放音效等
        // DropLoot();
        // PlayDeathSound();
        
        Debug.Log("MGoBlin destroyed after death animation");
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
        Debug.Log("MGoBlinDeadState: OnExit called (this should not happen)");
    }
}