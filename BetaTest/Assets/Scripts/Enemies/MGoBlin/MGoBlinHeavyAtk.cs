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
    
    // ��̹������б���
    private bool hasDashed = false;
    private bool hasDealtDamage = false;
    private Vector2 dashDirection;
    private float dashSpeed = 35f; // ���ӳ���ٶ�
    private float dashStartTime = 0.15f;  // ���翪ʼ���
    private float dashEndTime = 0.7f;     // ����������
    private float damageWindowStart = 0.2f; // �˺���ⴰ�ڿ�ʼʱ�� - ��ǰ
    private float damageWindowEnd = 0.6f;   // �˺���ⴰ�ڽ���ʱ�� - ����
    private float animStartTime;
    private bool isDashing = false;
    private bool hasTriggeredNextAttack = false;

    public MGoBlinHeavyAtkState(MGoBlinP manager, MGoBlinParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
        rb = manager.GetComponent<Rigidbody2D>();
    }
    
    public void OnEnter()
    {
        // ����״̬
        hasDashed = false;
        hasDealtDamage = false;
        isDashing = false;
        hasTriggeredNextAttack = false;
        animStartTime = Time.time;
        
        // ������������
        manager.IncrementCombo(MGoBlinStateType.HeavyAtk);
        
        // ȷ����̷���
        if (parameter.target != null)
        {
            Vector3 targetDirection = (parameter.target.position - manager.transform.position).normalized;
            dashDirection = new Vector2(targetDirection.x, 0); // ֻ��ˮƽ����
            
            // ����Ŀ��
            manager.FlipTo(parameter.target);
        }
        else
        {
            // ���û��Ŀ�꣬���ݵ�ǰ���������̷���
            float facing = manager.transform.localScale.x > 0 ? -1f : 1f;
            dashDirection = new Vector2(facing, 0);
        }
        
        // ���ų�̹�������
        parameter.animator.Play("MGoBlin_dashattack");
        
        Debug.Log($"MGoBlin ��ʼ��̹���! (������: {parameter.currentComboCount})");
    }

    public void OnUpdate()
    {
        if (parameter.isHit)
        {
            StopDash();
            manager.TransitionState(MGoBlinStateType.Hit);
            return;
        }
        
        float timeSinceStart = Time.time - animStartTime;
        
        // �������ƶ�
        HandleDashMovement(timeSinceStart);
        
        // �����˺����
        HandleDamageDetection(timeSinceStart);
        
        // ��鹥����Χ�����ں���״̬������
        isInAttackRange = CheckAttackRange();
        
        // ��ȡ������Ϣ
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        
        // ��ǰ�ж���һ������ - ������������
        if (!hasTriggeredNextAttack && info.normalizedTime >= 0.8f)
        {
            hasTriggeredNextAttack = true;
            DecideNextAction();
        }
        
        // ��鶯���Ƿ����
        if (info.normalizedTime >= 0.95f)
        {
            if (!hasTriggeredNextAttack)
            {
                DecideNextAction();
            }
        }
    }
    
    private void DecideNextAction()
    {
        // ֹͣ���
        StopDash();
        
        // ���Ŀ���Ƿ���Ч
        if (!manager.IsTargetValid())
        {
            Debug.Log("MGoBlinHeavyAtkState: Target invalid, switching to patrol");
            manager.TransitionState(MGoBlinStateType.Patrol);
            return;
        }
        
        // �����ܵ���һ���ж�����
        if (manager.CanChainAttack() && isInAttackRange)
        {
            MGoBlinStateType nextAttack = manager.GetNextAttackType();
            if (nextAttack != MGoBlinStateType.Chase)
            {
                Debug.Log($"MGoBlinHeavyAtkState: ���ӵ� {nextAttack} ״̬");
                manager.TransitionState(nextAttack);
                return;
            }
        }
        
        // �����Ƿ������������׷��
        if (isInAttackRange && UnityEngine.Random.value < parameter.comboChance * 0.6f)
        {
            Debug.Log("MGoBlinHeavyAtkState: �ڹ�����Χ�ڣ��л�����ͨ����");
            manager.TransitionState(MGoBlinStateType.Attack);
        }
        else
        {
            Debug.Log("MGoBlinHeavyAtkState: �л���׷��״̬");
            manager.TransitionState(MGoBlinStateType.Chase);
        }
    }
    
    private void HandleDashMovement(float timeSinceStart)
    {
        // ��ָ��ʱ�䴰����ִ�г��
        if (timeSinceStart >= dashStartTime && timeSinceStart <= dashEndTime)
        {
            if (!isDashing)
            {
                isDashing = true;
                Debug.Log("��ʼ����ƶ�");
            }
            
            // ���ڹ����Ե�������ٶ�
            float adjustedDashSpeed = dashSpeed * parameter.aggressionMultiplier;
            
            if (rb != null)
            {
                // ����ʽ���ó���ٶ�
                rb.velocity = new Vector2(dashDirection.x * adjustedDashSpeed, rb.velocity.y);
            }
            else
            {
                // �任�ƶ���ʽ�����û��Rigidbody2D��
                manager.transform.Translate(dashDirection * adjustedDashSpeed * Time.deltaTime);
            }
            
            hasDashed = true;
        }
        else if (isDashing && timeSinceStart > dashEndTime)
        {
            // ��̽�������ֹͣ
            SlowDownDash();
            isDashing = false;
        }
    }
    
    private void HandleDamageDetection(float timeSinceStart)
    {
        // ���˺���ⴰ���ڼ�����ײ
        if (!hasDealtDamage && timeSinceStart >= damageWindowStart && timeSinceStart <= damageWindowEnd)
        {
            // ʹ�ø���Ĺ�����Χ����̹���
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
        // ��Ŀ������˺�����������
        HeroLife heroLife = target.GetComponent<HeroLife>();
        if (heroLife == null)
        {
            heroLife = target.GetComponentInParent<HeroLife>();
        }
        
        if (heroLife != null)
        {
            // ��̹�����ɸ����˺���������������
            int comboBonus = Mathf.FloorToInt(parameter.currentComboCount * 0.7f);
            int dashDamage = Mathf.Max(1, parameter.damage * 2 + comboBonus);
            
            heroLife.TakeDamage(dashDamage);
            hasDealtDamage = true;
            
            Debug.Log($"MGoBlin ��̹�������! ��� {dashDamage} �˺� (��������: {comboBonus})");
            
            // ��Ŀ��ʩ�ӻ���Ч��
            ApplyKnockback(target);
        }
    }
    
    private void ApplyKnockback(Collider2D target)
    {
        Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
        if (targetRb != null)
        {
            Vector2 knockbackDirection = dashDirection.normalized;
            float knockbackForce = 6f; // ��̹����Ļ������ȸ���
            targetRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
            
            Debug.Log("Ӧ�û���Ч��");
        }
    }
    
    private void SlowDownDash()
    {
        if (rb != null)
        {
            // �𽥼��٣�����ͻȻֹͣ��ɵĲ���Ȼ�о�
            rb.velocity = new Vector2(rb.velocity.x * 0.5f, rb.velocity.y);
        }
    }
    
    private void StopDash()
    {
        if (rb != null)
        {
            // ȷ����ȫֹͣ�ƶ�
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        isDashing = false;
    }

    public void OnExit()
    {
        // ȷ����ȫֹͣ�ƶ�
        StopDash();
        
        // ����״̬����
        hasDashed = false;
        hasDealtDamage = false;
        isDashing = false;
        hasTriggeredNextAttack = false;
        
        Debug.Log("MGoBlin ��̹������");
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


