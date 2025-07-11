using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MGoBlinPatrolState : IState
{
    private MGoBlinP manager;
    private MGoBlinParameter parameter;
    private int patrolPosition;
    private float timer=0f;
    private Rigidbody2D rb;
    private bool isWaitingAtPoint; // �������ȴ�״̬��־

    public MGoBlinPatrolState(MGoBlinP manager, MGoBlinParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
        this.patrolPosition = 0;
        rb = manager.GetComponent<Rigidbody2D>();
    }
    public void OnEnter()
    {
        parameter.animator.Play("MGoBlin_walk");
        parameter.patrolPoints = parameter.patrolPoints ?? new Transform[0]; // ȷ�� patrolPoints ��Ϊ null
    }
    public void OnUpdate()
    {
        if (parameter.patrolPoints == null || parameter.patrolPoints.Length == 0)
            return;

        if (parameter.target != null &&
            parameter.target.position.x >= parameter.chasePoints[0].position.x &&
            parameter.target.position.x <= parameter.chasePoints[1].position.x)
        {
            manager.TransitionState(MGoBlinStateType.Chase);
            return;
        }

        manager.FlipTo(parameter.patrolPoints[patrolPosition]);

        if (Vector2.Distance(manager.transform.position, parameter.patrolPoints[patrolPosition].position) < 0.1f)
        {
            if (timer <= parameter.IdleTime)
            {
                patrolPosition = (patrolPosition + 1) % parameter.patrolPoints.Length;
                manager.TransitionState(MGoBlinStateType.Idle);
            }
            else
            {
                
                timer += Time.deltaTime;
            }
        }
        else
        {
            manager.transform.position = Vector2.MoveTowards(
            manager.transform.position,
            parameter.patrolPoints[patrolPosition].position,
            parameter.PatrolSpeed * Time.deltaTime);
        }
    }


    public void OnExit()
    {

    }
}
