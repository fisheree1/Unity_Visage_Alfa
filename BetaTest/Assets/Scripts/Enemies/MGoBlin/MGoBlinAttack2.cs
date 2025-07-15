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
    private float damageWindowStart = 0.2f; // �˺���ⴰ�ڿ�ʼʱ�� - ��ǰ
    private float damageWindowEnd = 0.6f;   // �˺���ⴰ�ڽ���ʱ�� - ����
    private float animStartTime;
    private bool hasTriggeredNextAttack = false;

    public MGoBlinAttack2State(MGoBlinP manager, MGoBlinParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }
    
    public void OnEnter()
    {
        parameter.animator.Play("MGoBlin_attack2");
        hasDealtDamage = false;
        hasTriggeredNextAttack = false;
        animStartTime = Time.time;
        
        // ������������
        manager.IncrementCombo(MGoBlinStateType.Attack2);
        
        Debug.Log($"MGoBlinAttack2State: ���빥��2״̬ (������: {parameter.currentComboCount})");
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
        
        // ��΢�Ĺ���ʱ�ƶ�
        HandleAttackMovement();
        
        isInAttackRange = CheckAttackRange();
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        
        // ��ǰ�ж���һ������ - ������������
        if (!hasTriggeredNextAttack && info.normalizedTime >= 0.7f && info.IsName("MGoBlin_attack2"))
        {
            hasTriggeredNextAttack = true;
            DecideNextAction();
        }
        
        // ������ɺ������״̬ת��
        if (info.normalizedTime >= 0.95f && info.IsName("MGoBlin_attack2"))
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
            Debug.Log("MGoBlinAttack2State: Target invalid, switching to patrol");
            manager.TransitionState(MGoBlinStateType.Patrol);
            return;
        }
        
        // �����ܵ���һ���ж�����
        if (manager.CanChainAttack() && isInAttackRange)
        {
            MGoBlinStateType nextAttack = manager.GetNextAttackType();
            if (nextAttack != MGoBlinStateType.Chase)
            {
                Debug.Log($"MGoBlinAttack2State: ���ӵ� {nextAttack} ״̬");
                manager.TransitionState(nextAttack);
                return;
            }
        }
        
        // ����������������ݾ���ͼ��ʾ���
        if (isInAttackRange && UnityEngine.Random.value < parameter.comboChance * 0.7f)
        {
            Debug.Log("MGoBlinAttack2State: �ڹ�����Χ�ڣ�ѭ���ع���1״̬");
            manager.TransitionState(MGoBlinStateType.Attack);
        }
        else
        {
            float distanceToTarget = parameter.target != null ? 
                Vector2.Distance(manager.transform.position, parameter.target.position) : float.MaxValue;
            
            // ����ڳ�̹�����Χ�ڣ��л���ʹ���ػ�
            if (distanceToTarget <= parameter.dashAttackArea && UnityEngine.Random.value < parameter.comboChance * 0.5f)
            {
                Debug.Log("MGoBlinAttack2State: �л����ػ�״̬");
                manager.TransitionState(MGoBlinStateType.HeavyAtk);
            }
            else
            {
                Debug.Log("MGoBlinAttack2State: �л���׷��״̬");
                manager.TransitionState(MGoBlinStateType.Chase);
            }
        }
    }
    
    private void HandleAttackMovement()
    {
        // �ڹ�����������΢��Ŀ���ƶ�
        if (manager.IsTargetValid() && !hasDealtDamage)
        {
            Vector3 targetDirection = (parameter.target.position - manager.transform.position).normalized;
            float moveDistance = parameter.aggressiveMovementSpeed * Time.deltaTime * 0.3f;
            
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
            // Attack2�������ɸ����˺���������������
            int comboBonus = Mathf.FloorToInt(parameter.currentComboCount * 0.5f);
            int attack2Damage = Mathf.Max(1, parameter.damage + 1 + comboBonus);
            
            heroLife.TakeDamage(attack2Damage);
            hasDealtDamage = true;
            
            Debug.Log($"MGoBlin ����2����! ��� {attack2Damage} �˺� (��������: {comboBonus})");
            
            // ��Ŀ��ʩ�ӻ���Ч��
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
            float knockbackForce = 3.5f; // Attack2�Ļ��������Դ�
            targetRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }
    }

    public void OnExit()
    {
        hasDealtDamage = false;
        hasTriggeredNextAttack = false;
        Debug.Log("MGoBlinAttack2State: �˳�����2״̬");
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


