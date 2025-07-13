
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MGoBlinDashAtkState : IState
{
    private MGoBlinP manager;
    private MGoBlinParameter parameter;
    private AnimatorStateInfo info;
    private Rigidbody2D rb;
    private bool isInAttackRange;
    private Vector3 origin_targetPosition;
    private Vector3 origin_transformPosition;

    public MGoBlinDashAtkState(MGoBlinP manager, MGoBlinParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
        rb = manager.GetComponent<Rigidbody2D>();
        
}
    public void OnEnter()
    {
        manager.FlipTo(parameter.target);
        origin_targetPosition = new Vector3(
            parameter.target.position.x,
            manager.transform.position.y,
            0
        );
        origin_transformPosition =
            new Vector3 ( manager.transform.position.x,
            manager.transform.position.y,0);
        parameter.animator.Play("MGoBlin_dashatk");
    }

    public void OnUpdate()
    {
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        if( info.normalizedTime < 0.95f)
        {

            MoveTowardsTarget();
            

            
        }
        else 
        {
            // 动画播放完毕后，切换回追击状态
            manager.TransitionState(MGoBlinStateType.Chase);
        }
        
    }
    private void MoveTowardsTarget()
    {
        

        Vector3 moveDir = (origin_targetPosition - origin_transformPosition).normalized;

        // 物理移动
        if (rb != null)
        {
            rb.velocity = new Vector2(
                moveDir.x * parameter.DashSpeed,
                rb.velocity.y
            );
        }
        else // 非物理移动
        {
            manager.transform.position = Vector2.MoveTowards(
                manager.transform.position,
                origin_targetPosition,
                parameter.DashSpeed * Time.deltaTime
            );
        }
    }

    public void OnExit()
    {
        Debug.Log("Exiting Attack State");
    }
}



