using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SorcererIdleState : IState
{
    private SorcererP manager;
    private SorcererParameter parameter;
    private float detectionCheckInterval = 0.2f; // Check for player every 0.2 seconds
    private float detectionTimer;
    
    public SorcererIdleState(SorcererP manager, SorcererParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }

    public void OnEnter()
    {
        parameter.isAttacking = false;
        parameter.isHit = false;
        detectionTimer = 0f;
        
        // Play idle animation
        if (parameter.animator != null)
        {
            parameter.animator.Play("Sorcerer_attack"); // 修复：使用正确的动画名称
        }
        
        Debug.Log("Sorcerer entered idle state");
    }

    public void OnUpdate()
    {
        detectionTimer += Time.deltaTime;
        
        // Periodically check for player
        if (detectionTimer >= detectionCheckInterval)
        {
            detectionTimer = 0f;
            CheckForPlayer();
        }
        
        // If we already have a target, check if they're in range
        if (parameter.target != null)
        {
            float distanceToPlayer = manager.GetDistanceToPlayer();
            
            if (distanceToPlayer <= parameter.detectionRange)
            {
                // Player is in range, start attacking
                manager.TransitionState(SorcererStateType.Attack);
            }
            else
            {
                // Player is too far, lose target
                parameter.target = null;
            }
        }
    }

    public void OnExit()
    {
        Debug.Log("Sorcerer exited idle state");
    }
    
    private void CheckForPlayer()
    {
        // Find player if we don't have a target
        if (parameter.target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                float distanceToPlayer = Vector2.Distance(manager.transform.position, player.transform.position);
                
                if (distanceToPlayer <= parameter.detectionRange)
                {
                    parameter.target = player.transform;
                    
                    // Face the player
                    manager.FlipTo(parameter.target);
                    
                    // Start attacking
                    manager.TransitionState(SorcererStateType.Attack);
                }
            }
        }
    }
}