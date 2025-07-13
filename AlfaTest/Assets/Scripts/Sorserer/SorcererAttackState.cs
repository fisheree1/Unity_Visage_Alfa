using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SorcererAttackState : IState
{
    private SorcererP manager;
    private SorcererParameter parameter;
    private AnimatorStateInfo info;
    private float attackTimer;
    private float attackPreDelay = 0.8f; // Time before firing projectile
    private float attackPostDelay = 0.5f; // Time after firing projectile
    private bool hasFireProjectile = false;
    private float stateTimer;
    
    public SorcererAttackState(SorcererP manager, SorcererParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }

    public void OnEnter()
    {
        parameter.isAttacking = true;
        attackTimer = parameter.attackCooldown;
        hasFireProjectile = false;
        stateTimer = 0f;
        
        // Face the player
        if (parameter.target != null)
        {
            manager.FlipTo(parameter.target);
        }
        
        // Play attack animation
        if (parameter.animator != null)
        {
            parameter.animator.Play("Sorcerer_attack");
        }
        
        Debug.Log("Sorcerer entered attack state");
    }

    public void OnUpdate()
    {
        stateTimer += Time.deltaTime;
        attackTimer -= Time.deltaTime;
        
        // Check if we should fire projectile
        if (!hasFireProjectile && stateTimer >= attackPreDelay)
        {
            if (parameter.target != null && manager.IsPlayerInRange(parameter.attackRange))
            {
                manager.FireProjectile();
                hasFireProjectile = true;
                Debug.Log("Sorcerer fired projectile");
            }
        }
        
        // Check if attack is finished
        bool attackFinished = false;
        
        if (parameter.animator != null)
        {
            info = parameter.animator.GetCurrentAnimatorStateInfo(0);
            attackFinished = info.normalizedTime >= 0.95f;
        }
        else
        {
            // Fallback timing
            attackFinished = stateTimer >= (attackPreDelay + attackPostDelay);
        }
        
        if (attackFinished)
        {
            // Check if we should continue attacking or return to idle
            if (parameter.target != null && manager.IsPlayerInRange(parameter.detectionRange) && attackTimer <= 0f)
            {
                // Continue attacking if player is still in range and cooldown is over
                manager.TransitionState(SorcererStateType.Attack);
            }
            else if (parameter.target != null && manager.IsPlayerInRange(parameter.detectionRange))
            {
                // Wait in idle if player is in range but cooldown is not over
                manager.TransitionState(SorcererStateType.Idle);
            }
            else
            {
                // Return to idle if player is out of range
                manager.TransitionState(SorcererStateType.Idle);
            }
        }
    }

    public void OnExit()
    {
        parameter.isAttacking = false;
        Debug.Log("Sorcerer exited attack state");
    }
}
