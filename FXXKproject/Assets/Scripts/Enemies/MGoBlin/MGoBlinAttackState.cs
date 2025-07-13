using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MGoBlinAttackState : IState
{
    private MGoBlinP manager;
    private MGoBlinParameter parameter;
    private AnimatorStateInfo info;

    private bool isInAttackRange = false;
    private bool hasDealtDamage = false;
    private float damageWindowStart = 0.3f; // 伤害检测窗口开始时间
    private float damageWindowEnd = 0.7f;   // 伤害检测窗口结束时间
    private float animStartTime;

    public MGoBlinAttackState(MGoBlinP manager, MGoBlinParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }
    
    public void OnEnter()
    {
        parameter.animator.Play("MGoBlin_attack");
        hasDealtDamage = false;
        animStartTime = Time.time;
        Debug.Log("MGoBlinAttackState: 进入攻击状态");
    }

    public void OnUpdate()
    {
        if (parameter.isHit)
        {
            manager.TransitionState(MGoBlinStateType.Hit);
            return;
        }
        
        float timeSinceStart = Time.time - animStartTime;
        
        // 处理伤害检测
        HandleDamageDetection(timeSinceStart);
        
        isInAttackRange = CheckAttackRange();
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        
        // 确保动画真正完成且在攻击范围内才切换到Attack2
        if (info.normalizedTime >= 0.95f && info.IsName("MGoBlin_attack"))
        {
            if (isInAttackRange)
            {
                Debug.Log("MGoBlinAttackState: 在攻击范围内，切换到攻击2状态");
                manager.TransitionState(MGoBlinStateType.Attack2);
                return;
            }
            else
            {
                Debug.Log("MGoBlinAttackState: 不在攻击范围内，切换到追击状态");
                manager.TransitionState(MGoBlinStateType.Chase);
            }
        }
    }
    
    private void HandleDamageDetection(float timeSinceStart)
    {
        // 在伤害检测窗口内检测碰撞
        if (!hasDealtDamage && timeSinceStart >= damageWindowStart && timeSinceStart <= damageWindowEnd)
        {
            Collider2D hitTarget = Physics2D.OverlapCircle(
                parameter.attackPoint.position,
                parameter.attackArea,
                parameter.targetLayer
            );
            
            if (hitTarget != null)
            {
                DealDamageToTarget(hitTarget);
            }
        }
    }
    
    private void DealDamageToTarget(Collider2D target)
    {
        // 查找目标的生命值组件
        HeroLife heroLife = target.GetComponent<HeroLife>();
        if (heroLife == null)
        {
            heroLife = target.GetComponentInParent<HeroLife>();
        }
        
        if (heroLife != null)
        {
            heroLife.TakeDamage(parameter.damage);
            hasDealtDamage = true;
            
            Debug.Log($"MGoBlin 攻击命中! 造成 {parameter.damage} 伤害");
            
            // 给目标施加轻微击退效果
            ApplyKnockback(target);
        }
    }
    
    private void ApplyKnockback(Collider2D target)
    {
        Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
        if (targetRb != null)
        {
            // 计算击退方向
            Vector2 knockbackDirection = (target.transform.position - manager.transform.position).normalized;
            float knockbackForce = 3f; // 普通攻击的击退力较小
            targetRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }
    }

    public void OnExit()
    {
        hasDealtDamage = false;
        Debug.Log("MGoBlinAttackState: 退出攻击状态");
    }
    
    private bool CheckAttackRange()
    {
        return Physics2D.OverlapCircle(
            parameter.attackPoint.position,
            parameter.attackArea,
            parameter.targetLayer
        );
    }
}


