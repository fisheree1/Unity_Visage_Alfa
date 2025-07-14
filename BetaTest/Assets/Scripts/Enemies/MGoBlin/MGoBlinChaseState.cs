using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class MGoBlinChaseState : IState
{
    private MGoBlinP manager;
    private MGoBlinParameter parameter;
    private Rigidbody2D rb;
    private bool isInAttackRange;
    private bool isInDashAtkRange;
    private float Timer = 0f;
    private float DashTimer = 0f;

    public MGoBlinChaseState(MGoBlinP manager, MGoBlinParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
        rb = manager.GetComponent<Rigidbody2D>();
    }

    public void OnEnter()
    {
        parameter.animator.Play("MGoBlin_chase");
        isInAttackRange = false;
    }

    public void OnUpdate()
    {
        if (parameter.isHit)
        {
            manager.TransitionState(MGoBlinStateType.Hit);
            return; // 如果被击中，立即切换状态
        }
        else
        {
            
            

            // 2. 保持面向目标
            manager.FlipTo(parameter.target);

            // 3. 移动逻辑（在未进入攻击范围时移动）
            if (!isInAttackRange)
            {
                parameter.animator.Play("MGoBlin_chase");
                MoveTowardsTarget();
            }
            else
            {
                StopMovement(); // 在攻击范围内时停止移动
            }

            // 4. 攻击范围检测
            isInAttackRange = CheckAttackRange();
            isInDashAtkRange = CheckDashAtkRange();
            if ((isInDashAtkRange)&&(!isInAttackRange))
            {
                if(DashTimer <= 0f)
                {
                    Debug.Log("冲刺攻击");
                    manager.TransitionState(MGoBlinStateType.HeavyAtk);
                    DashTimer = parameter.dashAtkCooldown;
                    return; // 立即切换状态，避免重复触发
                }
                else
                {
                    DashTimer -= Time.deltaTime;
                    
                }
            }

            // 5. 检测到攻击范围立即切换状态
            else if ((isInDashAtkRange)&&(isInAttackRange))
            {
                if (Timer <= 0f)
                {
                    Debug.Log("攻击");
                    manager.TransitionState(MGoBlinStateType.Attack);
                    Timer = parameter.attackCooldown;
                    return; // 立即切换状态，避免重复触发
                }
                else
                {
                    Timer -= Time.deltaTime;
                    parameter.animator.Play("MGoBlin_idle");
                    StopMovement();
                }
                StopMovement(); // 确保在攻击状态前停止移动

            }
            else
            {
                // 如果不在攻击范围内，继续追击
                if (parameter.target != null)
                {
                    manager.TransitionState(MGoBlinStateType.Chase);
                }
            }
        }
    }

    

    private void MoveTowardsTarget()
    {
        Vector3 targetPosition = new Vector3(
            parameter.target.position.x,
            manager.transform.position.y,
            0
        );

        Vector3 moveDir = (targetPosition - manager.transform.position).normalized;

        // 物理移动
        if (rb != null)
        {
            rb.velocity = new Vector2(
                moveDir.x * parameter.ChaseSpeed,
                rb.velocity.y
            );
        }
        else // 非物理移动
        {
            manager.transform.position = Vector2.MoveTowards(
                manager.transform.position,
                targetPosition,
                parameter.ChaseSpeed * Time.deltaTime
            );
        }
    }

    private bool CheckAttackRange()
    {
        return Physics2D.OverlapCircle(
            parameter.attackPoint.position,
            parameter.attackArea,
            parameter.targetLayer
        );
    }
    private bool CheckDashAtkRange()
    {
        return Physics2D.OverlapCircle(parameter.attackPoint.position,
            parameter.dashAttackArea, parameter.targetLayer);
    }

    private void StopMovement()
    {
        if (rb != null)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    public void OnExit()
    {
        StopMovement();
        isInAttackRange = false;
    }
}
