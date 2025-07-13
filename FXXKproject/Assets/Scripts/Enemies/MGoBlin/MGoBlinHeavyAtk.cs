using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MGoBlinHeavyAtkState : IState
{
    private MGoBlinP manager;
    private MGoBlinParameter parameter;
    private AnimatorStateInfo info;
    private bool isInAttackRange;
    private Rigidbody2D rb;
    
    // 冲刺攻击相关变量
    private bool hasDashed = false;
    private bool hasDealtDamage = false;
    private Vector2 dashDirection;
    private float dashSpeed = 30f;
    private float dashStartTime = 0.2f;  // 动画开始后0.2秒开始冲刺
    private float dashEndTime = 0.8f;    // 动画开始后0.8秒结束冲刺
    private float damageWindowStart = 0.3f; // 伤害检测窗口开始时间
    private float damageWindowEnd = 0.7f;   // 伤害检测窗口结束时间
    private float animStartTime;
    private bool isDashing = false;

    public MGoBlinHeavyAtkState(MGoBlinP manager, MGoBlinParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
        rb = manager.GetComponent<Rigidbody2D>();
    }
    
    public void OnEnter()
    {
        // 重置状态
        hasDashed = false;
        hasDealtDamage = false;
        isDashing = false;
        animStartTime = Time.time;
        
        // 确定冲刺方向
        if (parameter.target != null)
        {
            Vector3 targetDirection = (parameter.target.position - manager.transform.position).normalized;
            dashDirection = new Vector2(targetDirection.x, 0); // 只在水平方向冲刺
            
            // 面向目标
            manager.FlipTo(parameter.target);
        }
        else
        {
            // 如果没有目标，根据当前朝向决定冲刺方向
            float facing = manager.transform.localScale.x > 0 ? -1f : 1f;
            dashDirection = new Vector2(facing, 0);
        }
        
        // 播放冲刺攻击动画
        parameter.animator.Play("MGoBlin_dashattack");
        
        Debug.Log("MGoBlin 开始冲刺攻击!");
    }

    public void OnUpdate()
    {
       
        
        float timeSinceStart = Time.time - animStartTime;
        
        // 处理冲刺移动
        HandleDashMovement(timeSinceStart);
        
        // 处理伤害检测
        HandleDamageDetection(timeSinceStart);
        
        // 检查攻击范围（用于后续状态决策）
        isInAttackRange = CheckAttackRange();
        
        // 检查动画是否完成
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        if (info.normalizedTime >= 0.95f)
        {
            // 停止冲刺
            StopDash();
            
            // 根据是否在攻击范围内决定下一个状态
            if (isInAttackRange)
            {
                manager.TransitionState(MGoBlinStateType.Attack);
            }
            else
            {
                manager.TransitionState(MGoBlinStateType.Chase);
            }
        }
    }
    
    private void HandleDashMovement(float timeSinceStart)
    {
        // 在指定时间窗口内执行冲刺
        if (timeSinceStart >= dashStartTime && timeSinceStart <= dashEndTime)
        {
            if (!isDashing)
            {
                isDashing = true;
                Debug.Log("开始冲刺移动");
            }
            
            if (rb != null)
            {
                // 持续设置冲刺速度
                rb.velocity = new Vector2(dashDirection.x * dashSpeed, rb.velocity.y);
            }
            else
            {
                // 备用移动方式（如果没有Rigidbody2D）
                manager.transform.Translate(dashDirection * dashSpeed * Time.deltaTime);
            }
            
            hasDashed = true;
        }
        else if (isDashing && timeSinceStart > dashEndTime)
        {
            // 冲刺结束，减速
            StopDash();
            isDashing = false;
        }
    }
    
    private void HandleDamageDetection(float timeSinceStart)
    {
        // 在伤害检测窗口内检测碰撞
        if (!hasDealtDamage && timeSinceStart >= damageWindowStart && timeSinceStart <= damageWindowEnd)
        {
            // 使用更大的攻击范围检测冲刺攻击
            Collider2D hitTarget = Physics2D.OverlapCircle(
                parameter.attackPoint.position,
                parameter.dashAttackArea,
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
            // 冲刺攻击造成更高伤害
            int dashDamage = Mathf.Max(1, parameter.damage * 2);
            heroLife.TakeDamage(dashDamage);
            hasDealtDamage = true;
            
            Debug.Log($"MGoBlin 冲刺攻击命中! 造成 {dashDamage} 伤害");
            
            // 给目标施加击退效果
            ApplyKnockback(target);
        }
    }
    
    private void ApplyKnockback(Collider2D target)
    {
        Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
        if (targetRb != null)
        {
            Vector2 knockbackDirection = dashDirection.normalized;
            float knockbackForce = 8f;
            targetRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
            
            Debug.Log("应用击退效果");
        }
    }
    
    private void StopDash()
    {
        if (rb != null)
        {
            // 逐渐减速，而不是立即停止（保持更自然的感觉）
            rb.velocity = new Vector2(rb.velocity.x * 0.3f, rb.velocity.y);
        }
    }

    public void OnExit()
    {
        // 确保完全停止移动
        StopDash();
        
        // 重置状态变量
        hasDashed = false;
        hasDealtDamage = false;
        isDashing = false;
        
        Debug.Log("MGoBlin 冲刺攻击结束");
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


