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
    private float damageWindowStart = 0.3f; // �˺���ⴰ�ڿ�ʼʱ��
    private float damageWindowEnd = 0.7f;   // �˺���ⴰ�ڽ���ʱ��
    private float animStartTime;

    public MGoBlinAttackState(MGoBlinP manager, MGoBlinParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }
    
    public void OnEnter()
    {
        parameter.animator.Play("MGoBlin_attack");
        hasDealtDamage = false;
        animStartTime = Time.time;
        Debug.Log("MGoBlinAttackState: ���빥��״̬");
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
        
        isInAttackRange = CheckAttackRange();
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        
        // ȷ����������������ڹ�����Χ�ڲ��л���Attack2
        if (info.normalizedTime >= 0.95f && info.IsName("MGoBlin_attack"))
        {
            if (isInAttackRange)
            {
                Debug.Log("MGoBlinAttackState: �ڹ�����Χ�ڣ��л�������2״̬");
                manager.TransitionState(MGoBlinStateType.Attack2);
                return;
            }
            else
            {
                Debug.Log("MGoBlinAttackState: ���ڹ�����Χ�ڣ��л���׷��״̬");
                manager.TransitionState(MGoBlinStateType.Chase);
            }
        }
    }
    
    private void HandleDamageDetection(float timeSinceStart)
    {
        // ���˺���ⴰ���ڼ����ײ
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
        // ����Ŀ�������ֵ���
        HeroLife heroLife = target.GetComponent<HeroLife>();
        if (heroLife == null)
        {
            heroLife = target.GetComponentInParent<HeroLife>();
        }
        
        if (heroLife != null)
        {
            heroLife.TakeDamage(parameter.damage);
            hasDealtDamage = true;
            
            Debug.Log($"MGoBlin ��������! ��� {parameter.damage} �˺�");
            
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
            float knockbackForce = 3f; // ��ͨ�����Ļ�������С
            targetRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }
    }

    public void OnExit()
    {
        hasDealtDamage = false;
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


