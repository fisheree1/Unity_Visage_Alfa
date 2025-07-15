using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MGoBlinAttack2State : IState
{
    private MGoBlinP manager;
    private MGoBlinParameter parameter;
    private AnimatorStateInfo info;

    private bool isInAttackRange = false;
    private bool hasDealtDamage = false;
    private float damageWindowStart = 0.3f; // 伤害检测窗口开始时间
    private float damageWindowEnd = 0.7f;   // 伤害检测窗口结束时间
    private float animStartTime;

    public MGoBlinAttack2State(MGoBlinP manager, MGoBlinParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }
    
    public void OnEnter()
    {
        parameter.animator.Play("MGoBlin_attack2");
        hasDealtDamage = false;
        animStartTime = Time.time;
        Debug.Log("MGoBlinAttack2State: 进入攻击2状态");
    }

    public void OnUpdate()
    {
        if (parameter.isHit)
        {
            manager.TransitionState(MGoBlinStateType.Hit);
            return; // 如果被击中，立即切换状态
        }
        
        float timeSinceStart = Time.time - animStartTime;
        
        // 处理伤害检测
        HandleDamageDetection(timeSinceStart);
        
        isInAttackRange = CheckAttackRange();
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        
        // 确保动画真正完成
        if (info.normalizedTime >= 0.95f && info.IsName("MGoBlin_attack2"))
        {
            if (isInAttackRange)
            {
                Debug.Log("MGoBlinAttack2State: 在攻击范围内，循环回攻击1状态");
                manager.TransitionState(MGoBlinStateType.Attack);
            }
            else
            {
                Debug.Log("MGoBlinAttack2State: 不在攻击范围内，切换到追击状态");
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
            // Attack2可能造成稍高的伤害
            int attack2Damage = Mathf.Max(1, parameter.damage + 1);
            heroLife.TakeDamage(attack2Damage);
            hasDealtDamage = true;
            
            Debug.Log($"MGoBlin 攻击2命中! 造成 {attack2Damage} 伤害");
            
            // 给目标施加击退效果
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
            float knockbackForce = 4f; // Attack2的击退力稍大
            targetRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }
    }

    public void OnExit()
    {
        hasDealtDamage = false;
        Debug.Log("MGoBlinAttack2State: 退出攻击2状态");
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


