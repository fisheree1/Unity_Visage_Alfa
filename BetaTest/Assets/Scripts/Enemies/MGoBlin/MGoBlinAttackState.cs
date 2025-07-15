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
    private float damageWindowStart = 0.2f; // �˺���ⴰ�ڿ�ʼʱ�� - ��ǰ
    private float damageWindowEnd = 0.6f;   // �˺���ⴰ�ڽ���ʱ�� - ����
    private float animStartTime;
    private bool hasTriggeredNextAttack = false;

    public MGoBlinAttackState(MGoBlinP manager, MGoBlinParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }
    
    public void OnEnter()
    {
        parameter.animator.Play("MGoBlin_attack");
        hasDealtDamage = false;
        hasTriggeredNextAttack = false;
        animStartTime = Time.time;
        
        // ������������
        manager.IncrementCombo(MGoBlinStateType.Attack);
        
        Debug.Log($"MGoBlinAttackState: ���빥��״̬ (������: {parameter.currentComboCount})");
    }

    public void OnUpdate()
    {
        if (parameter.isHit)
        {
            manager.TransitionState(MGoBlinStateType.Hit);
            return;
        }
        
        float timeSinceStart = Time.time - animStartTime;
        
        // �����˺����
        HandleDamageDetection(timeSinceStart);
        
        // ��΢�Ĺ���ʱ�ƶ�����������
        HandleAttackMovement();
        
        isInAttackRange = CheckAttackRange();
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        
        // ��ǰ�ж���һ������ - ������������
        if (!hasTriggeredNextAttack && info.normalizedTime >= 0.7f && info.IsName("MGoBlin_attack"))
        {
            hasTriggeredNextAttack = true;
            DecideNextAction();
        }
        
        // ������ɺ������״̬ת��
        if (info.normalizedTime >= 0.95f && info.IsName("MGoBlin_attack"))
        {
            if (!hasTriggeredNextAttack)
            {
                DecideNextAction();
            }
        }
    }
    
    private void DecideNextAction()
    {
        // ���Ŀ���Ƿ���Ч
        if (!manager.IsTargetValid())
        {
            Debug.Log("MGoBlinAttackState: Target invalid, switching to patrol");
            manager.TransitionState(MGoBlinStateType.Patrol);
            return;
        }
        
        // �����ܵ���һ���ж�����
        if (manager.CanChainAttack() && isInAttackRange)
        {
            MGoBlinStateType nextAttack = manager.GetNextAttackType();
            if (nextAttack != MGoBlinStateType.Chase)
            {
                Debug.Log($"MGoBlinAttackState: ���ӵ� {nextAttack} ״̬");
                manager.TransitionState(nextAttack);
                return;
            }
        }
        
        // ��������������ڹ�����Χ��
        if (isInAttackRange && UnityEngine.Random.value < parameter.comboChance * 0.6f)
        {
            Debug.Log("MGoBlinAttackState: �ڹ�����Χ�ڣ��л�������2״̬");
            manager.TransitionState(MGoBlinStateType.Attack2);
        }
        else
        {
            Debug.Log("MGoBlinAttackState: ���ڹ�����Χ�ڣ��л���׷��״̬");
            manager.TransitionState(MGoBlinStateType.Chase);
        }
    }
    
    private void HandleAttackMovement()
    {
        // �ڹ�����������΢��Ŀ���ƶ���ʹ����������
        if (manager.IsTargetValid() && !hasDealtDamage)
        {
            Vector3 targetDirection = (parameter.target.position - manager.transform.position).normalized;
            float moveDistance = parameter.aggressiveMovementSpeed * Time.deltaTime * 0.5f;
            
            // ֻ�ڹ���ǰ����ƶ�
            AnimatorStateInfo currentInfo = parameter.animator.GetCurrentAnimatorStateInfo(0);
            if (currentInfo.normalizedTime < 0.5f)
            {
                manager.transform.position += targetDirection * moveDistance;
            }
        }
    }
    
    private void HandleDamageDetection(float timeSinceStart)
    {
        // ���˺���ⴰ���ڼ�����ײ
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
        // ��Ŀ������˺�����������
        HeroLife heroLife = target.GetComponent<HeroLife>();
        if (heroLife == null)
        {
            heroLife = target.GetComponentInParent<HeroLife>();
        }
        
        if (heroLife != null)
        {
            // ���������������˺�
            int comboBonus = Mathf.FloorToInt(parameter.currentComboCount * 0.5f);
            int totalDamage = parameter.damage + comboBonus;
            
            heroLife.TakeDamage(totalDamage);
            hasDealtDamage = true;
            
            Debug.Log($"MGoBlin ��������! ��� {totalDamage} �˺� (��������: {comboBonus})");
            
            // ��Ŀ��ʩ����΢����Ч��
            ApplyKnockback(target);
        }
    }
    
    private void ApplyKnockback(Collider2D target)
    {
        Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
        if (targetRb != null)
        {
            // ������˷���
            Vector2 knockbackDirection = (target.transform.position - manager.transform.position).normalized;
            float knockbackForce = 2.5f; // ���ٻ������ȣ�ʹ����������
            targetRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }
    }

    public void OnExit()
    {
        hasDealtDamage = false;
        hasTriggeredNextAttack = false;
        Debug.Log("MGoBlinAttackState: �˳�����״̬");
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


