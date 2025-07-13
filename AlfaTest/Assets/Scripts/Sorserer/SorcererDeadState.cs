using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SorcererDeadState : IState
{
    private SorcererP manager;
    private SorcererParameter parameter;
    private AnimatorStateInfo info;
    private bool hasPlayedDeathAnimation = false;
    private float deathTimer = 2f; // Time before destroying the object
    private float currentTimer;
    
    public SorcererDeadState(SorcererP manager, SorcererParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }

    public void OnEnter()
    {
        parameter.isAttacking = false;
        parameter.isHit = false;
        hasPlayedDeathAnimation = false;
        currentTimer = 0f;
        
        // Disable collider to prevent further interactions
        Collider2D collider = manager.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }
        
        // Play death animation
        if (parameter.animator != null)
        {
            parameter.animator.Play("Sorcerer_dead");
            hasPlayedDeathAnimation = true;
        }
        
        Debug.Log("Sorcerer entered dead state");
    }

    public void OnUpdate()
    {
        currentTimer += Time.deltaTime;
        
        // Check if death animation is finished
        if (hasPlayedDeathAnimation && parameter.animator != null)
        {
            info = parameter.animator.GetCurrentAnimatorStateInfo(0);
            if (info.normalizedTime >= 0.95f)
            {
                hasPlayedDeathAnimation = false; // Animation finished
            }
        }
        
        // Destroy the object after a delay
        if (currentTimer >= deathTimer)
        {
            if (manager.gameObject != null)
            {
                Object.Destroy(manager.gameObject);
            }
        }
    }

    public void OnExit()
    {
        Debug.Log("Sorcerer exited dead state");
    }
}