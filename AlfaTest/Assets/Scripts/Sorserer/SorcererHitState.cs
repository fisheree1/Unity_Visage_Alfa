using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SorcererHitState : IState
{
    private SorcererP manager;
    private SorcererParameter parameter;
    private AnimatorStateInfo info;
    private float hitDuration = 0.5f;
    private float hitTimer;
    
    public SorcererHitState(SorcererP manager, SorcererParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }

    public void OnEnter()
    {
        parameter.isHit = true;
        parameter.isAttacking = false;
        hitTimer = hitDuration;
        
        // Play hit animation
        if (parameter.animator != null)
        {
            parameter.animator.Play("Sorcerer_hurt");
        }
        
        Debug.Log("Sorcerer entered hit state");
    }

    public void OnUpdate()
    {
        hitTimer -= Time.deltaTime;
        
        // Check if animation is finished or timer expired
        if (parameter.animator != null)
        {
            info = parameter.animator.GetCurrentAnimatorStateInfo(0);
            if (info.normalizedTime >= 0.95f || hitTimer <= 0f)
            {
                // Check if sorcerer should die
                if (manager.currentHealth <= 0)
                {
                    manager.TransitionState(SorcererStateType.Dead);
                }
                else
                {
                    // Check if player is still in range
                    if (manager.IsPlayerInRange(parameter.detectionRange))
                    {
                        manager.TransitionState(SorcererStateType.Attack);
                    }
                    else
                    {
                        manager.TransitionState(SorcererStateType.Idle);
                    }
                }
            }
        }
        else
        {
            // Fallback if no animator
            if (hitTimer <= 0f)
            {
                if (manager.currentHealth <= 0)
                {
                    manager.TransitionState(SorcererStateType.Dead);
                }
                else
                {
                    if (manager.IsPlayerInRange(parameter.detectionRange))
                    {
                        manager.TransitionState(SorcererStateType.Attack);
                    }
                    else
                    {
                        manager.TransitionState(SorcererStateType.Idle);
                    }
                }
            }
        }
    }

    public void OnExit()
    {
        parameter.isHit = false;
        Debug.Log("Sorcerer exited hit state");
    }
}
